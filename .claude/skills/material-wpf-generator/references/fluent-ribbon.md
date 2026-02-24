# Fluent.Ribbon Reference -- Material Design WPF (.NET 8)

## 1. NuGet Package

| Package | Version | Target | Dependencies |
|---------|---------|--------|-------------|
| `Fluent.Ribbon` | **11.0.2** (latest) | .NET 6.0+ (compatible with .NET 8) | ControlzEx >= 7.0.3, System.Memory >= 4.5.5, System.Text.Json >= 8.0.5 |

```xml
<PackageReference Include="Fluent.Ribbon" Version="11.*" />
```

---

## 2. XMLNS Namespace

```xml
xmlns:Fluent="urn:fluent-ribbon"
```

Alternative (CLR-based, equivalent):
```xml
xmlns:Fluent="clr-namespace:Fluent;assembly=Fluent"
```

**Always use `urn:fluent-ribbon`** -- it is the canonical namespace URI.

---

## 3. App.xaml Setup

### Fluent.Ribbon Only

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### Combined with MaterialDesignInXamlToolkit (Standard Pattern)

```xml
<Application x:Class="RswareDesign.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- MaterialDesign Theme (FIRST) -->
                <materialDesign:BundledTheme
                    BaseTheme="Dark"
                    PrimaryColor="Blue"
                    SecondaryColor="Orange" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign3.Defaults.xaml" />

                <!-- Fluent.Ribbon Theme (AFTER MaterialDesign) -->
                <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />

                <!-- Custom Theme Tokens -->
                <ResourceDictionary Source="Themes/SharedTokens.xaml" />
                <ResourceDictionary Source="Themes/DarkTheme.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Order matters**: MaterialDesign dictionaries first, then Fluent.Ribbon Generic.xaml, then custom overrides.

---

## 4. Theme Integration

### 4.1 Built-in Themes

Fluent.Ribbon uses ControlzEx `ThemeManager`. Theme names follow the pattern `{BaseColor}.{ColorScheme}`.

**Base colors**: `Light`, `Dark`

**Color schemes**: Red, Green, Blue, Purple, Orange, Lime, Emerald, Teal, Cyan, Cobalt, Indigo, Violet, Pink, Magenta, Crimson, Amber, Yellow, Brown, Olive, Steel, Mauve, Taupe, Sienna

**Example theme names**: `Dark.Blue`, `Dark.Cobalt`, `Dark.Steel`, `Light.Blue`, `Light.Emerald`

### 4.2 Applying a Specific Theme via XAML

Add a theme XAML after Generic.xaml:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />
    <!-- Apply Dark Cobalt theme -->
    <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Themes/Dark.Cobalt.xaml" />
</ResourceDictionary.MergedDictionaries>
```

### 4.3 Changing Theme in Code-Behind

```csharp
using ControlzEx.Theming;

// Change entire application theme.
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Cobalt");

// Change just the base color (Dark/Light) while keeping the accent.
ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");

// Change just the color scheme while keeping the base.
ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Steel");
```

### 4.4 Coordinating with MaterialDesign Themes

MaterialDesign and Fluent.Ribbon manage themes independently. To synchronize Dark/Light switching:

```csharp
using ControlzEx.Theming;
using MaterialDesignThemes.Wpf;

public void SetDarkTheme()
{
    // MaterialDesign.
    var paletteHelper = new PaletteHelper();
    var theme = paletteHelper.GetTheme();
    theme.SetBaseTheme(BaseTheme.Dark);
    paletteHelper.SetTheme(theme);

    // Fluent.Ribbon.
    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");
}

public void SetLightTheme()
{
    var paletteHelper = new PaletteHelper();
    var theme = paletteHelper.GetTheme();
    theme.SetBaseTheme(BaseTheme.Light);
    paletteHelper.SetTheme(theme);

    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Light");
}
```

### 4.5 Custom Theme Resource Keys

Override these keys in a custom ResourceDictionary to customize ribbon colors:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:options="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options">

    <!-- Accent colors (primary ribbon accent) -->
    <Color x:Key="Fluent.Ribbon.Colors.AccentBaseColor">#FF1565C0</Color>
    <Color x:Key="Fluent.Ribbon.Colors.AccentColor80">#CC1565C0</Color>
    <Color x:Key="Fluent.Ribbon.Colors.AccentColor60">#991565C0</Color>
    <Color x:Key="Fluent.Ribbon.Colors.AccentColor40">#661565C0</Color>
    <Color x:Key="Fluent.Ribbon.Colors.AccentColor20">#331565C0</Color>

    <!-- Brushes referencing those colors -->
    <SolidColorBrush x:Key="Fluent.Ribbon.Brushes.AccentBase"
                     Color="{StaticResource Fluent.Ribbon.Colors.AccentBaseColor}"
                     options:Freeze="True" />
    <SolidColorBrush x:Key="Fluent.Ribbon.Brushes.Accent80"
                     Color="{StaticResource Fluent.Ribbon.Colors.AccentColor80}"
                     options:Freeze="True" />
    <SolidColorBrush x:Key="Fluent.Ribbon.Brushes.Accent60"
                     Color="{StaticResource Fluent.Ribbon.Colors.AccentColor60}"
                     options:Freeze="True" />
    <SolidColorBrush x:Key="Fluent.Ribbon.Brushes.Accent40"
                     Color="{StaticResource Fluent.Ribbon.Colors.AccentColor40}"
                     options:Freeze="True" />
    <SolidColorBrush x:Key="Fluent.Ribbon.Brushes.Accent20"
                     Color="{StaticResource Fluent.Ribbon.Colors.AccentColor20}"
                     options:Freeze="True" />

    <!-- Additional overridable keys (from Theme.Template.xaml): -->
    <!-- Fluent.Ribbon.Colors.HighlightColor -->
    <!-- Fluent.Ribbon.Colors.IdealForegroundColor -->
    <!-- Fluent.Ribbon.Brushes.RibbonTabItem.Active.Background -->
    <!-- Fluent.Ribbon.Brushes.RibbonTabItem.MouseOver.Background -->
    <!-- See: github.com/fluentribbon/Fluent.Ribbon/blob/develop/Fluent.Ribbon/Themes/Themes/Theme.Template.xaml -->
</ResourceDictionary>
```

---

## 5. Window Setup

### 5.1 Using Fluent:RibbonWindow (Fluent-managed titlebar)

```xml
<Fluent:RibbonWindow x:Class="RswareDesign.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:Fluent="urn:fluent-ribbon"
        Title="RswareDesign" Width="1920" Height="1080">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <Fluent:Ribbon Grid.Row="0">
            <!-- Ribbon content -->
        </Fluent:Ribbon>

        <!-- Main content area -->
        <ContentControl Grid.Row="1" />

        <!-- Status bar -->
        <Fluent:StatusBar Grid.Row="2" />
    </Grid>
</Fluent:RibbonWindow>
```

**Code-behind** for RibbonWindow:
```csharp
public partial class MainWindow : Fluent.RibbonWindow
{
    public MainWindow() { InitializeComponent(); }
}
```

### 5.2 Using Standard Window with DockPanel (MaterialDesign + Fluent.Ribbon)

When combining with MaterialDesign and AvalonDock, use a standard `Window` instead of `RibbonWindow`:

```xml
<Window x:Class="RswareDesign.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
        xmlns:Fluent="urn:fluent-ribbon"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        Title="{DynamicResource loc.app.title}"
        Width="1920" Height="1080"
        WindowState="Maximized"
        Background="{DynamicResource BackgroundBrush}">

    <DockPanel LastChildFill="True">
        <!-- Ribbon (top) -->
        <Fluent:Ribbon DockPanel.Dock="Top">
            <!-- ... -->
        </Fluent:Ribbon>

        <!-- StatusBar (bottom) -->
        <Fluent:StatusBar DockPanel.Dock="Bottom">
            <!-- ... -->
        </Fluent:StatusBar>

        <!-- AvalonDock (center fill) -->
        <avalonDock:DockingManager>
            <!-- ... -->
        </avalonDock:DockingManager>
    </DockPanel>
</Window>
```

---

## 6. Ribbon Control

### 6.1 Fluent:Ribbon -- Main Container

```xml
<Fluent:Ribbon x:Name="ribMain"
               AutomaticStateManagement="True"
               IsMinimized="False">

    <!-- Quick Access Toolbar -->
    <Fluent:Ribbon.QuickAccessItems>
        <Fluent:QuickAccessMenuItem IsChecked="True">
            <Fluent:Button Header="{DynamicResource loc.common.save}"
                           Icon="{materialDesign:PackIcon Kind=ContentSave, Size=16}"
                           Command="{Binding SaveCommand}" />
        </Fluent:QuickAccessMenuItem>
        <Fluent:QuickAccessMenuItem IsChecked="True">
            <Fluent:Button Header="{DynamicResource loc.common.undo}"
                           Icon="{materialDesign:PackIcon Kind=Undo, Size=16}"
                           Command="{Binding UndoCommand}" />
        </Fluent:QuickAccessMenuItem>
    </Fluent:Ribbon.QuickAccessItems>

    <!-- Backstage Menu (File button) -->
    <Fluent:Ribbon.Menu>
        <Fluent:Backstage>
            <!-- ... -->
        </Fluent:Backstage>
    </Fluent:Ribbon.Menu>

    <!-- Tabs -->
    <Fluent:RibbonTabItem Header="{DynamicResource loc.menu.file}">
        <!-- ... -->
    </Fluent:RibbonTabItem>
    <Fluent:RibbonTabItem Header="{DynamicResource loc.menu.tools}">
        <!-- ... -->
    </Fluent:RibbonTabItem>

</Fluent:Ribbon>
```

**Key Ribbon properties**:
| Property | Type | Description |
|----------|------|-------------|
| `AutomaticStateManagement` | bool | Saves/restores ribbon state (minimized, etc.) |
| `IsMinimized` | bool | Controls whether ribbon is collapsed |
| `CanMinimize` | bool | Allows user to minimize the ribbon |
| `IsQuickAccessToolBarVisible` | bool | Shows/hides the quick access toolbar |

---

## 7. RibbonTabItem -- Tab Pages

```xml
<Fluent:RibbonTabItem Header="{DynamicResource loc.menu.connection}"
                       x:Name="rtiConnection"
                       IsSelected="True"
                       KeyTip="C">
    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.connection.serial}">
        <!-- Controls here -->
    </Fluent:RibbonGroupBox>
    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.connection.actions}">
        <!-- Controls here -->
    </Fluent:RibbonGroupBox>
</Fluent:RibbonTabItem>
```

**Key RibbonTabItem properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Tab label text |
| `IsSelected` | bool | Whether this tab is currently selected |
| `KeyTip` | string | Keyboard shortcut character shown in KeyTip mode |
| `Group` | RibbonContextualTabGroup | For contextual (colored) tabs |

### Contextual Tabs

```xml
<Fluent:Ribbon>
    <Fluent:Ribbon.ContextualGroups>
        <Fluent:RibbonContextualTabGroup x:Name="ctgDriveTools"
                                          Header="Drive Tools"
                                          Visibility="Collapsed"
                                          Background="Green" />
    </Fluent:Ribbon.ContextualGroups>

    <!-- Normal tabs -->
    <Fluent:RibbonTabItem Header="Home" />

    <!-- Contextual tab (only visible when ctgDriveTools.Visibility = Visible) -->
    <Fluent:RibbonTabItem Header="Drive Settings"
                           Group="{Binding ElementName=ctgDriveTools}" />
</Fluent:Ribbon>
```

---

## 8. RibbonGroupBox -- Groups Within Tabs

```xml
<Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.connection.serial}"
                        x:Name="rgbConnection"
                        Icon="{materialDesign:PackIcon Kind=SerialPort, Size=16}"
                        IsLauncherVisible="True"
                        LauncherCommand="{Binding ShowSerialSettingsCommand}">

    <!-- Controls here -->

</Fluent:RibbonGroupBox>
```

**Key RibbonGroupBox properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Header` | string | Group header text shown below the group |
| `Icon` | object | Icon shown when the group is collapsed |
| `IsLauncherVisible` | bool | Shows the small launcher button at bottom-right |
| `LauncherCommand` | ICommand | Command executed when launcher button is clicked |
| `LauncherToolTip` | string | Tooltip for the launcher button |

---

## 9. Button Controls

### 9.1 Fluent:Button -- Standard Ribbon Button

```xml
<!-- Large button (default) -->
<Fluent:Button Header="{DynamicResource loc.menu.file.open}"
               x:Name="rbnOpen"
               LargeIcon="{materialDesign:PackIcon Kind=FolderOpen, Size=32}"
               Icon="{materialDesign:PackIcon Kind=FolderOpen, Size=16}"
               SizeDefinition="Large"
               KeyTip="O"
               Command="{Binding OpenCommand}" />

<!-- Medium button -->
<Fluent:Button Header="{DynamicResource loc.menu.file.save}"
               x:Name="rbnSave"
               Icon="{materialDesign:PackIcon Kind=ContentSave, Size=16}"
               SizeDefinition="Middle"
               Command="{Binding SaveCommand}" />

<!-- Small button -->
<Fluent:Button Header="{DynamicResource loc.menu.file.close}"
               x:Name="rbnClose"
               Icon="{materialDesign:PackIcon Kind=Close, Size=16}"
               SizeDefinition="Small"
               Command="{Binding CloseCommand}" />
```

**Key Button properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Button label text |
| `Icon` | object | Small icon (16x16), used in Medium/Small modes |
| `LargeIcon` | object | Large icon (32x32), used in Large mode |
| `SizeDefinition` | string | Size mode: `"Large"`, `"Middle"`, `"Small"` or 3-value |
| `Command` | ICommand | MVVM command binding |
| `CommandParameter` | object | Parameter for the command |
| `KeyTip` | string | Keyboard shortcut hint |
| `IsEnabled` | bool | Enable/disable state |
| `ToolTip` | object | Tooltip (can be a ScreenTip) |
| `Size` | RibbonControlSize | Explicit size (`Large`, `Middle`, `Small`) |

### 9.2 Fluent:ToggleButton -- For Panel Toggles

```xml
<Fluent:ToggleButton Header="{DynamicResource loc.menu.views.driveTree}"
                      x:Name="rtgDriveTree"
                      LargeIcon="{materialDesign:PackIcon Kind=FileTree, Size=32}"
                      Icon="{materialDesign:PackIcon Kind=FileTree, Size=16}"
                      SizeDefinition="Large"
                      IsChecked="{Binding IsDriveTreeVisible, Mode=TwoWay}"
                      KeyTip="T" />

<Fluent:ToggleButton Header="{DynamicResource loc.menu.views.errorLog}"
                      x:Name="rtgErrorLog"
                      Icon="{materialDesign:PackIcon Kind=AlertCircle, Size=16}"
                      SizeDefinition="Middle"
                      IsChecked="{Binding IsErrorLogVisible, Mode=TwoWay}" />
```

**Additional ToggleButton properties**:
| Property | Type | Description |
|----------|------|-------------|
| `IsChecked` | bool? | Toggle state (use TwoWay binding) |
| `GroupName` | string | Radio-button grouping (only one checked in group) |

### 9.3 Fluent:DropDownButton -- Submenu Button

```xml
<Fluent:DropDownButton Header="{DynamicResource loc.menu.file.export}"
                        x:Name="rbnExport"
                        LargeIcon="{materialDesign:PackIcon Kind=Export, Size=32}"
                        Icon="{materialDesign:PackIcon Kind=Export, Size=16}"
                        SizeDefinition="Large"
                        KeyTip="E">
    <Fluent:MenuItem Header="{DynamicResource loc.menu.file.export.csv}"
                      Icon="{materialDesign:PackIcon Kind=FileDelimited, Size=16}"
                      Command="{Binding ExportCsvCommand}" />
    <Fluent:MenuItem Header="{DynamicResource loc.menu.file.export.excel}"
                      Icon="{materialDesign:PackIcon Kind=MicrosoftExcel, Size=16}"
                      Command="{Binding ExportExcelCommand}" />
    <Separator />
    <Fluent:MenuItem Header="{DynamicResource loc.menu.file.export.pdf}"
                      Icon="{materialDesign:PackIcon Kind=FilePdfBox, Size=16}"
                      Command="{Binding ExportPdfCommand}" />
</Fluent:DropDownButton>
```

**Important**: Use `Fluent:MenuItem` (not `System.Windows.Controls.MenuItem`) as children of `DropDownButton` and `SplitButton`.

### 9.4 Fluent:SplitButton -- Button with Dropdown

```xml
<Fluent:SplitButton Header="{DynamicResource loc.menu.file.save}"
                     x:Name="rbnSaveSplit"
                     LargeIcon="{materialDesign:PackIcon Kind=ContentSave, Size=32}"
                     Icon="{materialDesign:PackIcon Kind=ContentSave, Size=16}"
                     SizeDefinition="Large"
                     Command="{Binding SaveCommand}"
                     KeyTip="S">
    <!-- Clicking the button itself executes SaveCommand -->
    <!-- Clicking the arrow shows this dropdown -->
    <Fluent:MenuItem Header="{DynamicResource loc.menu.file.save}"
                      Icon="{materialDesign:PackIcon Kind=ContentSave, Size=16}"
                      Command="{Binding SaveCommand}" />
    <Fluent:MenuItem Header="{DynamicResource loc.menu.file.saveAs}"
                      Icon="{materialDesign:PackIcon Kind=ContentSaveEdit, Size=16}"
                      Command="{Binding SaveAsCommand}" />
    <Fluent:MenuItem Header="{DynamicResource loc.menu.file.saveAll}"
                      Icon="{materialDesign:PackIcon Kind=ContentSaveAll, Size=16}"
                      Command="{Binding SaveAllCommand}" />
</Fluent:SplitButton>
```

### 9.5 Data-Bound DropDownButton

```xml
<Fluent:DropDownButton Header="{DynamicResource loc.menu.file.recent}"
                        LargeIcon="{materialDesign:PackIcon Kind=History, Size=32}"
                        ItemsSource="{Binding RecentFiles}">
    <Fluent:DropDownButton.ItemTemplate>
        <DataTemplate>
            <Fluent:MenuItem Header="{Binding FileName}"
                              Icon="{materialDesign:PackIcon Kind=File, Size=16}"
                              Command="{Binding DataContext.OpenRecentCommand,
                                        RelativeSource={RelativeSource AncestorType=Fluent:DropDownButton}}"
                              CommandParameter="{Binding}" />
        </DataTemplate>
    </Fluent:DropDownButton.ItemTemplate>
</Fluent:DropDownButton>
```

---

## 10. Fluent:ComboBox -- Dropdown in Ribbon

```xml
<Fluent:ComboBox Header="{DynamicResource loc.menu.connection.port}"
                  x:Name="rcbPort"
                  ItemsSource="{Binding AvailablePorts}"
                  SelectedItem="{Binding SelectedPort, Mode=TwoWay}"
                  IsEditable="False"
                  InputWidth="120"
                  SizeDefinition="Middle"
                  Icon="{materialDesign:PackIcon Kind=SerialPort, Size=16}" />

<!-- ComboBox with custom ItemTemplate -->
<Fluent:ComboBox Header="{DynamicResource loc.menu.connection.baudrate}"
                  x:Name="rcbBaudRate"
                  ItemsSource="{Binding BaudRates}"
                  SelectedItem="{Binding SelectedBaudRate, Mode=TwoWay}"
                  IsEditable="False"
                  InputWidth="80">
    <Fluent:ComboBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding}" />
        </DataTemplate>
    </Fluent:ComboBox.ItemTemplate>
</Fluent:ComboBox>
```

**Key ComboBox properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Header` | string | Label displayed next to the ComboBox |
| `ItemsSource` | IEnumerable | Data source |
| `SelectedItem` | object | Selected item (TwoWay) |
| `IsEditable` | bool | **Set to False** (default is True, unlike standard WPF) |
| `InputWidth` | double | Width of the input/dropdown area |
| `IsReadOnly` | bool | Prevents user text input |
| `Icon` | object | Icon displayed to the left |

---

## 11. Separator

### Inside RibbonGroupBox

```xml
<Fluent:RibbonGroupBox Header="Connection">
    <Fluent:ComboBox Header="Port" ItemsSource="{Binding Ports}" InputWidth="100" />
    <Fluent:ComboBox Header="Baud" ItemsSource="{Binding BaudRates}" InputWidth="80" />

    <!-- Separator between controls inside a RibbonGroupBox -->
    <Separator Style="{DynamicResource Fluent.Ribbon.Styles.GroupBoxSeparator}" />

    <Fluent:Button Header="Connect" Command="{Binding ConnectCommand}" />
</Fluent:RibbonGroupBox>
```

**Important**: Inside a `RibbonGroupBox`, use `Style="{DynamicResource Fluent.Ribbon.Styles.GroupBoxSeparator}"` on `Separator` for correct rendering.

### Inside DropDown Menus

```xml
<Fluent:DropDownButton Header="Export">
    <Fluent:MenuItem Header="CSV" />
    <Separator />  <!-- Standard separator in menu context -->
    <Fluent:MenuItem Header="PDF" />
</Fluent:DropDownButton>
```

---

## 12. Backstage (File Menu)

```xml
<Fluent:Ribbon.Menu>
    <Fluent:Backstage Header="{DynamicResource loc.menu.file}">
        <Fluent:BackstageTabControl>
            <!-- Tab items (pages) -->
            <Fluent:BackstageTabItem Header="{DynamicResource loc.menu.file.info}"
                                     Icon="{materialDesign:PackIcon Kind=Information, Size=16}"
                                     KeyTip="I">
                <Grid Margin="20">
                    <TextBlock Text="Application Information"
                               Style="{StaticResource MaterialDesignHeadline5TextBlock}" />
                </Grid>
            </Fluent:BackstageTabItem>

            <Fluent:BackstageTabItem Header="{DynamicResource loc.menu.file.new}"
                                     Icon="{materialDesign:PackIcon Kind=FilePlus, Size=16}"
                                     KeyTip="N">
                <Grid Margin="20">
                    <!-- New project content -->
                </Grid>
            </Fluent:BackstageTabItem>

            <Fluent:BackstageTabItem Header="{DynamicResource loc.menu.file.print}"
                                     Icon="{materialDesign:PackIcon Kind=Printer, Size=16}"
                                     KeyTip="P">
                <Grid Margin="20">
                    <!-- Print preview content -->
                </Grid>
            </Fluent:BackstageTabItem>

            <!-- Separator between tabs and action buttons -->
            <Separator />

            <!-- Action buttons (no tab content, just click actions) -->
            <Fluent:Button Header="{DynamicResource loc.menu.file.save}"
                           Icon="{materialDesign:PackIcon Kind=ContentSave, Size=16}"
                           Command="{Binding SaveCommand}"
                           KeyTip="S" />

            <Fluent:Button Header="{DynamicResource loc.menu.file.saveAs}"
                           Icon="{materialDesign:PackIcon Kind=ContentSaveEdit, Size=16}"
                           Command="{Binding SaveAsCommand}" />

            <Separator />

            <Fluent:Button Header="{DynamicResource loc.menu.file.exit}"
                           Icon="{materialDesign:PackIcon Kind=ExitToApp, Size=16}"
                           Command="{Binding ExitCommand}"
                           KeyTip="X" />
        </Fluent:BackstageTabControl>
    </Fluent:Backstage>
</Fluent:Ribbon.Menu>
```

**Key Backstage properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Text on the File button |
| `Background` | Brush | Backstage panel background color |
| `IsOpen` | bool | Controls backstage visibility programmatically |

---

## 13. StatusBar

```xml
<Fluent:StatusBar x:Name="stbMain"
                   Background="{DynamicResource StatusBarBrush}">

    <!-- Left-aligned items -->
    <Fluent:StatusBarItem Title="Connection"
                           Value="{Binding ConnectionStatus}"
                           HorizontalAlignment="Left">
        <StackPanel Orientation="Horizontal">
            <materialDesign:PackIcon Kind="{Binding ConnectionIcon}"
                                      Width="16" Height="16"
                                      Foreground="{DynamicResource StatusBarForeground}" />
            <TextBlock Text="{Binding ConnectionStatus}"
                       Margin="4,0,0,0"
                       Foreground="{DynamicResource StatusBarForeground}" />
        </StackPanel>
    </Fluent:StatusBarItem>

    <Separator HorizontalAlignment="Left" />

    <Fluent:StatusBarItem Title="Drive"
                           Value="{Binding SelectedDriveName}"
                           Content="{Binding SelectedDriveName}"
                           HorizontalAlignment="Left" />

    <!-- Right-aligned items -->
    <Fluent:StatusBarItem Title="Version"
                           Value="{Binding AppVersion}"
                           Content="{Binding AppVersion}"
                           HorizontalAlignment="Right" />

    <Separator HorizontalAlignment="Right" />

    <Fluent:StatusBarItem Title="Progress"
                           HorizontalAlignment="Right">
        <ProgressBar Value="{Binding Progress}"
                     Width="100" Height="14"
                     Visibility="{Binding IsProgressVisible, Converter={StaticResource BoolToVisConverter}}" />
    </Fluent:StatusBarItem>
</Fluent:StatusBar>
```

**Important**: You **must** set `HorizontalAlignment` on every `StatusBarItem` and every `Separator` (either `Left` or `Right`). The ContextMenu for toggling items is generated automatically.

**StatusBarItem properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Title` | string | Header shown in the auto-generated ContextMenu |
| `Value` | string | Value shown in the ContextMenu |
| `Content` | object | Content displayed in the StatusBar |
| `HorizontalAlignment` | Left/Right | **Required** -- determines left or right placement |

---

## 14. SizeDefinition (Sizing System)

### How It Works

Each ribbon control has a `SizeDefinition` property that defines its size at three group states:
1. **Large** state (group at full width)
2. **Middle** state (group partially reduced)
3. **Small** state (group fully reduced)

### Syntax

```
SizeDefinition="LargeState, MiddleState, SmallState"
```

Each value can be: `Large`, `Middle`, or `Small`.

### Shorthand

A single value applies to all three states:
- `SizeDefinition="Large"` equals `"Large, Large, Large"`
- `SizeDefinition="Middle"` equals `"Middle, Middle, Middle"`
- `SizeDefinition="Small"` equals `"Small, Small, Small"`

### Common Patterns

```xml
<!-- Always large (primary actions) -->
<Fluent:Button SizeDefinition="Large, Large, Large"
               Header="Connect"
               LargeIcon="{materialDesign:PackIcon Kind=LanConnect, Size=32}"
               Icon="{materialDesign:PackIcon Kind=LanConnect, Size=16}" />

<!-- Large normally, shrinks to middle then small -->
<Fluent:Button SizeDefinition="Large, Middle, Small"
               Header="Save"
               LargeIcon="{materialDesign:PackIcon Kind=ContentSave, Size=32}"
               Icon="{materialDesign:PackIcon Kind=ContentSave, Size=16}" />

<!-- Medium normally, shrinks to small -->
<Fluent:Button SizeDefinition="Middle, Middle, Small"
               Header="Refresh"
               Icon="{materialDesign:PackIcon Kind=Refresh, Size=16}" />

<!-- Always small (secondary actions) -->
<Fluent:Button SizeDefinition="Small"
               Header="Settings"
               Icon="{materialDesign:PackIcon Kind=Cog, Size=16}" />
```

### Visual Behavior

| Size | Icon | Label | Layout |
|------|------|-------|--------|
| **Large** | LargeIcon (32x32) | Below icon | Vertical, tall button |
| **Middle** | Icon (16x16) | Right of icon | Horizontal, medium button |
| **Small** | Icon (16x16) | Right of icon (may truncate) | Horizontal, compact button |

### Custom Icon Sizes

Override the default icon size with a style:

```xml
<Style x:Key="ExtraLargeIconButton" TargetType="Fluent:Button">
    <Style.Resources>
        <Style TargetType="Fluent:IconPresenter">
            <Setter Property="LargeSize" Value="48,48" />
            <Setter Property="SmallSize" Value="24,24" />
        </Style>
    </Style.Resources>
</Style>

<Fluent:Button Style="{StaticResource ExtraLargeIconButton}"
               LargeIcon="{materialDesign:PackIcon Kind=Monitor, Size=48}"
               Header="Large Monitor" SizeDefinition="Large" />
```

---

## 15. Icon Integration

### 15.1 MaterialDesign PackIcon (Recommended)

```xml
<!-- Using markup extension (inline) -->
<Fluent:Button Header="Save"
               LargeIcon="{materialDesign:PackIcon Kind=ContentSave, Size=32}"
               Icon="{materialDesign:PackIcon Kind=ContentSave, Size=16}" />

<!-- Using element syntax -->
<Fluent:Button Header="Open">
    <Fluent:Button.LargeIcon>
        <materialDesign:PackIcon Kind="FolderOpen" Width="32" Height="32" />
    </Fluent:Button.LargeIcon>
    <Fluent:Button.Icon>
        <materialDesign:PackIcon Kind="FolderOpen" Width="16" Height="16" />
    </Fluent:Button.Icon>
</Fluent:Button>
```

### 15.2 Image File

```xml
<Fluent:Button Header="Logo"
               LargeIcon="/Resources/Icons/logo_32.png"
               Icon="/Resources/Icons/logo_16.png" />

<!-- Or with pack URI -->
<Fluent:Button Header="Logo"
               LargeIcon="pack://application:,,,/Resources/Icons/logo_32.png"
               Icon="pack://application:,,,/Resources/Icons/logo_16.png" />
```

### 15.3 Geometry / Path Icon

```xml
<Fluent:Button Header="Custom">
    <Fluent:Button.LargeIcon>
        <Path Data="M12,2L2,22H22L12,2Z"
              Fill="{DynamicResource PrimaryBrush}"
              Width="32" Height="32" Stretch="Uniform" />
    </Fluent:Button.LargeIcon>
    <Fluent:Button.Icon>
        <Path Data="M12,2L2,22H22L12,2Z"
              Fill="{DynamicResource PrimaryBrush}"
              Width="16" Height="16" Stretch="Uniform" />
    </Fluent:Button.Icon>
</Fluent:Button>
```

### 15.4 Rectangle with VisualBrush (Colorable Vector)

```xml
<Fluent:Button Header="Custom Icon">
    <Fluent:Button.LargeIcon>
        <Rectangle Width="32" Height="32"
                   Fill="{StaticResource MyVectorIconBrush}" />
    </Fluent:Button.LargeIcon>
</Fluent:Button>
```

**Note**: The `Icon` and `LargeIcon` properties accept any `object`. When an `object` is a `string`, it is treated as an image URI. When it is a `UIElement` (PackIcon, Image, Path, Rectangle, etc.), it is rendered directly.

---

## 16. Complete Tab Examples

### File Tab (via Backstage -- see Section 12)

### Tools Tab

```xml
<Fluent:RibbonTabItem Header="{DynamicResource loc.menu.tools}" KeyTip="T">
    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.tools.parameters}">
        <Fluent:Button Header="{DynamicResource loc.menu.tools.readAll}"
                        LargeIcon="{materialDesign:PackIcon Kind=DatabaseArrowDown, Size=32}"
                        Icon="{materialDesign:PackIcon Kind=DatabaseArrowDown, Size=16}"
                        SizeDefinition="Large"
                        Command="{Binding ReadAllParametersCommand}" />
        <Fluent:Button Header="{DynamicResource loc.menu.tools.writeAll}"
                        LargeIcon="{materialDesign:PackIcon Kind=DatabaseArrowUp, Size=32}"
                        Icon="{materialDesign:PackIcon Kind=DatabaseArrowUp, Size=16}"
                        SizeDefinition="Large"
                        Command="{Binding WriteAllParametersCommand}" />
        <Separator Style="{DynamicResource Fluent.Ribbon.Styles.GroupBoxSeparator}" />
        <Fluent:Button Header="{DynamicResource loc.menu.tools.compare}"
                        Icon="{materialDesign:PackIcon Kind=FileCompare, Size=16}"
                        SizeDefinition="Middle"
                        Command="{Binding CompareParametersCommand}" />
    </Fluent:RibbonGroupBox>

    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.tools.firmware}">
        <Fluent:Button Header="{DynamicResource loc.menu.tools.fwUpdate}"
                        LargeIcon="{materialDesign:PackIcon Kind=Update, Size=32}"
                        Icon="{materialDesign:PackIcon Kind=Update, Size=16}"
                        SizeDefinition="Large"
                        Command="{Binding FirmwareUpdateCommand}" />
    </Fluent:RibbonGroupBox>
</Fluent:RibbonTabItem>
```

### Connection Tab

```xml
<Fluent:RibbonTabItem Header="{DynamicResource loc.menu.connection}" KeyTip="C">
    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.connection.serial}">
        <Fluent:ComboBox Header="{DynamicResource loc.menu.connection.port}"
                          x:Name="rcbPort"
                          ItemsSource="{Binding AvailablePorts}"
                          SelectedItem="{Binding SelectedPort, Mode=TwoWay}"
                          IsEditable="False"
                          InputWidth="120"
                          SizeDefinition="Middle" />
        <Fluent:ComboBox Header="{DynamicResource loc.menu.connection.baudrate}"
                          x:Name="rcbBaudRate"
                          ItemsSource="{Binding BaudRates}"
                          SelectedItem="{Binding SelectedBaudRate, Mode=TwoWay}"
                          IsEditable="False"
                          InputWidth="80"
                          SizeDefinition="Middle" />
        <Separator Style="{DynamicResource Fluent.Ribbon.Styles.GroupBoxSeparator}" />
        <Fluent:Button Header="{DynamicResource loc.menu.connection.refresh}"
                        Icon="{materialDesign:PackIcon Kind=Refresh, Size=16}"
                        SizeDefinition="Small"
                        Command="{Binding RefreshPortsCommand}" />
    </Fluent:RibbonGroupBox>

    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.connection.actions}">
        <Fluent:Button Header="{DynamicResource loc.menu.connection.connect}"
                        x:Name="rbnConnect"
                        LargeIcon="{materialDesign:PackIcon Kind=LanConnect, Size=32}"
                        Icon="{materialDesign:PackIcon Kind=LanConnect, Size=16}"
                        SizeDefinition="Large"
                        Command="{Binding ConnectCommand}" />
        <Fluent:Button Header="{DynamicResource loc.menu.connection.disconnect}"
                        x:Name="rbnDisconnect"
                        LargeIcon="{materialDesign:PackIcon Kind=LanDisconnect, Size=32}"
                        Icon="{materialDesign:PackIcon Kind=LanDisconnect, Size=16}"
                        SizeDefinition="Large"
                        Command="{Binding DisconnectCommand}" />
    </Fluent:RibbonGroupBox>
</Fluent:RibbonTabItem>
```

### Options Tab

```xml
<Fluent:RibbonTabItem Header="{DynamicResource loc.menu.options}" KeyTip="O">
    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.options.theme}">
        <Fluent:DropDownButton Header="{DynamicResource loc.menu.options.theme}"
                                LargeIcon="{materialDesign:PackIcon Kind=Palette, Size=32}"
                                Icon="{materialDesign:PackIcon Kind=Palette, Size=16}"
                                SizeDefinition="Large">
            <Fluent:MenuItem Header="{DynamicResource loc.theme.dark}"
                              Icon="{materialDesign:PackIcon Kind=WeatherNight, Size=16}"
                              Command="{Binding SetDarkThemeCommand}" />
            <Fluent:MenuItem Header="{DynamicResource loc.theme.light}"
                              Icon="{materialDesign:PackIcon Kind=WeatherSunny, Size=16}"
                              Command="{Binding SetLightThemeCommand}" />
        </Fluent:DropDownButton>
    </Fluent:RibbonGroupBox>

    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.options.language}">
        <Fluent:DropDownButton Header="{DynamicResource loc.menu.options.language}"
                                LargeIcon="{materialDesign:PackIcon Kind=Translate, Size=32}"
                                Icon="{materialDesign:PackIcon Kind=Translate, Size=16}"
                                SizeDefinition="Large">
            <Fluent:MenuItem Header="Korean"
                              Command="{Binding SetLanguageCommand}"
                              CommandParameter="ko" />
            <Fluent:MenuItem Header="English"
                              Command="{Binding SetLanguageCommand}"
                              CommandParameter="en" />
        </Fluent:DropDownButton>
    </Fluent:RibbonGroupBox>
</Fluent:RibbonTabItem>
```

### Views Tab

```xml
<Fluent:RibbonTabItem Header="{DynamicResource loc.menu.views}" KeyTip="V">
    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.views.panels}">
        <Fluent:ToggleButton Header="{DynamicResource loc.menu.views.driveTree}"
                              x:Name="rtgDriveTree"
                              LargeIcon="{materialDesign:PackIcon Kind=FileTree, Size=32}"
                              Icon="{materialDesign:PackIcon Kind=FileTree, Size=16}"
                              SizeDefinition="Large"
                              IsChecked="{Binding IsDriveTreeVisible, Mode=TwoWay}" />
        <Fluent:ToggleButton Header="{DynamicResource loc.menu.views.paramEditor}"
                              x:Name="rtgParamEditor"
                              LargeIcon="{materialDesign:PackIcon Kind=TableEdit, Size=32}"
                              Icon="{materialDesign:PackIcon Kind=TableEdit, Size=16}"
                              SizeDefinition="Large"
                              IsChecked="{Binding IsParameterEditorVisible, Mode=TwoWay}" />
        <Fluent:ToggleButton Header="{DynamicResource loc.menu.views.monitor}"
                              x:Name="rtgMonitor"
                              LargeIcon="{materialDesign:PackIcon Kind=MonitorDashboard, Size=32}"
                              Icon="{materialDesign:PackIcon Kind=MonitorDashboard, Size=16}"
                              SizeDefinition="Large"
                              IsChecked="{Binding IsMonitorVisible, Mode=TwoWay}" />
    </Fluent:RibbonGroupBox>

    <Fluent:RibbonGroupBox Header="{DynamicResource loc.menu.views.tools}">
        <Fluent:ToggleButton Header="{DynamicResource loc.menu.views.errorLog}"
                              x:Name="rtgErrorLog"
                              Icon="{materialDesign:PackIcon Kind=AlertCircle, Size=16}"
                              SizeDefinition="Middle"
                              IsChecked="{Binding IsErrorLogVisible, Mode=TwoWay}" />
        <Fluent:ToggleButton Header="{DynamicResource loc.menu.views.actionPanel}"
                              x:Name="rtgActionPanel"
                              Icon="{materialDesign:PackIcon Kind=DotsVertical, Size=16}"
                              SizeDefinition="Middle"
                              IsChecked="{Binding IsActionPanelVisible, Mode=TwoWay}" />
    </Fluent:RibbonGroupBox>
</Fluent:RibbonTabItem>
```

---

## 17. ScreenTip (Enhanced Tooltips)

```xml
<Fluent:Button Header="Connect" Command="{Binding ConnectCommand}">
    <Fluent:Button.ToolTip>
        <Fluent:ScreenTip Title="{DynamicResource loc.menu.connection.connect}"
                           Text="{DynamicResource loc.tooltip.connect.description}"
                           Image="/Resources/Images/connect_help.png"
                           HelpTopic="https://docs.example.com/connect"
                           DisableReason="{DynamicResource loc.tooltip.connect.disableReason}"
                           IsRibbonAligned="True" />
    </Fluent:Button.ToolTip>
</Fluent:Button>
```

---

## 18. Quick Reference Table

| Control | XAML Tag | Key Properties |
|---------|----------|---------------|
| Ribbon container | `<Fluent:Ribbon>` | AutomaticStateManagement, IsMinimized |
| Tab | `<Fluent:RibbonTabItem>` | Header, IsSelected, KeyTip, Group |
| Group | `<Fluent:RibbonGroupBox>` | Header, Icon, IsLauncherVisible |
| Button | `<Fluent:Button>` | Header, Icon, LargeIcon, SizeDefinition, Command |
| Toggle | `<Fluent:ToggleButton>` | Header, Icon, LargeIcon, IsChecked, GroupName |
| Split button | `<Fluent:SplitButton>` | Header, Icon, LargeIcon, Command + child MenuItems |
| Dropdown | `<Fluent:DropDownButton>` | Header, Icon, LargeIcon + child MenuItems |
| ComboBox | `<Fluent:ComboBox>` | Header, ItemsSource, SelectedItem, IsEditable, InputWidth |
| Menu item | `<Fluent:MenuItem>` | Header, Icon, Command, CommandParameter |
| Separator | `<Separator>` | Style=GroupBoxSeparator (inside RibbonGroupBox) |
| Backstage | `<Fluent:Backstage>` | Header, Background, IsOpen |
| Backstage tab | `<Fluent:BackstageTabItem>` | Header, Icon, KeyTip |
| StatusBar | `<Fluent:StatusBar>` | Background |
| StatusBar item | `<Fluent:StatusBarItem>` | Title, Value, Content, HorizontalAlignment |
| Quick Access | `<Fluent:QuickAccessMenuItem>` | IsChecked, Target |
| ScreenTip | `<Fluent:ScreenTip>` | Title, Text, Image, HelpTopic |

---

## 19. Sources

- NuGet: https://www.nuget.org/packages/Fluent.Ribbon
- GitHub: https://github.com/fluentribbon/Fluent.Ribbon
- Documentation: https://fluentribbon.github.io/documentation/
- Basic Setup: https://fluentribbon.github.io/documentation/basic-setup
- Sizing: https://fluentribbon.github.io/documentation/concepts/sizing
- Backstage: https://fluentribbon.github.io/documentation/controls/backstage
- StatusBar: https://fluentribbon.github.io/documentation/controls/statusbar-and-statusbaritem
- Themes (v8+): https://fluentribbon.github.io/documentation/styles_since_8
- Theme Template: https://github.com/fluentribbon/Fluent.Ribbon/blob/develop/Fluent.Ribbon/Themes/Themes/Theme.Template.xaml
- ControlzEx ThemeManager: https://github.com/ControlzEx/ControlzEx/blob/develop/Wiki/ThemeManager.md
