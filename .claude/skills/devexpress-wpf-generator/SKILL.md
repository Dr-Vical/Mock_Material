---
name: devexpress-wpf-generator
description: Generate DevExpress WPF UI from text descriptions. Creates XAML views, MVVM ViewModels, Models, and themes for .NET 8+. Trigger on any WPF screen/form/dashboard request mentioning DevExpress or its controls (GridControl, RibbonControl, DockLayoutManager, ChartControl, etc.), even partial cues like "make a grid" or "add a ribbon".
---

# DevExpress WPF Generator

## Purpose

Generate complete or partial DevExpress WPF project structures from natural language.
Covers five categories:

- **Data** — GridControl, TreeListControl, PivotGrid, PropertyGrid
- **Layout** — DockLayoutManager, LayoutControl, RibbonControl, TabControl
- **Visualization** — ChartControl, GaugeControl, HeatmapControl, TreeMapControl, MapControl
- **Productivity** — SchedulerControl, GanttControl, SpreadsheetControl, RichEditControl, DiagramControl
- **Infrastructure** — ThemedWindow, Themes, MVVM services, DataEditors, Navigation, PdfViewer, Reporting

## When to Run

- User asks to create, scaffold, or generate a WPF screen using DevExpress
- User describes a UI and expects DevExpress control selection + XAML output
- User mentions DevExpress control class names or common keywords (grid, ribbon, dock, chart, scheduler)
- User requests DevExpress theme/style setup or MVVM project scaffolding
- User wants to add a new View/ViewModel to an existing DevExpress WPF project

## Related Files

Always read **mvvm**, **themes**, **project-templates**, **naming-conventions**, and **xaml-conventions**.
Read control-specific files based on the request.

| Category | Files (in `references/`) |
|----------|-------------------------|
| **Always** | `devexpress-wpf-mvvm.md`, `devexpress-wpf-themes.md`, `project-templates.md`, `naming-conventions.md`, `xaml-conventions.md` |
| Data | `devexpress-wpf-datagrid.md`, `devexpress-wpf-treelist.md`, `devexpress-wpf-pivotgrid.md`, `devexpress-wpf-propertygrid.md` |
| Layout | `devexpress-wpf-docking.md`, `devexpress-wpf-layout.md`, `devexpress-wpf-ribbon.md`, `devexpress-wpf-tabcontrol.md` |
| Visualization | `devexpress-wpf-charts.md`, `devexpress-wpf-gauges.md`, `devexpress-wpf-heatmap.md`, `devexpress-wpf-treemap.md`, `devexpress-wpf-map.md` |
| Productivity | `devexpress-wpf-scheduler.md`, `devexpress-wpf-gantt.md`, `devexpress-wpf-spreadsheet.md`, `devexpress-wpf-richedit.md`, `devexpress-wpf-diagram.md` |
| Infrastructure | `devexpress-wpf-data-editors.md`, `devexpress-wpf-navigation.md`, `devexpress-wpf-pdfviewer.md`, `devexpress-wpf-reporting.md`, `devexpress-wpf-windows-ui.md` |

## Workflow

### Step 1: Parse the Request

Identify from the user's description:
- Required DevExpress controls (map to reference files above)
- Data entities and their relationships
- User interactions (CRUD, navigation, filtering, etc.)
- Whether this is a new project or adding to an existing one

### Step 2: Read References

Read `references/devexpress-wpf-mvvm.md` and `references/devexpress-wpf-themes.md` first.
Then read the control-specific reference files identified in Step 1.
Read `references/project-templates.md` for .csproj, App.xaml, and folder structure.
Read `references/naming-conventions.md` for C# and UI naming rules.
Read `references/xaml-conventions.md` for xmlns prefixes and XAML patterns.

### Step 3: Design Model → ViewModel → View

1. **Model** — Define C# classes for data entities. Use PascalCase class names, camelCase fields.
2. **ViewModel** — Create POCO ViewModel (DevExpress MVVM). `virtual` properties for binding,
   public `void`/`async Task` methods for commands. Use `ViewModelBase` only when
   Messenger or manual command logic is needed.
3. **View** — Write XAML with `dx:ThemedWindow` as base. Bind DataContext via
   `dxmvvm:ViewModelSource`. Register services in `dxmvvm:Interaction.Behaviors`.

### Step 4: Select Theme

| Context | Theme | Constant |
|---------|-------|----------|
| Modern business (default) | Win11Light | `Theme.Win11LightName` |
| Dark mode | Win11Dark | `Theme.Win11DarkName` |
| Office style | Office2019Colorful | `Theme.Office2019ColorfulName` |
| IDE / dev tool | VS2019Dark | `Theme.VS2019DarkName` |
| Auto light/dark | Win11System | `Theme.Win11SystemName` |

### Step 5: Generate Output

For **new projects**, generate the full structure per `references/project-templates.md`:
```
ProjectName/
├── ProjectName.csproj       ← .NET 8, DevExpress NuGet refs
├── App.xaml / App.xaml.cs   ← Theme setup
├── Models/                  ← Domain classes
├── ViewModels/              ← POCO or ViewModelBase classes
├── Views/                   ← XAML + code-behind
├── Services/                ← Optional
├── Converters/              ← Optional
└── Resources/               ← Optional
```

For **existing projects**, generate only the new View + ViewModel + Model files,
matching the existing namespace and folder structure.

### Step 6: Validate

Before presenting output, verify:
- Only used xmlns prefixes are declared in XAML
- NuGet packages match the controls actually used
- All ViewModel properties bound in XAML are `virtual` (POCO) or use `GetProperty`/`SetProperty`
- Naming conventions from `references/naming-conventions.md` are followed
- Code-behind inherits `ThemedWindow` (not `Window`)

## Output Format

Present files in project-tree order. Each file as a code block with the relative path as header:

```
ProjectName/Models/Customer.cs
ProjectName/ViewModels/CustomerListViewModel.cs
ProjectName/Views/CustomerListView.xaml
ProjectName/Views/CustomerListView.xaml.cs
```

## Exceptions

- If the user asks for a non-DevExpress WPF control (e.g., Telerik, Syncfusion), inform them
  this skill is DevExpress-specific and offer to generate a standard WPF equivalent.
- If the user requests WinForms or ASP.NET, clarify this skill targets WPF only.
- If a requested feature has no DevExpress control equivalent, suggest the closest alternative
  and explain the gap.
- If the user provides a Figma design, extract layout structure and map UI elements to
  DevExpress controls rather than pixel-perfect replication.
