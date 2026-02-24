# save-context.ps1
# Claude Code Hook: Stop
# Parses transcript and appends context summary to CLAUDE.md
param()

$ErrorActionPreference = "SilentlyContinue"

# Force UTF-8 encoding
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Read JSON from stdin
$reader    = [System.IO.StreamReader]::new([Console]::OpenStandardInput(), [System.Text.Encoding]::UTF8)
$inputJson = $reader.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($inputJson)) { exit 0 }

try {
    $data = $inputJson | ConvertFrom-Json
} catch {
    exit 0
}

$transcriptPath = $data.transcript_path
$sessionId      = $data.session_id

# ========================================
# Derive paths
# ========================================
$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$claudeMdPath = Join-Path $projectRoot "CLAUDE.md"

if (-not ($transcriptPath -and (Test-Path $transcriptPath))) { exit 0 }

# ========================================
# Parse transcript
# ========================================
$modifiedFiles  = [System.Collections.Generic.HashSet[string]]::new()
$errorMessages  = [System.Collections.Generic.List[string]]::new()
$todoItems      = [System.Collections.Generic.List[string]]::new()
$decisionItems  = [System.Collections.Generic.List[string]]::new()
$codeSummaries  = [System.Collections.Generic.List[string]]::new()

try {
    $lines = [System.IO.File]::ReadAllLines($transcriptPath, [System.Text.Encoding]::UTF8)

    # Find last user message index (scan last 1000 lines)
    $lastHumanIdx = -1
    $scanStart    = [Math]::Max(0, $lines.Count - 1000)
    for ($i = $lines.Count - 1; $i -ge $scanStart; $i--) {
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

    $startIdx = if ($lastHumanIdx -ge 0) { $lastHumanIdx + 1 } else { [Math]::Max(0, $lines.Count - 300) }

    for ($i = $startIdx; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        try {
            $entry = $line | ConvertFrom-Json

            # ── Tool use: extract modified files ──────────────────────────
            if ($entry.type -eq "assistant" -and $entry.message -and $entry.message.content) {
                foreach ($block in $entry.message.content) {
                    if ($block.type -eq "tool_use") {
                        $toolName = $block.name
                        $input    = $block.input

                        # File write / edit tools
                        if ($toolName -in @("Write", "Edit", "MultiEdit", "str_replace_based_edit_tool", "create_file", "str_replace")) {
                            $filePath = if ($input.path) { $input.path } elseif ($input.file_path) { $input.file_path } else { $null }
                            if ($filePath) {
                                $normalized = $filePath -replace "\\", "/"
                                [void]$modifiedFiles.Add($normalized)
                            }
                        }

                        # Bash tool: detect file writes
                        if ($toolName -eq "Bash" -and $input.command) {
                            $cmd = $input.command
                            if ($cmd -match ">\s*[`"']?([^\s`"'|&;]+\.(cs|xaml|json|xml|ps1|md|txt|py|js|ts))[`"']?") {
                                [void]$modifiedFiles.Add($matches[1] -replace "\\", "/")
                            }
                        }
                    }

                    # ── Text blocks: extract TODO / decisions / errors / code summary ──
                    if ($block.type -eq "text" -and $block.text) {
                        $text = $block.text

                        # TODO items
                        $todoMatches = [regex]::Matches($text, "(?im)^[-*]\s*(TODO|다음\s*작업|next\s*step)[:\s]+(.+)$")
                        foreach ($m in $todoMatches) {
                            $item = $m.Groups[2].Value.Trim()
                            if ($item.Length -gt 0 -and $item.Length -le 200) {
                                $todoItems.Add($item)
                            }
                        }

                        # Decision / architecture keywords
                        $decisionMatches = [regex]::Matches($text, "(?im)^[-*]\s*(결정|아키텍처|구조|패턴|방식|선택)[:\s]+(.+)$")
                        foreach ($m in $decisionMatches) {
                            $item = $m.Groups[2].Value.Trim()
                            if ($item.Length -gt 0 -and $item.Length -le 200) {
                                $decisionItems.Add($item)
                            }
                        }

                        # Error lines
                        $errorMatches = [regex]::Matches($text, "(?im)(error|exception|오류|에러)[:\s]+(.{10,150})")
                        foreach ($m in $errorMatches) {
                            $item = $m.Groups[2].Value.Trim()
                            if ($item.Length -gt 0) {
                                $errorMessages.Add($item)
                            }
                        }

                        # Code summary: first non-empty paragraph (max 300 chars)
                        if ($codeSummaries.Count -lt 3 -and $text.Trim().Length -gt 30) {
                            $firstPara = ($text.Trim() -split "`n`n")[0].Trim()
                            $firstPara = $firstPara -replace "`n", " "
                            if ($firstPara.Length -gt 300) { $firstPara = $firstPara.Substring(0, 300) + "..." }
                            $codeSummaries.Add($firstPara)
                        }
                    }
                }
            }
        } catch { continue }
    }
} catch {
    exit 0
}

# ========================================
# Build append block
# ========================================
$now       = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$dateLabel = Get-Date -Format "yyyy-MM-dd"

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine("")
[void]$sb.AppendLine("---")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## [$dateLabel] Session Update — $now")
[void]$sb.AppendLine("**Session ID:** $sessionId")
[void]$sb.AppendLine("")

# 1. 현재 작업 중인 파일 목록
[void]$sb.AppendLine("### 현재 작업 중인 파일 목록")
if ($modifiedFiles.Count -gt 0) {
    foreach ($f in ($modifiedFiles | Sort-Object)) {
        [void]$sb.AppendLine("- $f")
    }
} else {
    [void]$sb.AppendLine("- (없음)")
}
[void]$sb.AppendLine("")

# 2. 최근 수정된 코드 요약
[void]$sb.AppendLine("### 최근 수정된 코드 요약")
if ($codeSummaries.Count -gt 0) {
    $idx = 1
    foreach ($s in $codeSummaries) {
        [void]$sb.AppendLine("$idx. $s")
        $idx++
    }
} else {
    [void]$sb.AppendLine("- (없음)")
}
[void]$sb.AppendLine("")

# 3. 주요 결정사항 및 아키텍처
[void]$sb.AppendLine("### 주요 결정사항 및 아키텍처")
if ($decisionItems.Count -gt 0) {
    $shown = $decisionItems | Select-Object -Unique | Select-Object -First 10
    foreach ($d in $shown) {
        [void]$sb.AppendLine("- $d")
    }
} else {
    [void]$sb.AppendLine("- (없음)")
}
[void]$sb.AppendLine("")

# 4. 다음 작업 TODO
[void]$sb.AppendLine("### 다음 작업 TODO")
if ($todoItems.Count -gt 0) {
    $shown = $todoItems | Select-Object -Unique | Select-Object -First 10
    foreach ($t in $shown) {
        [void]$sb.AppendLine("- [ ] $t")
    }
} else {
    [void]$sb.AppendLine("- [ ] (없음)")
}
[void]$sb.AppendLine("")

# 5. 에러 및 해결 내역
[void]$sb.AppendLine("### 에러 및 해결 내역")
if ($errorMessages.Count -gt 0) {
    $shown = $errorMessages | Select-Object -Unique | Select-Object -First 10
    foreach ($e in $shown) {
        [void]$sb.AppendLine("- $e")
    }
} else {
    [void]$sb.AppendLine("- (없음)")
}
[void]$sb.AppendLine("")

# ========================================
# Append to CLAUDE.md
# ========================================
$appendContent = $sb.ToString()

if (-not (Test-Path $claudeMdPath)) {
    $header = @"
# CLAUDE.md
> 이 파일은 Claude Code가 자동으로 관리합니다. 세션 종료 시 작업 컨텍스트가 누적 기록됩니다.

"@
    [System.IO.File]::WriteAllText($claudeMdPath, $header, [System.Text.Encoding]::UTF8)
}

[System.IO.File]::AppendAllText($claudeMdPath, $appendContent, [System.Text.Encoding]::UTF8)

# Log
$archiveBase = Join-Path $projectRoot "Doc\AI Archive"
$logPath     = Join-Path $archiveBase "archive.log"
if (Test-Path (Split-Path $logPath)) {
    $logEntry = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [save-context] CLAUDE.md updated (files=$($modifiedFiles.Count) todos=$($todoItems.Count) errors=$($errorMessages.Count))"
    [System.IO.File]::AppendAllText($logPath, "$logEntry`n", [System.Text.Encoding]::UTF8)
}

exit 0
