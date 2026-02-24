# AvalonDock for WPF (.NET 8) - Comprehensive XAML Reference

> **Purpose**: Reference material for generating WPF docking layout XAML with AvalonDock.
> **Last Updated**: 2026-02-24

---

## 1. NuGet Packages

### Xceed (Official / Commercial with Free Community Edition)

| Package | Latest Version | Target |
|---|---|---|
| `Xceed.Products.Wpf.Toolkit.AvalonDock` | 5.1.25458.6678 | .NET Framework 4.0+, .NET Core 3.1+, .NET 5/6/7/8/9 |
| `Xceed.Products.Wpf.Toolkit.AvalonDock.Themes` | 5.1.25458.6678 | Same as above |

> **Note**: Xceed v5.x requires a license key set in code:
> `Xceed.Wpf.Toolkit.Licenser.LicenseKey = "WTKXX-XXXXX-XXXXX-XXXX";`
> The Community Edition (v3.8.x) was free but is now legacy.

### Dirkster99 Community Fork (Free, Open Source - Recommended)

| Package | Latest Version | Target |
|---|---|---|
| `Dirkster.AvalonDock` | 4.72.1 | .NET Framework 4.0+, .NET Core 3.0+, .NET 5.0+ (compatible with .NET 8) |
| `Dirkster.AvalonDock.Themes.Aero` | 4.72.1 | Same |
| `Dirkster.AvalonDock.Themes.Metro` | 4.72.1 | Same |
| `Dirkster.AvalonDock.Themes.VS2010` | 4.72.1 | Same |
| `Dirkster.AvalonDock.Themes.VS2013` | 4.72.1 | Same |
| `Dirkster.AvalonDock.Themes.Expression` | 4.72.1 | Same |

### Deprecated (Do Not Use)

| Package | Note |
|---|---|
| `DotNetProjects.AvalonDock` | Deprecated, legacy. Use `Dirkster.AvalonDock` instead. |

### .csproj PackageReference (Dirkster - Recommended for .NET 8)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Dirkster.AvalonDock" Version="4.72.1" />
    <PackageReference Include="Dirkster.AvalonDock.Themes.VS2013" Version="4.72.1" />
    <!-- Optional additional themes -->
    <PackageReference Include="Dirkster.AvalonDock.Themes.Metro" Version="4.72.1" />
    <PackageReference Include="Dirkster.AvalonDock.Themes.Aero" Version="4.72.1" />
  </ItemGroup>
</Project>
```

---

## 2. XMLNS Namespace Declarations

### Option A: Schema URI (Works for both Xceed and Dirkster)

```xml
xmlns:avalonDock="http://schemas.xceed.com/wpf/xaml/avalondock"
```

This is the **recommended** single-namespace approach. Both the Xceed package and the Dirkster fork register this XML namespace URI. All core types (`DockingManager`, `LayoutRoot`, `LayoutPanel`, `LayoutAnchorable`, `LayoutDocument`, etc.) are accessible through this one prefix.

Common prefix conventions:
- `avalonDock` - descriptive, clear
- `xcad` - short, common in Xceed docs
- `ad` - minimal
- `dock` - intuitive

### Option B: CLR Namespace (Explicit Assembly Reference)

```xml
<!-- Core DockingManager -->
xmlns:ad="clr-namespace:AvalonDock;assembly=AvalonDock"

<!-- Layout model classes -->
xmlns:adLayout="clr-namespace:AvalonDock.Layout;assembly=AvalonDock"

<!-- Controls (rarely needed in XAML) -->
xmlns:adControls="clr-namespace:AvalonDock.Controls;assembly=AvalonDock"
```

### Theme Namespaces

```xml
<!-- VS2013 Theme -->
xmlns:adTheme="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.VS2013"

<!-- Metro Theme -->
xmlns:adTheme="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.Metro"

<!-- Aero Theme -->
xmlns:adTheme="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.Aero"
```

### Standard XAML Header Template

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="http://schemas.xceed.com/wpf/xaml/avalondock"
        Title="MainWindow" Height="800" Width="1200">
```

---

## 3. App.xaml Setup

### Minimal Setup (No Explicit Theme - Uses Generic/Default)

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <!-- AvalonDock loads its own generic.xaml automatically -->
            <!-- No explicit merge required for basic usage -->
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### With VS2013 Dark Theme Brushes

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- AvalonDock VS2013 Dark brush resources -->
                <ResourceDictionary Source="/AvalonDock.Themes.VS2013;component/DarkBrushs.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### With VS2013 Light Theme Brushes

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/AvalonDock.Themes.VS2013;component/LightBrushs.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

> **Important**: The brush ResourceDictionaries only theme AvalonDock controls. You should also use a general WPF theming library (e.g., MaterialDesignInXamlToolkit, MahApps.Metro) for non-AvalonDock controls.

---

## 4. Available Themes

### Theme Classes and Their NuGet Packages

| Theme Class | NuGet Package (Dirkster) | Appearance |
|---|---|---|
| `GenericTheme` | (built-in, no extra package) | Default WPF look |
| `AeroTheme` | `Dirkster.AvalonDock.Themes.Aero` | Windows Aero style |
| `MetroTheme` | `Dirkster.AvalonDock.Themes.Metro` | Modern flat Metro |
| `ExpressionDarkTheme` | `Dirkster.AvalonDock.Themes.Expression` | Expression Blend Dark |
| `ExpressionLightTheme` | `Dirkster.AvalonDock.Themes.Expression` | Expression Blend Light |
| `Vs2010Theme` | `Dirkster.AvalonDock.Themes.VS2010` | Visual Studio 2010 |
| `Vs2013DarkTheme` | `Dirkster.AvalonDock.Themes.VS2013` | VS2013 Dark |
| `Vs2013LightTheme` | `Dirkster.AvalonDock.Themes.VS2013` | VS2013 Light |
| `Vs2013BlueTheme` | `Dirkster.AvalonDock.Themes.VS2013` | VS2013 Blue |

### Xceed Additional Themes (Commercial v5.x only)

| Theme | Description |
|---|---|
| `FluentDesignTheme` | Windows 11 Fluent Design |
| `MaterialDesignTheme` | Google Material Design |
| `MetroAccentTheme` | Metro with accent colors |
| `Office2007Theme` | Office 2007 Ribbon style |
| `Windows10Theme` | Windows 10 native |

### Applying a Theme in XAML

```xml
<avalonDock:DockingManager>
    <avalonDock:DockingManager.Theme>
        <avalonDock:Vs2013DarkTheme />
    </avalonDock:DockingManager.Theme>
    <!-- layout content here -->
</avalonDock:DockingManager>
```

### Applying a Theme in Code-Behind

```csharp
using AvalonDock.Themes;

dockingManager.Theme = new Vs2013DarkTheme();
// or
dockingManager.Theme = new MetroTheme();
// or
dockingManager.Theme = new AeroTheme();
```

---

## 5. Core Layout Structure - Element Hierarchy

```
DockingManager                          (Root container, one per window)
  └─ LayoutRoot                         (Single root layout node)
       ├─ LayoutPanel                   (Horizontal/Vertical splitting container)
       │    ├─ LayoutAnchorablePaneGroup    (Groups tool panes together)
       │    │    └─ LayoutAnchorablePane    (Tab container for tool windows)
       │    │         ├─ LayoutAnchorable   (Individual tool panel)
       │    │         └─ LayoutAnchorable
       │    ├─ LayoutDocumentPaneGroup      (Groups document panes)
       │    │    └─ LayoutDocumentPane      (Tab container for documents)
       │    │         ├─ LayoutDocument     (Individual document tab)
       │    │         └─ LayoutDocument
       │    └─ LayoutPanel                  (Nested panels for complex layouts)
       │         └─ ...
       ├─ LeftSide                      (Auto-hide anchors on left)
       │    └─ LayoutAnchorGroup
       │         └─ LayoutAnchorable
       ├─ RightSide                     (Auto-hide anchors on right)
       ├─ TopSide                       (Auto-hide anchors on top)
       └─ BottomSide                    (Auto-hide anchors on bottom)
```

### Key Concepts

| Element | Role | Can Contain |
|---|---|---|
| `DockingManager` | Root WPF control, hosts everything | `LayoutRoot` |
| `LayoutRoot` | Single layout root | `LayoutPanel`, side anchors |
| `LayoutPanel` | Splits area H or V | Other `LayoutPanel`, pane groups, panes |
| `LayoutAnchorablePaneGroup` | Groups tool panes | `LayoutAnchorablePane` |
| `LayoutAnchorablePane` | Tabbed tool window host | `LayoutAnchorable` |
| `LayoutAnchorable` | Single tool window | Any WPF content |
| `LayoutDocumentPaneGroup` | Groups document panes | `LayoutDocumentPane` |
| `LayoutDocumentPane` | Tabbed document host | `LayoutDocument`, `LayoutAnchorable` |
| `LayoutDocument` | Single document tab | Any WPF content |

---

## 6. Key Properties Reference

### LayoutAnchorable Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Tab header text |
| `ContentId` | `string` | `null` | Unique ID for serialization/restore |
| `CanClose` | `bool` | `true` | User can close this panel |
| `CanFloat` | `bool` | `true` | User can drag to floating window |
| `CanHide` | `bool` | `true` | User can hide this panel |
| `CanAutoHide` | `bool` | `true` | User can auto-hide (pin/unpin) |
| `CanDockAsTabbedDocument` | `bool` | `true` | Can be docked into document area |
| `CanMove` | `bool` | `true` | User can drag/reposition |
| `AutoHideWidth` | `double` | `0` | Width when shown from auto-hide (left/right) |
| `AutoHideHeight` | `double` | `0` | Height when shown from auto-hide (top/bottom) |
| `AutoHideMinWidth` | `double` | `100` | Minimum width when auto-hide shown |
| `AutoHideMinHeight` | `double` | `100` | Minimum height when auto-hide shown |
| `FloatingWidth` | `double` | `0` | Initial width of floating window |
| `FloatingHeight` | `double` | `0` | Initial height of floating window |
| `FloatingLeft` | `double` | `0` | Initial X position of floating window |
| `FloatingTop` | `double` | `0` | Initial Y position of floating window |
| `IsActive` | `bool` | `false` | Whether this content is active/focused |
| `IsSelected` | `bool` | `false` | Whether this tab is selected in its pane |
| `IconSource` | `ImageSource` | `null` | Icon shown in tab header |
| `ToolTip` | `object` | `null` | Tooltip for the tab |

### LayoutDocument Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Tab header text |
| `ContentId` | `string` | `null` | Unique ID for serialization |
| `CanClose` | `bool` | `true` | User can close this document |
| `CanFloat` | `bool` | `true` | User can float this document |
| `CanMove` | `bool` | `true` | User can drag/reposition |
| `IsActive` | `bool` | `false` | Whether this document is active |
| `IsSelected` | `bool` | `false` | Whether this tab is selected |
| `IsLastFocusedDocument` | `bool` | `false` | Read-only, was last focused |
| `Description` | `string` | `""` | Document description |
| `IconSource` | `ImageSource` | `null` | Icon shown in tab header |

### LayoutAnchorablePane / LayoutDocumentPane Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `DockWidth` | `GridLength` | `*` | Width of this pane (e.g., `200`, `*`, `2*`) |
| `DockHeight` | `GridLength` | `*` | Height of this pane |
| `DockMinWidth` | `double` | `25` | Minimum width when resizing |
| `DockMinHeight` | `double` | `25` | Minimum height when resizing |
| `Name` | `string` | `null` | x:Name for code-behind access |
| `FloatingWidth` | `double` | `0` | Width when group floats |
| `FloatingHeight` | `double` | `0` | Height when group floats |
| `FloatingLeft` | `double` | `0` | X when group floats |
| `FloatingTop` | `double` | `0` | Y when group floats |

### LayoutAnchorablePaneGroup / LayoutDocumentPaneGroup Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Orientation` | `Orientation` | `Horizontal` | How children are split |
| `DockWidth` | `GridLength` | `*` | Width of this group |
| `DockHeight` | `GridLength` | `*` | Height of this group |
| `DockMinWidth` | `double` | `25` | Minimum width |
| `DockMinHeight` | `double` | `25` | Minimum height |
| `FloatingWidth` | `double` | `0` | Float width |
| `FloatingHeight` | `double` | `0` | Float height |

### LayoutPanel Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Orientation` | `Orientation` | `Horizontal` | Horizontal or Vertical split |
| `DockWidth` | `GridLength` | `*` | Width of this panel |
| `DockHeight` | `GridLength` | `*` | Height of this panel |

### DockingManager Key Properties

| Property | Type | Description |
|---|---|---|
| `Theme` | `Theme` | Sets the visual theme |
| `DocumentsSource` | `IEnumerable` | MVVM binding source for documents |
| `AnchorablesSource` | `IEnumerable` | MVVM binding source for anchorables |
| `ActiveContent` | `object` | Currently active content (bindable) |
| `DocumentPaneTemplate` | `ControlTemplate` | Template for document pane tabs |
| `AnchorablePaneTemplate` | `ControlTemplate` | Template for anchorable pane tabs |
| `LayoutItemTemplate` | `DataTemplate` | Default template for layout items |
| `LayoutItemTemplateSelector` | `DataTemplateSelector` | Selects templates by type |
| `LayoutItemContainerStyle` | `Style` | Style for LayoutItem containers |
| `LayoutItemContainerStyleSelector` | `StyleSelector` | Selects styles by type |
| `DocumentHeaderTemplate` | `DataTemplate` | Template for document tab headers |
| `DocumentTitleTemplate` | `DataTemplate` | Template for document title |
| `AnchorableHeaderTemplate` | `DataTemplate` | Template for anchorable tab headers |
| `AnchorableTitleTemplate` | `DataTemplate` | Template for anchorable title |
| `AllowMixedOrientation` | `bool` | Allow mixed H/V in same group |
| `AutoHideWindowClosingTimer` | `int` | ms before auto-hide window closes |
| `ShowSystemMenu` | `bool` | Show system menu on floating windows |
| `AllowDrop` | `bool` | Allow drag-drop docking |

---

## 7. MVVM Binding Pattern

### ViewModel Structure

```csharp
// Base class for all dockable content
public abstract class PaneViewModel : ObservableObject
{
    public string Title { get; set; }
    public string ContentId { get; set; }
    public bool IsSelected { get; set; }
    public bool IsActive { get; set; }
}

// Tool window ViewModel
public class ToolViewModel : PaneViewModel
{
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
}

// Document ViewModel
public class DocumentViewModel : PaneViewModel
{
    public string FilePath { get; set; }
    public string Content { get; set; }
    public bool IsDirty { get; set; }
}

// Main workspace ViewModel
public class WorkspaceViewModel : ObservableObject
{
    public ObservableCollection<DocumentViewModel> Documents { get; }
        = new ObservableCollection<DocumentViewModel>();

    public ObservableCollection<ToolViewModel> Tools { get; }
        = new ObservableCollection<ToolViewModel>();

    private object _activeDocument;
    public object ActiveDocument
    {
        get => _activeDocument;
        set => SetProperty(ref _activeDocument, value);
    }
}
```

### XAML with MVVM Bindings

```xml
<avalonDock:DockingManager
    DocumentsSource="{Binding Documents}"
    AnchorablesSource="{Binding Tools}"
    ActiveContent="{Binding ActiveDocument, Mode=TwoWay}">

    <!-- Template Selector: choose DataTemplate based on ViewModel type -->
    <avalonDock:DockingManager.LayoutItemTemplateSelector>
        <local:PaneTemplateSelector>
            <local:PaneTemplateSelector.DocumentTemplate>
                <DataTemplate>
                    <TextBox Text="{Binding Content}" AcceptsReturn="True" />
                </DataTemplate>
            </local:PaneTemplateSelector.DocumentTemplate>
            <local:PaneTemplateSelector.ToolTemplate>
                <DataTemplate>
                    <ContentPresenter Content="{Binding}" />
                </DataTemplate>
            </local:PaneTemplateSelector.ToolTemplate>
        </local:PaneTemplateSelector>
    </avalonDock:DockingManager.LayoutItemTemplateSelector>

    <!-- Container Style Selector: set LayoutItem properties from ViewModel -->
    <avalonDock:DockingManager.LayoutItemContainerStyleSelector>
        <local:PaneStyleSelector>
            <local:PaneStyleSelector.DocumentStyle>
                <Style TargetType="{x:Type avalonDock:LayoutItem}">
                    <Setter Property="Title" Value="{Binding Model.Title}" />
                    <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
                    <Setter Property="CloseCommand" Value="{Binding Model.CloseCommand}" />
                    <Setter Property="CanClose" Value="{Binding Model.CanClose}" />
                </Style>
            </local:PaneStyleSelector.DocumentStyle>
            <local:PaneStyleSelector.ToolStyle>
                <Style TargetType="{x:Type avalonDock:LayoutItem}">
                    <Setter Property="Title" Value="{Binding Model.Title}" />
                    <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
                    <Setter Property="Visibility"
                            Value="{Binding Model.IsVisible,
                                    Converter={StaticResource BoolToVisConverter},
                                    Mode=TwoWay}" />
                    <Setter Property="CanClose" Value="False" />
                </Style>
            </local:PaneStyleSelector.ToolStyle>
        </local:PaneStyleSelector>
    </avalonDock:DockingManager.LayoutItemContainerStyleSelector>

    <!-- Static layout structure (positions are defined here, content comes from bindings) -->
    <avalonDock:DockingManager.Layout>
        <avalonDock:LayoutRoot>
            <avalonDock:LayoutPanel Orientation="Horizontal">
                <avalonDock:LayoutAnchorablePane DockWidth="250" />
                <avalonDock:LayoutDocumentPane />
                <avalonDock:LayoutAnchorablePane DockWidth="250" />
            </avalonDock:LayoutPanel>
        </avalonDock:LayoutRoot>
    </avalonDock:DockingManager.Layout>

</avalonDock:DockingManager>
```

### Template Selector Implementation (C#)

```csharp
public class PaneTemplateSelector : DataTemplateSelector
{
    public DataTemplate DocumentTemplate { get; set; }
    public DataTemplate ToolTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is DocumentViewModel)
            return DocumentTemplate;
        if (item is ToolViewModel)
            return ToolTemplate;
        return base.SelectTemplate(item, container);
    }
}

public class PaneStyleSelector : StyleSelector
{
    public Style DocumentStyle { get; set; }
    public Style ToolStyle { get; set; }

    public override Style SelectStyle(object item, DependencyObject container)
    {
        if (item is DocumentViewModel)
            return DocumentStyle;
        if (item is ToolViewModel)
            return ToolStyle;
        return base.SelectStyle(item, container);
    }
}
```

---

## 8. Layout Save / Restore (XmlLayoutSerializer)

### Save Layout

```csharp
using AvalonDock.Layout.Serialization;

private void SaveLayout()
{
    var serializer = new XmlLayoutSerializer(dockingManager);
    using var writer = new StreamWriter("layout.xml");
    serializer.Serialize(writer);
}
```

### Restore Layout

```csharp
private void RestoreLayout()
{
    var serializer = new XmlLayoutSerializer(dockingManager);

    // Handle content restoration during deserialization
    serializer.LayoutSerializationCallback += (sender, args) =>
    {
        // args.Model is the LayoutContent being deserialized
        // args.Content is the content to associate
        // Match by ContentId
        if (args.Model.ContentId == "Explorer")
        {
            args.Content = FindOrCreateExplorerView();
        }
        else if (args.Model.ContentId == "Output")
        {
            args.Content = FindOrCreateOutputView();
        }
        else
        {
            // If content not found, cancel to remove from layout
            args.Cancel = true;
        }
    };

    if (File.Exists("layout.xml"))
    {
        using var reader = new StreamReader("layout.xml");
        serializer.Deserialize(reader);
    }
}
```

### Save/Restore to String

```csharp
// Save to string
private string SerializeLayout()
{
    var serializer = new XmlLayoutSerializer(dockingManager);
    using var writer = new StringWriter();
    serializer.Serialize(writer);
    return writer.ToString();
}

// Restore from string
private void DeserializeLayout(string layoutXml)
{
    var serializer = new XmlLayoutSerializer(dockingManager);
    serializer.LayoutSerializationCallback += OnLayoutSerializationCallback;
    using var reader = new StringReader(layoutXml);
    serializer.Deserialize(reader);
}
```

> **Critical**: Every `LayoutAnchorable` and `LayoutDocument` must have a unique `ContentId` set for serialization to work correctly. Without `ContentId`, the serializer cannot match saved layout positions to content.

---

## 9. Theme Customization

### Override AvalonDock Colors with Custom Dark Theme

Create a custom ResourceDictionary that overrides the brush keys used by AvalonDock themes:

```xml
<!-- CustomAvalonDockBrushes.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Main background colors -->
    <SolidColorBrush x:Key="AvalonDock_ThemeBackground" Color="#1E1E1E" />
    <SolidColorBrush x:Key="AvalonDock_ThemeForeground" Color="#D4D4D4" />

    <!-- Document tab well -->
    <SolidColorBrush x:Key="AvalonDock_DocumentWellTabSelectedActiveBackground" Color="#1E1E1E" />
    <SolidColorBrush x:Key="AvalonDock_DocumentWellTabSelectedInactiveBackground" Color="#2D2D30" />
    <SolidColorBrush x:Key="AvalonDock_DocumentWellTabUnselectedBackground" Color="#2D2D30" />
    <SolidColorBrush x:Key="AvalonDock_DocumentWellTabUnselectedHoverBackground" Color="#3E3E42" />

    <!-- Tool window tabs -->
    <SolidColorBrush x:Key="AvalonDock_ToolWindowTabSelectedActiveBackground" Color="#007ACC" />
    <SolidColorBrush x:Key="AvalonDock_ToolWindowTabSelectedInactiveBackground" Color="#3F3F46" />

    <!-- Title bar -->
    <SolidColorBrush x:Key="AvalonDock_ToolWindowCaptionActiveBackground" Color="#007ACC" />
    <SolidColorBrush x:Key="AvalonDock_ToolWindowCaptionInactiveBackground" Color="#2D2D30" />
    <SolidColorBrush x:Key="AvalonDock_ToolWindowCaptionActiveForeground" Color="#FFFFFF" />
    <SolidColorBrush x:Key="AvalonDock_ToolWindowCaptionInactiveForeground" Color="#D0D0D0" />

    <!-- Borders and separators -->
    <SolidColorBrush x:Key="AvalonDock_ThemeBorderBrush" Color="#3F3F46" />
    <SolidColorBrush x:Key="AvalonDock_AutoHideTabHover" Color="#3E3E42" />

    <!-- Floating window -->
    <SolidColorBrush x:Key="AvalonDock_FloatingWindowTitleBarActive" Color="#007ACC" />
    <SolidColorBrush x:Key="AvalonDock_FloatingWindowTitleBarInactive" Color="#2D2D30" />
    <SolidColorBrush x:Key="AvalonDock_FloatingWindowBorder" Color="#007ACC" />

</ResourceDictionary>
```

### Merging Custom Brushes in App.xaml

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Load base VS2013 Dark theme first -->
            <ResourceDictionary Source="/AvalonDock.Themes.VS2013;component/DarkBrushs.xaml" />
            <!-- Then override with custom brushes (order matters - last wins) -->
            <ResourceDictionary Source="Themes/CustomAvalonDockBrushes.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Runtime Theme Switching

```csharp
public void SetTheme(string themeName)
{
    switch (themeName)
    {
        case "Dark":
            dockingManager.Theme = new Vs2013DarkTheme();
            break;
        case "Light":
            dockingManager.Theme = new Vs2013LightTheme();
            break;
        case "Blue":
            dockingManager.Theme = new Vs2013BlueTheme();
            break;
        case "Metro":
            dockingManager.Theme = new MetroTheme();
            break;
    }
}
```

### VS2013 Dark Theme Key Color Palette

| Element | Color Code |
|---|---|
| Background | `#2D2D30` |
| Panel Background | `#252526` |
| Active Tab | `#007ACC` |
| Inactive Tab | `#3F3F46` |
| Border | `#3F3F46` |
| Text | `#F1F1F1` |
| Inactive Text | `#D0D0D0` |
| Hover | `#3E3E42` |
| Selection | `#264F78` |

---

## 10. Complete Example - Realistic IDE-Style Docking Layout

### MainWindow.xaml

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="http://schemas.xceed.com/wpf/xaml/avalondock"
        Title="My Application" Height="800" Width="1280"
        WindowStartupLocation="CenterScreen">

    <Grid>
        <!-- Optional: Menu/Toolbar above DockingManager -->
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Menu Bar -->
        <Menu Grid.Row="0">
            <MenuItem Header="_File">
                <MenuItem Header="_New" />
                <MenuItem Header="_Open" />
                <Separator />
                <MenuItem Header="E_xit" />
            </MenuItem>
            <MenuItem Header="_View">
                <MenuItem Header="_Explorer" />
                <MenuItem Header="_Output" />
                <MenuItem Header="_Actions" />
            </MenuItem>
        </Menu>

        <!-- ===================== AVALONDOCK DOCKING MANAGER ===================== -->
        <avalonDock:DockingManager x:Name="dockingManager" Grid.Row="1"
                                    AllowMixedOrientation="True">

            <!-- ===== THEME ===== -->
            <avalonDock:DockingManager.Theme>
                <avalonDock:Vs2013DarkTheme />
            </avalonDock:DockingManager.Theme>

            <!-- ===== LAYOUT ROOT ===== -->
            <avalonDock:LayoutRoot>

                <!-- ===== MAIN HORIZONTAL PANEL (Left | Center | Right) ===== -->
                <avalonDock:LayoutPanel Orientation="Horizontal">

                    <!-- ===== LEFT: Tree View / Explorer Panel ===== -->
                    <avalonDock:LayoutAnchorablePaneGroup DockWidth="260"
                                                          DockMinWidth="180">
                        <avalonDock:LayoutAnchorablePane>
                            <avalonDock:LayoutAnchorable Title="Explorer"
                                                         ContentId="Explorer"
                                                         CanClose="False"
                                                         CanFloat="True"
                                                         CanAutoHide="True"
                                                         CanHide="False"
                                                         AutoHideWidth="260"
                                                         IconSource="/Assets/explorer.png">
                                <TreeView>
                                    <TreeViewItem Header="Solution 'MyApp'" IsExpanded="True">
                                        <TreeViewItem Header="MyApp" IsExpanded="True">
                                            <TreeViewItem Header="Properties" />
                                            <TreeViewItem Header="References" />
                                            <TreeViewItem Header="Models">
                                                <TreeViewItem Header="User.cs" />
                                                <TreeViewItem Header="Product.cs" />
                                            </TreeViewItem>
                                            <TreeViewItem Header="ViewModels">
                                                <TreeViewItem Header="MainViewModel.cs" />
                                                <TreeViewItem Header="EditorViewModel.cs" />
                                            </TreeViewItem>
                                            <TreeViewItem Header="Views">
                                                <TreeViewItem Header="MainWindow.xaml" />
                                                <TreeViewItem Header="EditorView.xaml" />
                                            </TreeViewItem>
                                            <TreeViewItem Header="App.xaml" />
                                        </TreeViewItem>
                                    </TreeViewItem>
                                </TreeView>
                            </avalonDock:LayoutAnchorable>

                            <avalonDock:LayoutAnchorable Title="Toolbox"
                                                         ContentId="Toolbox"
                                                         CanClose="False"
                                                         CanFloat="True"
                                                         CanAutoHide="True"
                                                         AutoHideWidth="260">
                                <ListBox>
                                    <ListBoxItem Content="Button" />
                                    <ListBoxItem Content="TextBox" />
                                    <ListBoxItem Content="ComboBox" />
                                    <ListBoxItem Content="DataGrid" />
                                    <ListBoxItem Content="TreeView" />
                                </ListBox>
                            </avalonDock:LayoutAnchorable>
                        </avalonDock:LayoutAnchorablePane>
                    </avalonDock:LayoutAnchorablePaneGroup>

                    <!-- ===== CENTER: Vertical split (Documents on top, Errors on bottom) ===== -->
                    <avalonDock:LayoutPanel Orientation="Vertical">

                        <!-- ===== CENTER-TOP: Tabbed Document Area ===== -->
                        <avalonDock:LayoutDocumentPaneGroup>
                            <avalonDock:LayoutDocumentPane>
                                <avalonDock:LayoutDocument Title="MainWindow.xaml"
                                                           ContentId="Doc_MainWindow"
                                                           CanClose="True"
                                                           CanFloat="True"
                                                           IsSelected="True">
                                    <TextBox AcceptsReturn="True"
                                             AcceptsTab="True"
                                             FontFamily="Consolas"
                                             FontSize="13"
                                             Background="#1E1E1E"
                                             Foreground="#D4D4D4"
                                             Text="&lt;Window x:Class='MyApp.MainWindow'&#x0a;        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'&gt;&#x0a;    &lt;Grid&gt;&#x0a;        &lt;!-- Content here --&gt;&#x0a;    &lt;/Grid&gt;&#x0a;&lt;/Window&gt;" />
                                </avalonDock:LayoutDocument>

                                <avalonDock:LayoutDocument Title="MainViewModel.cs"
                                                           ContentId="Doc_MainViewModel"
                                                           CanClose="True"
                                                           CanFloat="True">
                                    <TextBox AcceptsReturn="True"
                                             AcceptsTab="True"
                                             FontFamily="Consolas"
                                             FontSize="13"
                                             Background="#1E1E1E"
                                             Foreground="#D4D4D4"
                                             Text="public class MainViewModel : ObservableObject&#x0a;{&#x0a;    // ViewModel code here&#x0a;}" />
                                </avalonDock:LayoutDocument>

                                <avalonDock:LayoutDocument Title="EditorView.xaml"
                                                           ContentId="Doc_EditorView"
                                                           CanClose="True"
                                                           CanFloat="True">
                                    <TextBlock Text="EditorView content placeholder"
                                               Padding="10"
                                               Foreground="#D4D4D4" />
                                </avalonDock:LayoutDocument>
                            </avalonDock:LayoutDocumentPane>
                        </avalonDock:LayoutDocumentPaneGroup>

                        <!-- ===== BOTTOM: Error List / Output / Log Panels ===== -->
                        <avalonDock:LayoutAnchorablePaneGroup DockHeight="200"
                                                              DockMinHeight="100">
                            <avalonDock:LayoutAnchorablePane>
                                <avalonDock:LayoutAnchorable Title="Error List"
                                                             ContentId="ErrorList"
                                                             CanClose="False"
                                                             CanFloat="True"
                                                             CanAutoHide="True"
                                                             AutoHideHeight="200"
                                                             IsSelected="True">
                                    <DataGrid AutoGenerateColumns="False"
                                              CanUserAddRows="False"
                                              HeadersVisibility="Column"
                                              GridLinesVisibility="None"
                                              Background="#252526"
                                              Foreground="#D4D4D4">
                                        <DataGrid.Columns>
                                            <DataGridTextColumn Header="Level" Width="80" />
                                            <DataGridTextColumn Header="Code" Width="80" />
                                            <DataGridTextColumn Header="Description" Width="*" />
                                            <DataGridTextColumn Header="File" Width="200" />
                                            <DataGridTextColumn Header="Line" Width="60" />
                                        </DataGrid.Columns>
                                    </DataGrid>
                                </avalonDock:LayoutAnchorable>

                                <avalonDock:LayoutAnchorable Title="Output"
                                                             ContentId="Output"
                                                             CanClose="False"
                                                             CanFloat="True"
                                                             CanAutoHide="True"
                                                             AutoHideHeight="200">
                                    <TextBox IsReadOnly="True"
                                             AcceptsReturn="True"
                                             VerticalScrollBarVisibility="Auto"
                                             FontFamily="Consolas"
                                             FontSize="12"
                                             Background="#1E1E1E"
                                             Foreground="#D4D4D4"
                                             Text="Build started...&#x0a;Restoring packages...&#x0a;Build succeeded. 0 Errors, 0 Warnings." />
                                </avalonDock:LayoutAnchorable>

                                <avalonDock:LayoutAnchorable Title="Terminal"
                                                             ContentId="Terminal"
                                                             CanClose="False"
                                                             CanFloat="True"
                                                             CanAutoHide="True"
                                                             AutoHideHeight="200">
                                    <TextBox AcceptsReturn="True"
                                             FontFamily="Consolas"
                                             FontSize="12"
                                             Background="#1E1E1E"
                                             Foreground="#00FF00"
                                             Text="PS C:\MyApp&gt; " />
                                </avalonDock:LayoutAnchorable>
                            </avalonDock:LayoutAnchorablePane>
                        </avalonDock:LayoutAnchorablePaneGroup>

                    </avalonDock:LayoutPanel>
                    <!-- END CENTER VERTICAL PANEL -->

                    <!-- ===== RIGHT: Action Buttons / Properties Panel ===== -->
                    <avalonDock:LayoutAnchorablePaneGroup DockWidth="240"
                                                          DockMinWidth="160">
                        <avalonDock:LayoutAnchorablePane>
                            <avalonDock:LayoutAnchorable Title="Actions"
                                                         ContentId="Actions"
                                                         CanClose="False"
                                                         CanFloat="True"
                                                         CanAutoHide="True"
                                                         AutoHideWidth="240"
                                                         IsSelected="True">
                                <StackPanel Margin="8">
                                    <TextBlock Text="Quick Actions"
                                               FontWeight="Bold"
                                               FontSize="14"
                                               Foreground="#D4D4D4"
                                               Margin="0,0,0,10" />
                                    <Button Content="Build Solution"
                                            Margin="0,0,0,4"
                                            Padding="8,6" />
                                    <Button Content="Run Tests"
                                            Margin="0,0,0,4"
                                            Padding="8,6" />
                                    <Button Content="Deploy"
                                            Margin="0,0,0,4"
                                            Padding="8,6" />
                                    <Button Content="Clean Solution"
                                            Margin="0,0,0,4"
                                            Padding="8,6" />
                                    <Separator Margin="0,8" />
                                    <Button Content="Git Commit"
                                            Margin="0,0,0,4"
                                            Padding="8,6" />
                                    <Button Content="Git Push"
                                            Margin="0,0,0,4"
                                            Padding="8,6" />
                                </StackPanel>
                            </avalonDock:LayoutAnchorable>

                            <avalonDock:LayoutAnchorable Title="Properties"
                                                         ContentId="Properties"
                                                         CanClose="False"
                                                         CanFloat="True"
                                                         CanAutoHide="True"
                                                         AutoHideWidth="240">
                                <StackPanel Margin="8">
                                    <TextBlock Text="Properties"
                                               FontWeight="Bold"
                                               FontSize="14"
                                               Foreground="#D4D4D4"
                                               Margin="0,0,0,10" />
                                    <TextBlock Text="Name:" Foreground="#999" />
                                    <TextBox Text="MainWindow" Margin="0,2,0,8" />
                                    <TextBlock Text="Width:" Foreground="#999" />
                                    <TextBox Text="1280" Margin="0,2,0,8" />
                                    <TextBlock Text="Height:" Foreground="#999" />
                                    <TextBox Text="800" Margin="0,2,0,8" />
                                </StackPanel>
                            </avalonDock:LayoutAnchorable>
                        </avalonDock:LayoutAnchorablePane>
                    </avalonDock:LayoutAnchorablePaneGroup>

                </avalonDock:LayoutPanel>
                <!-- END MAIN HORIZONTAL PANEL -->

                <!-- ===== AUTO-HIDE SIDE ANCHORS (Optional) ===== -->
                <!-- Items placed in LeftSide/RightSide/TopSide/BottomSide start auto-hidden -->
                <avalonDock:LayoutRoot.LeftSide>
                    <avalonDock:LayoutAnchorSide>
                        <avalonDock:LayoutAnchorGroup>
                            <!-- Auto-hidden panels go here -->
                        </avalonDock:LayoutAnchorGroup>
                    </avalonDock:LayoutAnchorSide>
                </avalonDock:LayoutRoot.LeftSide>

                <avalonDock:LayoutRoot.RightSide>
                    <avalonDock:LayoutAnchorSide>
                        <avalonDock:LayoutAnchorGroup>
                            <!-- Auto-hidden panels go here -->
                        </avalonDock:LayoutAnchorGroup>
                    </avalonDock:LayoutAnchorSide>
                </avalonDock:LayoutRoot.RightSide>

                <avalonDock:LayoutRoot.BottomSide>
                    <avalonDock:LayoutAnchorSide>
                        <avalonDock:LayoutAnchorGroup>
                            <!-- Auto-hidden panels go here -->
                        </avalonDock:LayoutAnchorGroup>
                    </avalonDock:LayoutAnchorSide>
                </avalonDock:LayoutRoot.BottomSide>

            </avalonDock:LayoutRoot>

        </avalonDock:DockingManager>

        <!-- Status Bar -->
        <StatusBar Grid.Row="2">
            <StatusBarItem Content="Ready" />
        </StatusBar>

    </Grid>
</Window>
```

### Visual Layout Diagram

```
+----------------------------------------------------------------------+
| Menu Bar                                                             |
+----------+-----------------------------------------+---------+-------+
|          |                                         |         |
| Explorer | MainWindow.xaml | MainViewModel.cs | Ed |  Actions|
| ---------|                                         | --------|
| [Tree]   |                                         |  [Build]|
|  Solution|    (Document Content Area)              |  [Test] |
|   MyApp  |                                         |  [Deploy|
|    Models|                                         |         |
|    Views |                                         | Properti|
|    ...   |                                         |  [Props]|
|          |                                         |         |
| Toolbox  |                                         |         |
+----------+-----------------------------------------+---------+
|          | Error List | Output | Terminal |                   |
|          |------------------------------------------+         |
|          | Level | Code | Description | File | Line|         |
|          |                                         |         |
+----------+-----------------------------------------+---------+
| Status Bar: Ready                                            |
+--------------------------------------------------------------+
```

---

## Appendix A: Common Layout Patterns

### Two-Column (Left Tools + Center Documents)

```xml
<avalonDock:LayoutRoot>
    <avalonDock:LayoutPanel Orientation="Horizontal">
        <avalonDock:LayoutAnchorablePane DockWidth="250">
            <avalonDock:LayoutAnchorable Title="Explorer" ContentId="Explorer" />
        </avalonDock:LayoutAnchorablePane>
        <avalonDock:LayoutDocumentPane />
    </avalonDock:LayoutPanel>
</avalonDock:LayoutRoot>
```

### Three-Column (Left + Center + Right)

```xml
<avalonDock:LayoutRoot>
    <avalonDock:LayoutPanel Orientation="Horizontal">
        <avalonDock:LayoutAnchorablePane DockWidth="200">
            <avalonDock:LayoutAnchorable Title="Navigator" ContentId="Nav" />
        </avalonDock:LayoutAnchorablePane>
        <avalonDock:LayoutDocumentPane />
        <avalonDock:LayoutAnchorablePane DockWidth="250">
            <avalonDock:LayoutAnchorable Title="Properties" ContentId="Props" />
        </avalonDock:LayoutAnchorablePane>
    </avalonDock:LayoutPanel>
</avalonDock:LayoutRoot>
```

### L-Shaped (Left + Center/Bottom)

```xml
<avalonDock:LayoutRoot>
    <avalonDock:LayoutPanel Orientation="Horizontal">
        <avalonDock:LayoutAnchorablePane DockWidth="250">
            <avalonDock:LayoutAnchorable Title="Explorer" ContentId="Explorer" />
        </avalonDock:LayoutAnchorablePane>
        <avalonDock:LayoutPanel Orientation="Vertical">
            <avalonDock:LayoutDocumentPane />
            <avalonDock:LayoutAnchorablePane DockHeight="200">
                <avalonDock:LayoutAnchorable Title="Output" ContentId="Output" />
            </avalonDock:LayoutAnchorablePane>
        </avalonDock:LayoutPanel>
    </avalonDock:LayoutPanel>
</avalonDock:LayoutRoot>
```

### Full IDE Layout (Left + Center/Bottom + Right)

```xml
<avalonDock:LayoutRoot>
    <avalonDock:LayoutPanel Orientation="Horizontal">
        <avalonDock:LayoutAnchorablePaneGroup DockWidth="250">
            <avalonDock:LayoutAnchorablePane>
                <avalonDock:LayoutAnchorable Title="Explorer" ContentId="Explorer" />
                <avalonDock:LayoutAnchorable Title="Search" ContentId="Search" />
            </avalonDock:LayoutAnchorablePane>
        </avalonDock:LayoutAnchorablePaneGroup>
        <avalonDock:LayoutPanel Orientation="Vertical">
            <avalonDock:LayoutDocumentPaneGroup>
                <avalonDock:LayoutDocumentPane />
            </avalonDock:LayoutDocumentPaneGroup>
            <avalonDock:LayoutAnchorablePaneGroup DockHeight="200">
                <avalonDock:LayoutAnchorablePane>
                    <avalonDock:LayoutAnchorable Title="Errors" ContentId="Errors" />
                    <avalonDock:LayoutAnchorable Title="Output" ContentId="Output" />
                    <avalonDock:LayoutAnchorable Title="Terminal" ContentId="Terminal" />
                </avalonDock:LayoutAnchorablePane>
            </avalonDock:LayoutAnchorablePaneGroup>
        </avalonDock:LayoutPanel>
        <avalonDock:LayoutAnchorablePaneGroup DockWidth="240">
            <avalonDock:LayoutAnchorablePane>
                <avalonDock:LayoutAnchorable Title="Properties" ContentId="Props" />
            </avalonDock:LayoutAnchorablePane>
        </avalonDock:LayoutAnchorablePaneGroup>
    </avalonDock:LayoutPanel>
</avalonDock:LayoutRoot>
```

---

## Appendix B: Quick XAML Snippet Templates

### Minimal DockingManager (Copy-Paste Starter)

```xml
<avalonDock:DockingManager>
    <avalonDock:DockingManager.Theme>
        <avalonDock:Vs2013DarkTheme />
    </avalonDock:DockingManager.Theme>
    <avalonDock:LayoutRoot>
        <avalonDock:LayoutPanel Orientation="Horizontal">
            <avalonDock:LayoutDocumentPane />
        </avalonDock:LayoutPanel>
    </avalonDock:LayoutRoot>
</avalonDock:DockingManager>
```

### LayoutAnchorable Template

```xml
<avalonDock:LayoutAnchorable Title="Panel Name"
                             ContentId="UniqueId"
                             CanClose="False"
                             CanFloat="True"
                             CanHide="True"
                             CanAutoHide="True"
                             AutoHideWidth="250">
    <!-- Content here -->
</avalonDock:LayoutAnchorable>
```

### LayoutDocument Template

```xml
<avalonDock:LayoutDocument Title="Document Name"
                           ContentId="Doc_UniqueId"
                           CanClose="True"
                           CanFloat="True"
                           IsSelected="True">
    <!-- Content here -->
</avalonDock:LayoutDocument>
```

---

## Appendix C: Gotchas and Tips

1. **LayoutRoot is singular**: There can only be one `LayoutRoot` inside a `DockingManager`. It is the direct content, not wrapped in `DockingManager.Layout` property element (though `<avalonDock:DockingManager.Layout>` wrapper also works).

2. **DockWidth/DockHeight use GridLength**: Values can be `200` (fixed pixels), `*` (fill), or `2*` (proportional). They behave like Grid column/row definitions.

3. **ContentId is essential for serialization**: Always set `ContentId` on every `LayoutAnchorable` and `LayoutDocument` if you plan to save/restore layouts.

4. **CanHide vs CanClose**: `CanHide` controls the auto-hide pin button. `CanClose` controls the X close button. Setting `CanClose="False"` and `CanHide="False"` makes a panel permanently visible.

5. **Theme must be set on DockingManager**: Themes are set via `DockingManager.Theme` property, not via App.xaml ResourceDictionary merge alone. The ResourceDictionary merge provides brush overrides.

6. **Floating windows are separate OS windows**: Floating panels become top-level windows. This can cause issues with single-window application requirements.

7. **MVVM pane placement**: When using `DocumentsSource` / `AnchorablesSource`, the initial pane placement is defined by the `LayoutRoot` structure in XAML. AvalonDock places items into the first available matching pane type.

8. **LayoutAnchorable in DocumentPane**: A `LayoutAnchorable` can be placed inside a `LayoutDocumentPane` (it appears as a tab alongside documents). A `LayoutDocument` can only be in a `LayoutDocumentPane`.

9. **Nested LayoutPanel**: You can nest `LayoutPanel` elements with alternating `Orientation` to create complex grid-like layouts (Horizontal > Vertical > Horizontal).

10. **Auto-hide side anchors**: To start a panel in auto-hidden state, place it inside `LayoutRoot.LeftSide`, `RightSide`, `TopSide`, or `BottomSide` within a `LayoutAnchorSide > LayoutAnchorGroup` structure.
