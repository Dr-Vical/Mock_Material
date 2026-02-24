# Project Templates

## Folder Structure

```
ProjectName/
├── ProjectName.csproj
├── App.xaml
├── App.xaml.cs
├── Models/
│   └── [DomainModel].cs
├── ViewModels/
│   ├── MainViewModel.cs
│   └── [Feature]ViewModel.cs
├── Views/
│   ├── MainView.xaml
│   ├── MainView.xaml.cs
│   └── [Feature]View.xaml
├── Services/
│   └── [Optional service interfaces/implementations]
├── Converters/
│   └── [Optional value converters]
└── Resources/
    └── [Optional shared styles, resource dictionaries]
```

## .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <!-- Add only the DevExpress NuGet packages actually used. -->
    <PackageReference Include="DevExpress.Wpf.Core" Version="25.2.*" />
    <!-- Example: <PackageReference Include="DevExpress.Wpf.Grid.Core" Version="25.2.*" /> -->
  </ItemGroup>
</Project>
```

Only include NuGet packages for controls actually used.
Check each control's reference file for the correct package name.

## App.xaml

```xml
<Application x:Class="ProjectName.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="Views/MainView.xaml">
    <Application.Resources>
        <!-- Shared resources here. -->
    </Application.Resources>
</Application>
```

## App.xaml.cs

```csharp
using DevExpress.Xpf.Core;
using System.Windows;

namespace ProjectName
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Apply theme globally before any UI loads.
            ApplicationThemeHelper.ApplicationThemeName = Theme.Win11LightName;
            base.OnStartup(e);
        }
    }
}
```

## View Code-Behind (ThemedWindow)

```csharp
using DevExpress.Xpf.Core;

namespace ProjectName.Views
{
    public partial class MainView : ThemedWindow
    {
        public MainView()
        {
            InitializeComponent();
        }
    }
}
```

## View XAML (ThemedWindow)

```xml
<dx:ThemedWindow x:Class="ProjectName.Views.MainView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:dx="http://schemas.devexpress.com/winfx/2008/xaml/core"
    xmlns:dxmvvm="http://schemas.devexpress.com/winfx/2008/xaml/mvvm"
    xmlns:local="clr-namespace:ProjectName.ViewModels"
    Title="{Binding Title}"
    Width="1200" Height="800">

    <dx:ThemedWindow.DataContext>
        <dxmvvm:ViewModelSource Type="{x:Type local:MainViewModel}"/>
    </dx:ThemedWindow.DataContext>

    <dxmvvm:Interaction.Behaviors>
        <dx:DXMessageBoxService/>
    </dxmvvm:Interaction.Behaviors>

    <!-- Content here. -->
</dx:ThemedWindow>
```

## POCO ViewModel Template

```csharp
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

namespace ProjectName.ViewModels
{
    public class MainViewModel
    {
        // virtual property -> automatic INotifyPropertyChanged.
        public virtual string Title { get; set; }

        // public void method -> auto-generated DelegateCommand (SaveCommand).
        public void Save()
        {
            // Save logic.
        }

        // CanSave() -> SaveCommand.CanExecute binding.
        public bool CanSave()
        {
            return !string.IsNullOrEmpty(Title);
        }

        // Service access via extension method.
        public IMessageBoxService MessageBoxService
            => this.GetService<IMessageBoxService>();

        // Factory method for creating POCO ViewModel instances.
        public static MainViewModel Create()
            => ViewModelSource.Create(() => new MainViewModel());
    }
}
```

## ViewModelBase Template (use when Messenger or manual commands are needed)

```csharp
using DevExpress.Mvvm;

namespace ProjectName.ViewModels
{
    public class DetailViewModel : ViewModelBase
    {
        public string ItemName
        {
            get => GetProperty(() => ItemName);
            set => SetProperty(() => ItemName, value);
        }

        public DelegateCommand SaveCommand { get; }

        public DetailViewModel()
        {
            SaveCommand = new DelegateCommand(
                () => Save(),
                () => !string.IsNullOrEmpty(ItemName)
            );
        }

        private void Save() { /* ... */ }
    }
}
```
