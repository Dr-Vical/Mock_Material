---
name: material-wpf-generator
description: Generate Material Design WPF UI from text descriptions. Creates XAML views, MVVM ViewModels, and themes for .NET 8 using MaterialDesignInXamlToolkit, Fluent.Ribbon, AvalonDock, and ScottPlot. Trigger on any WPF screen/form/dashboard request mentioning Material Design, or controls like DataGrid, Ribbon, Docking, Chart, TreeView.
---

# Material Design WPF Generator

## Purpose

Generate complete or partial Material Design WPF project structures from natural language.
Covers five categories:

- **Data** — WPF DataGrid (MaterialDesign styled), TreeView, ListView, PropertyGrid-like panels
- **Layout** — AvalonDock (DockingManager, LayoutAnchorable, LayoutDocument), Fluent.Ribbon (RibbonTabItem, RibbonGroupBox)
- **Visualization** — ScottPlot.WPF (real-time monitoring, oscilloscope, bode plot)
- **Controls** — MaterialDesign controls (Card, ColorZone, DialogHost, PackIcon, Buttons, TextBox, ComboBox, CheckBox, ToggleButton)
- **Infrastructure** — Theme system (Dark/Light), i18n (ko/en), CommunityToolkit.Mvvm, DI setup

## When to Run

- User asks to create, scaffold, or generate a WPF screen using Material Design
- User describes a UI and expects MaterialDesign + AvalonDock + Fluent.Ribbon control selection + XAML output
- User mentions control keywords: grid, ribbon, dock, chart, tree, parameter, monitor, oscilloscope
- User requests Material Design theme/style setup or MVVM project scaffolding
- User wants to add a new View/ViewModel to an existing Material Design WPF project
- User mentions "RswareDesign" project or servo drive configuration UI

## Related Files

Always read **project-templates**, **naming-conventions**, and **xaml-conventions**.
Read control-specific files based on the request.

| Category | Files (in `references/`) |
|----------|-------------------------|
| **Always** | `project-templates.md`, `naming-conventions.md`, `xaml-conventions.md` |
| Material Controls | `material-design-controls.md` |
| Ribbon | `fluent-ribbon.md` |
| Docking | `avalondock.md` |
| Charts | `scottplot.md` |

## Workflow

### Step 1: Parse the Request

Identify from the user's description:
- Required UI controls (map to reference files above)
- Data entities and their relationships
- User interactions (parameter editing, navigation, monitoring, etc.)
- Whether this is a new project or adding to an existing one

### Step 2: Read References

1. Read `references/project-templates.md` for .csproj, App.xaml, folder structure
2. Read `references/naming-conventions.md` for C# and UI naming rules
3. Read `references/xaml-conventions.md` for xmlns prefixes and XAML patterns
4. Read control-specific reference files identified in Step 1

### Step 3: Design Model -> ViewModel -> View

1. **Model** — Define C# classes for data entities (Parameter, Drive, DriveGroup)
2. **ViewModel** — CommunityToolkit.Mvvm pattern:
   - Inherit `ObservableObject`
   - Use `[ObservableProperty]` for bindable properties
   - Use `[RelayCommand]` / `[RelayCommand(CanExecute=...)]` for commands
   - Use `WeakReferenceMessenger` for inter-module messaging
3. **View** — Write XAML with:
   - `Window` as base (MaterialDesign themed via BundledTheme)
   - AvalonDock `DockingManager` for main layout
   - Fluent.Ribbon for ribbon menu
   - MaterialDesign styled controls for content

### Step 4: Select Theme

| Context | Base Theme | Primary Swatch |
|---------|-----------|----------------|
| Light mode (default) | Light | Blue |
| Dark mode | Dark | Blue |
| Custom industrial | Dark | DeepBlue + Orange accent |

### Step 5: Generate Output

For **new projects**, generate the full structure:
```
RswareDesign/
├── RswareDesign.csproj
├── App.xaml / App.xaml.cs          ← Theme + MaterialDesign + Fluent.Ribbon setup
├── MainWindow.xaml / .xaml.cs      ← AvalonDock layout + Fluent.Ribbon
├── Models/
│   ├── Parameter.cs
│   ├── Drive.cs
│   └── DriveTreeNode.cs
├── ViewModels/
│   ├── MainWindowViewModel.cs
│   ├── DriveTreeViewModel.cs
│   ├── ParameterEditorViewModel.cs
│   └── [Feature]ViewModel.cs
├── Views/
│   ├── DriveTreeView.xaml
│   ├── ParameterEditorView.xaml
│   ├── MonitorView.xaml
│   ├── OscilloscopeView.xaml
│   └── ErrorLogView.xaml
├── Themes/
│   ├── DarkTheme.xaml
│   ├── LightTheme.xaml
│   ├── SharedTokens.xaml
│   ├── ButtonStyles.xaml
│   ├── DataGridStyles.xaml
│   ├── TreeViewStyles.xaml
│   └── DockingStyles.xaml
├── Services/
│   ├── ThemeService.cs
│   ├── LocalizationService.cs
│   └── NavigationService.cs
├── Resources/
│   ├── Languages/
│   │   ├── ko.json
│   │   └── en.json
│   └── Icons/
└── Converters/
```

For **existing projects**, generate only new View + ViewModel + Model files.

### Step 6: Validate

Before presenting output, verify:
- Only used xmlns prefixes are declared in XAML
- NuGet packages match the controls actually used
- All ViewModel properties use `[ObservableProperty]` or manual `SetProperty`
- Design tokens used (no hardcoded colors, fonts, spacing)
- i18n keys used (no hardcoded Korean/English strings)
- Code-behind is minimal (DataContext binding only)

## Output Format

Present files in project-tree order. Each file as a code block with the relative path as header.

## Design Rules

### Color Constraint: MAX 5 roles
1. **Primary** — Accent, active elements, primary buttons
2. **Secondary** — Highlights, selected items
3. **Surface** — Panel/card backgrounds
4. **Background** — App background
5. **Error** — Error/danger states

### Design Token Mandatory
```
NEVER: Foreground="#CCCCCC"     -> USE: Foreground="{DynamicResource TextPrimary}"
NEVER: FontSize="12"           -> USE: FontSize="{DynamicResource FontSizeMD}"
NEVER: Background="#1E1E1E"    -> USE: Background="{DynamicResource SurfaceBrush}"
NEVER: Text="저장"              -> USE: Text="{DynamicResource loc.common.save}"
```

### i18n Mandatory
- All UI text: `{DynamicResource loc.*}` keys
- ViewModel text: LocalizationService.Get()
- ko.json and en.json must have identical key sets

## Exceptions

- If the user asks for DevExpress controls, inform them this skill is Material Design-specific
- If the user requests WinForms or ASP.NET, clarify this skill targets WPF only
- If a requested feature has no Material Design equivalent, suggest a custom control approach
