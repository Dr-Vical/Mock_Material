# DevExpress WPF Windows Modern UI & Dialogs

## Overview
- ThemedWindow, DXMessageBox/ThemedMessageBox, FlyoutControl, NotificationService, SplashScreenManager, WaitIndicator, NavigationFrame.

## Key Classes
| Class | Namespace | Description |
|-------|-----------|-------------|
| `ThemedWindow` | `DevExpress.Xpf.Core` | Theme-aware Window |
| `ThemedMessageBox` | `DevExpress.Xpf.Core` | Themed message box |
| `FlyoutControl` | `DevExpress.Xpf.Core` | Context popup panel |
| `NotificationService` | `DevExpress.Xpf.Core` | Toast notifications |
| `SplashScreenManager` | `DevExpress.Xpf.Core` | Splash screen |
| `WaitIndicator` | `DevExpress.Xpf.Core` | Loading indicator |
| `NavigationFrame` | `DevExpress.Xpf.WindowsUI` | Modern UI navigation |

- **xmlns**: `dx="http://schemas.devexpress.com/winfx/2008/xaml/core"`
- **NuGet**: `DevExpress.Wpf.Core`

## ThemedWindow
```xml
<dx:ThemedWindow x:Class="MyApp.Views.MainView"
    xmlns:dx="http://schemas.devexpress.com/winfx/2008/xaml/core"
    Title="My Application" Width="1200" Height="800">
</dx:ThemedWindow>
```
```csharp
public partial class MainView : ThemedWindow
{
    public MainView() { InitializeComponent(); }
}
```

## ThemedMessageBox
```csharp
ThemedMessageBox.Show("Operation completed.", "Info",
    MessageBoxButton.OK, MessageBoxImage.Information);
```

## MVVM MessageBox Service
```xml
<dxmvvm:Interaction.Behaviors>
    <dx:DXMessageBoxService/>
</dxmvvm:Interaction.Behaviors>
```
```csharp
MessageBoxService.ShowMessage("Saved!", "Success", MessageButton.OK);
```

## FlyoutControl
```xml
<dx:FlyoutControl IsOpen="{Binding ShowFlyout}" PlacementTarget="{Binding ElementName=btnSettings}">
    <StackPanel Margin="10">
        <TextBlock Text="Settings Panel"/>
    </StackPanel>
</dx:FlyoutControl>
```

## NotificationService (MVVM)
```xml
<dxmvvm:Interaction.Behaviors>
    <dx:NotificationService x:Name="notificationService"
        CustomNotificationPosition="TopRight"
        CustomNotificationDuration="0:0:3"/>
</dxmvvm:Interaction.Behaviors>
```

## WaitIndicator
```xml
<dx:WaitIndicator DeferedVisibility="True"
                   Content="Loading..." ShowShadow="True"/>
```

## NavigationFrame
```xml
<dxwui:NavigationFrame x:Name="navFrame" AnimationType="SlideHorizontal">
    <dxwui:NavigationFrame.Source>
        <local:DashboardView/>
    </dxwui:NavigationFrame.Source>
</dxwui:NavigationFrame>
```
```csharp
navFrame.Navigate(new SettingsView());
navFrame.GoBack();
```

## Reference
- https://docs.devexpress.com/WPF/114860/controls-and-libraries/windows-and-utility-controls
