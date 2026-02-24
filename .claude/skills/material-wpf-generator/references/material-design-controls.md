# MaterialDesignInXamlToolkit - Comprehensive XAML Reference

> **Version**: 5.3.0 | **Targets**: .NET 8.0+ / .NET Framework 4.6.2+ | **Design Spec**: Material Design 2 & 3

---

## 1. NuGet Packages

| Package | Version | Description |
|---------|---------|-------------|
| `MaterialDesignThemes` | 5.3.0 | Main package: styles, controls, themes |
| `MaterialDesignColors` | 5.3.0 | Color palette definitions (auto-dependency) |
| `MaterialDesignThemes.MahApps` | 5.3.0 | MahApps.Metro integration (optional) |
| `Microsoft.Xaml.Behaviors.Wpf` | >= 1.1.77 | Required dependency (auto-installed) |

### Installation

```xml
<!-- .csproj -->
<PackageReference Include="MaterialDesignThemes" Version="5.3.0" />
```

```
dotnet add package MaterialDesignThemes --version 5.3.0
```

```
Install-Package MaterialDesignThemes -Version 5.3.0
```

---

## 2. App.xaml Setup

### BundledTheme (Predefined Swatches)

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <materialDesign:BundledTheme BaseTheme="Light"
                                             PrimaryColor="DeepPurple"
                                             SecondaryColor="Lime" />
                <!-- Material Design 3 (modern) -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign3.Defaults.xaml" />
                <!-- OR Material Design 2 (classic) -->
                <!-- <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml" /> -->
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### CustomColorTheme (Arbitrary Colors)

```xml
<materialDesign:CustomColorTheme BaseTheme="Light"
                                 PrimaryColor="Aqua"
                                 SecondaryColor="DarkGreen" />
```

### Window Configuration

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
        Style="{StaticResource MaterialDesignWindow}"
        TextElement.Foreground="{DynamicResource MaterialDesignBody}"
        TextElement.FontWeight="Regular"
        TextElement.FontSize="13"
        TextOptions.TextFormattingMode="Ideal"
        TextOptions.TextRenderingMode="Auto"
        Background="{DynamicResource MaterialDesignPaper}"
        FontFamily="{DynamicResource MaterialDesignFont}"
        Title="MainWindow" Height="600" Width="800">
```

---

## 3. XMLNS Namespace Prefixes

```xml
<!-- Primary namespace - controls, attached properties, everything -->
xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"

<!-- Standard WPF namespaces (always needed) -->
xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"

<!-- For Behaviors (dependency of MaterialDesign) -->
xmlns:behaviors="http://schemas.microsoft.com/xaml/behaviors"
```

All Material Design controls, attached properties (HintAssist, TextFieldAssist, ShadowAssist, etc.), and the PackIcon are under the single `materialDesign` namespace.

---

## 4. Theme Switching at Runtime

### C# - PaletteHelper

```csharp
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;

// Get current theme
var paletteHelper = new PaletteHelper();
Theme theme = paletteHelper.GetTheme();

// Switch Base Theme (Light / Dark)
theme.SetBaseTheme(BaseTheme.Dark);    // Dark mode
theme.SetBaseTheme(BaseTheme.Light);   // Light mode

// Apply changes
paletteHelper.SetTheme(theme);
```

### BundledTheme Properties

| Property | Type | Values |
|----------|------|--------|
| `BaseTheme` | `BaseTheme` enum | `Light`, `Dark` |
| `PrimaryColor` | `PrimaryColor` enum | `Red`, `Pink`, `Purple`, `DeepPurple`, `Indigo`, `Blue`, `LightBlue`, `Cyan`, `Teal`, `Green`, `LightGreen`, `Lime`, `Yellow`, `Amber`, `Orange`, `DeepOrange`, `Brown`, `Grey`, `BlueGrey` |
| `SecondaryColor` | `SecondaryColor` enum | `Red`, `Pink`, `Purple`, `DeepPurple`, `Indigo`, `Blue`, `LightBlue`, `Cyan`, `Teal`, `Green`, `LightGreen`, `Lime`, `Yellow`, `Amber`, `Orange`, `DeepOrange` |

---

## 5. Custom Color Palette

### C# - Setting Arbitrary Colors

```csharp
var paletteHelper = new PaletteHelper();
Theme theme = paletteHelper.GetTheme();

// Set primary color (auto-generates Light/Mid/Dark variants)
theme.SetPrimaryColor(Colors.Purple);

// Set secondary/accent color
theme.SetSecondaryColor(Colors.Blue);

// Set individual hue pairs (color + foreground)
theme.PrimaryLight = new ColorPair(Colors.LightBlue, Colors.Black);
theme.PrimaryMid = new ColorPair(Colors.Blue, Colors.White);
theme.PrimaryDark = new ColorPair(Colors.DarkBlue, Colors.White);

theme.SecondaryLight = new ColorPair(Colors.LightGreen, Colors.Black);
theme.SecondaryMid = new ColorPair(Colors.Green, Colors.White);
theme.SecondaryDark = new ColorPair(Colors.DarkGreen, Colors.White);

paletteHelper.SetTheme(theme);
```

### XAML - Advanced Custom Palette (App.xaml)

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Import color palettes -->
            <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.Indigo.xaml" />
            <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Secondary/MaterialDesignColor.Yellow.xaml" />

            <!-- Map specific hues to theme brushes -->
            <ResourceDictionary>
                <!-- Primary -->
                <SolidColorBrush x:Key="MaterialDesign.Brush.Primary.Light" Color="{StaticResource Primary200}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Primary.Light.Foreground" Color="{StaticResource Primary200Foreground}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Primary" Color="{StaticResource Primary500}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Primary.Foreground" Color="{StaticResource Primary500Foreground}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Primary.Dark" Color="{StaticResource Primary700}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Primary.Dark.Foreground" Color="{StaticResource Primary700Foreground}" />

                <!-- Secondary -->
                <SolidColorBrush x:Key="MaterialDesign.Brush.Secondary.Light" Color="{StaticResource Secondary200}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Secondary.Light.Foreground" Color="{StaticResource Secondary200Foreground}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Secondary" Color="{StaticResource Secondary400}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Secondary.Foreground" Color="{StaticResource Secondary400Foreground}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Secondary.Dark" Color="{StaticResource Secondary700}" />
                <SolidColorBrush x:Key="MaterialDesign.Brush.Secondary.Dark.Foreground" Color="{StaticResource Secondary700Foreground}" />
            </ResourceDictionary>

            <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign3.Defaults.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Using SwatchHelper for Predefined Colors

```csharp
using MaterialDesignColors;

PrimaryColor primary = PrimaryColor.DeepPurple;
Color primaryColor = SwatchHelper.Lookup[(MaterialDesignColor)primary];
theme.SetPrimaryColor(primaryColor);
```

---

## 6. Key Controls and XAML Usage

### 6.1 Card

```xml
<!-- Basic Card -->
<materialDesign:Card Padding="16" Margin="8">
    <TextBlock Text="This is a card" />
</materialDesign:Card>

<!-- Card with UniformCornerRadius and Shadow -->
<materialDesign:Card UniformCornerRadius="8"
                     materialDesign:ElevationAssist.Elevation="Dp4"
                     Padding="16" Margin="8">
    <StackPanel>
        <TextBlock Style="{StaticResource MaterialDesignHeadline6TextBlock}" Text="Card Title" />
        <TextBlock Text="Card body content goes here." Margin="0,8,0,0" />
        <Button Style="{StaticResource MaterialDesignFlatButton}" Content="ACTION" HorizontalAlignment="Right" Margin="0,16,0,0" />
    </StackPanel>
</materialDesign:Card>

<!-- Card with Image -->
<materialDesign:Card Width="300" materialDesign:ElevationAssist.Elevation="Dp2">
    <StackPanel>
        <Image Source="/images/header.jpg" Stretch="UniformToFill" Height="160" />
        <StackPanel Margin="16">
            <TextBlock Style="{StaticResource MaterialDesignHeadline6TextBlock}" Text="Image Card" />
            <TextBlock Text="Description text" />
        </StackPanel>
    </StackPanel>
</materialDesign:Card>
```

### 6.2 ColorZone

```xml
<!-- App Bar / Toolbar -->
<materialDesign:ColorZone Mode="PrimaryMid" Padding="16"
                          materialDesign:ElevationAssist.Elevation="Dp4">
    <DockPanel>
        <ToggleButton Style="{StaticResource MaterialDesignHamburgerToggleButton}"
                      DockPanel.Dock="Left"
                      IsChecked="{Binding IsDrawerOpen}" />
        <TextBlock Text="My Application"
                   Style="{StaticResource MaterialDesignHeadline6TextBlock}"
                   VerticalAlignment="Center" Margin="16,0,0,0" />
    </DockPanel>
</materialDesign:ColorZone>

<!-- ColorZone Modes: -->
<!-- Mode="Standard" | "Inverted" | "PrimaryLight" | "PrimaryMid" | "PrimaryDark" -->
<!-- Mode="SecondaryLight" | "SecondaryMid" | "SecondaryDark" | "Light" | "Dark" -->
<!-- Mode="Custom" (use Background/Foreground manually) -->
```

### 6.3 Chip

```xml
<!-- Basic Chip -->
<materialDesign:Chip Content="Example Chip" />

<!-- Chip with Icon -->
<materialDesign:Chip Content="User"
                     Icon="Account"
                     IsDeletable="True"
                     DeleteCommand="{Binding DeleteChipCommand}" />

<!-- Chip with custom icon content -->
<materialDesign:Chip Content="Settings">
    <materialDesign:Chip.Icon>
        <materialDesign:PackIcon Kind="Cog" />
    </materialDesign:Chip.Icon>
</materialDesign:Chip>

<!-- Filter Chip (CheckBox style) -->
<CheckBox Style="{StaticResource MaterialDesignFilterChipCheckBox}" Content="Filter A" />
<CheckBox Style="{StaticResource MaterialDesignFilterChipOutlineCheckBox}" Content="Filter B" />
<CheckBox Style="{StaticResource MaterialDesignFilterChipPrimaryCheckBox}" Content="Filter C" />

<!-- Choice Chip (RadioButton style) -->
<ListBox Style="{StaticResource MaterialDesignChoiceChipListBox}">
    <ListBoxItem Content="Choice 1" />
    <ListBoxItem Content="Choice 2" />
    <ListBoxItem Content="Choice 3" />
</ListBox>

<!-- Choice Chip (RadioButton style, outlined) -->
<ListBox Style="{StaticResource MaterialDesignChoiceChipOutlineListBox}">
    <ListBoxItem Content="Option A" />
    <ListBoxItem Content="Option B" />
</ListBox>
```

### 6.4 Snackbar

```xml
<!-- Basic Snackbar -->
<materialDesign:Snackbar Message="Hello World" IsActive="True" />

<!-- Snackbar with Action -->
<materialDesign:Snackbar IsActive="{Binding IsActive}">
    <materialDesign:SnackbarMessage Content="Item Deleted"
                                    ActionContent="UNDO"
                                    ActionCommand="{Binding UndoCommand}" />
</materialDesign:Snackbar>

<!-- Snackbar with MessageQueue (recommended approach) -->
<materialDesign:Snackbar x:Name="MainSnackbar"
                         MessageQueue="{materialDesign:MessageQueue}" />

<!-- Snackbar with MVVM MessageQueue -->
<materialDesign:Snackbar MessageQueue="{Binding MyMessageQueue}" />
```

```csharp
// C# - Enqueue messages
snackbarMessageQueue.Enqueue("Wow, easy!");

// With action callback
snackbarMessageQueue.Enqueue("Item Deleted", "UNDO", () => HandleUndo());

// With parameter
snackbarMessageQueue.Enqueue($"Item {id} Deleted", "UNDO", undoId => HandleUndo(undoId), id);
```

### 6.5 PopupBox

```xml
<!-- Basic PopupBox (three-dot menu) -->
<materialDesign:PopupBox PlacementMode="BottomAndAlignRightEdges"
                         StaysOpen="False">
    <StackPanel Width="150">
        <Button Content="Option 1" />
        <Button Content="Option 2" />
        <Button Content="Option 3" />
        <Separator />
        <Button Content="Settings" />
    </StackPanel>
</materialDesign:PopupBox>

<!-- PopupBox with custom toggle icon -->
<materialDesign:PopupBox>
    <materialDesign:PopupBox.ToggleContent>
        <materialDesign:PackIcon Kind="DotsHorizontal" />
    </materialDesign:PopupBox.ToggleContent>
    <StackPanel>
        <Button Content="Edit" />
        <Button Content="Delete" />
    </StackPanel>
</materialDesign:PopupBox>

<!-- Multi Floating Action Button (Speed Dial) -->
<materialDesign:PopupBox Style="{StaticResource MaterialDesignMultiFloatingActionPopupBox}"
                         PlacementMode="TopAndAlignCentres"
                         ToolTip="Add">
    <materialDesign:PopupBox.ToggleContent>
        <materialDesign:PackIcon Kind="Plus" Width="24" Height="24" />
    </materialDesign:PopupBox.ToggleContent>
    <StackPanel>
        <Button ToolTip="Add Person" Command="{Binding AddPersonCommand}">
            <materialDesign:PackIcon Kind="AccountPlus" />
        </Button>
        <Button ToolTip="Add File" Command="{Binding AddFileCommand}">
            <materialDesign:PackIcon Kind="FilePlus" />
        </Button>
    </StackPanel>
</materialDesign:PopupBox>

<!-- Tool PopupBox Styles -->
<!-- Style="{StaticResource MaterialDesignPopupBox}" (default) -->
<!-- Style="{StaticResource MaterialDesignToolPopupBox}" -->
<!-- Style="{StaticResource MaterialDesignToolForegroundPopupBox}" -->
<!-- Style="{StaticResource MaterialDesignMultiFloatingActionPopupBox}" -->
<!-- Style="{StaticResource MaterialDesignMultiFloatingActionLightPopupBox}" -->
<!-- Style="{StaticResource MaterialDesignMultiFloatingActionDarkPopupBox}" -->
<!-- Style="{StaticResource MaterialDesignMultiFloatingActionAccentPopupBox}" -->
```

### 6.6 DrawerHost

```xml
<materialDesign:DrawerHost IsLeftDrawerOpen="{Binding IsDrawerOpen}">
    <!-- Left Drawer Content -->
    <materialDesign:DrawerHost.LeftDrawerContent>
        <StackPanel Width="220" Margin="0,16,0,0">
            <TextBlock Text="Navigation" Style="{StaticResource MaterialDesignHeadline6TextBlock}"
                       Margin="16,0,16,16" />
            <ListBox Style="{StaticResource MaterialDesignNavigationPrimaryListBox}">
                <ListBoxItem Content="Home" />
                <ListBoxItem Content="Settings" />
                <ListBoxItem Content="About" />
            </ListBox>
        </StackPanel>
    </materialDesign:DrawerHost.LeftDrawerContent>

    <!-- Right Drawer Content (optional) -->
    <materialDesign:DrawerHost.RightDrawerContent>
        <StackPanel Width="300" Margin="16">
            <TextBlock Text="Details" Style="{StaticResource MaterialDesignHeadline6TextBlock}" />
        </StackPanel>
    </materialDesign:DrawerHost.RightDrawerContent>

    <!-- Top/Bottom drawers also available -->
    <!-- <materialDesign:DrawerHost.TopDrawerContent> -->
    <!-- <materialDesign:DrawerHost.BottomDrawerContent> -->

    <!-- Main Content -->
    <Grid>
        <materialDesign:ColorZone Mode="PrimaryMid" Padding="16"
                                  VerticalAlignment="Top"
                                  materialDesign:ElevationAssist.Elevation="Dp4">
            <DockPanel>
                <ToggleButton Style="{StaticResource MaterialDesignHamburgerToggleButton}"
                              IsChecked="{Binding IsDrawerOpen}" />
                <TextBlock Text="My App" VerticalAlignment="Center" Margin="16,0,0,0" />
            </DockPanel>
        </materialDesign:ColorZone>
    </Grid>
</materialDesign:DrawerHost>

<!-- DrawerHost Properties -->
<!-- IsLeftDrawerOpen, IsRightDrawerOpen, IsTopDrawerOpen, IsBottomDrawerOpen -->
<!-- OpenMode="Standard" | "Modal" -->
```

### 6.7 DataGrid Styling

```xml
<!-- Styled DataGrid -->
<DataGrid Style="{StaticResource MaterialDesignDataGrid}"
          ItemsSource="{Binding Items}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          materialDesign:DataGridAssist.CellPadding="13 8 8 8"
          materialDesign:DataGridAssist.ColumnHeaderPadding="8">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Name"
                            Binding="{Binding Name}"
                            EditingElementStyle="{StaticResource MaterialDesignDataGridTextColumnEditingStyle}" />
        <DataGridComboBoxColumn Header="Category"
                                ItemsSource="{Binding Categories}"
                                SelectedItemBinding="{Binding Category}"
                                EditingElementStyle="{StaticResource MaterialDesignDataGridComboBox}" />
        <DataGridCheckBoxColumn Header="Active"
                                Binding="{Binding IsActive}"
                                ElementStyle="{StaticResource MaterialDesignDataGridCheckBoxColumnStyle}"
                                EditingElementStyle="{StaticResource MaterialDesignDataGridCheckBoxColumnEditingStyle}" />
    </DataGrid.Columns>
</DataGrid>

<!-- DataGrid Style Keys -->
<!-- Style="{StaticResource MaterialDesignDataGrid}" -->
<!-- Cell: MaterialDesignDataGridCell -->
<!-- ColumnHeader: MaterialDesignDataGridColumnHeader -->
<!-- Row: MaterialDesignDataGridRow -->
<!-- RowHeader: MaterialDesignDataGridRowHeader -->
<!-- ComboBox column: MaterialDesignDataGridComboBox -->
```

### 6.8 TreeView Styling

```xml
<TreeView Style="{StaticResource MaterialDesignTreeView}">
    <TreeViewItem Header="Root"
                  Style="{StaticResource MaterialDesignTreeViewItem}"
                  IsExpanded="True">
        <TreeViewItem Header="Child 1" Style="{StaticResource MaterialDesignTreeViewItem}" />
        <TreeViewItem Header="Child 2" Style="{StaticResource MaterialDesignTreeViewItem}">
            <TreeViewItem Header="Grandchild" Style="{StaticResource MaterialDesignTreeViewItem}" />
        </TreeViewItem>
    </TreeViewItem>
</TreeView>

<!-- TreeView with Icons -->
<TreeView Style="{StaticResource MaterialDesignTreeView}">
    <TreeViewItem Style="{StaticResource MaterialDesignTreeViewItem}" IsExpanded="True">
        <TreeViewItem.Header>
            <StackPanel Orientation="Horizontal">
                <materialDesign:PackIcon Kind="Folder" VerticalAlignment="Center" Margin="0,0,8,0" />
                <TextBlock Text="Documents" VerticalAlignment="Center" />
            </StackPanel>
        </TreeViewItem.Header>
        <TreeViewItem Style="{StaticResource MaterialDesignTreeViewItem}">
            <TreeViewItem.Header>
                <StackPanel Orientation="Horizontal">
                    <materialDesign:PackIcon Kind="File" VerticalAlignment="Center" Margin="0,0,8,0" />
                    <TextBlock Text="Report.pdf" VerticalAlignment="Center" />
                </StackPanel>
            </TreeViewItem.Header>
        </TreeViewItem>
    </TreeViewItem>
</TreeView>
```

### 6.9 Button Variants

```xml
<!-- RAISED BUTTONS -->
<Button Style="{StaticResource MaterialDesignRaisedButton}" Content="RAISED" />
<Button Style="{StaticResource MaterialDesignRaisedLightButton}" Content="LIGHT" />
<Button Style="{StaticResource MaterialDesignRaisedDarkButton}" Content="DARK" />
<Button Style="{StaticResource MaterialDesignRaisedSecondaryButton}" Content="ACCENT" />
<Button Style="{StaticResource MaterialDesignRaisedSecondaryDarkButton}" Content="SEC DARK" />
<Button Style="{StaticResource MaterialDesignRaisedSecondaryLightButton}" Content="SEC LIGHT" />

<!-- FLAT BUTTONS -->
<Button Style="{StaticResource MaterialDesignFlatButton}" Content="FLAT" />
<Button Style="{StaticResource MaterialDesignFlatLightButton}" Content="FLAT LIGHT" />
<Button Style="{StaticResource MaterialDesignFlatDarkButton}" Content="FLAT DARK" />
<Button Style="{StaticResource MaterialDesignFlatSecondaryButton}" Content="FLAT SEC" />
<Button Style="{StaticResource MaterialDesignFlatMidBgButton}" Content="FLAT MID BG" />
<Button Style="{StaticResource MaterialDesignFlatAccentButton}" Content="FLAT ACCENT" />
<Button Style="{StaticResource MaterialDesignFlatAccentBgButton}" Content="FLAT ACCENT BG" />

<!-- OUTLINED BUTTONS -->
<Button Style="{StaticResource MaterialDesignOutlinedButton}" Content="OUTLINED" />
<Button Style="{StaticResource MaterialDesignOutlinedLightButton}" Content="OUT LIGHT" />
<Button Style="{StaticResource MaterialDesignOutlinedDarkButton}" Content="OUT DARK" />
<Button Style="{StaticResource MaterialDesignOutlinedSecondaryButton}" Content="OUT SEC" />
<Button Style="{StaticResource MaterialDesignOutlinedSecondaryLightButton}" Content="OUT SEC LIGHT" />
<Button Style="{StaticResource MaterialDesignOutlinedSecondaryDarkButton}" Content="OUT SEC DARK" />

<!-- FLOATING ACTION BUTTONS (FAB) -->
<Button Style="{StaticResource MaterialDesignFloatingActionButton}" ToolTip="Add">
    <materialDesign:PackIcon Kind="Plus" Width="24" Height="24" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionLightButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionDarkButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionSecondaryButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionAccentButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>

<!-- MINI FLOATING ACTION BUTTONS -->
<Button Style="{StaticResource MaterialDesignFloatingActionMiniButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionMiniLightButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionMiniDarkButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionMiniSecondaryButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>
<Button Style="{StaticResource MaterialDesignFloatingActionMiniAccentButton}">
    <materialDesign:PackIcon Kind="Plus" />
</Button>

<!-- ICON BUTTONS (borderless, icon only) -->
<Button Style="{StaticResource MaterialDesignIconButton}" ToolTip="Search">
    <materialDesign:PackIcon Kind="Magnify" />
</Button>
<Button Style="{StaticResource MaterialDesignIconForegroundButton}" ToolTip="Settings">
    <materialDesign:PackIcon Kind="Cog" />
</Button>

<!-- TOOL BUTTONS -->
<Button Style="{StaticResource MaterialDesignToolButton}" Content="TOOL" />
<Button Style="{StaticResource MaterialDesignToolForegroundButton}" Content="TOOL FG" />

<!-- PAPER BUTTONS -->
<Button Style="{StaticResource MaterialDesignPaperButton}" Content="PAPER" />
<Button Style="{StaticResource MaterialDesignPaperLightButton}" Content="PAPER LIGHT" />
<Button Style="{StaticResource MaterialDesignPaperDarkButton}" Content="PAPER DARK" />
<Button Style="{StaticResource MaterialDesignPaperSecondaryButton}" Content="PAPER SEC" />
```

### 6.10 TextBox (FloatingHint, HelperText)

```xml
<!-- Default TextBox -->
<TextBox Style="{StaticResource MaterialDesignTextBox}"
         materialDesign:HintAssist.Hint="Username" />

<!-- Floating Hint TextBox -->
<TextBox Style="{StaticResource MaterialDesignFloatingHintTextBox}"
         materialDesign:HintAssist.Hint="Email Address"
         materialDesign:HintAssist.IsFloating="True" />

<!-- Outlined TextBox -->
<TextBox Style="{StaticResource MaterialDesignOutlinedTextBox}"
         materialDesign:HintAssist.Hint="Search"
         materialDesign:HintAssist.FloatingScale="0.75" />

<!-- Filled TextBox -->
<TextBox Style="{StaticResource MaterialDesignFilledTextBox}"
         materialDesign:HintAssist.Hint="Name" />

<!-- TextBox with HelperText -->
<TextBox Style="{StaticResource MaterialDesignFloatingHintTextBox}"
         materialDesign:HintAssist.Hint="Password"
         materialDesign:HintAssist.HelperText="Min 8 characters"
         materialDesign:HintAssist.HelperTextFontSize="10" />

<!-- TextBox with Leading Icon -->
<TextBox Style="{StaticResource MaterialDesignFloatingHintTextBox}"
         materialDesign:HintAssist.Hint="Search"
         materialDesign:TextFieldAssist.HasLeadingIcon="True"
         materialDesign:TextFieldAssist.LeadingIcon="Magnify" />

<!-- TextBox with Prefix/Suffix -->
<TextBox Style="{StaticResource MaterialDesignFloatingHintTextBox}"
         materialDesign:HintAssist.Hint="Amount"
         materialDesign:TextFieldAssist.PrefixText="$"
         materialDesign:TextFieldAssist.SuffixText=".00" />

<!-- TextBox with Clear Button -->
<TextBox Style="{StaticResource MaterialDesignFloatingHintTextBox}"
         materialDesign:HintAssist.Hint="Clear me"
         materialDesign:TextFieldAssist.HasClearButton="True" />

<!-- PasswordBox -->
<PasswordBox Style="{StaticResource MaterialDesignFloatingHintPasswordBox}"
             materialDesign:HintAssist.Hint="Password" />
<PasswordBox Style="{StaticResource MaterialDesignOutlinedPasswordBox}"
             materialDesign:HintAssist.Hint="Password" />
<PasswordBox Style="{StaticResource MaterialDesignFilledPasswordBox}"
             materialDesign:HintAssist.Hint="Password" />

<!-- RichTextBox -->
<RichTextBox Style="{StaticResource MaterialDesignRichTextBox}"
             materialDesign:HintAssist.Hint="Enter text..."
             materialDesign:HintAssist.IsFloating="True" />
```

#### HintAssist Attached Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HintAssist.Hint` | `object` | null | Hint/label content |
| `HintAssist.IsFloating` | `bool` | varies | Enable floating animation |
| `HintAssist.FloatingScale` | `double` | 0.74 | Scale when floating |
| `HintAssist.FloatingOffset` | `Point` | varies | Position offset |
| `HintAssist.HintOpacity` | `double` | 0.56 | Hint opacity |
| `HintAssist.HelperText` | `string` | null | Helper text below control |
| `HintAssist.HelperTextFontSize` | `double` | 10 | Helper text font size |

#### TextFieldAssist Attached Properties

| Property | Type | Description |
|----------|------|-------------|
| `TextFieldAssist.HasLeadingIcon` | `bool` | Show leading icon |
| `TextFieldAssist.LeadingIcon` | `PackIconKind` | Icon kind |
| `TextFieldAssist.HasTrailingIcon` | `bool` | Show trailing icon |
| `TextFieldAssist.TrailingIcon` | `PackIconKind` | Trailing icon kind |
| `TextFieldAssist.PrefixText` | `string` | Prefix text |
| `TextFieldAssist.SuffixText` | `string` | Suffix text |
| `TextFieldAssist.HasClearButton` | `bool` | Show clear (X) button |
| `TextFieldAssist.HasFilledTextField` | `bool` | Filled style |
| `TextFieldAssist.HasOutlinedTextField` | `bool` | Outlined style |
| `TextFieldAssist.DecorationVisibility` | `Visibility` | Underline visibility |
| `TextFieldAssist.UnderlineBrush` | `Brush` | Underline color |
| `TextFieldAssist.RippleOnFocusEnabled` | `bool` | Ripple effect |

### 6.11 ComboBox

```xml
<!-- Default ComboBox -->
<ComboBox Style="{StaticResource MaterialDesignComboBox}"
          materialDesign:HintAssist.Hint="Select Item">
    <ComboBoxItem Content="Item 1" />
    <ComboBoxItem Content="Item 2" />
    <ComboBoxItem Content="Item 3" />
</ComboBox>

<!-- Floating Hint ComboBox -->
<ComboBox Style="{StaticResource MaterialDesignFloatingHintComboBox}"
          materialDesign:HintAssist.Hint="Category"
          ItemsSource="{Binding Categories}"
          SelectedItem="{Binding SelectedCategory}" />

<!-- Outlined ComboBox -->
<ComboBox Style="{StaticResource MaterialDesignOutlinedComboBox}"
          materialDesign:HintAssist.Hint="Priority" />

<!-- Filled ComboBox -->
<ComboBox Style="{StaticResource MaterialDesignFilledComboBox}"
          materialDesign:HintAssist.Hint="Status" />
```

### 6.12 CheckBox

```xml
<!-- Default CheckBox -->
<CheckBox Style="{StaticResource MaterialDesignCheckBox}" Content="Default" />
<CheckBox Style="{StaticResource MaterialDesignDarkCheckBox}" Content="Dark" />
<CheckBox Style="{StaticResource MaterialDesignLightCheckBox}" Content="Light" />
<CheckBox Style="{StaticResource MaterialDesignAccentCheckBox}" Content="Accent" />
<CheckBox Style="{StaticResource MaterialDesignUserForegroundCheckBox}" Content="User Foreground" />

<!-- Action CheckBox (icon style) -->
<CheckBox Style="{StaticResource MaterialDesignActionCheckBox}" Content="Action" />
<CheckBox Style="{StaticResource MaterialDesignActionLightCheckBox}" Content="Action Light" />
<CheckBox Style="{StaticResource MaterialDesignActionDarkCheckBox}" Content="Action Dark" />
<CheckBox Style="{StaticResource MaterialDesignActionAccentCheckBox}" Content="Action Accent" />

<!-- Filter Chip CheckBox -->
<CheckBox Style="{StaticResource MaterialDesignFilterChipCheckBox}" Content="Filter" />
<CheckBox Style="{StaticResource MaterialDesignFilterChipOutlineCheckBox}" Content="Filter Outline" />
<CheckBox Style="{StaticResource MaterialDesignFilterChipPrimaryCheckBox}" Content="Filter Primary" />
<CheckBox Style="{StaticResource MaterialDesignFilterChipPrimaryOutlineCheckBox}" Content="Filter Primary Outline" />
<CheckBox Style="{StaticResource MaterialDesignFilterChipAccentCheckBox}" Content="Filter Accent" />
<CheckBox Style="{StaticResource MaterialDesignFilterChipAccentOutlineCheckBox}" Content="Filter Accent Outline" />
```

### 6.13 ToggleButton

```xml
<!-- Switch Toggle -->
<ToggleButton Style="{StaticResource MaterialDesignSwitchToggleButton}"
              IsChecked="{Binding IsEnabled}"
              ToolTip="Enable Feature" />
<ToggleButton Style="{StaticResource MaterialDesignSwitchLightToggleButton}" />
<ToggleButton Style="{StaticResource MaterialDesignSwitchDarkToggleButton}" />
<ToggleButton Style="{StaticResource MaterialDesignSwitchAccentToggleButton}" />

<!-- Hamburger Toggle (for drawer navigation) -->
<ToggleButton Style="{StaticResource MaterialDesignHamburgerToggleButton}"
              IsChecked="{Binding IsDrawerOpen}" />

<!-- Action Toggle -->
<ToggleButton Style="{StaticResource MaterialDesignActionToggleButton}">
    <materialDesign:PackIcon Kind="Heart" />
    <materialDesign:ToggleButtonAssist.OnContent>
        <materialDesign:PackIcon Kind="HeartOutline" />
    </materialDesign:ToggleButtonAssist.OnContent>
</ToggleButton>
<ToggleButton Style="{StaticResource MaterialDesignActionLightToggleButton}" />
<ToggleButton Style="{StaticResource MaterialDesignActionDarkToggleButton}" />
<ToggleButton Style="{StaticResource MaterialDesignActionAccentToggleButton}" />

<!-- Flat Toggle -->
<ToggleButton Style="{StaticResource MaterialDesignFlatToggleButton}" Content="Toggle" />
<ToggleButton Style="{StaticResource MaterialDesignFlatPrimaryToggleButton}" Content="Primary" />
```

### 6.14 DialogHost

```xml
<!-- DialogHost wrapping entire window content -->
<materialDesign:DialogHost DialogTheme="Inherit"
                           CloseOnClickAway="True"
                           Identifier="RootDialog">
    <Grid>
        <!-- Your main content here -->

        <!-- Button to open dialog via command -->
        <Button Command="{x:Static materialDesign:DialogHost.OpenDialogCommand}"
                Content="Open Dialog">
            <Button.CommandParameter>
                <!-- Dialog content passed as parameter -->
                <StackPanel Margin="16">
                    <TextBlock Text="Are you sure?" />
                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,16,0,0">
                        <Button Style="{StaticResource MaterialDesignFlatButton}"
                                Command="{x:Static materialDesign:DialogHost.CloseDialogCommand}"
                                CommandParameter="False"
                                Content="CANCEL" />
                        <Button Style="{StaticResource MaterialDesignFlatButton}"
                                Command="{x:Static materialDesign:DialogHost.CloseDialogCommand}"
                                CommandParameter="True"
                                Content="OK" IsDefault="True" />
                    </StackPanel>
                </StackPanel>
            </Button.CommandParameter>
        </Button>
    </Grid>
</materialDesign:DialogHost>

<!-- DialogHost with inline DialogContent -->
<materialDesign:DialogHost>
    <materialDesign:DialogHost.DialogContent>
        <StackPanel Margin="16">
            <TextBlock Text="Dialog Title" Style="{StaticResource MaterialDesignHeadline6TextBlock}" />
            <TextBox materialDesign:HintAssist.Hint="Enter value" Margin="0,16,0,0" />
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,16,0,0">
                <Button Style="{StaticResource MaterialDesignFlatButton}"
                        Command="{x:Static materialDesign:DialogHost.CloseDialogCommand}"
                        Content="CANCEL" />
                <Button Style="{StaticResource MaterialDesignFlatButton}"
                        Command="{x:Static materialDesign:DialogHost.CloseDialogCommand}"
                        Content="SAVE" IsDefault="True" />
            </StackPanel>
        </StackPanel>
    </materialDesign:DialogHost.DialogContent>

    <!-- Main content -->
    <Button Content="SHOW DIALOG"
            Command="{x:Static materialDesign:DialogHost.OpenDialogCommand}" />
</materialDesign:DialogHost>

<!-- DialogHost with DialogClosing event -->
<materialDesign:DialogHost DialogClosing="DialogHost_OnDialogClosing" />

<!-- DialogHost with DialogClosingCallback (MVVM) -->
<materialDesign:DialogHost DialogClosingCallback="{Binding DialogClosingHandler}" />

<!-- Embedded DialogHost style (no overlay) -->
<!-- Style="{StaticResource MaterialDesignEmbeddedDialogHost}" -->
```

```csharp
// C# - Show dialog programmatically
var result = await DialogHost.Show(dialogContent, "RootDialog");

// With closing event handler
var result = await DialogHost.Show(dialogContent, "RootDialog",
    (sender, args) => { /* opening */ },
    (sender, args) => { /* closing - args.Parameter has result */ });

// Close from code
DialogHost.Close("RootDialog");
DialogHost.Close("RootDialog", resultParameter);
```

### 6.15 NavigationDrawer (via DrawerHost + ListBox)

```xml
<!-- Full Navigation Drawer Pattern -->
<materialDesign:DrawerHost IsLeftDrawerOpen="{Binding IsNavDrawerOpen}">
    <materialDesign:DrawerHost.LeftDrawerContent>
        <DockPanel MinWidth="220">
            <!-- Header -->
            <materialDesign:ColorZone Mode="PrimaryDark" DockPanel.Dock="Top" Padding="16">
                <StackPanel>
                    <materialDesign:PackIcon Kind="Account" Width="48" Height="48" />
                    <TextBlock Text="User Name" Margin="0,8,0,0" />
                </StackPanel>
            </materialDesign:ColorZone>

            <!-- Navigation Items -->
            <ListBox Style="{StaticResource MaterialDesignNavigationPrimaryListBox}"
                     SelectedIndex="{Binding SelectedNavIndex}">
                <ListBoxItem>
                    <StackPanel Orientation="Horizontal">
                        <materialDesign:PackIcon Kind="Home" Margin="0,0,16,0" />
                        <TextBlock Text="Home" />
                    </StackPanel>
                </ListBoxItem>
                <ListBoxItem>
                    <StackPanel Orientation="Horizontal">
                        <materialDesign:PackIcon Kind="Email" Margin="0,0,16,0" />
                        <TextBlock Text="Messages" />
                    </StackPanel>
                </ListBoxItem>
                <ListBoxItem>
                    <StackPanel Orientation="Horizontal">
                        <materialDesign:PackIcon Kind="Cog" Margin="0,0,16,0" />
                        <TextBlock Text="Settings" />
                    </StackPanel>
                </ListBoxItem>
            </ListBox>
        </DockPanel>
    </materialDesign:DrawerHost.LeftDrawerContent>

    <!-- Main content area -->
    <DockPanel>
        <materialDesign:ColorZone Mode="PrimaryMid" Padding="16" DockPanel.Dock="Top"
                                  materialDesign:ElevationAssist.Elevation="Dp4">
            <DockPanel>
                <ToggleButton Style="{StaticResource MaterialDesignHamburgerToggleButton}"
                              IsChecked="{Binding IsNavDrawerOpen}" DockPanel.Dock="Left" />
                <TextBlock Text="My Application" VerticalAlignment="Center" Margin="16,0,0,0"
                           Style="{StaticResource MaterialDesignHeadline6TextBlock}" />
            </DockPanel>
        </materialDesign:ColorZone>
        <ContentControl Content="{Binding CurrentView}" Margin="16" />
    </DockPanel>
</materialDesign:DrawerHost>

<!-- Navigation ListBox Styles -->
<!-- Style="{StaticResource MaterialDesignNavigationListBox}" -->
<!-- Style="{StaticResource MaterialDesignNavigationPrimaryListBox}" -->
<!-- Style="{StaticResource MaterialDesignNavigationAccentListBox}" -->
```

### 6.16 PackIcon

```xml
<!-- Basic icon -->
<materialDesign:PackIcon Kind="Home" />

<!-- Sized icon -->
<materialDesign:PackIcon Kind="Magnify" Width="24" Height="24" />

<!-- Colored icon -->
<materialDesign:PackIcon Kind="Heart" Foreground="Red" />

<!-- Icon using theme brush -->
<materialDesign:PackIcon Kind="Star"
                         Foreground="{DynamicResource MaterialDesign.Brush.Primary}" />

<!-- Icon inside Button (markup extension shorthand) -->
<Button Content="{materialDesign:PackIcon Search}" ToolTip="Search" />

<!-- Common icon names (PackIconKind enum values) -->
<!--
  Navigation: Home, Menu, ArrowBack, ArrowForward, ChevronLeft, ChevronRight, ChevronDown, ChevronUp
  Actions: Plus, Minus, Close, Check, Delete, Edit, Pencil, ContentSave, ContentCopy, ContentPaste
  Communication: Email, Phone, Message, Chat, Send, Bell, BellOutline
  Files: File, FileDocument, Folder, FolderOpen, Attachment, Download, Upload, CloudUpload, CloudDownload
  Media: Play, Pause, Stop, SkipNext, SkipPrevious, VolumeHigh, VolumeMute
  Social: Heart, HeartOutline, ThumbUp, ThumbDown, Star, StarOutline, Share, Account, AccountCircle
  Navigation UI: DotsVertical, DotsHorizontal, Magnify, Filter, Sort, Refresh, Sync
  Status: Alert, AlertCircle, Information, InformationOutline, CheckCircle, CloseCircle, HelpCircle
  Hardware: Printer, Monitor, Cellphone, Laptop, Keyboard, Mouse
  Toggle: Eye, EyeOff, Lock, LockOpen, Pin, PinOff
  Weather: WeatherSunny, WeatherNight, WeatherCloudy
  Misc: Cog, Settings, Power, Calendar, Clock, Map, MapMarker, Link, OpenInNew, Launch, ViewDashboard
-->
```

---

## 7. Complete Style Keys Reference

### Button Styles

| Style Key | Color Scheme | Type |
|-----------|-------------|------|
| `MaterialDesignRaisedButton` | Primary Mid | Raised |
| `MaterialDesignRaisedLightButton` | Primary Light | Raised |
| `MaterialDesignRaisedDarkButton` | Primary Dark | Raised |
| `MaterialDesignRaisedAccentButton` | Accent | Raised |
| `MaterialDesignRaisedSecondaryButton` | Secondary Mid | Raised |
| `MaterialDesignRaisedSecondaryLightButton` | Secondary Light | Raised |
| `MaterialDesignRaisedSecondaryDarkButton` | Secondary Dark | Raised |
| `MaterialDesignFlatButton` | Primary | Flat |
| `MaterialDesignFlatLightButton` | Primary Light | Flat |
| `MaterialDesignFlatDarkButton` | Primary Dark | Flat |
| `MaterialDesignFlatSecondaryButton` | Secondary | Flat |
| `MaterialDesignFlatAccentButton` | Accent | Flat |
| `MaterialDesignFlatAccentBgButton` | Accent BG | Flat |
| `MaterialDesignFlatMidBgButton` | Primary Mid BG | Flat |
| `MaterialDesignFlatDarkBgButton` | Primary Dark BG | Flat |
| `MaterialDesignFlatLightBgButton` | Primary Light BG | Flat |
| `MaterialDesignOutlinedButton` | Primary | Outlined |
| `MaterialDesignOutlinedLightButton` | Primary Light | Outlined |
| `MaterialDesignOutlinedDarkButton` | Primary Dark | Outlined |
| `MaterialDesignOutlinedSecondaryButton` | Secondary | Outlined |
| `MaterialDesignOutlinedSecondaryLightButton` | Secondary Light | Outlined |
| `MaterialDesignOutlinedSecondaryDarkButton` | Secondary Dark | Outlined |
| `MaterialDesignFloatingActionButton` | Primary Mid | FAB |
| `MaterialDesignFloatingActionLightButton` | Primary Light | FAB |
| `MaterialDesignFloatingActionDarkButton` | Primary Dark | FAB |
| `MaterialDesignFloatingActionSecondaryButton` | Secondary | FAB |
| `MaterialDesignFloatingActionAccentButton` | Accent | FAB |
| `MaterialDesignFloatingActionMiniButton` | Primary Mid | Mini FAB |
| `MaterialDesignFloatingActionMiniLightButton` | Primary Light | Mini FAB |
| `MaterialDesignFloatingActionMiniDarkButton` | Primary Dark | Mini FAB |
| `MaterialDesignFloatingActionMiniSecondaryButton` | Secondary | Mini FAB |
| `MaterialDesignFloatingActionMiniAccentButton` | Accent | Mini FAB |
| `MaterialDesignIconButton` | Default | Icon |
| `MaterialDesignIconForegroundButton` | Inherit Foreground | Icon |
| `MaterialDesignToolButton` | Default | Tool |
| `MaterialDesignToolForegroundButton` | Inherit Foreground | Tool |
| `MaterialDesignPaperButton` | Paper | Paper |
| `MaterialDesignPaperLightButton` | Light | Paper |
| `MaterialDesignPaperDarkButton` | Dark | Paper |
| `MaterialDesignPaperSecondaryButton` | Secondary | Paper |

### TextBox Styles

| Style Key | Description |
|-----------|-------------|
| `MaterialDesignTextBox` | Standard underline TextBox |
| `MaterialDesignTextBoxBase` | Base style |
| `MaterialDesignFloatingHintTextBox` | Floating hint label |
| `MaterialDesignOutlinedTextBox` | Outlined border |
| `MaterialDesignFilledTextBox` | Filled background |

### ComboBox Styles

| Style Key | Description |
|-----------|-------------|
| `MaterialDesignComboBox` | Standard ComboBox |
| `MaterialDesignFloatingHintComboBox` | With floating hint |
| `MaterialDesignOutlinedComboBox` | Outlined border |
| `MaterialDesignFilledComboBox` | Filled background |

### ToggleButton Styles

| Style Key | Description |
|-----------|-------------|
| `MaterialDesignSwitchToggleButton` | iOS-style switch |
| `MaterialDesignSwitchLightToggleButton` | Light switch |
| `MaterialDesignSwitchDarkToggleButton` | Dark switch |
| `MaterialDesignSwitchAccentToggleButton` | Accent switch |
| `MaterialDesignHamburgerToggleButton` | Hamburger menu icon |
| `MaterialDesignActionToggleButton` | Icon action toggle |
| `MaterialDesignActionLightToggleButton` | Light action toggle |
| `MaterialDesignActionDarkToggleButton` | Dark action toggle |
| `MaterialDesignActionAccentToggleButton` | Accent action toggle |
| `MaterialDesignFlatToggleButton` | Flat toggle |
| `MaterialDesignFlatPrimaryToggleButton` | Primary flat toggle |

### CheckBox Styles

| Style Key | Description |
|-----------|-------------|
| `MaterialDesignCheckBox` | Default |
| `MaterialDesignDarkCheckBox` | Dark theme |
| `MaterialDesignLightCheckBox` | Light theme |
| `MaterialDesignAccentCheckBox` | Accent color |
| `MaterialDesignUserForegroundCheckBox` | Inherits foreground |
| `MaterialDesignActionCheckBox` | Action style |
| `MaterialDesignFilterChipCheckBox` | Filter chip |
| `MaterialDesignFilterChipOutlineCheckBox` | Outlined filter chip |
| `MaterialDesignFilterChipPrimaryCheckBox` | Primary filter chip |
| `MaterialDesignFilterChipAccentCheckBox` | Accent filter chip |

### DataGrid Styles

| Style Key | Description |
|-----------|-------------|
| `MaterialDesignDataGrid` | Main DataGrid style |
| `MaterialDesignDataGridCell` | Cell style |
| `MaterialDesignDataGridColumnHeader` | Column header |
| `MaterialDesignDataGridRow` | Row style |
| `MaterialDesignDataGridRowHeader` | Row header |
| `MaterialDesignDataGridComboBox` | Editing ComboBox |

### Other Control Styles

| Style Key | Control |
|-----------|---------|
| `MaterialDesignTreeView` | TreeView |
| `MaterialDesignTreeViewItem` | TreeViewItem |
| `MaterialDesignListView` | ListView |
| `MaterialDesignListBox` | ListBox |
| `MaterialDesignListBoxItem` | ListBoxItem |
| `MaterialDesignMenu` | Menu |
| `MaterialDesignMenuItem` | MenuItem |
| `MaterialDesignContextMenu` | ContextMenu |
| `MaterialDesignToolBar` | ToolBar |
| `MaterialDesignToolBarTray` | ToolBarTray |
| `MaterialDesignToolTip` | ToolTip |
| `MaterialDesignLabel` | Label |
| `MaterialDesignExpander` | Expander |
| `MaterialDesignGroupBox` | GroupBox |
| `MaterialDesignCardGroupBox` | Card-style GroupBox |
| `MaterialDesignScrollViewer` | ScrollViewer |
| `MaterialDesignScrollBar` | ScrollBar |
| `MaterialDesignScrollBarMinimal` | Thin ScrollBar |
| `MaterialDesignGridSplitter` | GridSplitter |
| `MaterialDesignWindow` | Window |
| `MaterialDesignEmbeddedDialogHost` | Embedded DialogHost |
| `MaterialDesignLinearProgressBar` | Linear ProgressBar |
| `MaterialDesignCircularProgressBar` | Circular ProgressBar |
| `MaterialDesignSlider` | Slider |
| `MaterialDesignDiscreteSlider` | Discrete Slider |
| `MaterialDesignDatePicker` | DatePicker |
| `MaterialDesignFloatingHintDatePicker` | Floating hint DatePicker |
| `MaterialDesignOutlinedDatePicker` | Outlined DatePicker |
| `MaterialDesignFilledDatePicker` | Filled DatePicker |
| `MaterialDesignPasswordBox` | PasswordBox |
| `MaterialDesignFloatingHintPasswordBox` | Floating hint PasswordBox |
| `MaterialDesignOutlinedPasswordBox` | Outlined PasswordBox |
| `MaterialDesignFilledPasswordBox` | Filled PasswordBox |
| `MaterialDesignTimePicker` | TimePicker |
| `MaterialDesignFloatingHintTimePicker` | Floating hint TimePicker |
| `MaterialDesignOutlinedTimePicker` | Outlined TimePicker |
| `MaterialDesignFilledTimePicker` | Filled TimePicker |
| `MaterialDesignRichTextBox` | RichTextBox |
| `MaterialDesignCalendarPortrait` | Calendar |
| `MaterialDesignCardFlipper` | Flipper |
| `MaterialDesignRadioButton` | RadioButton |

### RadioButton / Choice Chip Styles

| Style Key | Description |
|-----------|-------------|
| `MaterialDesignRadioButton` | Default RadioButton |
| `MaterialDesignDarkRadioButton` | Dark theme |
| `MaterialDesignLightRadioButton` | Light theme |
| `MaterialDesignAccentRadioButton` | Accent color |
| `MaterialDesignUserForegroundRadioButton` | Inherit foreground |
| `MaterialDesignTabRadioButton` | Tab-style |
| `MaterialDesignTabRadioButtonTop` | Tab top |
| `MaterialDesignTabRadioButtonBottom` | Tab bottom |
| `MaterialDesignTabRadioButtonLeft` | Tab left |
| `MaterialDesignTabRadioButtonRight` | Tab right |
| `MaterialDesignToolRadioButton` | Tool style |
| `MaterialDesignChoiceChipRadioButton` | Choice chip |
| `MaterialDesignChoiceChipOutlineRadioButton` | Outlined choice chip |
| `MaterialDesignChoiceChipPrimaryRadioButton` | Primary choice chip |
| `MaterialDesignChoiceChipAccentRadioButton` | Accent choice chip |
| `MaterialDesignChoiceChipPrimaryOutlineRadioButton` | Primary outlined |
| `MaterialDesignChoiceChipAccentOutlineRadioButton` | Accent outlined |

### ListBox Styles (Navigation & Chips)

| Style Key | Description |
|-----------|-------------|
| `MaterialDesignListBox` | Default |
| `MaterialDesignNavigationListBox` | Navigation drawer list |
| `MaterialDesignNavigationPrimaryListBox` | Primary navigation |
| `MaterialDesignNavigationAccentListBox` | Accent navigation |
| `MaterialDesignChoiceChipListBox` | Choice chips |
| `MaterialDesignChoiceChipOutlineListBox` | Outlined choice chips |
| `MaterialDesignChoiceChipPrimaryListBox` | Primary choice chips |
| `MaterialDesignChoiceChipAccentListBox` | Accent choice chips |
| `MaterialDesignFilterChipListBox` | Filter chips |
| `MaterialDesignFilterChipOutlineListBox` | Outlined filter chips |
| `MaterialDesignFilterChipPrimaryListBox` | Primary filter chips |
| `MaterialDesignFilterChipAccentListBox` | Accent filter chips |
| `MaterialDesignToolToggleListBox` | Tool toggle list |
| `MaterialDesignToolVerticalToggleListBox` | Vertical tool toggle |
| `MaterialDesignToolToggleFlatListBox` | Flat tool toggle |
| `MaterialDesignCardsListBox` | Card items |

### Typography TextBlock Styles

| Style Key | Usage |
|-----------|-------|
| `MaterialDesignHeadline1TextBlock` | Largest heading |
| `MaterialDesignHeadline2TextBlock` | Large heading |
| `MaterialDesignHeadline3TextBlock` | Medium heading |
| `MaterialDesignHeadline4TextBlock` | Small heading |
| `MaterialDesignHeadline5TextBlock` | Smaller heading |
| `MaterialDesignHeadline6TextBlock` | App bar / section title |
| `MaterialDesignSubtitle1TextBlock` | Subtitle |
| `MaterialDesignSubtitle2TextBlock` | Secondary subtitle |
| `MaterialDesignBody1TextBlock` | Body text |
| `MaterialDesignBody2TextBlock` | Secondary body |
| `MaterialDesignCaptionTextBlock` | Caption / label |
| `MaterialDesignOverlineTextBlock` | Overline text |
| `MaterialDesignButtonTextBlock` | Button-style text |

---

## 8. Resource Keys (Brushes & Colors)

### Theme Brushes (v5.0+ naming)

| Resource Key | Description |
|-------------|-------------|
| `MaterialDesign.Brush.Primary.Light` | Primary light hue |
| `MaterialDesign.Brush.Primary.Light.Foreground` | Text on primary light |
| `MaterialDesign.Brush.Primary` | Primary mid hue (main) |
| `MaterialDesign.Brush.Primary.Foreground` | Text on primary |
| `MaterialDesign.Brush.Primary.Dark` | Primary dark hue |
| `MaterialDesign.Brush.Primary.Dark.Foreground` | Text on primary dark |
| `MaterialDesign.Brush.Secondary.Light` | Secondary light hue |
| `MaterialDesign.Brush.Secondary.Light.Foreground` | Text on secondary light |
| `MaterialDesign.Brush.Secondary` | Secondary mid hue |
| `MaterialDesign.Brush.Secondary.Foreground` | Text on secondary |
| `MaterialDesign.Brush.Secondary.Dark` | Secondary dark hue |
| `MaterialDesign.Brush.Secondary.Dark.Foreground` | Text on secondary dark |

### Legacy Brush Names (still commonly used)

| Resource Key | Description |
|-------------|-------------|
| `PrimaryHueLightBrush` | Primary light |
| `PrimaryHueLightForegroundBrush` | Foreground on primary light |
| `PrimaryHueMidBrush` | Primary mid (main) |
| `PrimaryHueMidForegroundBrush` | Foreground on primary mid |
| `PrimaryHueDarkBrush` | Primary dark |
| `PrimaryHueDarkForegroundBrush` | Foreground on primary dark |
| `SecondaryHueLightBrush` | Secondary light |
| `SecondaryHueLightForegroundBrush` | Foreground on secondary light |
| `SecondaryHueMidBrush` | Secondary mid |
| `SecondaryHueMidForegroundBrush` | Foreground on secondary mid |
| `SecondaryHueDarkBrush` | Secondary dark |
| `SecondaryHueDarkForegroundBrush` | Foreground on secondary dark |

### Surface & Background Brushes

| Resource Key | Description |
|-------------|-------------|
| `MaterialDesignPaper` | Main background |
| `MaterialDesignCardBackground` | Card background |
| `MaterialDesignToolBarBackground` | Toolbar background |
| `MaterialDesignBody` | Body text foreground |
| `MaterialDesignBodyLight` | Light body text |
| `MaterialDesignColumnHeader` | Column header text |
| `MaterialDesignCheckBoxOff` | Unchecked checkbox |
| `MaterialDesignCheckBoxDisabled` | Disabled checkbox |
| `MaterialDesignTextBoxBorder` | TextBox underline |
| `MaterialDesignDivider` | Dividers and separators |
| `MaterialDesignSelection` | Selection highlight |
| `MaterialDesignFlatButtonClick` | Flat button click effect |
| `MaterialDesignFlatButtonRipple` | Flat button ripple |
| `MaterialDesignToolTipBackground` | Tooltip background |
| `MaterialDesignChipBackground` | Chip background |
| `MaterialDesignSnackbarBackground` | Snackbar background |
| `MaterialDesignSnackbarMouseOver` | Snackbar hover |
| `MaterialDesignSnackbarRipple` | Snackbar ripple |
| `MaterialDesignFont` | Default font family |

### Usage in XAML

```xml
<!-- As DynamicResource (recommended for theme switching) -->
<Border Background="{DynamicResource MaterialDesignPaper}">
    <TextBlock Foreground="{DynamicResource MaterialDesignBody}" Text="Hello" />
</Border>

<!-- Using primary/secondary brushes -->
<Border Background="{DynamicResource PrimaryHueMidBrush}">
    <TextBlock Foreground="{DynamicResource PrimaryHueMidForegroundBrush}" Text="Primary" />
</Border>

<!-- v5.0+ naming -->
<Border Background="{DynamicResource MaterialDesign.Brush.Primary}">
    <TextBlock Foreground="{DynamicResource MaterialDesign.Brush.Primary.Foreground}" Text="Primary" />
</Border>
```

---

## 9. Attached Properties Quick Reference

### ElevationAssist

```xml
<!-- Elevation levels: Dp0, Dp1, Dp2, Dp3, Dp4, Dp6, Dp8, Dp12, Dp16, Dp24 -->
<Border materialDesign:ElevationAssist.Elevation="Dp4" />
```

### ShadowAssist

```xml
<!-- Performance optimization for shadows -->
<materialDesign:Card materialDesign:ShadowAssist.CacheMode="True" />
```

### DataGridAssist

```xml
<DataGrid materialDesign:DataGridAssist.CellPadding="13 8 8 8"
          materialDesign:DataGridAssist.ColumnHeaderPadding="8"
          materialDesign:DataGridAssist.EnableEditBoxAssist="True" />
```

### RippleAssist

```xml
<Button materialDesign:RippleAssist.IsDisabled="False"
        materialDesign:RippleAssist.ClipToBounds="True"
        materialDesign:RippleAssist.Feedback="{DynamicResource PrimaryHueMidBrush}" />
```

### TransitionAssist

```xml
<materialDesign:Card materialDesign:TransitionAssist.DisableTransitions="False" />
```

---

## 10. Complete Starter Template

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
        Style="{StaticResource MaterialDesignWindow}"
        TextElement.Foreground="{DynamicResource MaterialDesignBody}"
        TextElement.FontWeight="Regular"
        TextElement.FontSize="13"
        TextOptions.TextFormattingMode="Ideal"
        TextOptions.TextRenderingMode="Auto"
        Background="{DynamicResource MaterialDesignPaper}"
        FontFamily="{DynamicResource MaterialDesignFont}"
        Title="Material Design App" Height="700" Width="1000">

    <materialDesign:DialogHost Identifier="RootDialog">
        <materialDesign:DrawerHost IsLeftDrawerOpen="{Binding IsDrawerOpen}">
            <materialDesign:DrawerHost.LeftDrawerContent>
                <StackPanel Width="240">
                    <materialDesign:ColorZone Mode="PrimaryDark" Padding="16">
                        <TextBlock Text="Navigation"
                                   Style="{StaticResource MaterialDesignHeadline6TextBlock}" />
                    </materialDesign:ColorZone>
                    <ListBox Style="{StaticResource MaterialDesignNavigationPrimaryListBox}">
                        <ListBoxItem>
                            <StackPanel Orientation="Horizontal">
                                <materialDesign:PackIcon Kind="Home" Margin="0,0,16,0" />
                                <TextBlock Text="Home" />
                            </StackPanel>
                        </ListBoxItem>
                        <ListBoxItem>
                            <StackPanel Orientation="Horizontal">
                                <materialDesign:PackIcon Kind="Cog" Margin="0,0,16,0" />
                                <TextBlock Text="Settings" />
                            </StackPanel>
                        </ListBoxItem>
                    </ListBox>
                </StackPanel>
            </materialDesign:DrawerHost.LeftDrawerContent>

            <DockPanel>
                <!-- App Bar -->
                <materialDesign:ColorZone Mode="PrimaryMid" Padding="16" DockPanel.Dock="Top"
                                          materialDesign:ElevationAssist.Elevation="Dp4">
                    <DockPanel>
                        <ToggleButton Style="{StaticResource MaterialDesignHamburgerToggleButton}"
                                      IsChecked="{Binding IsDrawerOpen}" DockPanel.Dock="Left" />
                        <materialDesign:PopupBox DockPanel.Dock="Right" PlacementMode="BottomAndAlignRightEdges">
                            <StackPanel Width="120">
                                <Button Content="Settings" />
                                <Button Content="About" />
                            </StackPanel>
                        </materialDesign:PopupBox>
                        <TextBlock Text="My App" VerticalAlignment="Center" Margin="16,0"
                                   Style="{StaticResource MaterialDesignHeadline6TextBlock}" />
                    </DockPanel>
                </materialDesign:ColorZone>

                <!-- Snackbar -->
                <materialDesign:Snackbar DockPanel.Dock="Bottom" x:Name="MainSnackbar"
                                        MessageQueue="{materialDesign:MessageQueue}" />

                <!-- Main Content -->
                <ScrollViewer Padding="16">
                    <StackPanel>
                        <materialDesign:Card Padding="16" Margin="0,0,0,16"
                                             materialDesign:ElevationAssist.Elevation="Dp2">
                            <StackPanel>
                                <TextBlock Style="{StaticResource MaterialDesignHeadline5TextBlock}"
                                           Text="Welcome" />
                                <TextBox Style="{StaticResource MaterialDesignFloatingHintTextBox}"
                                         materialDesign:HintAssist.Hint="Your Name"
                                         materialDesign:HintAssist.HelperText="Enter your full name"
                                         Margin="0,16,0,0" />
                                <ComboBox Style="{StaticResource MaterialDesignFloatingHintComboBox}"
                                          materialDesign:HintAssist.Hint="Category"
                                          Margin="0,16,0,0">
                                    <ComboBoxItem Content="Option A" />
                                    <ComboBoxItem Content="Option B" />
                                </ComboBox>
                                <StackPanel Orientation="Horizontal" Margin="0,16,0,0">
                                    <Button Style="{StaticResource MaterialDesignRaisedButton}"
                                            Content="SUBMIT" Margin="0,0,8,0" />
                                    <Button Style="{StaticResource MaterialDesignOutlinedButton}"
                                            Content="CANCEL" />
                                </StackPanel>
                            </StackPanel>
                        </materialDesign:Card>

                        <!-- FAB positioned at bottom-right (usually in a Grid) -->
                        <Button Style="{StaticResource MaterialDesignFloatingActionButton}"
                                HorizontalAlignment="Right"
                                ToolTip="Add Item">
                            <materialDesign:PackIcon Kind="Plus" Width="24" Height="24" />
                        </Button>
                    </StackPanel>
                </ScrollViewer>
            </DockPanel>
        </materialDesign:DrawerHost>
    </materialDesign:DialogHost>
</Window>
```

---

## Sources

- [NuGet: MaterialDesignThemes 5.3.0](https://www.nuget.org/packages/MaterialDesignThemes/)
- [NuGet: MaterialDesignColors 5.3.0](https://www.nuget.org/packages/MaterialDesignColors/)
- [GitHub: MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- [Wiki: Getting Started](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Getting-Started)
- [Wiki: Advanced Theming](https://github.com/materialdesigninxaml/materialdesigninxamltoolkit/wiki/Advanced-Theming)
- [Wiki: Brush Names](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Brush-Names)
- [Wiki: Custom Palette Hues](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Custom-Palette-Hues)
- [Wiki: Button Styles](https://github.com/materialdesigninxaml/materialdesigninxamltoolkit/wiki/Button-Styles)
- [Wiki: Toggle Button Styles](https://github.com/materialdesigninxaml/materialdesigninxamltoolkit/wiki/Toggle-Button-Styles)
- [Wiki: ControlStyleList](https://github.com/materialdesigninxaml/materialdesigninxamltoolkit/wiki/ControlStyleList)
- [Wiki: Icons](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Icons)
- [Wiki: Dialogs](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Dialogs)
- [Wiki: Snackbar](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Snackbar)
- [Wiki: PopupBox](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/PopupBox)
- [DeepWiki: Theme Configuration](https://deepwiki.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/2.2-theme-configuration)
- [DeepWiki: Text Input Controls](https://deepwiki.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/3.2-text-input-controls-and-smart-hints)
- [Material Design Icons](https://materialdesignicons.com/)
