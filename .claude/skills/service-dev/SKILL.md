---
name: service-dev
description: Service/SDK/Infrastructure development workflow — interface design, async/await, dependency direction, plugin lifecycle
---

# Service Development Skill

**All reports, questions, and approval requests must be in Korean.**

When the user inputs `/service-dev`, collect requirements interactively and execute the following workflow.

---

## Specification References (Required Reading)

**Specs to read during Step 0 / Step 2:**

| Spec | Purpose | When to Read |
|------|---------|-------------|
| `.claude/specs/ARCHITECTURE.md` | Layer structure, DI map, Prism init order, regions | Step 2 design — **mandatory** |
| `.claude/specs/SDK-INTERFACES.md` | SDK service interfaces with full method signatures | Step 2 design — **mandatory** |
| `.claude/specs/PLUGIN-SYSTEM.md` | Plugin lifecycle, manifest schema, ALC isolation | When classifying as `PLUGIN` |

> Stability-First Principle and Build Safety Rules are defined in `CLAUDE.md` (shared across all skills).

### Service-Specific Rules (this skill only)
- **Backward compatibility**: Always verify existing plugin builds when changing SDK interfaces
- **sync-over-async prohibited**: Use async for I/O operations — `.Wait()` / `.Result` usage prohibited
- **Dependency direction compliance**: Infrastructure → Presentation reference prohibited
- **DevExpress version check**: Verify version-specific API differences before modifying

---

## Development Request Classification

AI analyzes user requirements and automatically classifies into one of the 4 categories below.

| Category | Code | Target | Risk Level |
|----------|------|--------|------------|
| Plugin Development | `PLUGIN` | External plugin new development or major modification | Medium |
| SDK Interface | `SDK` | Sdk.Abstractions / Sdk.Common changes | High |
| Infrastructure | `INFRA` | Plugin system, persistence, communication layer changes | High |
| Application Service | `APP` | Application.Core services, model changes | Medium |

### Classification Criteria

| Keyword/Pattern | Category |
|-----------------|----------|
| "plugin", "external module", separate repo/solution | `PLUGIN` |
| "interface addition", "SDK extension", "service API" | `SDK` |
| "plugin loading", "persistence", "ALC", "lifecycle" | `INFRA` |
| "service addition", "business logic", "project management" | `APP` |

---

## Execution Procedure

### Step 0: Project Assessment

Systematically assess the project before starting work. **This step must be completed before modifying any code.**

#### 0-1. Git Status Check
- Current branch, uncommitted changes, last 5 commits
- Remote connection status, upstream tracking status

#### 0-2. Spec Reading
1. **Architecture spec** (`.claude/specs/ARCHITECTURE.md`) — layer structure, DI registration map, Prism init order
2. **SDK interfaces spec** (`.claude/specs/SDK-INTERFACES.md`) — service signatures, DTOs, events
3. **Plugin system spec** (`.claude/specs/PLUGIN-SYSTEM.md`) — when working on `PLUGIN` category
4. **Target repository README** (for plugin work) — project-specific features, dependencies, build method

#### 0-3. Solution Structure Exploration
- Check project list from `.sln` file (`dotnet sln list`)
- Explore target project folder/file structure (Glob)
- Check project references and NuGet packages from `.csproj`

#### 0-4. Existing Code Pattern Search
Find **similar existing implementations** to use as reference patterns:

| Category | Reference Target | What to Check |
|----------|-----------------|---------------|
| `PLUGIN` | MotorMonitor or MotionCalculator | ModuleClass structure, OnActivate/OnDeactivate, CancellationToken |
| `SDK` | Existing SDK interfaces (IShellService etc.) | Interface design patterns, Shell-side implementation |
| `INFRA` | Infrastructure.Plugins | Lifecycle management, async patterns, thread safety |
| `APP` | Application.Core Services | Service interfaces, implementation patterns, DI registration |

**Search method**: Use Grep to search for key class/interface names to verify actual usage patterns.

#### 0-5. Status Summary Report

Report assessment results to the user:

```
[Git] Branch: {current branch} / Uncommitted: {yes/no}
[README] {Main app tech stack summary} / {Target project summary}
[Structure] Solution projects: {N} / Target files: {N}
[Patterns] Reference implementation: {similar code found} ({file path})
```

> This step proceeds automatically without approval.

---

### Step 1: Requirements Collection and Classification (Unified Approval)

#### 1-1. Interactive Requirements Collection + Information Gathering

Ask the user: **"What development should we proceed with? Please describe freely."**

Analyze the user's response to:
1. **Auto-classify category** — select 1 of the 4 above
2. **Explain classification rationale** — briefly explain why this category was chosen
3. **Check for missing required information** — ask additional questions only for items not explained by the user

#### 1-2. Required Information by Category (ask only if missing)

Do not re-ask items already explained by the user, **only ask about missing items**.

##### PLUGIN (Plugin Development)
| Required Item | Description |
|---------------|-------------|
| Plugin ID / Name | ModuleId, DisplayName |
| Feature summary | Core features in 1-3 lines |
| SDK service usage | IShellService, IDialogService, INavigatorService, etc. |
| External dependencies | NuGet packages, hardware SDK, etc. |
| Repository | Existing repo path or new creation |

##### SDK (SDK Interface)
| Required Item | Description |
|---------------|-------------|
| Interface/class name | New or modification target |
| Purpose | Problem this change solves |
| API design | Expected method/property signatures |
| Implementation location | Shell service / Common base class |
| Compatibility | Impact on existing plugins |

##### INFRA (Infrastructure)
| Required Item | Description |
|---------------|-------------|
| Target area | Plugins / Persistence / Communication |
| Change type | New feature / Bug fix / Refactoring |
| Impact scope | Projects affected by this change |
| Backward compatibility | Whether existing plugin/module behavior is maintained |

##### APP (Application Service)
| Required Item | Description |
|---------------|-------------|
| Service name | Interface + implementation class name |
| Responsibility | Role this service is responsible for |
| Dependencies | Other services needed, external packages |
| Impact scope | Whether used in Presentation layer |

**After collecting all information, provide unified summary report** → **Wait for user approval (1 time)**

```
Classification: {category} ({rationale})
Required information:
  - {item1}: {value}
  - {item2}: {value}
  ...
→ Shall we proceed with the above classification and information?
```

---

### Step 2: Analysis and Design

Analyze the codebase and establish an implementation plan based on collected requirements.

#### 2-1. Existing Code Analysis

**Always read related code first** — do not design without reading code.

- Read all existing files that are targets for change
- Use similar already-implemented code as reference patterns
- Understand dependency graph (who references this file)

**Reference pattern priority**:
1. Similar implementation within the same project (highest priority)
2. MotorMonitor plugin (for PLUGIN category)
3. Existing SDK services (for SDK category)
4. Infrastructure.Plugins (for INFRA category)

#### 2-2. Impact Analysis

| Analysis Item | Content |
|---------------|---------|
| Files to modify | Existing files needing changes (path + expected changes) |
| New files to create | Files to create (path + purpose) |
| Project references | Project references to add/change |
| NuGet packages | Packages to add/update |
| SDK DLL | Whether SDK DLL copy is needed for plugins |

#### 2-3. Risk Assessment

Evaluate risk factors by category:

| Category | Key Risk Factors |
|----------|-----------------|
| `PLUGIN` | SDK version mismatch, ALC isolation issues, resource cleanup failures |
| `SDK` | **Backward compatibility breakage**, existing plugin build failures, interface bloat |
| `INFRA` | Plugin lifecycle changes, data migration, concurrency issues |
| `APP` | DI registration order, Prism initialization timing, service circular references |

#### 2-4. Implementation Plan

**Per-file change plan table**:

| # | File | Change Type | Details |
|---|------|-------------|---------|
| 1 | ... | New/Modify/Delete | ... |

**Implementation order** — dependency-aware ordering:
1. Interface / contract definition
2. Base classes / common code
3. Implementations (services, models)
4. DI registration
5. Integration test

#### 2-5. Design Checklist by Category

##### PLUGIN
- [ ] `PluginModuleBase` inheritance structure
- [ ] `plugin.json` manifest definition
- [ ] `RegisterTypes`: DI registration list
- [ ] `OnActivate` / `OnDeactivate`: lifecycle
- [ ] SDK service usage plan (Navigator, Properties, Status, etc.)
- [ ] Resource cleanup (`CancellationToken`, `Dispose`)

##### SDK
- [ ] Interface definition (`Sdk.Abstractions`)
- [ ] Default implementation or base class (`Sdk.Common`)
- [ ] Shell-side service implementation
- [ ] DI registration (`App.xaml.cs`)
- [ ] **Backward compatibility**: verify existing method signatures maintained
- [ ] Existing plugin build test plan

##### INFRA
- [ ] Target class/interface for changes
- [ ] Thread safety review (`ConcurrentDictionary`, etc.)
- [ ] `async/await` pattern application
- [ ] `IDisposable` implementation (for unmanaged resources)
- [ ] Existing caller impact analysis

##### APP
- [ ] Interface separation (Application.Core.Interfaces)
- [ ] Implementation (Application.Core.Services)
- [ ] DI registration order verification
- [ ] Prism initialization timing (must be before InitializeShell)

**Report full design document** → **Wait for user approval**

---

### Step 3: Implementation

Write code based on the approved design.

#### Implementation Rules

- Implement in **file group units** (2-5 related files at a time)
- **Build verification** after each group (`dotnet build`)
- On build failure, fix immediately → do not proceed to next group until rebuild confirms success
- **Progress report** after each group (completed files, remaining files)
- **Always read existing code first** before modifying — never modify unread files

#### Implementation Order (common across categories)

1. **Infrastructure/Interface** → csproj modifications, project references, interface definitions
2. **Core logic** → services, models, business logic
3. **Integration** → DI registration, App.xaml.cs configuration
4. **Finalization** → full build verification, check for omissions

#### PLUGIN Category Additional Procedure

```
1. Verify SDK DLL existence (Sdk.Abstractions.dll, Sdk.Common.dll)
   → If missing, build DevTestWpfCalApp first then copy
2. Set up csproj references (SDK + Prism + required NuGet)
3. Create plugin.json
4. Implement ModuleClass : PluginModuleBase
5. dotnet build → verify 0 errors
```

#### SDK Category Additional Procedure

```
1. Add interface to Sdk.Abstractions
2. Add base implementation to Sdk.Common (if needed)
3. Implement service in Shell
4. Register DI in App.xaml.cs
5. Verify full DevTestWpfCalApp build
6. Verify existing plugin builds (MotorMonitor, MotionCalculator)
```

**After each group** → build verification → report progress to user

---

### Step 4: Verification

Perform comprehensive verification after all implementation is complete.

#### 4-1. Build Verification
- Build target project
- Build related projects within impact scope
- On SDK changes: verify all plugin builds

#### 4-2. Pattern Verification
- DI: constructor injection pattern compliance
- async/await: no sync-over-async (`Task.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- Resource cleanup: `IDisposable` / `CancellationToken` used appropriately
- Naming convention compliance (PascalCase / camelCase / _camelCase)

#### 4-3. Additional Verification by Category

| Category | Additional Verification Items |
|----------|-------------------------------|
| `PLUGIN` | plugin.json validity, SDK version compatibility, output DLL path verification |
| `SDK` | Existing plugin build success, interface backward compatibility |
| `INFRA` | Existing functionality working correctly, concurrency review |
| `APP` | DI registration order, Prism initialization timing |

#### 4-4. Anti-pattern Verification (auto-executed)

**sync-over-async pattern detection (Critical)**:
```bash
grep -rn "\.Wait()" --include="*.cs"
grep -rn "\.Result" --include="*.cs"
grep -rn "\.GetAwaiter().GetResult()" --include="*.cs"
```

**Action on discovery**:
- Report issue list
- **Proceed with immediate fix** (no approval needed)
- Convert to async Task + interface change + caller modifications
- Re-verify after fix

**Reverse dependency detection (Critical)**:
```bash
# Infrastructure → Presentation reverse reference
grep -rn "using.*\.Shell\." src/Infrastructure/ --include="*.cs"
grep -rn "using.*\.UI\." src/Infrastructure/ --include="*.cs"

# Application → Presentation reverse reference
grep -rn "using.*\.Shell\." src/Application/ --include="*.cs"
```

**Action on discovery**:
- Report issue list
- **Proceed with immediate fix** (no approval needed)
- Move interface to SDK or restructure dependencies
- Re-verify after fix

**Verification result report**:
- **No issues** → proceed automatically (no approval needed)
- **Issues found** → fix immediately → re-verify

---

### Step 5: Completion

#### 5-1. Change Summary

Compile and report all changes:

| Item | Content |
|------|---------|
| Category | {classification code} |
| Summary | {1-2 line summary} |
| Changed file count | New N, Modified N, Deleted N |
| Build result | Success/Failure |

**Per-file change details**:

| # | File | Change Type | Content |
|---|------|-------------|---------|
| 1 | ... | ... | ... |

**Cautions** (if any):
- Items requiring follow-up work
- Items requiring manual testing
- Known limitations

#### 5-2. Spec Update Check

After completion, check if any specs need updating based on what changed:

| Change Type | Target Spec | Update Content |
|-------------|-------------|----------------|
| SDK service interface change | `SDK-INTERFACES.md` | Method signatures, events, DTOs |
| DI registration added/changed | `ARCHITECTURE.md` | DI registration map |
| Project added/deleted | `ARCHITECTURE.md` | Project map |
| Plugin pattern change | `PLUGIN-SYSTEM.md` | Lifecycle, build config |

If applicable → auto-update specs → report "N specs updated" to user.

#### 5-3. README Update Assessment

Determine whether changes warrant README updates.

**Update criteria**:

| Criterion | Example |
|-----------|---------|
| New module/plugin added | Project structure, module list changes |
| SDK service added/changed | API list, service descriptions |
| Architecture changes | Layer structure, dependency rule changes |
| Major feature addition | Implementation status, feature list changes |
| Tech stack changes | NuGet packages, framework versions |

**Procedure**:
1. Determine if above criteria apply
2. If applicable → propose README changes → **wait for user approval** → update README on approval
3. If not applicable → report "README update unnecessary" and proceed to next step

**Target READMEs**:
- Main app: `repos/DevTestWpfCalApp/README.md`
- Plugin: repository-specific `README.md` (e.g., `repos/MotorMonitor/README.md`)
- On SDK changes: check both main app README + affected plugin READMEs

#### 5-4. git-flow Suggestion

Ask the user after completion:

**"Development is complete. Shall we proceed with Git + Jira integration?"**

| Option | Action |
|--------|--------|
| `/git-flow-jira` | Jira issue creation + branch + commit + push + PR (recommended) |
| `/git-flow` | Git only (branch + commit + push + PR) |
| Decline | Output change summary only and end |

---

## Architecture & SDK Quick Reference

> Full details in `.claude/specs/ARCHITECTURE.md`, `.claude/specs/SDK-INTERFACES.md`, `.claude/specs/PLUGIN-SYSTEM.md`.

### Dependency Rules
- **Only top-down references allowed** (Presentation → Application → Infrastructure → SDK)
- **Reverse references prohibited** (Infrastructure → Presentation, etc.)
- **Plugins**: can only reference SDK

### Prism Init Order (must verify when registering services)
`RegisterTypes()` → `CreateShell()` → `InitializeShell()` → `InitializeModules()`

**Caution**: Services needed before `InitializeModules()` must be registered in `RegisterTypes()` and initialized in `InitializeShell()`.

---

## Important Notes

- **User approval is required at each step** (except Step 0 and Step 4 which are automatic)
- **Build verification required after code modification** (do not proceed if build fails)
- **Backward compatibility must be verified when changing SDK interfaces**
- **Always read and understand existing code before modifying**
- **Avoid over-engineering** — make only the minimum changes required by the requirements
- **Follow existing patterns** — do not invent new patterns, reference existing implementations in the project
- **Namespace conflict check required when changing using statements**
- **sync-over-async prohibited** — use async for I/O operations, `.Wait()` / `.Result` usage prohibited
- **Dependency direction compliance** — Infrastructure → Presentation reference prohibited
