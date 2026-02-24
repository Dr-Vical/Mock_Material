---
name: test
description: Feature/technical verification and test workflow — test planning, execution, result storage, and follow-up automation
---

# Test Skill

**All reports, questions, and approval requests must be in Korean.**

When the user inputs `/test` or `/test <target>`, execute the following workflow.

## Mode Selection

| Mode | Invocation | Steps | Purpose |
|------|-----------|-------|---------|
| **Full mode** (default) | `/test` or `/test <target>` | Steps 0-5 | FUNC, REG, INT - systematic test + result storage |
| **Light mode** | `/test --quick <target>` | Steps 0, 2, 3 only | POC, one-off verification - quick validation only (skip storage) |

**Light mode characteristics**:
- Skip test plan creation → execute immediately
- Report results only (no storage)
- Skip follow-up processing
- **Suitable for POC and simple technical verification**

---

## Test Type Classification

| Type | Code | Description | Example |
|------|------|-------------|---------|
| Technical verification | `POC` | Verify specific technology/library behavior | DryIoc child container isolation test |
| Functional verification | `FUNC` | Verify implemented feature works correctly | Plugin Activate/Deactivate behavior |
| Regression test | `REG` | Verify existing features remain intact after changes | Build + functionality check after refactoring |
| Integration test | `INT` | Verify inter-module communication | Shell ↔ Plugin communication test |
| Performance test | `PERF` | Measure memory, speed, rendering performance | Plugin loading time, memory usage |

---

## Test Categories (for storage)

Maps to `AI Archive/Tests/` subdirectories:

| Category | Target |
|----------|--------|
| `Architecture` | Architecture layers, DI, Clean Architecture |
| `Plugin` | Plugin lifecycle, hot reload, ALC |
| `UI` | Controls, bindings, region navigation |
| `Integration` | Inter-module communication, E2E flows |
| `Performance` | Memory, startup speed, rendering |
| `Data` | Project save/load, settings persistence |

---

## Step 0: Identify Test Target

### 0-1. Project Status Check
1. **Git status**: Current branch, uncommitted changes
2. **Build status**: Verify target project build (`dotnet build`)
3. **README review**: Understand target project structure and architecture

### 0-2. Search Existing Test History (skip in light mode)
1. Search for related test result files in `AI Archive/Tests/`
   - Match by target component name, module name, keywords
2. If previous test results exist, report summary
   - Last test date, result (PASSED/FAILED), findings

> **Light mode**: Skip history search

### 0-3. Related Code Analysis
1. **Always read the test target code first**
2. Identify dependencies (services, interfaces referenced by this code)
3. Identify entry points, configuration, and prerequisites needed for testing

**Status summary report**:
```
[Git] Branch: {current branch} / Uncommitted: {yes/no}
[Build] {success/failure}
[History] Related tests: {N} (latest: {date} - {result})
[Target] {component name} ({file path})
```

> This step proceeds automatically without approval.

---

## Step 1: Test Plan (skip in light mode)

### 1-1. Test Requirements Collection

Ask the user: **"What test should we run? Please describe the target and purpose."**
(If invoked as `/test <target>`, analyze based on that content)

> **Light mode**: Skip entire Step 1 → go directly to Step 2 (execution)

Analyze the user's response to:
1. **Auto-classify test type** — POC / FUNC / REG / INT / PERF
2. **Auto-classify storage category** — Architecture / Plugin / UI / Integration / Performance / Data
3. **Explain classification rationale**
4. → **Wait for user confirmation**

### 1-2. Test Scenario Creation

| Item | Content |
|------|---------|
| **Test name** | Used as storage filename (e.g., `PluginLoader_HotReload`) |
| **Target component** | Class/module/feature to test |
| **Purpose** | What this test aims to verify |
| **Prerequisites** | Required state/environment for test execution |
| **Scenarios** | Step-by-step test procedure (numbered) |
| **Expected results** | Success criteria for each scenario |
| **Verification method** | Build / execution / log review / value comparison, etc. |

**Report test plan** → **Wait for user approval**

---

## Step 2: Test Execution

### 2-1. Environment Preparation
- Verify build
- Prepare test data
- **Write test code in existing test projects** (see mapping below)

### Test Project Mapping

Use the following projects when writing test code:

| Test Type | Target Project | Path |
|-----------|---------------|------|
| `POC` / `FUNC` (unit) | `DevTestWpfCalApp.UnitTests` | `repos/DevTestWpfCalApp/tests/DevTestWpfCalApp.UnitTests/` |
| `INT` / `REG` (integration) | `DevTestWpfCalApp.IntegrationTests` | `repos/DevTestWpfCalApp/tests/DevTestWpfCalApp.IntegrationTests/` |
| `FUNC` (UI-related) | `DevTestWpfCalApp.UI.Tests` | `repos/DevTestWpfCalApp/tests/DevTestWpfCalApp.UI.Tests/` |
| `PERF` | UnitTests or separate benchmark depending on situation | Discuss with user |

**Test fixtures (TestPlugins)**:
- Test plugins exist under `tests/TestPlugins/` (TestPluginAlpha, TestPluginBeta, FakeLib V1/V2)
- Use these fixtures for plugin-related tests

**Execution commands**:
- All: `dotnet test repos/DevTestWpfCalApp/tests/`
- Specific project: `dotnet test repos/DevTestWpfCalApp/tests/DevTestWpfCalApp.UnitTests/`
- Specific test: `dotnet test --filter "FullyQualifiedName~TestClassName"`

### 2-2. Scenario Execution

Execute each scenario in order:

1. **Execute**: Perform scenario steps (build, code analysis, execution, etc.)
2. **Observe**: Record actual results
3. **Judge**: Compare with expected results
   - `PASSED` — Behaved as expected
   - `FAILED` — Behaved differently from expected
   - `PARTIAL` — Only partially passed
4. **Intermediate report after each scenario completion**

### 2-3. Code Modification Rules

- Only modify/add the **minimum code necessary** for test execution
- Do not modify existing feature code (test code only)
- Ask user about cleanup of temporary code after testing

### Build Safety Rules
- Always verify build after adding test code
- Ignore DX1000/DX1001 warnings (license warnings)
- Immediately analyze and fix build failures

---

## Step 3: Result Report

### 3-1. Overall Verdict

| Verdict | Condition |
|---------|-----------|
| **PASSED** | All scenarios passed |
| **FAILED** | One or more scenarios failed |
| **PARTIAL** | Only some scenarios passed or conditional success |

### 3-2. Result Report Format

```
Test Result: {test name}
  Type: {POC/FUNC/REG/INT/PERF}
  Category: {Architecture/Plugin/...}
  Target: {component name}

  Scenario Results:
    1. {scenario name} — PASSED ✓
    2. {scenario name} — FAILED ✗ (cause: ...)
    3. {scenario name} — PASSED ✓

  Overall: {PASSED / FAILED / PARTIAL}

  Findings:
    - ...
    - ...
```

> Result report proceeds automatically without approval.

---

## Step 4: Result Storage (skip in light mode)

After test completion, auto-save results to `AI Archive/Tests/{category}/`.
Follow the "Test Result Storage Rules" format from `CLAUDE.md`.

> **Light mode**: Skip storage → end after Step 3 result report

### Storage Procedure
1. **Confirm category + test name** → **Wait for user approval**
2. If the file already exists, add session after date separator (`---`)
3. If not, create a new file

### File Format

`AI Archive/Tests/{category}/{test-name}.md`

```markdown
# {Test Name}

## Test Overview
| Item | Content |
|------|---------|
| Category | {category} |
| Target | {target component} |
| Purpose | {test purpose} |

---

## {Date Time}

### Test Type: {POC/FUNC/REG/INT/PERF}

### Test Scenarios
1. ...
2. ...

### Prerequisites
- ...

### Result: {PASSED / FAILED / PARTIAL}
- Scenario 1: PASSED ✓
- Scenario 2: FAILED ✗ (cause: ...)

### Findings
- ...

### JIRA Reference
- Type: {technical verification/functional verification/regression/integration/performance}
- Priority: {highest/high/medium/low/lowest}
- Impact scope: {module/project name}
- Related component: {component name}
```

### Storage Rules
- Never include virtual/estimated data — record only actual test content
- Do not judge usage of public/protected members
- Include only essential parts of test code snippets (no full copy)

---

## Step 5: Follow-up Processing (skip in light mode)

Suggest follow-up actions based on test results.

> **Light mode**: Skip follow-up processing

### On PASSED

Ask the user: **"The test passed. Please select a follow-up action."**

| Option | Action |
|--------|--------|
| Create Jira subtask | `/git-flow-jira` integration — register test results as subtask |
| End | Complete after result storage only |

### On FAILED / PARTIAL

Ask the user: **"Issues were found during testing. Please select a follow-up action."**

| Option | Action |
|--------|--------|
| Fix and retest | Fix discovered issues and re-run only affected scenarios |
| Switch to `/dev` | Switch to development workflow when substantial fixes are needed |
| Register Jira bug | `/git-flow-jira` integration — register discovered issues as bug |
| End | Complete after result storage only |

---

## Project Structure Reference

| Path | Repository | Description |
|------|-----------|-------------|
| `repos/DevTestWpfCalApp/` | Main app | Shell, SDK, Modules, Infrastructure |
| `repos/MotionCalculator/` | Plugin | Calculator plugin (independent repo) |
| `repos/MotorMonitor/` | Plugin | Monitor plugin (independent repo) |

---

## Important Notes

- **Test plan must be approved by user before execution**
- **Do not modify existing feature code** — only add/modify test code
- **Build verification required** — always verify build after adding test code
- **Get approval for category/test name before saving results**
- **No virtual data** — record only actual execution results
- **Clean up temporary code** — ask user about removing temporary code after testing

---

## Usage Examples

### Full mode (default)
```
/test
→ "What test should we run?"
→ "I want to verify that DryIoc child container isolation works properly"
→ AI classifies as POC + Architecture → write scenarios → execute → save results → follow-up
```

```
/test repos/DevTestWpfCalApp
→ "What test should we run?"
→ "Check if there are memory leaks after plugin hot reload"
→ AI classifies as FUNC + Plugin → write scenarios → execute → save results → follow-up
```

### Light mode
```
/test --quick PluginLoader
→ Step 0 (status check) → Step 2 (immediate execution) → Step 3 (result report) → End
→ No plan writing, no storage, no follow-up
```
