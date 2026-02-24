---
name: git-flow
description: Git branch/commit/push/PR workflow automation
---

# Git Flow Skill

**All reports, questions, and approval requests must be in Korean.**

When the user invokes `/git-flow <project-path>`, execute the following workflow.

---

## Step 0: Repository Status Check

Check Git status at the target project path:

1. **Repository check**: `.git` existence, remote connection status (`git remote -v`)
2. **Branch status**: Current branch name, upstream tracking status
3. **Working status**: Uncommitted changes, staged files, untracked files list
4. **Sync status**: Local ↔ remote diff (`git status -sb`)

**If issues found**: Report status and ask user whether to continue

> This step proceeds automatically without approval.

---

## Step 1: Branch

Ask the user: **"Create a new branch or work on an existing branch?"**

### If existing branch selected
- Display local branch list
- Checkout the user's selected branch

### If new branch creation selected
Collect info and **auto-generate** a branch name, then propose:

**Branch naming convention**: `{area}/{type}-{description}`

| Item | Options | Description |
|------|---------|-------------|
| Area | `arch`, `ui`, `infra`, `sdk`, `plugin`, `test` | Change area |
| Type | `feat`, `fix`, `improve`, `refactor`, `docs`, `chore` | Change purpose |
| Description | Auto-generated | 2-4 words, lowercase, hyphen-separated |

**Examples**:
- `ui/feat-add-dark-mode-toggle`
- `arch/refactor-plugin-lifecycle`
- `sdk/fix-dialog-service-null`
- `plugin/improve-calculator-ux`

**Procedure**:
1. Analyze current changes or user description to auto-determine area/type/description
2. Propose branch name → **Wait for user approval**
3. Confirm base branch (default: `develop`, fallback: `main`)
4. Execute `git checkout -b {branch-name}`

---

## Step 2: Commit

Analyze changes and **auto-generate** a commit message, then propose.

### Change Analysis
1. Review all changes via `git diff --staged` + `git diff` + `git status`
2. Analyze changed files, additions/deletions/modifications

### Commit Message Rules

**Format**:
```
<type> <subject>

<body>
```

**Subject rules**:
- `<type> <summary>` — imperative mood, max 50 chars, **written in English**
- No colon between type and summary
- Types: `feat`, `fix`, `refactor`, `improve`, `docs`, `test`, `chore`

**Good examples**:
- `feat Add velocity limit check`
- `fix Resolve namespace conflict in DialogService`
- `refactor Replace MessageBox with IDialogService`

**Bad examples**:
- `feat: Add feature` (no colons allowed)
- `Updated files` (missing type)
- `fix bug` (insufficient description)
- `feat 디자인 토큰 추가` (no Korean allowed)

**Body rules**:
- **Written in English** (Why, What, Impact — all in English)
- Why: Background/reason for the change
- What: Main changes (per file)
- Impact: Scope of impact

**Good body example**:
```
Added design system tokens to ShellView:
- Replaced hardcoded colors with DynamicResource tokens
- Replaced FontSize values with design tokens

Impact: Shell project, all views using tokens
```

**Bad body example**:
```
디자인 토큰을 ShellView에 추가:
- 하드코딩 색상을 토큰으로 교체

Impact: Shell 프로젝트  ← No Korean allowed
```

**Procedure**:
1. Auto-analyze changes
2. **Write summary in Korean** (for internal analysis)
3. **Convert to English commit message** (Subject + Body)
4. Propose commit message → **Wait for user approval**
5. On approval, execute staging + commit
6. If multiple logical units exist, suggest split commits

### Commit Safety Rules
- `.env`, `credentials`, key files, etc. are **never staged**
- Warn and exclude if sensitive files are found
- `git add` uses explicit filenames (never use `git add .` or `git add -A`)

---

## Step 3: Push

Ask the user: **"Push to remote repository?"**

**If declined**: Summarize current status and end

**If approved**:
1. Check upstream configuration
2. First push: `git push -u origin {branch-name}`
3. Subsequent pushes: `git push`
4. Report push results

**Caution**:
- Never use `--force` (unless user explicitly requests it)
- Warn when pushing directly to main/develop

---

## Step 4: PR Creation

Ask the user: **"Create a Pull Request?"**

**If declined**: Summarize current status and end

**If approved**:
1. Analyze full commit history of the branch (`git log {base}..HEAD`)
2. Auto-generate PR content → **Wait for user approval**
3. Execute `gh pr create`

### PR Format

**Title**: `<Short Description>` (max 70 chars, no colons)

**Body**:
```markdown
## Summary
- Key changes in 1-3 lines

## Changes
- List only functional changes (bullet points)
- Organized by file/module

## Impact
- Scope: {module/project name}
- Breaking changes: yes/no

## Testing
- Build verification status
- Test method/results
```

**Rules**:
- No AI/Claude-related text insertion
- Write Korean summary first → generate English PR
- Base branch: default `develop`, fallback `main`

---

## Project Structure Reference

| Path | Repository |
|------|-----------|
| `repos/DevTestWpfCalApp/` | Main app (Shell, SDK, Modules) |
| `repos/MotionCalculator/` | Calculator plugin (independent repo) |
| `repos/MotorMonitor/` | Monitor plugin (independent repo) |

---

## Important Notes

- **User approval is required at each step**
- **No `--force` push** (unless explicitly requested)
- **No committing sensitive files** (.env, credentials, key files)
- **Warn when pushing directly to main/develop**
- **No AI/Co-Authored-By text in commit messages**
