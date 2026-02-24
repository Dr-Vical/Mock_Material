# archive-prompt.ps1
# Claude Code Hook: UserPromptSubmit
# Classifies prompt → saves pending info for Stop hook to combine
param()

$ErrorActionPreference = "SilentlyContinue"

# Force UTF-8 encoding (Windows PowerShell defaults to CP949)
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Read JSON from stdin (byte-level UTF-8 read)
$reader = [System.IO.StreamReader]::new([Console]::OpenStandardInput(), [System.Text.Encoding]::UTF8)
$inputJson = $reader.ReadToEnd()

# DEBUG: Capture raw stdin for troubleshooting
$debugDir = Join-Path (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))) "Doc\AI Archive"
if (-not (Test-Path $debugDir)) { New-Item -ItemType Directory -Path $debugDir -Force | Out-Null }
$debugFile = Join-Path $debugDir "_debug_stdin.txt"
[System.IO.File]::WriteAllText($debugFile, "TIMESTAMP: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`nRAW_INPUT: $inputJson`nLENGTH: $($inputJson.Length)", [System.Text.Encoding]::UTF8)

if ([string]::IsNullOrWhiteSpace($inputJson)) { exit 0 }

try {
    $data = $inputJson | ConvertFrom-Json
} catch {
    # DEBUG: Log parse error
    [System.IO.File]::AppendAllText($debugFile, "`nPARSE_ERROR: $_", [System.Text.Encoding]::UTF8)
    exit 0
}

$prompt = $data.prompt
$sessionId = $data.session_id
$cwd = $data.cwd

# Strip IDE metadata tags from prompt
$prompt = $prompt -replace "(?s)<ide_opened_file>.*?</ide_opened_file>\s*", ""
$prompt = $prompt -replace "(?s)<ide_selection>.*?</ide_selection>\s*", ""
$prompt = $prompt.Trim()

# Skip empty or very short prompts
if ([string]::IsNullOrWhiteSpace($prompt)) { exit 0 }
if ($prompt.Length -lt 5) { exit 0 }

# ========================================
# Category classification
# Priority 1: Skill commands (deterministic, 100% accurate)
# Priority 2: Keyword patterns (word-boundary protected)
# ========================================
$category = "General"

# --- Priority 1: Skill commands (slash command at start) ---
if ($prompt -match "^/git-flow-jira\b") {
    $category = "Git-Workflow"
} elseif ($prompt -match "^/git-flow\b") {
    $category = "Git-Workflow"
} elseif ($prompt -match "^/code-cleanup\b") {
    $category = "Code-Review"
} elseif ($prompt -match "^/service-dev\b") {
    $category = "Service"
} elseif ($prompt -match "^/ui-dev\b") {
    $category = "UI"
} elseif ($prompt -match "^/test\b") {
    $category = "Testing"
} elseif ($prompt -match "^/dev\b") {
    $category = "Development"
# Priority 2: Keyword patterns (more specific first, broader last)
# Code-Review: 코드 정리, 리팩토링, 컨벤션
} elseif ($prompt -match "코드\s?정리|코드\s?리뷰|리팩토링|데드\s?코드|컨벤션|\brefactor\b|\bcleanup\b|\bdead\s?code\b|\bconvention\b|\bcode.?review\b") {
    $category = "Code-Review"
# Git-Workflow: git 관련 (word boundary로 .dtproj 등 오탐 방지)
} elseif ($prompt -match "\bgit\b|\bcommit\b|\bbranch\b|\bPR\b|\bpush\b|\bmerge\b|\brebase\b|\bpull.?request\b|커밋|브랜치|푸시|원격\s?저장소|풀\s?리퀘|깃") {
    $category = "Git-Workflow"
# Testing: 테스트 관련
} elseif ($prompt -match "\bunit\s?test\b|\bxUnit\b|\bassert\b|테스트\s?결과|테스트\s?실행|테스트\s?작성|단위\s?테스트|유닛\s?테스트|테스트\s?저장") {
    $category = "Testing"
# UI: 디자인 토큰, DevExpress, 레이아웃, 화면, XAML, 뷰
} elseif ($prompt -match "\bDevExpress\b|\bDockLayout\b|\bThemedWindow\b|\bDynamicResource\b|\bStaticResource\b|\bdesign.?token\b|디자인\s?토큰|폰트\s?변경|색상|테마\s?변경|레이아웃|다이얼로그|UI\s?개선|화면\s?개선|화면\s?수정|스타일|다국어|i18n|\bloc\.|\.xaml\b|\bXAML\b|\bViewModel\b|\bBinding\b|\bUserControl\b|뷰모델|뷰\s?수정|바인딩") {
    $category = "UI"
# Architecture: 아키텍처, 모듈, 플러그인, DI
} elseif ($prompt -match "\bPrism\b|\bRegionManager\b|\bIModule\b|\bALC\b|\bDryIoc\b|\bClean.?Arch\b|\bPlugin\b|아키텍처|모듈\s?구조|플러그인|의존성\s?주입|\bDI\b|계층\s?구조|네비게이션") {
    $category = "Architecture"
# Service: 서비스, 인터페이스, 인프라
} elseif ($prompt -match "서비스\s?개발|서비스\s?추가|인터페이스\s?설계|인프라\s?변경|\bservice\b.*\b(add|create|design)\b|\binfrastructure\b") {
    $category = "Service"
# Debugging: 오류, 버그, 예외
} elseif ($prompt -match "\bException\b|\bbug\b|\bdebug\b|\bcrash\b|예외|오류|에러|디버그|버그|크래시|충돌|안\s?됨|안\s?열림|안\s?나옴") {
    $category = "Debugging"
# Performance: 성능, 최적화, 메모리
} elseif ($prompt -match "\bperformance\b|\boptimize\b|\bmemory\s?leak\b|\bslow\b|성능|최적화|메모리\s?누수|느림|지연|렌더링\s?느림") {
    $category = "Performance"
# Config: 훅, 설정, 환경
} elseif ($prompt -match "\bhook\b|\bMCP\b|\.claude|훅\s?설정|훅\s?수정|환경\s?설정|아카이브\s?설정") {
    $category = "Config"
}

# ========================================
# Save pending info for Stop hook
# ========================================
# Derive project root from script location: .claude/hooks/ → project root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$archiveBase = Join-Path $projectRoot "Doc\AI Archive"
$pendingFile = Join-Path $archiveBase "_pending_${sessionId}.json"

$pending = @{
    prompt     = $prompt
    category   = $category
    timestamp  = (Get-Date -Format "yyyy-MM-dd_HHmmss")
    month      = (Get-Date -Format "yyyy-MM")
    date       = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    session_id = $sessionId
    cwd        = $cwd
} | ConvertTo-Json -Compress

[System.IO.File]::WriteAllText($pendingFile, $pending, [System.Text.Encoding]::UTF8)

exit 0
