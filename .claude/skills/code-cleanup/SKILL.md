---
name: code-cleanup
description: C# project code cleanup automation (XML comments, conventions, refactoring, patterns, dead code)
---

# Code Cleanup Skill

**All reports, questions, and approval requests must be in Korean.**

When the user invokes `/code-cleanup <path>`, execute the following workflow.

---

> Stability-First Principle and Build Safety Rules are defined in `CLAUDE.md` (shared across all skills).

### Code-Cleanup Specific Rules

**Pattern Verification (Step 5) — HIGH severity violations are fixed unconditionally:**
- ViewModel → View direct reference
- sync-over-async (`.Wait()`, `.GetAwaiter().GetResult()`)
- Reverse dependency reference (Infrastructure → Presentation)
- Memory/resource leak

Fix procedure: report → fix immediately (no approval needed) → build verify → proceed.

**Only LOW severity items offer choice:**
- `RelayCommand` → `DelegateCommand` consistency
- Simple Code-Behind under 30 lines

**Build Verification (optimized for cleanup):**
- Build baseline: Before Step 1
- Step 2 (XML comments): **Skip** build
- Step 3 (Conventions): **Skip** build
- **Step 4 (Refactoring)**: Build **required**
- Step 5 (Pattern verification): Skip build (analysis only)
- **Step 6 (Dead code)**: Build **required**

**Cautions:**
- Watch for namespace conflicts (e.g., `Prism.Dialogs.IDialogService` vs `Sdk.Abstractions.Services.IDialogService`)
- Ignore DX1000/DX1001 warnings (license warnings)

## Execution Procedure

### Step 0: Project Assessment (with caching optimization)

Collect context about the target project before starting:

1. **README check (cached)**
   - Search for the nearest `README.md` from the target path (target folder → parent folder → solution root)
   - **Reuse summary only for previously read READMEs** (on repeated calls in the same session)
   - Use cached summary if README content hasn't changed
2. **What to read**: Project structure, dependencies (NuGet/SDK), key namespaces, build method, special notes
3. **If no README**: Determine from `.csproj` dependencies + solution structure
4. **Output assessment summary** (report to user):
   - Project name / target path
   - Key dependencies (framework, NuGet packages, SDK references)
   - Namespace structure
   - Build target `.csproj` path

> This step proceeds automatically without approval.
> **README caching**: Previously read READMEs reference summary only (efficiency ↑)

### Step 1: Scan
- List all `.cs` files at the target path
- Show line count for each file
- Summary of total file count and total line count

### Step 2: XML Comment Inspection
Find and report the following:
- Missing `<summary>` on public/protected classes, interfaces, methods, properties
- Missing `<param>` on method parameters
- Missing `<returns>` on methods with return values
- Recommend `/// <inheritdoc/>` for interface implementations

**Report format**: Filename, line number, member name, missing comment type

**Wait for user approval** → add XML comments on approval

### Step 3: Convention Inspection
Inspect the following:
- **Naming**: PascalCase (public), camelCase (local), _camelCase (private fields)
- **Field naming detailed rules**:
  | Kind | Rule | Example |
  |------|------|---------|
  | `const` | PascalCase | `const int MaxRetry = 3;` |
  | `private static readonly` | _camelCase | `private static readonly JsonSerializerOptions _options` |
  | `private / private readonly` | _camelCase | `private readonly IService _service` |
  | `public static readonly` | PascalCase | `public static readonly string DefaultName` |
- **Unnecessary using**: Unused using statements
- **Formatting**: Indentation, brace style, whitespace consistency
- **Access modifiers**: Missing explicit access modifiers

**Report format**: Filename, line number, violation description, recommended fix

**Wait for user approval** → fix convention violations on approval

### Step 4: Refactoring Analysis
Identify the following:
- **Long methods**: Methods exceeding 50 lines → suggest splitting
- **Deep nesting**: Nesting exceeding 3 levels → suggest early return, method extraction
- **Duplicate code**: Repeated identical logic → suggest extracting common method

**Report format**: Filename, method name, current line count/nesting depth, refactoring suggestion

**Wait for user approval** → apply refactoring on approval

### Step 5: Pattern Verification
Branch verification items based on the target project's layer.

#### Presentation Layer (Shell, UI.Core, UI.Modules.*)
- **Prism MVVM**: ViewModel inherits `BindableBase`
- **Commands**: Uses `DelegateCommand` / `AsyncDelegateCommand`
- **Plugins**: Inherits `PluginModuleBase`, implements `OnActivate`/`OnDeactivate`
- **Regions**: Navigation via `RegionManager`
- **ViewModel → View separation**:
  - Detect `new Views.*`, `new *.View()`, `new *.Window()`, `new *.Dialog()` patterns in ViewModel files (*.ViewModel.cs) → HIGH violation
  - `using *.Views;` exists + direct View type reference → HIGH violation
  - Direct `ShowDialog()` call instead of `IDialogService` → HIGH violation

#### Infrastructure Layer (Plugins, Persistence, Communication, Build)
- **Dependency direction**: Only Application/Domain/SDK interfaces allowed. No reverse references to Presentation layer (`using *.Shell.*`, `using *.UI.*`, etc. = violation)
- **IDisposable**: Dispose pattern implementation for classes holding unmanaged resources (ALC, FileStream, network)
- **async/await**: Use async for I/O operations. No sync-over-async anti-pattern (`Task.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- **Thread safety**: Use concurrent collections (`ConcurrentDictionary`, etc.) for shared state accessible from multiple threads

#### Application / SDK Layer
- **Dependency direction**: Application references only Domain/SDK. No Infrastructure/Presentation references
- **Interface segregation**: Service interfaces defined in Application.Core or Sdk.Abstractions. Implementations do not self-define interfaces

#### Common (All Layers)
- **DI**: Constructor injection pattern
- **Service locator anti-pattern**: No direct `ServiceLocator.Current`, `Container.Resolve` calls (except Composition Root)

**Pattern verification exclusions** (do not flag as violations):
- **Bootstrap/Startup code**: Code executed before Prism Shell loads (App.xaml.cs Shell initialization, StartupWindow, etc.). Prism infrastructure (IDialogService, ViewModelLocator, RegionManager) not yet initialized, so patterns cannot apply
- **Composition Root**: `App.xaml.cs`'s `RegisterTypes()`, `CreateShell()`, direct container manipulation

**Pattern exceptions (LOW severity)**:
- **Small simple Code-Behind**: Simple OK/Cancel dialogs with Code-Behind under 30 lines (no ViewModel)

**Severity classification**:
- **HIGH** (fix recommended):
  - ViewModel directly creates View (`new Views.*`, `new SomeDialog()`)
  - Direct `MessageBox.Show` call in runtime code (should use IDialogService)
  - ViewModel directly depends on `System.Windows.*` types
  - Layer reverse reference violation (Infrastructure → Presentation)
  - sync-over-async anti-pattern (`Task.Result`, `.Wait()`)
- **LOW** (optional):
  - `RelayCommand` → `DelegateCommand` consistency
  - Simple Code-Behind dialog under 30 lines
- **SKIP** (cannot fix):
  - Bootstrap/Composition Root code (`App.xaml.cs` Shell initialization, StartupWindow)

**Report format**: Filename, violated pattern, severity (HIGH/LOW/SKIP), recommended fix

**After reporting, always ask the user about fixes:**
- "Please select which pattern violation items to fix. (All HIGH / individual selection / SKIP)"
- Fix only user-approved items → build verification after fix
- Do not proceed to next step without approval

### Step 6: Dead Code Analysis
Find and report the following (never auto-delete):
- **Unused private members**: Fields, methods, properties
- **Commented-out code blocks**: Commented-out code blocks of 5+ lines

**Exclusions** (never flag):
- public/protected members (may be called externally)
- XAML binding properties (runtime binding)
- Reflection targets (members with Attributes)
- Serialization members

**Report format**: Filename, line range, code snippet, reason for suspicion

**Wait for user approval** → selective deletion on approval

### Step 7: Re-review Suggestion
After completing Step 6, ask the user:
- "Full cleanup is complete. Would you like a comprehensive re-check for anything missed?"
- If user accepts: comprehensively re-review Steps 1-6 results and report any omissions
- If user declines: report full change summary and end

## Project-Specific Patterns (Reference)

- **UI Framework**: DevExpress WPF (GridControl, DockLayoutManager)
- **DI Container**: DryIoc (Prism integration)
- **Architecture**: Clean Architecture (Core → Infrastructure → UI)
- **SDK Structure**: `Sdk.Abstractions` (interfaces), `Sdk.Common` (base classes)

## Usage Example

```
/code-cleanup src/Application/DevTestWpfCalApp.Application.Core
```

## Important Notes

- **User approval is required at each step**
- **Never auto-delete dead code**
- **Build verification required after code modification steps** (do not proceed if build fails)
- **Always consider external reference possibilities**
- **Namespace conflict check required when changing using statements**
