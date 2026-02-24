# log-response.ps1
# Claude Code Hook: Stop
# Reads pending prompt info + extracts response from transcript → writes combined Q&A file
param()

$ErrorActionPreference = "SilentlyContinue"

# Force UTF-8 encoding (Windows PowerShell defaults to CP949)
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Read JSON from stdin (byte-level UTF-8 read)
$reader = [System.IO.StreamReader]::new([Console]::OpenStandardInput(), [System.Text.Encoding]::UTF8)
$inputJson = $reader.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($inputJson)) { exit 0 }

try {
    $data = $inputJson | ConvertFrom-Json
} catch {
    exit 0
}

$transcriptPath = $data.transcript_path
$sessionId = $data.session_id

# ========================================
# Read pending prompt info
# ========================================
# Derive project root from script location: .claude/hooks/ → project root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$archiveBase = Join-Path $projectRoot "Doc\AI Archive"
$pendingFile = Join-Path $archiveBase "_pending_${sessionId}.json"

if (-not (Test-Path $pendingFile)) { exit 0 }

try {
    $pendingJson = [System.IO.File]::ReadAllText($pendingFile, [System.Text.Encoding]::UTF8)
    $pending = $pendingJson | ConvertFrom-Json
} catch {
    exit 0
}

$prompt = $pending.prompt
$category = $pending.category
$timestamp = $pending.timestamp
$monthStr = $pending.month
$promptDate = $pending.date
$sessionId = $pending.session_id
$cwd = $pending.cwd

# ========================================
# Extract full response from transcript
# Collects ALL assistant text blocks after the last user message
# ========================================
$textBlocks = [System.Collections.Generic.List[string]]::new()
$toolsUsed = [System.Collections.Generic.List[string]]::new()

if ($transcriptPath -and (Test-Path $transcriptPath)) {
    try {
        $lines = [System.IO.File]::ReadAllLines($transcriptPath, [System.Text.Encoding]::UTF8)

        # Step 1: Find last user (human) message index
        $lastHumanIdx = -1
        for ($i = $lines.Count - 1; $i -ge 0 -and $i -ge ($lines.Count - 500); $i--) {
            $line = $lines[$i]
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            try {
                $entry = $line | ConvertFrom-Json
                if ($entry.type -eq "user") {
                    $lastHumanIdx = $i
                    break
                }
            } catch { continue }
        }

        # Step 2: Collect ALL assistant text blocks after last human message
        $startIdx = if ($lastHumanIdx -ge 0) { $lastHumanIdx + 1 } else { [Math]::Max(0, $lines.Count - 200) }
        for ($i = $startIdx; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            try {
                $entry = $line | ConvertFrom-Json
                if ($entry.type -eq "assistant" -and $entry.message -and $entry.message.content) {
                    foreach ($block in $entry.message.content) {
                        if ($block.type -eq "text" -and $block.text -and $block.text.Trim().Length -gt 0) {
                            $textBlocks.Add($block.text.Trim())
                        }
                        if ($block.type -eq "tool_use" -and $block.name) {
                            if (-not $toolsUsed.Contains($block.name)) {
                                $toolsUsed.Add($block.name)
                            }
                        }
                    }
                }
            } catch { continue }
        }
    } catch {
        $textBlocks.Add("(transcript parse error)")
    }
}

$lastResponse = ($textBlocks -join "`n`n---`n`n")

# Truncate very long responses (50K char limit)
if ($lastResponse.Length -gt 50000) {
    $responseSummary = $lastResponse.Substring(0, 50000) + "`n`n... (truncated, $($lastResponse.Length) chars total)"
} else {
    $responseSummary = $lastResponse
}

# Tools summary
if ($toolsUsed.Count -gt 0) {
    $toolsSummary = ($toolsUsed -join ", ")
} else {
    $toolsSummary = "none"
}

# ========================================
# Write combined Q&A file: Category/yyyy-MM/yyyy-MM-dd/timestamp.md
# ========================================
$promptsDir = Join-Path $archiveBase "Prompts"
$categoryDir = Join-Path $promptsDir $category
$monthDir = Join-Path $categoryDir $monthStr
$dayStr = $timestamp.Substring(0, 10)  # yyyy-MM-dd
$dayDir = Join-Path $monthDir $dayStr

if (-not (Test-Path $dayDir)) {
    New-Item -ItemType Directory -Path $dayDir -Force | Out-Null
}

$fileName = "${timestamp}.md"
$filePath = Join-Path $dayDir $fileName

$content = @"
---
date: $promptDate
category: $category
session_id: $sessionId
tools_used: $toolsSummary
cwd: $cwd
---

# Prompt

$prompt

---

# Response

$responseSummary
"@

[System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)

# Clean up pending file
Remove-Item $pendingFile -Force

# Log entry
$logPath = Join-Path $archiveBase "archive.log"
$responseLen = if ($lastResponse) { $lastResponse.Length } else { 0 }
$logEntry = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [$category] $fileName (prompt=$($prompt.Length)chars response=${responseLen}chars)"
[System.IO.File]::AppendAllText($logPath, "$logEntry`n", [System.Text.Encoding]::UTF8)

# ========================================
# Update INDEX.md
# ========================================
$indexPath = Join-Path $promptsDir "INDEX.md"
$categories = @("Architecture", "Code-Review", "Config", "Debugging", "Development", "General", "Git-Workflow", "Performance", "Service", "Testing", "UI", "UI-Controls", "WPF")

# --- Pass 1: Collect data per category ---
$totalCount = 0
$catData = @{}

foreach ($cat in $categories) {
    $catDir = Join-Path $promptsDir $cat
    if (-not (Test-Path $catDir)) { continue }

    $files = Get-ChildItem -Path $catDir -Recurse -Filter "*.md" | Sort-Object Name -Descending
    if ($files.Count -eq 0) { continue }

    $totalCount += $files.Count
    $entries = @()

    foreach ($f in $files) {
        $fileText = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
        $preview = ""
        if ($fileText -match "(?s)# Prompt\s+(.+?)(\s*---|\z)") {
            $raw = $matches[1].Trim()
            # Strip noise tags
            $raw = $raw -replace "(?s)<task-notification>.*?</task-notification>\s*", ""
            $raw = $raw -replace "(?s)<[^>]+>.*?</[^>]+>\s*", ""
            $raw = $raw -replace "<[^>]+>\s*", ""
            $raw = $raw.Trim()
            if ($raw.Length -gt 60) {
                $preview = $raw.Substring(0, 60) + "..."
            } else {
                $preview = $raw
            }
            # Collapse whitespace
            $preview = $preview -replace "\s+", " "
        }

        $relativePath = $f.FullName.Replace($promptsDir + "\", "").Replace("\", "/")
        # Extract date (yyyy-MM-dd) and time (HH:mm) from filename like 2026-02-10_153003
        $dayStr2 = $f.BaseName.Substring(0, 10)
        $timeStr = ""
        if ($f.BaseName.Length -ge 16) {
            $timeStr = $f.BaseName.Substring(11, 2) + ":" + $f.BaseName.Substring(13, 2)
        }

        $entries += [PSCustomObject]@{
            Day = $dayStr2
            Time = $timeStr
            Preview = $preview
            RelPath = $relativePath
        }
    }

    $latestDay = if ($entries.Count -gt 0) { $entries[0].Day } else { "-" }
    $catData[$cat] = @{ Count = $files.Count; Latest = $latestDay; Entries = $entries }
}

# --- Pass 2: Build INDEX content ---
$indexContent = @"
# Prompt Archive Index

> Updated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss") | Total: $totalCount prompts

| Category | Count | Latest |
|----------|------:|--------|
"@

foreach ($cat in $categories) {
    if (-not $catData.ContainsKey($cat)) { continue }
    $d = $catData[$cat]
    $indexContent += "| $cat | $($d.Count) | $($d.Latest) |`n"
}

foreach ($cat in $categories) {
    if (-not $catData.ContainsKey($cat)) { continue }
    $d = $catData[$cat]

    $indexContent += "`n---`n`n## $cat ($($d.Count))`n"

    # Group entries by day
    $grouped = $d.Entries | Group-Object -Property Day
    foreach ($g in $grouped) {
        $indexContent += "`n### $($g.Name)`n"
        foreach ($e in $g.Group) {
            $indexContent += "- ``$($e.Time)`` [$($e.Preview)]($($e.RelPath))`n"
        }
    }
}

[System.IO.File]::WriteAllText($indexPath, $indexContent, [System.Text.Encoding]::UTF8)

exit 0
