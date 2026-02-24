---
name: dev
description: Development request workflow — requirements collection, classification, design, implementation, verification automation
---

# Dev Request Skill

**All reports, questions, and approval requests must be in Korean.**

When the user inputs `/dev`, collect requirements interactively and execute the following workflow.

---

## Specification References (Required Reading)

**Specs to read during Step 0 / Step 2 (by category):**

| Category | Required Specs |
|----------|---------------|
| `PLUGIN` | `ARCHITECTURE.md`, `SDK-INTERFACES.md`, `PLUGIN-SYSTEM.md` |
| `MODULE` | `ARCHITECTURE.md`, `SDK-INTERFACES.md` |
| `SDK` | `SDK-INTERFACES.md`, `PLUGIN-SYSTEM.md` |
| `INFRA` | `ARCHITECTURE.md`, `PLUGIN-SYSTEM.md` |
| `UI` | `DESIGN-SYSTEM.md`, `I18N.md`, `ARCHITECTURE.md` |

All specs are in `.claude/specs/`.

> Stability-First Principle and Build Safety Rules are defined in `CLAUDE.md` (shared across all skills).

### Category-Specific Rules

**UI work:**
- Design system mandatory — no hardcoded colors, FontSize, Padding/Margin, FontFamily
- i18n mandatory — XAML `{DynamicResource loc.*}`, ViewModel `ILocalizationService.Get()`
- Font tokens: `{DynamicResource FontFamilyUI}` / `{DynamicResource FontFamilyCode}` — CJK fallback handled by DevExpressThemeBridge
- Standalone windows: `dx:ThemedWindow` required (`<Window>` prohibited)
- WPF DynamicResource type matching — ResourceDictionary values must match CLR type expected by XAML

**Service/SDK work:**
- sync-over-async prohibited — `.Wait()` / `.Result` usage prohibited
- Backward compatibility must be verified when changing SDK interfaces
- Dependency direction: Infrastructure → Presentation reference prohibited

**Plugin i18n:**
- Key prefix mandatory: `{pluginId}.{key}` format (e.g., `monitor.title`, `calc.input.header`)
- DevExpress BarItem/RibbonItem may not auto-refresh on language switch — verify with `SetLanguage()`

---

## Development Request Classification

AI analyzes user requirements and automatically classifies into one of the 5 categories below.
For composite requests (e.g., SDK interface addition + plugin using it), classify by primary category and note the secondary category.

| Category | Code | Target | Risk Level |
|----------|------|--------|------------|
| Plugin Development | `PLUGIN` | External plugin new development or major modification | Medium |
| Built-in Module | `MODULE` | Shell built-in module new development or major modification | Medium |
| SDK Interface | `SDK` | Sdk.Abstractions / Sdk.Common changes | High |
| Infrastructure | `INFRA` | Plugin system, persistence, communication layer changes | High |
| UI Feature | `UI` | Shell UI, layout, common control changes | Low |

### Classification Criteria

| Keyword/Pattern | Category |
|-----------------|----------|
| "plugin", "external module", separate repo/solution | `PLUGIN` |
| "built-in module", "builtin", module within Shell project | `MODULE` |
| "interface addition", "SDK extension", "service API" | `SDK` |
| "plugin loading", "persistence", "ALC", "lifecycle" | `INFRA` |
| "layout", "ribbon", "theme", "shell UI", "dialog" | `UI` |

---

## Execution Procedure

### Step 0: Project Assessment

Systematically assess the project before starting work. **This step must be completed before modifying any code.**

#### 0-1. Git Status Check
- Current branch, uncommitted changes, last 5 commits
- Remote connection status, upstream tracking status

#### 0-2. Spec Reading
Read the relevant specs based on the expected category (see Specification References above):
1. **Architecture spec** — layer structure, DI map, init order (always read)
2. **Category-specific specs** — SDK interfaces, plugin system, design system, i18n (as needed)
3. **Target repository README** (for plugin work) — project-specific features, dependencies

#### 0-3. Solution Structure Exploration
- Check project list from `.sln` file (`dotnet sln list`)
- Explore target project folder/file structure (Glob)
- Check project references and NuGet packages from `.csproj`

#### 0-4. Existing Code Pattern Search
Find **similar existing implementations** to use as reference patterns:

| Category | Reference Target | What to Check |
|----------|-----------------|---------------|
| `PLUGIN` | MotorMonitor or MotionCalculator | ModuleClass structure, OnActivate/OnDeactivate, View-ViewModel connection |
| `MODULE` | ProjectExplorerModule | IModule registration, Region placement, ViewModel binding |
| `SDK` | Existing SDK interfaces (IShellService etc.) | Interface design patterns, Shell-side implementation |
| `INFRA` | Infrastructure.Plugins | Lifecycle management, async patterns, thread safety |
| `UI` | StartupWindow, CompletionView, Step3_SettingsView | XAML layout, DevExpress controls, **design tokens** (DynamicResource/StaticResource), **i18n** (`loc.*` keys), **ThemedWindow** pattern |

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
1. **Auto-classify category** — select 1 of the 5 above (note primary/secondary for composite)
2. **Explain classification rationale** — briefly explain why this category was chosen
3. **Check for missing required information** — ask additional questions only for items not explained by the user

#### 1-2. Required Information by Category (ask only if missing)

Do not re-ask items already explained by the user, **only ask about missing items**.

##### PLUGIN (Plugin Development)
| Required Item | Description |
|---------------|-------------|
| Plugin ID / Name | ModuleId, DisplayName |
| Feature summary | Core features in 1-3 lines |
| UI composition | Document tab / Tool panel / Navigator node / Properties panel |
| SDK service usage | IShellService, IDialogService, INavigatorService, etc. |
| External dependencies | NuGet packages, hardware SDK, etc. |
| Repository | Existing repo path or new creation |

##### MODULE (Built-in Module)
| Required Item | Description |
|---------------|-------------|
| Module name | ModuleName |
| Placement region | DocumentRegion / ProjectExplorerRegion / PropertiesRegion / OutputRegion |
| Feature summary | Core features in 1-3 lines |
| Other module integration | EventAggregator events, shared services |

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

##### UI (UI Feature)
| Required Item | Description |
|---------------|-------------|
| Target area | Shell layout / Common controls / Theme / Ribbon / Dialog |
| Feature description | Expected visual and functional outcome |
| DevExpress controls | DX controls to use (if any) |
| Impact scope | Impact of changes on other views/modules |

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
3. ProjectExplorerModule (for MODULE category)
4. Existing SDK services (for SDK category)

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
| `MODULE` | Region conflicts, existing module interference, DI registration order |
| `SDK` | **Backward compatibility breakage**, existing plugin build failures, interface bloat |
| `INFRA` | Plugin lifecycle changes, data migration, concurrency issues |
| `UI` | DevExpress version API differences, layout breakage, theme inconsistency |

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
5. ViewModel
6. View (XAML)
7. Module registration / plugin.json
8. Integration test

#### 2-5. Design Checklist by Category

##### PLUGIN
- [ ] `PluginModuleBase` inheritance structure
- [ ] `plugin.json` manifest definition
- [ ] `RegisterTypes`: DI registration list
- [ ] `OnActivate` / `OnDeactivate`: view lifecycle
- [ ] `BringToFront`: tab activation logic
- [ ] SDK service usage plan (Navigator, Properties, Status, etc.)
- [ ] Resource cleanup (`CancellationToken`, `Dispose`)
- [ ] `ViewModelLocator.AutoWireViewModel="False"` (manual DataContext)

##### MODULE
- [ ] Module class (`PluginModuleBase` or Prism `IModule`)
- [ ] Region placement plan
- [ ] ViewModel: `BindableBase` inheritance
- [ ] Commands: `DelegateCommand` / `AsyncDelegateCommand`
- [ ] View ↔ ViewModel binding

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

##### UI
- [ ] XAML layout structure
- [ ] DevExpress control selection
- [ ] **Standalone windows**: use `dx:ThemedWindow` (Win11Dark theme chrome consistency)
- [ ] **Design system integration plan** (4-Layer token usage)
  - [ ] **Colors**: use `DynamicResource` (TextPrimary, AccentPrimary, StatusError, etc.) — no hardcoding (`#RRGGBB`)
  - [ ] **Typography**: use `StaticResource` styles (TextStyle.Heading.*, TextStyle.Body.*, etc.) — no hardcoded FontSize/FontWeight
  - [ ] **Font family**: `{DynamicResource FontFamilyUI}` / `{DynamicResource FontFamilyCode}` — no hardcoded font names
  - [ ] **Spacing**: use `StaticResource` resources (Padding.*, Margin.*, Radius.*, etc.) — no hardcoded Padding/Margin
  - [ ] **Custom controls**: use UI.Controls (see UI.Controls inventory below)
  - [ ] **Reference existing Views**: check design token usage patterns (StartupWindow, CompletionView, etc.)
- [ ] **i18n (localization) plan**
  - [ ] All XAML text: use `{DynamicResource loc.*}` keys — no hardcoded strings
  - [ ] ViewModel text: use `ILocalizationService.Get("loc.*")`
  - [ ] Add keys to `ko.json` + `en.json` (synchronize both)
  - [ ] No hardcoded text in `StringFormat`
- [ ] ViewModel binding design
- [ ] Region integration method
- [ ] Responsive/resize handling

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
3. **ViewModel** → `BindableBase` inheritance, commands, properties
4. **View** → XAML, bindings, converters
5. **Integration** → module registration, DI configuration, plugin.json
6. **Finalization** → full build verification, check for omissions

#### PLUGIN Category Additional Procedure

```
1. Verify SDK DLL existence (Sdk.Abstractions.dll, Sdk.Common.dll)
   → If missing, build DevTestWpfCalApp first then copy
2. Set up csproj references (SDK + Prism + required NuGet)
3. Create plugin.json
4. Implement ModuleClass : PluginModuleBase
5. ViewModel + View
6. dotnet build → verify 0 errors
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
- No direct View references in ViewModel
- DI: constructor injection pattern compliance
- async/await: no sync-over-async (`Task.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- Resource cleanup: `IDisposable` / `CancellationToken` used appropriately
- Naming convention compliance (PascalCase / camelCase / _camelCase)

#### 4-3. Additional Verification by Category

| Category | Additional Verification Items |
|----------|-------------------------------|
| `PLUGIN` | plugin.json validity, SDK version compatibility, output DLL path verification |
| `MODULE` | Region registration verification, module catalog registration verification |
| `SDK` | Existing plugin build success, interface backward compatibility |
| `INFRA` | Existing functionality working correctly, concurrency review |
| `UI` | No XAML syntax errors, DevExpress control compatibility, **design token usage verification**, **i18n completeness verification**, **ThemedWindow usage verification** |

**Design token usage verification method (auto-executed when XAML is modified):**

- **Only verify when XAML files are modified** (regardless of category)
- Unified grep for hardcoded pattern batch search:

```bash
# Hardcoded pattern unified search (1 time)
grep -rE '(#[0-9A-Fa-f]{6}|FontSize="[0-9]|Padding="[0-9]|Margin="[0-9]|FontFamily="[^{]|Foreground="#|Background="#|Text="[^{]|Content="[^{]|Header="[^{]|<Window )' *.xaml | \
  grep -v 'TextBlock.Text' | grep -v 'x:Key'
```

**Detailed classification on discovery**:
- Hardcoded color → `{DynamicResource tokenName}`
- Hardcoded FontSize → `Style="{StaticResource TextStyle.*}"`
- Hardcoded spacing → `{StaticResource Padding.*}` / `{StaticResource Margin.*}`
- Hardcoded font → `{DynamicResource FontFamilyUI}`
- Hardcoded string → `{DynamicResource loc.*}` + add ko.json/en.json keys
- `<Window>` → `<dx:ThemedWindow>`

**Action on discovery:**
- Hardcoded color → replace with `{DynamicResource tokenName}`
- Hardcoded FontSize/FontWeight → replace with `Style="{StaticResource TextStyle.*}"`
- Hardcoded spacing → replace with `{StaticResource Padding.*}` / `{StaticResource Margin.*}`
- Hardcoded font → replace with `{DynamicResource FontFamilyUI}`
- Hardcoded string → replace with `{DynamicResource loc.*}` + add ko.json/en.json keys
- `<Window>` → replace with `<dx:ThemedWindow>`
- Report to user → apply fixes after approval

**Verification result report**:
- **No issues** → proceed automatically (no approval needed)
- **Issues found** → report fix plan → **wait for approval** → fix and re-verify

> This step requests approval **only when issues are found** (efficiency ↑)

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
| Design token added/changed | `DESIGN-SYSTEM.md` | Token/style inventory |
| Custom control added | `DESIGN-SYSTEM.md` | Controls inventory |
| SDK interface changed | `SDK-INTERFACES.md` | Method signatures, DTOs |
| DI registration added | `ARCHITECTURE.md` | DI map |
| Plugin pattern changed | `PLUGIN-SYSTEM.md` | Lifecycle, build config |
| i18n key added | `I18N.md` | Key inventory |

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

> Full details in `.claude/specs/ARCHITECTURE.md`, `.claude/specs/SDK-INTERFACES.md`, `.claude/specs/PLUGIN-SYSTEM.md`, `.claude/specs/DESIGN-SYSTEM.md`, `.claude/specs/I18N.md`.

### Dependency Rules
- **Only top-down references allowed** (Presentation → Application → Infrastructure → SDK)
- **Reverse references prohibited** (Infrastructure → Presentation, etc.)
- **Plugins**: can only reference SDK

### Token Usage Rules (UI category)

| Target | Usage Method | Example |
|--------|-------------|---------|
| Colors | `{DynamicResource tokenName}` | `Background="{DynamicResource AppBackground}"` |
| Text styles | `Style="{StaticResource styleName}"` | `Style="{StaticResource TextStyle.Body.Bold}"` |
| Spacing | `{StaticResource resourceName}` | `Margin="{StaticResource Margin.FormField}"` |
| Font family | `{DynamicResource tokenName}` | `FontFamily="{DynamicResource FontFamilyUI}"` |

### ViewModel Pattern

```csharp
public class MyViewModel : BindableBase
{
    private readonly ISomeService _service;
    private readonly ILocalizationService _loc;
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // i18n: When providing dynamic strings from ViewModel
    public string StatusText => _loc.Get("loc.module.status");

    public DelegateCommand SaveCommand { get; }
    public AsyncDelegateCommand LoadCommand { get; }

    public MyViewModel(ISomeService service, ILocalizationService loc)
    {
        _service = service;
        _loc = loc;
        SaveCommand = new DelegateCommand(OnSave);
        LoadCommand = new AsyncDelegateCommand(OnLoadAsync);

        // Refresh bound strings on language change
        _loc.LanguageChanged += (s, e) => RaisePropertyChanged(nameof(StatusText));
    }
}
```

### XAML View Template (UserControl)

```xml
<UserControl x:Class="MyNamespace.Views.MyView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d">

    <Grid Background="{DynamicResource PanelBackground}">
        <!-- Title: StaticResource style + DynamicResource i18n -->
        <TextBlock Text="{DynamicResource loc.module.title}"
                   Style="{StaticResource TextStyle.Heading.Medium}"
                   Foreground="{DynamicResource TextPrimary}"
                   Margin="{StaticResource Margin.Section}" />

        <!-- Body: Colors/fonts/spacing all use tokens -->
        <TextBlock Text="{DynamicResource loc.module.description}"
                   Style="{StaticResource TextStyle.Body}"
                   Foreground="{DynamicResource TextSecondary}"
                   Margin="{StaticResource Margin.FormField}" />

        <!-- Button: Use ButtonStyle -->
        <Button Content="{DynamicResource loc.module.action}"
                Style="{StaticResource ButtonStyle.Primary}"
                Command="{Binding ActionCommand}" />

        <!-- Input: Background/foreground/border tokens -->
        <TextBox Text="{Binding InputValue, UpdateSourceTrigger=PropertyChanged}"
                 FontSize="{DynamicResource FontSizeMD}"
                 Padding="{StaticResource Padding.Input}"
                 Background="{DynamicResource InputBackground}"
                 Foreground="{DynamicResource TextPrimary}"
                 BorderBrush="{DynamicResource BorderDefault}"
                 CaretBrush="{DynamicResource TextPrimary}" />
    </Grid>
</UserControl>
```

### XAML View Template (Standalone Window — ThemedWindow)

```xml
<!-- Standalone windows must use dx:ThemedWindow (WPF Window prohibited) -->
<dx:ThemedWindow x:Class="MyNamespace.Views.MyWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:dx="http://schemas.devexpress.com/winfx/2008/xaml/core"
        Title="{DynamicResource loc.window.title}"
        Height="500" Width="700"
        WindowStartupLocation="CenterScreen">

    <Grid Background="{DynamicResource AppBackground}">
        <!-- Content -->
    </Grid>
</dx:ThemedWindow>
```

> Custom controls inventory: see `.claude/specs/DESIGN-SYSTEM.md` Section 5.

---

## Project Structure Reference

| Path | Repository | Description |
|------|-----------|-------------|
| `repos/DevTestWpfCalApp/` | Main app | Shell, SDK, Modules, Infrastructure |
| `repos/MotionCalculator/` | Plugin | Calculator plugin (independent repo) |
| `repos/MotorMonitor/` | Plugin | Monitor plugin (independent repo) |

---

## Usage Examples

```
/dev
→ "What development should we proceed with?"
→ "I want to create a motor control simulator plugin"
→ AI classifies as PLUGIN → collect additional info → design → implement → verify → suggest git-flow
```

```
/dev
→ "What development should we proceed with?"
→ "I need a feature to save/load project settings"
→ AI classifies as INFRA (primary) + SDK (secondary) → interface + implementation design → ...
```

---

## Important Notes

- **User approval is required at each step** (except Step 0 and Step 4 which are automatic)
- **Build verification required after code modification** (do not proceed if build fails)
- **Backward compatibility must be verified when changing SDK interfaces**
- **Always read and understand existing code before modifying**
- **Avoid over-engineering** — make only the minimum changes required by the requirements
- **Follow existing patterns** — do not invent new patterns, reference existing implementations in the project
- **Namespace conflict check required when changing using statements**
- **Design system mandatory for UI work** — no hardcoded colors/fonts/spacing/font names/strings
- **i18n mandatory** — XAML `{DynamicResource loc.*}` + ViewModel `ILocalizationService.Get()` + ko.json/en.json key synchronization
- **Standalone windows must use `dx:ThemedWindow`** — `<Window>` usage prohibited (Win11Dark theme chrome consistency)
- **WPF DynamicResource type matching** — ResourceDictionary values must be stored as the CLR type expected by XAML (e.g., `FontFamily` token → `FontFamily` object, not `string`)

### Prism Init Order (must verify when registering services)
`RegisterTypes()` → `CreateShell()` → `InitializeShell()` → `InitializeModules()`

**Caution**: Services needed before `InitializeModules()` must be registered in `RegisterTypes()` and initialized in `InitializeShell()`. Full details in `.claude/specs/ARCHITECTURE.md`.
