# Project Templates — Material Design WPF (.NET 8)

## Folder Structure

```
RswareDesign/
├── RswareDesign.csproj
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Models/
│   ├── Parameter.cs
│   ├── Drive.cs
│   ├── DriveTreeNode.cs
│   └── DriveGroup.cs
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
│   ├── ErrorLogView.xaml
│   ├── ActionPanelView.xaml
│   └── [Feature]View.xaml
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
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   ├── AccessModeToEditableConverter.cs
│   └── ValueOutOfRangeConverter.cs
└── Resources/
    ├── Languages/
    │   ├── ko.json
    │   └── en.json
    └── Icons/
```

## .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <!-- Material Design -->
    <PackageReference Include="MaterialDesignThemes" Version="5.*" />
    <PackageReference Include="MaterialDesignColors" Version="3.*" />
    <!-- MVVM -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
    <!-- DI -->
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.*" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.*" />
    <!-- Docking -->
    <PackageReference Include="AvalonDock" Version="4.*" />
    <!-- OR community fork: -->
    <!-- <PackageReference Include="Dirkster.AvalonDock" Version="4.*" /> -->
    <!-- Ribbon -->
    <PackageReference Include="Fluent.Ribbon" Version="11.*" />
    <!-- Charts (add only when Monitor/Oscilloscope is needed) -->
    <PackageReference Include="ScottPlot.WPF" Version="5.*" />
    <!-- Serial (add only when communication is needed) -->
    <PackageReference Include="System.IO.Ports" Version="8.*" />
    <!-- CSV (add only when parameter loading is needed) -->
    <PackageReference Include="CsvHelper" Version="33.*" />
  </ItemGroup>
</Project>
```

Only include NuGet packages for controls actually used.

## App.xaml — Theme Setup

```xml
<Application x:Class="RswareDesign.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- MaterialDesign Theme -->
                <materialDesign:BundledTheme
                    BaseTheme="Dark"
                    PrimaryColor="Blue"
                    SecondaryColor="Orange" />

                <!-- MaterialDesign Defaults -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign3.Defaults.xaml" />

                <!-- Fluent.Ribbon Theme -->
                <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />

                <!-- Custom Theme Tokens -->
                <ResourceDictionary Source="Themes/SharedTokens.xaml" />
                <ResourceDictionary Source="Themes/DarkTheme.xaml" />

                <!-- Custom Styles -->
                <ResourceDictionary Source="Themes/ButtonStyles.xaml" />
                <ResourceDictionary Source="Themes/DataGridStyles.xaml" />
                <ResourceDictionary Source="Themes/TreeViewStyles.xaml" />
                <ResourceDictionary Source="Themes/DockingStyles.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

## App.xaml.cs — DI + Theme Initialization

```csharp
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace RswareDesign;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<ILocalizationService, LocalizationService>();
                services.AddSingleton<INavigationService, NavigationService>();

                // ViewModels
                services.AddSingleton<MainWindowViewModel>();
                services.AddTransient<DriveTreeViewModel>();
                services.AddTransient<ParameterEditorViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainWindowViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
```

## MainWindow.xaml — Shell Structure

```xml
<Window x:Class="RswareDesign.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
        xmlns:fluent="urn:fluent-ribbon"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        xmlns:vm="clr-namespace:RswareDesign.ViewModels"
        xmlns:views="clr-namespace:RswareDesign.Views"
        Title="{DynamicResource loc.app.title}"
        Width="1920" Height="1080"
        WindowState="Maximized"
        Background="{DynamicResource BackgroundBrush}"
        TextElement.Foreground="{DynamicResource TextPrimary}"
        TextElement.FontFamily="{DynamicResource FontFamilyUI}"
        TextElement.FontSize="{DynamicResource FontSizeMD}">

    <DockPanel LastChildFill="True">
        <!-- Ribbon Menu (top) -->
        <fluent:Ribbon DockPanel.Dock="Top">
            <!-- Ribbon content here -->
        </fluent:Ribbon>

        <!-- Status Bar (bottom) -->
        <StatusBar DockPanel.Dock="Bottom"
                   Background="{DynamicResource StatusBarBrush}"
                   Foreground="{DynamicResource StatusBarForeground}">
            <!-- StatusBar content here -->
        </StatusBar>

        <!-- AvalonDock Main Layout (center) -->
        <avalonDock:DockingManager>
            <avalonDock:LayoutRoot>
                <avalonDock:LayoutPanel Orientation="Vertical">
                    <!-- Upper area -->
                    <avalonDock:LayoutPanel Orientation="Horizontal">
                        <!-- Left: Drive Tree -->
                        <avalonDock:LayoutAnchorablePane DockWidth="250">
                            <avalonDock:LayoutAnchorable Title="Drive Tree" ContentId="driveTree" />
                        </avalonDock:LayoutAnchorablePane>
                        <!-- Center: Documents -->
                        <avalonDock:LayoutDocumentPane />
                        <!-- Right: Action Panel -->
                        <avalonDock:LayoutAnchorablePane DockWidth="160">
                            <avalonDock:LayoutAnchorable Title="Actions" ContentId="actions" />
                        </avalonDock:LayoutAnchorablePane>
                    </avalonDock:LayoutPanel>
                    <!-- Bottom: Error Log -->
                    <avalonDock:LayoutAnchorablePane DockHeight="150">
                        <avalonDock:LayoutAnchorable Title="Error Log" ContentId="errorLog" />
                    </avalonDock:LayoutAnchorablePane>
                </avalonDock:LayoutPanel>
            </avalonDock:LayoutRoot>
        </avalonDock:DockingManager>
    </DockPanel>
</Window>
```

## MainWindow.xaml.cs

```csharp
using System.Windows;

namespace RswareDesign;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

## ViewModel Template (CommunityToolkit.Mvvm)

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace RswareDesign.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "RswareDesign";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _selectedPort = "";

    [RelayCommand]
    private void Connect()
    {
        // Connection logic
    }

    [RelayCommand(CanExecute = nameof(CanSaveParameters))]
    private void SaveParameters()
    {
        // Save logic
    }

    private bool CanSaveParameters() => IsConnected;
}
```

## UserControl View Template

```xml
<UserControl x:Class="RswareDesign.Views.ParameterEditorView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             Background="{DynamicResource SurfaceBrush}">

    <Grid>
        <!-- Parameter DataGrid -->
        <DataGrid ItemsSource="{Binding Parameters}"
                  SelectedItem="{Binding SelectedParameter}"
                  AutoGenerateColumns="False"
                  Style="{StaticResource MaterialDesignDataGrid}"
                  Background="{DynamicResource SurfaceBrush}"
                  Foreground="{DynamicResource TextPrimary}">
            <DataGrid.Columns>
                <DataGridTextColumn Header="FT NUM" Binding="{Binding FtNumber}" IsReadOnly="True" />
                <DataGridTextColumn Header="PARAMETER" Binding="{Binding Name}" IsReadOnly="True" Width="*" />
                <DataGridTextColumn Header="VALUE" Binding="{Binding Value}" />
                <DataGridTextColumn Header="UNITS" Binding="{Binding Unit}" IsReadOnly="True" />
                <DataGridTextColumn Header="DEFAULT" Binding="{Binding Default}" IsReadOnly="True" />
                <DataGridTextColumn Header="MIN" Binding="{Binding Min}" IsReadOnly="True" />
                <DataGridTextColumn Header="MAX" Binding="{Binding Max}" IsReadOnly="True" />
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```
