# XAML Conventions — Material Design WPF

## xmlns Declarations

Declare only the namespaces actually used in the file.

| Prefix | URI | Controls |
|--------|-----|----------|
| *(default)* | `http://schemas.microsoft.com/winfx/2006/xaml/presentation` | Window, Grid, Button, TextBlock, DataGrid, TreeView, etc. |
| x | `http://schemas.microsoft.com/winfx/2006/xaml` | x:Class, x:Name, x:Key, x:Type |
| d | `http://schemas.microsoft.com/expression/blend/2008` | d:DesignHeight, d:DesignWidth |
| mc | `http://schemas.openxmlformats.org/markup-compatibility/2006` | mc:Ignorable="d" |
| materialDesign | `http://materialdesigninxaml.net/winfx/xaml/themes` | Card, ColorZone, PackIcon, DialogHost, Chip, Snackbar, PopupBox, DrawerHost, TransitioningContent, BundledTheme |
| fluent | `urn:fluent-ribbon` | Ribbon, RibbonTabItem, RibbonGroupBox, Backstage |
| avalonDock | `https://github.com/Dirkster99/AvalonDock` | DockingManager, LayoutRoot, LayoutPanel, LayoutAnchorablePane, LayoutDocumentPane, LayoutAnchorable, LayoutDocument |
| avalonDockTheme | `clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.VS2013` | Vs2013DarkTheme, Vs2013LightTheme |
| scottPlot | `clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF` | WpfPlot |
| vm | `clr-namespace:RswareDesign.ViewModels` | ViewModel references |
| views | `clr-namespace:RswareDesign.Views` | View references |
| converters | `clr-namespace:RswareDesign.Converters` | Converter references |
| models | `clr-namespace:RswareDesign.Models` | Model references |

## Window Base Class

Use standard `Window` with MaterialDesign theme applied (NOT DevExpress ThemedWindow).

```xml
<Window x:Class="RswareDesign.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
    xmlns:fluent="urn:fluent-ribbon"
    xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
    TextElement.Foreground="{DynamicResource MaterialDesignBody}"
    TextElement.FontFamily="{DynamicResource MaterialDesignFont}"
    Background="{DynamicResource MaterialDesignPaper}"
    Title="{DynamicResource loc.app.title}"
    Width="1920" Height="1080">
```

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

## UserControl Base

```xml
<UserControl x:Class="RswareDesign.Views.ParameterEditorView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
    d:DesignHeight="600" d:DesignWidth="800"
    Background="{DynamicResource SurfaceBrush}">
```

## DataContext Binding (CommunityToolkit.Mvvm)

DataContext is set via DI in App.xaml.cs, NOT in XAML.

```csharp
// In App.xaml.cs
var mainWindow = _host.Services.GetRequiredService<MainWindow>();
mainWindow.DataContext = _host.Services.GetRequiredService<MainWindowViewModel>();
```

For sub-views within AvalonDock, DataContext flows from parent or is set in code-behind:

```csharp
// In MainWindow.xaml.cs or AvalonDock template
var paramView = new ParameterEditorView
{
    DataContext = _host.Services.GetRequiredService<ParameterEditorViewModel>()
};
```

## Style Dictionary Structure

All tokens are defined in `Themes/` and loaded in `App.xaml` in this order:

| File | Contents | Source |
|------|----------|--------|
| `Themes/Colors.xaml` | 5-role colors, text/border brushes, component brushes, `RibbonItemBrush` | Colors → Fonts → Styles → Buttons |
| `Themes/Fonts.xaml` | FontFamily (UI, Code, Mono), FontSize (XS~3XL) | |
| `Themes/Styles.xaml` | Spacing, sizes, icon effects, labels, sliders | |
| `Themes/Buttons.xaml` | Button, ToggleButton, ComboBox styles | |

**Color constraint: 5 roles only.** NEVER define per-item colors.
- `RibbonItemBrush` = Secondary (all ribbon icons use this ONE color)
- `RibbonItemErrorBrush` = Error (disconnect/error icons only)

## Design Token Binding Rules

| Category | Binding Type | Reason | Example |
|----------|-------------|--------|---------|
| Colors (brushes) | `{DynamicResource}` | Runtime theme switching | `Foreground="{DynamicResource TextPrimary}"` |
| Font family | `{DynamicResource}` | Runtime font switching | `FontFamily="{DynamicResource FontFamilyUI}"` |
| Font size | `{DynamicResource}` | Runtime scaling | `FontSize="{DynamicResource FontSizeMD}"` |
| i18n strings | `{DynamicResource}` | Runtime language switch | `Text="{DynamicResource loc.menu.file}"` |
| Spacing (Padding/Margin) | `{StaticResource}` | Compile-time (fixed) | `Padding="{StaticResource Padding.Panel}"` |
| CornerRadius | `{StaticResource}` | Compile-time | `CornerRadius="{StaticResource Radius.MD}"` |
| Sizes (double) | `{StaticResource}` | Compile-time | `Height="{StaticResource Size.StatusBarHeight}"` |
| Text styles | `{StaticResource}` | Compile-time | `Style="{StaticResource RibbonButtonLabel}"` |
| Button styles | `{StaticResource}` | Compile-time | `Style="{StaticResource RibbonLargeRipple}"` |
| Icon styles | `{StaticResource}` | Includes Foreground color | `Style="{StaticResource RibbonIconOpacityPulse}"` |

## MaterialDesign Attached Properties

```xml
<!-- Floating hint on TextBox -->
<TextBox materialDesign:HintAssist.Hint="{DynamicResource loc.param.search}"
         materialDesign:HintAssist.IsFloating="True"
         Style="{StaticResource MaterialDesignOutlinedTextBox}" />

<!-- Icon in Button -->
<Button Style="{StaticResource MaterialDesignIconButton}"
        ToolTip="{DynamicResource loc.action.refresh}">
    <materialDesign:PackIcon Kind="Refresh" />
</Button>

<!-- Elevation on Card -->
<materialDesign:Card materialDesign:ElevationAssist.Elevation="Dp2">
    <!-- Content -->
</materialDesign:Card>
```

## Fluent.Ribbon Patterns

```xml
<fluent:Ribbon>
    <!-- File Tab -->
    <fluent:RibbonTabItem Header="{DynamicResource loc.menu.file}">
        <fluent:RibbonGroupBox Header="{DynamicResource loc.menu.file.group}">
            <fluent:Button Header="{DynamicResource loc.menu.file.open}"
                          LargeIcon="/Resources/Icons/open_32.png"
                          Icon="/Resources/Icons/open_16.png"
                          Command="{Binding OpenCommand}"
                          SizeDefinition="Large" />
        </fluent:RibbonGroupBox>
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

## AvalonDock Patterns

```xml
<avalonDock:DockingManager
    DocumentsSource="{Binding Documents}"
    AnchorablesSource="{Binding ToolPanels}"
    ActiveContent="{Binding ActiveDocument, Mode=TwoWay}">

    <avalonDock:DockingManager.Theme>
        <avalonDockTheme:Vs2013DarkTheme />
    </avalonDock:DockingManager.Theme>

    <avalonDock:LayoutRoot>
        <!-- Layout structure -->
    </avalonDock:LayoutRoot>
</avalonDock:DockingManager>
```

## ScottPlot Patterns

```xml
<!-- In XAML -->
<scottPlot:WpfPlot x:Name="pltMonitor" />
```

```csharp
// In code-behind or ViewModel initialization
pltMonitor.Plot.Style.DarkMode();
pltMonitor.Plot.Axes.Title.Label.Text = "Monitor";
var sig = pltMonitor.Plot.Add.Signal(dataArray);
sig.Color = ScottPlot.Color.FromHex("#90CAF9");
pltMonitor.Refresh();
```

## Prohibited Patterns

```
NEVER: Foreground="#CCCCCC"           -> USE: Foreground="{DynamicResource TextPrimary}"
NEVER: FontSize="12"                 -> USE: FontSize="{DynamicResource FontSizeMD}"
NEVER: FontFamily="Segoe UI"         -> USE: FontFamily="{DynamicResource FontFamilyUI}"
NEVER: Background="#1E1E1E"          -> USE: Background="{DynamicResource SurfaceBrush}"
NEVER: Padding="8"                   -> USE: Padding="{StaticResource Padding.ToolWindow}"
NEVER: Margin="0,0,0,12"            -> USE: Margin="{StaticResource Margin.FormField}"
NEVER: CornerRadius="4"             -> USE: CornerRadius="{StaticResource Radius.MD}"
NEVER: Text="저장"                   -> USE: Text="{DynamicResource loc.common.save}"
NEVER: Width="250"                   -> USE: Width="{StaticResource Size.SidebarWidth}"
NEVER: <dx:ThemedWindow>            -> USE: <Window> (MaterialDesign themed)
NEVER: dxmvvm:ViewModelSource       -> USE: DI from App.xaml.cs
NEVER: BindableBase                 -> USE: ObservableObject (CommunityToolkit.Mvvm)
NEVER: DelegateCommand              -> USE: [RelayCommand] attribute
NEVER: IEventAggregator             -> USE: WeakReferenceMessenger
NEVER: Per-icon Foreground="#FF..."  -> USE: Style with built-in Foreground (RibbonIconOpacityPulse)
NEVER: 18 different icon colors      -> USE: Single RibbonItemBrush (Secondary role)
```
