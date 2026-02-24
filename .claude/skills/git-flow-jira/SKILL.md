---
name: git-flow-jira
description: Git branch/commit/push/PR workflow + Jira issue integration automation
---

# Git Flow + Jira Skill

**All reports, questions, and approval requests must be in Korean.**

When the user invokes `/git-flow-jira <project-path>`, execute the following workflow.
Refer to the "Jira Integration Settings" section in `CLAUDE.md` for Jira configuration.

---

## Step 0: Repository Status Check + Jira Connection Verification

### Git Status Check
1. **Repository check**: `.git` existence, remote connection status (`git remote -v`)
2. **Branch status**: Current branch name, upstream tracking status
3. **Working status**: Uncommitted changes, staged files, untracked files list
4. **Sync status**: Local ↔ remote diff (`git status -sb`)

### Jira Connection Verification
1. **MCP tool check**: Verify `mcp__jira__jira_get` tool availability
2. **Auth check**: Call `/rest/api/3/myself` to verify connection status
3. **Project mapping**: Check Jira project key for current repository from `CLAUDE.md`
4. **Issue type/field check**: Verify issue types, fix versions, epic info for the mapped project

**Report format**:
```
Git Repository: {path}
Branch: {current branch}
Jira Project: {project key} ({project name})
Jira User: {displayName}
Status: ✓ OK / ✗ Issues found
```

**If issues found**: Report status and ask user whether to continue

> This step proceeds automatically without approval.

---

## Step 1: Branch + Jira Issue Creation

Ask the user: **"Create a new branch or work on an existing branch?"**

### If existing branch selected
1. Display local branch list
2. Checkout the user's selected branch
3. Extract Jira key from branch name (pattern: `{area}/{type}-{PROJ-NNN}-{description}`)
4. Check if the Jira issue exists → display issue info if found

### If new branch creation selected

#### 1-0. Issue Type Determination

Analyze the branch type and existing issue status to determine the issue type to create.

**Default mapping** (see `CLAUDE.md` branch type → issue type mapping):
- `feat` → New Feature (`10120`)
- `fix` → Bug (`10084`)
- `improve` → Improvement (`10119`)
- `refactor`, `docs`, `chore` → Task (`10081`)

**Subtask determination rules** (auto-scan test history):
1. Search `AI Archive/Tests/` directory for test result files related to the current work target
   - Search criteria: Target component name, module name, keyword matching (filename + Test Overview `Target` field)
   - Example: `PluginLoader`-related work → search related files in `AI Archive/Tests/Plugin/`
2. Check current repository's epic key from `CLAUDE.md`
3. Query epic sub-issues (JQL: `parent = {epic-key}`)
4. Determination:
   - **No related test files found** → create issue per default mapping above (Task/Bug/New Feature/Improvement)
   - **Related test files found + related issue exists under epic** → create **Subtask** (`10083`)
     - Parent: Existing Task/Bug key (task level, not epic)
     - Summary: `TC - {test file's test name}`
     - Description: Excerpt from test file scenario/result content
     - Assignee: Referenced from `CLAUDE.md` (automatic)
     - Labels: `recommended`

**Test file scan report format**:
```
Test History Scan Results:
  Search path: AI Archive/Tests/
  Search keyword: {component name}
  Found: {N} items
  - Tests/Plugin/PluginLoader_HotReload.md (PASSED, 2026-02-10)
  - Tests/Architecture/Core_Assembly_Isolation.md (PASSED, 2026-02-09)
  → Suggesting creation as subtask.
```

> Report scan results + determination to user and get approval.

#### 1-1. Jira Issue Creation

Collect/auto-generate the following information:

| Field | Handling |
|-------|---------|
| **Issue type** | Type determined in 1-0 (automatic) |
| **Summary** | **Written in English**: Level-0 issue: `{epic abbreviation} - {Summary}` (e.g., `MC - Add velocity limit check`), subtask: `TC - {Test Title}` → approval |
| **Description** | **Written in English**: Auto-written to match branch creation reason (test content for subtasks) → approval |
| **Priority** | Ask user (Highest/High/Medium/Low/Lowest) |
| **Assignee** | Referenced from `CLAUDE.md` (automatic) |
| **Parent** | Parent task/bug key for subtasks, otherwise epic key reference |
| **Fix version** | Referenced per repository from `CLAUDE.md` (leave empty if TODO) |
| **Labels** | Auto-mapped from branch area (`CLAUDE.md` label reference) + `recommended` for subtasks, `Document` for `docs` type |
| **Start date** | Issue creation timestamp (automatic) |
| **Due date** | Recommend estimated duration → approval or manual input |

**Issue creation procedure**:
1. **Summarize changes in Korean** (for internal analysis)
2. **Convert to English** (summary, description)
3. Compose field values and display preview → **Wait for user approval**
4. On approval, create issue via `mcp__jira__jira_post`
5. Obtain created issue key (e.g., `UMS-42`)

#### 1-2. Branch Creation

**Branch naming convention**: `{area}/{type}-{issue-key}-{description}`

| Item | Options | Description |
|------|---------|-------------|
| Area | `arch`, `ui`, `infra`, `sdk`, `plugin`, `test` | Change area |
| Type | `feat`, `fix`, `improve`, `refactor`, `docs`, `chore` | Change purpose |
| Issue key | Key created in Jira | e.g., `UMS-42` |
| Description | Auto-generated | 2-4 words, lowercase, hyphen-separated |

**Examples**:
- `ui/feat-UMS-42-add-dark-mode-toggle`
- `arch/refactor-UMS-43-plugin-lifecycle`
- `sdk/fix-UMS-44-dialog-service-null`

**Procedure**:
1. Analyze Jira issue key + change content to auto-determine area/type/description
2. Propose branch name → **Wait for user approval**
3. Confirm base branch (default: `develop`, fallback: `main`)
4. Execute `git checkout -b {branch-name}`

#### 1-3. Status Propagation (immediately after issue creation)

Issue created + branch created = work started, so propagate status.

**Procedure**:
1. Transition created issue to **"In Progress"** (`21`)
2. Check and propagate parent status:
   - **If subtask**:
     - Check parent (task/bug) status → if not "In Progress" → transition to `21`
     - Check parent's parent (epic) status → if not "In Progress" → transition to `21`
   - **If task/bug/new feature/improvement**:
     - Check parent (epic) status → if not "In Progress" → transition to `21`
3. **Skip if already "In Progress"** (prevent unnecessary transitions)
4. Report propagation results

**Report format**:
```
Status Propagation Results:
  {issue-key} → In Progress ✓
  {parent-key} → In Progress ✓ (already In Progress — skipped)
  {epic-key} → In Progress ✓
```

> Status propagation proceeds automatically without approval.

---

## Step 2: Commit + Jira Issue Update

### 2-1. Commit Creation

Analyze changes and **auto-generate** a commit message, then propose.

#### Change Analysis
1. Review all changes via `git diff --staged` + `git diff` + `git status`
2. Analyze changed files, additions/deletions/modifications
3. Extract Jira key from branch name (pattern: `{area}/{type}-{PROJ-NNN}-{description}`)

#### Commit Message Rules

**Format**:
```
{issue-key} <type> <subject>

<body>
```

**Subject rules**:
- `{issue-key} <type> <summary>` — issue key first, imperative mood, max 60 chars, **written in English**
- No colon between type and summary

**Good examples**:
- `UMS-42 feat Add velocity limit check`
- `UMS-43 fix Resolve namespace conflict in DialogService`
- `UMS-44 refactor Replace MessageBox with IDialogService`

**Bad examples**:
- `feat: Add feature` (missing issue key, no colons allowed)
- `UMS-42: Updated files` (no colons allowed)
- `UMS-42 feat 디자인 토큰 추가` (no Korean allowed)

**Body rules**:
- **Written in English** (Why, What, Impact — all in English)
- Why: Background/reason for the change
- What: Main changes (per file)
- Impact: Scope of impact

**Procedure**:
1. Auto-analyze changes
2. **Write summary in Korean** (for internal analysis)
3. **Convert to English commit message** (issue key + Subject + Body)
4. Propose commit message → **Wait for user approval**
5. On approval, execute staging + commit
6. If multiple logical units exist, suggest split commits

#### Commit Safety Rules
- `.env`, `credentials`, key files, etc. are **never staged**
- Warn and exclude if sensitive files are found
- `git add` uses explicit filenames (never use `git add .` or `git add -A`)

### 2-2. Jira Issue Update

Update the Jira issue after commit completion:

| Field | Handling |
|-------|---------|
| **Summary** | **Written in English**: Auto-update to match changes + branch content → approval |
| **Description** | **Written in English**: Enhance to match commit reason → approval |
| **Labels** | Recommend if needed → ask user about assignment |
| **Due date** | Re-recommend estimated duration → approval or manual input |

> **Note**: Priority, assignee, parent, fix version, and start date were already set during branch creation and should not be changed.

**Procedure**:
1. Display update preview → **Wait for user approval**
2. On approval, update issue via `mcp__jira__jira_patch`
3. Report update results

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
2. Extract Jira key from branch name
3. Auto-generate PR content → **Wait for user approval**
4. Execute `gh pr create`

### PR Format

**Title**: `{issue-key} <Short Description>` (max 70 chars, no colons)

**Body**:
```markdown
## Summary
- Key changes in 1-3 lines

## Changes
- List only functional changes (bullet points)
- Organized by file/module

## Jira
- Issue: [{issue-key}](https://rsarndsw.atlassian.net/browse/{issue-key})

## Impact
- Scope: {module/project name}
- Breaking changes: yes/no

## Testing
- Build verification status
- Test method/results
```

**Rules**:
- **PR title/body written in English** (explain in Korean to user, then generate English PR)
- No AI/Claude-related text insertion
- Base branch: default `develop`, fallback `main`

### 4-2. Completion Status Processing (after PR creation)

Check work completion status after PR creation and update Jira status.

Ask the user: **"Has all feature implementation and test verification been completed?"**

**If "Yes"**:
1. Transition current issue to **"Done"** (`31`)
2. Check sibling issues (query sub-issues of the same parent):
   - All siblings "Done" → also transition parent (task/bug) to `31`
   - All of parent's siblings also "Done" → also transition epic to `31`
   - Any incomplete → keep parent as "In Progress"
3. **Skip if already "Done"** (prevent unnecessary transitions)
4. Report propagation results

**If "No"**: No status change ("In Progress" maintained), report results only

**Report format**:
```
Completion Status Processing:
  {issue-key} → Done ✓
  {parent-key} → Done ✓ (siblings 3/3 done)
  {epic-key} → In Progress maintained (siblings 2/5 done)
```

---

## Project Structure Reference

| Path | Repository | Jira Project |
|------|-----------|-------------|
| `repos/DevTestWpfCalApp/` | Main app (Shell, SDK, Modules) | UMS |
| `repos/MotionCalculator/` | Calculator plugin (independent repo) | UMS |
| `repos/MotorMonitor/` | Monitor plugin (independent repo) | UMS |

---

## Jira Issue Key Extraction Rules

Regex for extracting issue key from branch name:
```
Pattern: {area}/{type}-({project-key}-\d+)-{description}
Regex: [A-Z]+-\d+
Example: ui/feat-UMS-42-add-dark-mode → UMS-42
```

---

## Workflow Transitions

| Transition ID | Name | Description |
|--------------|------|-------------|
| `11` | To Do | Initial state (backlog) |
| `21` | In Progress | Work in progress |
| `31` | Done | Work completed |

**Transition API**: `mcp__jira__jira_post` → `/rest/api/3/issue/{issue-key}/transitions`
```json
{ "transition": { "id": "21" } }
```

**Status check**: Determine current status via issue's `fields.status.name` value

---

## Important Notes

- **All Git + Jira artifacts must be written in English** — branch names, commit messages, PR title/body, Jira issue summary/description must be in English (Korean summary then English conversion)
- **User approval is required at each step**
- **Preview → approval required before Jira issue creation/modification**
- **No `--force` push** (unless explicitly requested)
- **No committing sensitive files** (.env, credentials, key files)
- **Warn when pushing directly to main/develop**
- **No AI/Co-Authored-By text in commit messages**
- **Leave fields empty if `CLAUDE.md` setting is `TODO`** (epic, fix version, etc.)
