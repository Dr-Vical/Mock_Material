#Requires -Version 5.1
<#
.SYNOPSIS
    RswareDesign Diagnostics Tool
.DESCRIPTION
    Collects system info, serial port status, DLL integrity, and app logs
    for troubleshooting RswareDesign installation/runtime issues.
    Generates an HTML report on the Desktop.
#>

param(
    [string]$AppDir = (Split-Path -Parent $PSScriptRoot),
    [string]$OutputDir = [Environment]::GetFolderPath("Desktop")
)

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = Join-Path $OutputDir "RswareDesign_Diag_$timestamp.html"

# ── Collect Data ──────────────────────────────────────────────

function Get-SystemInfo {
    $os = Get-CimInstance Win32_OperatingSystem
    $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
    $mem = [math]::Round($os.TotalVisibleMemorySize / 1MB, 1)
    $freeMem = [math]::Round($os.FreePhysicalMemory / 1MB, 1)
    $disk = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'"
    $freeGB = [math]::Round($disk.FreeSpace / 1GB, 1)
    $totalGB = [math]::Round($disk.Size / 1GB, 1)

    # .NET runtime detection
    $dotnetVersions = @()
    try {
        $dotnetVersions = & dotnet --list-runtimes 2>&1 | Where-Object { $_ -match "Microsoft\." }
    } catch {
        $dotnetVersions = @("dotnet CLI not found (self-contained app — OK)")
    }

    # Windows version detail
    $build = "$($os.Version) (Build $($os.BuildNumber))"

    return @{
        OS           = "$($os.Caption) $($os.OSArchitecture)"
        Build        = $build
        CPU          = $cpu.Name
        MemoryGB     = "$freeMem / $mem GB free"
        DiskC        = "$freeGB / $totalGB GB free"
        DotNetRuntimes = $dotnetVersions
        UserName     = $env:USERNAME
        MachineName  = $env:COMPUTERNAME
        Culture      = (Get-Culture).Name
        TimeZone     = (Get-TimeZone).DisplayName
    }
}

function Get-SerialPortInfo {
    $results = @()

    # WMI serial port enumeration
    $wmiPorts = @()
    try {
        $wmiPorts = Get-CimInstance Win32_SerialPort -ErrorAction SilentlyContinue
    } catch {}

    # PnP device enumeration (more reliable for USB-Serial)
    $pnpPorts = @()
    try {
        $pnpPorts = Get-CimInstance Win32_PnPEntity -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match "COM\d+" -or $_.Service -match "FTDI|usbser|CH341|CP210" }
    } catch {}

    # Registry-based COM port list
    $regPorts = @()
    try {
        $regPath = "HKLM:\HARDWARE\DEVICEMAP\SERIALCOMM"
        if (Test-Path $regPath) {
            $regPorts = Get-ItemProperty $regPath -ErrorAction SilentlyContinue |
                Get-Member -MemberType NoteProperty |
                Where-Object { $_.Name -notmatch "^PS" } |
                ForEach-Object {
                    $val = (Get-ItemProperty $regPath).$($_.Name)
                    @{ Registry = $_.Name; Port = $val }
                }
        }
    } catch {}

    # .NET SerialPort enumeration
    $dotnetPorts = @()
    try {
        Add-Type -AssemblyName System.IO.Ports -ErrorAction SilentlyContinue
        $dotnetPorts = [System.IO.Ports.SerialPort]::GetPortNames()
    } catch {}

    # FTDI driver check
    $ftdiDriver = $null
    try {
        $ftdiDriver = Get-CimInstance Win32_PnPSignedDriver -ErrorAction SilentlyContinue |
            Where-Object { $_.DeviceName -match "FTDI|FT232|FT2232|FT4232" } |
            Select-Object DeviceName, DriverVersion, DriverDate -First 3
    } catch {}

    # CH340/CP2102 driver check
    $usbSerialDrivers = @()
    try {
        $usbSerialDrivers = Get-CimInstance Win32_PnPSignedDriver -ErrorAction SilentlyContinue |
            Where-Object { $_.DeviceName -match "CH340|CH341|CP210|Prolific|USB-SERIAL" } |
            Select-Object DeviceName, DriverVersion, DriverDate
    } catch {}

    # Port access test
    $portTests = @()
    foreach ($port in $dotnetPorts) {
        $test = @{ Port = $port; Accessible = $false; Error = "" }
        try {
            $sp = New-Object System.IO.Ports.SerialPort $port, 9600
            $sp.Open()
            $sp.Close()
            $sp.Dispose()
            $test.Accessible = $true
        } catch {
            $test.Error = $_.Exception.Message
        }
        $portTests += $test
    }

    return @{
        WmiPorts        = $wmiPorts
        PnpPorts        = $pnpPorts
        RegistryPorts   = $regPorts
        DotNetPorts     = $dotnetPorts
        FtdiDriver      = $ftdiDriver
        UsbSerialDrivers = $usbSerialDrivers
        PortAccessTests = $portTests
    }
}

function Get-DllIntegrity {
    $requiredDlls = @(
        "RswareDesign.exe",
        "RswareDesign.dll",
        "AvalonDock.dll",
        "AvalonDock.Themes.VS2013.dll",
        "Fluent.dll",
        "MaterialDesignThemes.Wpf.dll",
        "MaterialDesignColors.dll",
        "CommunityToolkit.Mvvm.dll",
        "ScottPlot.dll",
        "ScottPlot.WPF.dll",
        "System.IO.Ports.dll",
        "CsvHelper.dll",
        "coreclr.dll",
        "hostpolicy.dll",
        "hostfxr.dll",
        "wpfgfx_cor3.dll",
        "ControlzEx.dll",
        "Microsoft.Xaml.Behaviors.dll"
    )

    $results = @()
    foreach ($dll in $requiredDlls) {
        $path = Join-Path $AppDir $dll
        $info = @{ Name = $dll; Exists = $false; Size = ""; Version = ""; Path = $path }
        if (Test-Path $path) {
            $file = Get-Item $path
            $info.Exists = $true
            $info.Size = "{0:N0} KB" -f ($file.Length / 1KB)
            try {
                $ver = $file.VersionInfo.FileVersion
                if ($ver) { $info.Version = $ver }
            } catch {}
        }
        $results += $info
    }

    # Check Localization folder
    $locDir = Join-Path $AppDir "Localization"
    $locFiles = @()
    if (Test-Path $locDir) {
        $locFiles = Get-ChildItem $locDir -File | Select-Object Name, Length
    }

    return @{
        DllChecks = $results
        LocalizationFiles = $locFiles
        TotalFilesInAppDir = (Get-ChildItem $AppDir -File -ErrorAction SilentlyContinue).Count
    }
}

function Get-AppLogs {
    $logs = @{}

    # crash.log
    $crashLog = Join-Path $AppDir "crash.log"
    if (Test-Path $crashLog) {
        $content = Get-Content $crashLog -Tail 100 -ErrorAction SilentlyContinue
        $logs["crash.log"] = @{
            Path = $crashLog
            Size = "{0:N0} KB" -f ((Get-Item $crashLog).Length / 1KB)
            LastModified = (Get-Item $crashLog).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
            LastLines = $content -join "`n"
        }
    }

    # Serilog files (common patterns)
    $logPatterns = @("*.log", "logs/*.log", "logs/*.txt")
    foreach ($pattern in $logPatterns) {
        $found = Get-ChildItem (Join-Path $AppDir $pattern) -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending | Select-Object -First 3
        foreach ($f in $found) {
            if (-not $logs.ContainsKey($f.Name)) {
                $content = Get-Content $f.FullName -Tail 50 -ErrorAction SilentlyContinue
                $logs[$f.Name] = @{
                    Path = $f.FullName
                    Size = "{0:N0} KB" -f ($f.Length / 1KB)
                    LastModified = $f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                    LastLines = $content -join "`n"
                }
            }
        }
    }

    # Windows Event Log - Application errors related to RswareDesign
    $eventLogs = @()
    try {
        $eventLogs = Get-WinEvent -FilterHashtable @{
            LogName = 'Application'
            Level = 1,2  # Critical, Error
            StartTime = (Get-Date).AddDays(-7)
        } -MaxEvents 20 -ErrorAction SilentlyContinue |
            Where-Object { $_.Message -match "RswareDesign|\.NET|CLR|wpf" } |
            Select-Object TimeCreated, Id, LevelDisplayName, Message -First 10
    } catch {}

    # .NET crash dumps
    $crashDumps = @()
    $dumpPaths = @(
        "$env:LOCALAPPDATA\CrashDumps",
        "$env:TEMP"
    )
    foreach ($dp in $dumpPaths) {
        if (Test-Path $dp) {
            $dumps = Get-ChildItem $dp -Filter "RswareDesign*.dmp" -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending | Select-Object -First 3
            $crashDumps += $dumps
        }
    }

    return @{
        AppLogs = $logs
        EventLogs = $eventLogs
        CrashDumps = $crashDumps
    }
}

function Get-EnvironmentCheck {
    # Check if app is blocked by SmartScreen / Zone.Identifier
    $exePath = Join-Path $AppDir "RswareDesign.exe"
    $zoneBlocked = $false
    if (Test-Path $exePath) {
        try {
            $streams = Get-Item $exePath -Stream * -ErrorAction SilentlyContinue
            $zoneBlocked = ($streams | Where-Object { $_.Stream -eq "Zone.Identifier" }) -ne $null
        } catch {}
    }

    # Check Windows Defender exclusions / recent blocks
    $defenderBlocks = @()
    try {
        $defenderBlocks = Get-MpThreatDetection -ErrorAction SilentlyContinue |
            Where-Object { $_.Resources -match "RswareDesign" } |
            Select-Object -First 5
    } catch {}

    # Display info
    $displays = @()
    try {
        $displays = Get-CimInstance Win32_VideoController |
            Select-Object Name, VideoModeDescription, CurrentHorizontalResolution, CurrentVerticalResolution, DriverVersion
    } catch {}

    # DPI scaling
    $dpiScale = 100
    try {
        $dpiReg = Get-ItemProperty "HKCU:\Control Panel\Desktop\WindowMetrics" -Name "AppliedDPI" -ErrorAction SilentlyContinue
        if ($dpiReg) { $dpiScale = [math]::Round($dpiReg.AppliedDPI / 96 * 100) }
    } catch {}

    # UAC level
    $uacEnabled = $false
    try {
        $uac = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" -ErrorAction SilentlyContinue
        $uacEnabled = $uac.EnableLUA -eq 1
    } catch {}

    # Power plan
    $powerPlan = ""
    try {
        $powerPlan = (powercfg /getactivescheme 2>&1) -join " "
    } catch {}

    return @{
        ZoneBlocked    = $zoneBlocked
        DefenderBlocks = $defenderBlocks
        Displays       = $displays
        DpiScale       = "$dpiScale%"
        UacEnabled     = $uacEnabled
        PowerPlan      = $powerPlan
    }
}

# ── Generate HTML Report ──────────────────────────────────────

function ConvertTo-HtmlReport {
    param($Sys, $Serial, $Dll, $Logs, $Env)

    $passIcon = "&#x2705;"
    $failIcon = "&#x274C;"
    $warnIcon = "&#x26A0;&#xFE0F;"

    # Summary counters
    $dllMissing = ($Dll.DllChecks | Where-Object { -not $_.Exists }).Count
    $portCount = $Serial.DotNetPorts.Count
    $portErrors = ($Serial.PortAccessTests | Where-Object { -not $_.Accessible }).Count
    $hasLogs = $Logs.AppLogs.Count -gt 0
    $hasEvents = $Logs.EventLogs.Count -gt 0
    $isBlocked = $Env.ZoneBlocked

    $html = @"
<!DOCTYPE html>
<html lang="ko">
<head>
<meta charset="UTF-8">
<title>RswareDesign Diagnostics Report</title>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body { font-family: 'Segoe UI', 'Malgun Gothic', sans-serif; background: #0d1117; color: #c9d1d9; padding: 24px; line-height: 1.6; }
  h1 { color: #58a6ff; margin-bottom: 8px; font-size: 24px; }
  .subtitle { color: #8b949e; margin-bottom: 24px; font-size: 13px; }
  .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin-bottom: 32px; }
  .summary-card { background: #161b22; border: 1px solid #30363d; border-radius: 8px; padding: 16px; text-align: center; }
  .summary-card.pass { border-color: #238636; }
  .summary-card.fail { border-color: #da3633; }
  .summary-card.warn { border-color: #d29922; }
  .summary-card .num { font-size: 28px; font-weight: 700; }
  .summary-card .num.pass { color: #3fb950; }
  .summary-card .num.fail { color: #f85149; }
  .summary-card .num.warn { color: #d29922; }
  .summary-card .label { color: #8b949e; font-size: 12px; margin-top: 4px; }
  section { background: #161b22; border: 1px solid #30363d; border-radius: 8px; margin-bottom: 20px; overflow: hidden; }
  section h2 { background: #1c2128; padding: 12px 16px; font-size: 15px; color: #58a6ff; border-bottom: 1px solid #30363d; cursor: pointer; user-select: none; }
  section h2:hover { background: #22272e; }
  section h2::before { content: "▸ "; }
  section.open h2::before { content: "▾ "; }
  .content { padding: 16px; display: none; }
  section.open .content { display: block; }
  table { width: 100%; border-collapse: collapse; font-size: 13px; }
  th { text-align: left; padding: 8px 12px; background: #1c2128; color: #8b949e; font-weight: 600; border-bottom: 1px solid #30363d; }
  td { padding: 8px 12px; border-bottom: 1px solid #21262d; }
  tr:hover td { background: #1c2128; }
  .tag { display: inline-block; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; }
  .tag-pass { background: #23362544; color: #3fb950; border: 1px solid #23862244; }
  .tag-fail { background: #da363322; color: #f85149; border: 1px solid #da363344; }
  .tag-warn { background: #d2992222; color: #d29922; border: 1px solid #d2992244; }
  .tag-info { background: #58a6ff22; color: #58a6ff; border: 1px solid #58a6ff44; }
  pre { background: #0d1117; border: 1px solid #30363d; border-radius: 6px; padding: 12px; font-size: 12px; overflow-x: auto; white-space: pre-wrap; word-break: break-all; max-height: 400px; overflow-y: auto; color: #e6edf3; }
  .kv { display: grid; grid-template-columns: 180px 1fr; gap: 4px 16px; }
  .kv .k { color: #8b949e; font-weight: 600; }
  .kv .v { color: #c9d1d9; }
  .actions { display: flex; gap: 8px; margin-bottom: 24px; }
  .btn { background: #21262d; color: #c9d1d9; border: 1px solid #30363d; padding: 8px 16px; border-radius: 6px; cursor: pointer; font-size: 13px; }
  .btn:hover { background: #30363d; }
  .btn-primary { background: #238636; border-color: #2ea043; color: #fff; }
  .btn-primary:hover { background: #2ea043; }
  footer { text-align: center; color: #484f58; font-size: 11px; margin-top: 32px; padding-top: 16px; border-top: 1px solid #21262d; }
</style>
</head>
<body>

<h1>RswareDesign Diagnostics Report</h1>
<p class="subtitle">Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss") &nbsp;|&nbsp; Machine: $($Sys.MachineName) &nbsp;|&nbsp; User: $($Sys.UserName)</p>

<div class="actions">
  <button class="btn" onclick="toggleAll(true)">Expand All</button>
  <button class="btn" onclick="toggleAll(false)">Collapse All</button>
  <button class="btn btn-primary" onclick="window.print()">Print / Save PDF</button>
</div>

<!-- Summary Cards -->
<div class="summary">
  <div class="summary-card $(if($dllMissing -eq 0){'pass'}else{'fail'})">
    <div class="num $(if($dllMissing -eq 0){'pass'}else{'fail'})">$(if($dllMissing -eq 0){$passIcon}else{"$dllMissing"})</div>
    <div class="label">DLL Missing</div>
  </div>
  <div class="summary-card $(if($portCount -gt 0){'pass'}elseif($portCount -eq 0){'warn'})">
    <div class="num $(if($portCount -gt 0){'pass'}else{'warn'})">$portCount</div>
    <div class="label">COM Ports</div>
  </div>
  <div class="summary-card $(if($portErrors -eq 0){'pass'}else{'fail'})">
    <div class="num $(if($portErrors -eq 0){'pass'}else{'fail'})">$portErrors</div>
    <div class="label">Port Errors</div>
  </div>
  <div class="summary-card $(if(-not $isBlocked){'pass'}else{'fail'})">
    <div class="num $(if(-not $isBlocked){'pass'}else{'fail'})">$(if(-not $isBlocked){$passIcon}else{$failIcon})</div>
    <div class="label">SmartScreen</div>
  </div>
  <div class="summary-card $(if(-not $hasEvents){'pass'}else{'warn'})">
    <div class="num $(if(-not $hasEvents){'pass'}else{'warn'})">$(if($hasEvents){$Logs.EventLogs.Count}else{0})</div>
    <div class="label">Event Errors (7d)</div>
  </div>
</div>

<!-- 1. System Info -->
<section class="open">
<h2>System Information</h2>
<div class="content">
<div class="kv">
  <span class="k">OS</span><span class="v">$($Sys.OS)</span>
  <span class="k">Build</span><span class="v">$($Sys.Build)</span>
  <span class="k">CPU</span><span class="v">$($Sys.CPU)</span>
  <span class="k">Memory</span><span class="v">$($Sys.MemoryGB)</span>
  <span class="k">Disk C:</span><span class="v">$($Sys.DiskC)</span>
  <span class="k">Culture</span><span class="v">$($Sys.Culture)</span>
  <span class="k">TimeZone</span><span class="v">$($Sys.TimeZone)</span>
  <span class="k">DPI Scale</span><span class="v">$($Env.DpiScale)</span>
  <span class="k">UAC</span><span class="v">$(if($Env.UacEnabled){'Enabled'}else{'Disabled'})</span>
</div>
<h3 style="margin-top:16px;font-size:13px;color:#8b949e;">.NET Runtimes</h3>
<pre>$(($Sys.DotNetRuntimes | ForEach-Object { [System.Web.HttpUtility]::HtmlEncode($_) }) -join "`n")</pre>
<h3 style="margin-top:16px;font-size:13px;color:#8b949e;">Display</h3>
<table>
<tr><th>GPU</th><th>Resolution</th><th>Driver</th></tr>
$(foreach ($d in $Env.Displays) {
"<tr><td>$([System.Web.HttpUtility]::HtmlEncode($d.Name))</td><td>$($d.CurrentHorizontalResolution)x$($d.CurrentVerticalResolution)</td><td>$($d.DriverVersion)</td></tr>"
})
</table>
</div>
</section>

<!-- 2. Serial Ports -->
<section class="open">
<h2>Serial Port Diagnostics</h2>
<div class="content">
<h3 style="font-size:13px;color:#8b949e;margin-bottom:8px;">Detected COM Ports (.NET)</h3>
$(if ($Serial.DotNetPorts.Count -eq 0) {
'<p><span class="tag tag-warn">No COM ports detected</span> — Check USB cable and driver installation.</p>'
} else {
"<p>Ports: <strong>$($Serial.DotNetPorts -join ', ')</strong></p>"
})

<h3 style="font-size:13px;color:#8b949e;margin-top:16px;margin-bottom:8px;">Port Access Test</h3>
<table>
<tr><th>Port</th><th>Status</th><th>Error</th></tr>
$(foreach ($t in $Serial.PortAccessTests) {
$status = if($t.Accessible){'<span class="tag tag-pass">OK</span>'}else{'<span class="tag tag-fail">FAILED</span>'}
$err = [System.Web.HttpUtility]::HtmlEncode($t.Error)
"<tr><td>$($t.Port)</td><td>$status</td><td>$err</td></tr>"
})
</table>

<h3 style="font-size:13px;color:#8b949e;margin-top:16px;margin-bottom:8px;">Registry Ports</h3>
<table>
<tr><th>Registry Key</th><th>Port</th></tr>
$(foreach ($r in $Serial.RegistryPorts) {
"<tr><td>$([System.Web.HttpUtility]::HtmlEncode($r.Registry))</td><td>$($r.Port)</td></tr>"
})
</table>

<h3 style="font-size:13px;color:#8b949e;margin-top:16px;margin-bottom:8px;">USB-Serial Drivers</h3>
<table>
<tr><th>Device</th><th>Version</th><th>Date</th></tr>
$(foreach ($d in $Serial.FtdiDriver) {
"<tr><td>$([System.Web.HttpUtility]::HtmlEncode($d.DeviceName))</td><td>$($d.DriverVersion)</td><td>$($d.DriverDate)</td></tr>"
})
$(foreach ($d in $Serial.UsbSerialDrivers) {
"<tr><td>$([System.Web.HttpUtility]::HtmlEncode($d.DeviceName))</td><td>$($d.DriverVersion)</td><td>$($d.DriverDate)</td></tr>"
})
$(if ((-not $Serial.FtdiDriver) -and ($Serial.UsbSerialDrivers.Count -eq 0)) {
'<tr><td colspan="3"><span class="tag tag-warn">No USB-Serial drivers found</span></td></tr>'
})
</table>
</div>
</section>

<!-- 3. DLL Integrity -->
<section>
<h2>DLL Integrity Check ($($Dll.TotalFilesInAppDir) files in app dir)</h2>
<div class="content">
<table>
<tr><th>File</th><th>Status</th><th>Size</th><th>Version</th></tr>
$(foreach ($d in $Dll.DllChecks) {
$status = if($d.Exists){'<span class="tag tag-pass">OK</span>'}else{'<span class="tag tag-fail">MISSING</span>'}
"<tr><td>$($d.Name)</td><td>$status</td><td>$($d.Size)</td><td>$($d.Version)</td></tr>"
})
</table>

$(if ($Dll.LocalizationFiles.Count -gt 0) {
"<h3 style='font-size:13px;color:#8b949e;margin-top:16px;margin-bottom:8px;'>Localization Files</h3><ul style='padding-left:20px;'>"
foreach ($f in $Dll.LocalizationFiles) {
"<li>$($f.Name) ($([math]::Round($f.Length/1KB,1)) KB)</li>"
}
"</ul>"
})
</div>
</section>

<!-- 4. Application Logs -->
<section$(if($hasLogs -or $hasEvents){' class="open"'})>
<h2>Application Logs &amp; Events</h2>
<div class="content">
$(foreach ($key in $Logs.AppLogs.Keys) {
$log = $Logs.AppLogs[$key]
@"
<h3 style="font-size:13px;color:#8b949e;margin-bottom:4px;">$key <span class="tag tag-info">$($log.Size)</span> <span style="color:#484f58;">Last modified: $($log.LastModified)</span></h3>
<pre>$([System.Web.HttpUtility]::HtmlEncode($log.LastLines))</pre>
"@
})

$(if ($hasEvents) {
@"
<h3 style="font-size:13px;color:#8b949e;margin-top:16px;margin-bottom:8px;">Windows Event Log (last 7 days, app-related errors)</h3>
<table>
<tr><th>Time</th><th>Level</th><th>ID</th><th>Message</th></tr>
$(foreach ($e in $Logs.EventLogs) {
$msg = [System.Web.HttpUtility]::HtmlEncode(($e.Message -replace "`n"," ").Substring(0, [Math]::Min($e.Message.Length, 200)))
"<tr><td>$($e.TimeCreated.ToString('MM-dd HH:mm'))</td><td>$($e.LevelDisplayName)</td><td>$($e.Id)</td><td style='max-width:500px;overflow:hidden;text-overflow:ellipsis;'>$msg</td></tr>"
})
</table>
"@
})

$(if ($Logs.CrashDumps.Count -gt 0) {
"<h3 style='font-size:13px;color:#8b949e;margin-top:16px;margin-bottom:8px;'>Crash Dumps Found</h3><ul style='padding-left:20px;'>"
foreach ($d in $Logs.CrashDumps) {
"<li>$($d.Name) ($([math]::Round($d.Length/1MB,1)) MB) — $($d.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))</li>"
}
"</ul>"
})

$(if ((-not $hasLogs) -and (-not $hasEvents)) {
'<p><span class="tag tag-pass">No error logs or events found</span></p>'
})
</div>
</section>

<!-- 5. Environment -->
<section>
<h2>Environment &amp; Security</h2>
<div class="content">
<div class="kv">
  <span class="k">SmartScreen Block</span>
  <span class="v">$(if($Env.ZoneBlocked){'<span class="tag tag-fail">BLOCKED — Right-click EXE → Properties → Unblock</span>'}else{'<span class="tag tag-pass">Not blocked</span>'})</span>
  <span class="k">Power Plan</span>
  <span class="v">$([System.Web.HttpUtility]::HtmlEncode($Env.PowerPlan))</span>
</div>
$(if ($Env.DefenderBlocks.Count -gt 0) {
'<p style="margin-top:12px;"><span class="tag tag-fail">Windows Defender has blocked RswareDesign files</span> — Add an exclusion for the installation directory.</p>'
})
</div>
</section>

<footer>
  RswareDesign Diagnostics Tool v1.0 &nbsp;|&nbsp; Report: $reportFile
</footer>

<script>
document.querySelectorAll('section h2').forEach(h => {
  h.addEventListener('click', () => h.parentElement.classList.toggle('open'));
});
function toggleAll(open) {
  document.querySelectorAll('section').forEach(s => {
    if (open) s.classList.add('open'); else s.classList.remove('open');
  });
}
</script>
</body>
</html>
"@
    return $html
}

# ── Main ──────────────────────────────────────────────────────

Add-Type -AssemblyName System.Web

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RswareDesign Diagnostics Tool v1.0" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "App Directory: $AppDir" -ForegroundColor Gray
Write-Host ""

Write-Host "[1/5] Collecting system information..." -ForegroundColor Yellow
$sysInfo = Get-SystemInfo

Write-Host "[2/5] Scanning serial ports..." -ForegroundColor Yellow
$serialInfo = Get-SerialPortInfo

Write-Host "[3/5] Checking DLL integrity..." -ForegroundColor Yellow
$dllInfo = Get-DllIntegrity

Write-Host "[4/5] Reading application logs..." -ForegroundColor Yellow
$logInfo = Get-AppLogs

Write-Host "[5/5] Checking environment & security..." -ForegroundColor Yellow
$envInfo = Get-EnvironmentCheck

Write-Host ""
Write-Host "Generating report..." -ForegroundColor Yellow
$html = ConvertTo-HtmlReport -Sys $sysInfo -Serial $serialInfo -Dll $dllInfo -Logs $logInfo -Env $envInfo
$html | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host ""
Write-Host "Report saved: $reportFile" -ForegroundColor Green
Write-Host ""

# Auto-open report
Start-Process $reportFile

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
