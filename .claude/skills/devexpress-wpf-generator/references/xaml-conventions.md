# XAML Conventions

## xmlns Declarations

Declare only the namespaces actually used in the file.

| Prefix | URI | Controls |
|--------|-----|----------|
| dx | `http://schemas.devexpress.com/winfx/2008/xaml/core` | ThemedWindow, SimpleButton, DXTabControl |
| dxmvvm | `http://schemas.devexpress.com/winfx/2008/xaml/mvvm` | ViewModelSource, Interaction.Behaviors |
| dxg | `http://schemas.devexpress.com/winfx/2008/xaml/grid` | GridControl, TreeListControl |
| dxdo | `http://schemas.devexpress.com/winfx/2008/xaml/docking` | DockLayoutManager |
| dxr | `http://schemas.devexpress.com/winfx/2008/xaml/ribbon` | RibbonControl |
| dxb | `http://schemas.devexpress.com/winfx/2008/xaml/bars` | BarManager, BarButtonItem |
| dxlc | `http://schemas.devexpress.com/winfx/2008/xaml/layoutcontrol` | LayoutControl |
| dxe | `http://schemas.devexpress.com/winfx/2008/xaml/editors` | TextEdit, ComboBoxEdit |
| dxc | `http://schemas.devexpress.com/winfx/2008/xaml/charts` | ChartControl |
| dxsch | `http://schemas.devexpress.com/winfx/2008/xaml/scheduling` | SchedulerControl |
| dxgn | `http://schemas.devexpress.com/winfx/2008/xaml/gantt` | GanttControl |
| dxga | `http://schemas.devexpress.com/winfx/2008/xaml/gauges` | GaugeControls |
| dxm | `http://schemas.devexpress.com/winfx/2008/xaml/map` | MapControl |
| dxnav | `http://schemas.devexpress.com/winfx/2008/xaml/accordion` | AccordionControl |
| dxpdf | `http://schemas.devexpress.com/winfx/2008/xaml/pdf` | PdfViewerControl |
| dxpg | `http://schemas.devexpress.com/winfx/2008/xaml/pivotgrid` | PivotGridControl |
| dxprg | `http://schemas.devexpress.com/winfx/2008/xaml/propertygrid` | PropertyGridControl |
| dxre | `http://schemas.devexpress.com/winfx/2008/xaml/richedit` | RichEditControl |
| dxsps | `http://schemas.devexpress.com/winfx/2008/xaml/spreadsheet` | SpreadsheetControl |
| dxtm | `http://schemas.devexpress.com/winfx/2008/xaml/treemap` | TreeMapControl |
| dxdiag | `http://schemas.devexpress.com/winfx/2008/xaml/diagram` | DiagramControl |
| dxht | `http://schemas.devexpress.com/winfx/2008/xaml/charts/heatmap` | HeatmapControl |

## Window Base Class

Always use `dx:ThemedWindow` instead of `Window`.
Code-behind must inherit `ThemedWindow`, not `Window`.

```xml
<dx:ThemedWindow x:Class="ProjectName.Views.MainView"
    xmlns:dx="http://schemas.devexpress.com/winfx/2008/xaml/core"
    ...>
```

```csharp
public partial class MainView : ThemedWindow
{
    public MainView() { InitializeComponent(); }
}
```

## DataContext Binding (POCO ViewModel)

```xml
<dx:ThemedWindow.DataContext>
    <dxmvvm:ViewModelSource Type="{x:Type local:MainViewModel}"/>
</dx:ThemedWindow.DataContext>
```

## Service Registration

Register services inside the View that the ViewModel consumes.
Add only services the ViewModel actually uses.

```xml
<dxmvvm:Interaction.Behaviors>
    <dx:DXMessageBoxService/>
    <dxmvvm:DispatcherService/>
    <dxmvvm:OpenFileDialogService Filter="All Files|*.*"/>
    <dx:DialogService>
        <dx:DialogService.ViewTemplate>
            <DataTemplate>
                <local:EditView/>
            </DataTemplate>
        </dx:DialogService.ViewTemplate>
    </dx:DialogService>
</dxmvvm:Interaction.Behaviors>
```

## EventToCommand Pattern

```xml
<dxmvvm:Interaction.Behaviors>
    <dxmvvm:EventToCommand EventName="Loaded"
                           Command="{Binding LoadDataCommand}"/>
</dxmvvm:Interaction.Behaviors>
```
