# DevExpress WPF MVVM Framework

## Overview
- Complete MVVM infrastructure: ViewModels, Commands, Services, Behaviors, DXBinding.
- Works with and without DevExpress WPF controls.
- NuGet: `DevExpress.Mvvm`, `DevExpress.Wpf.Core`
- xmlns: `dxmvvm="http://schemas.devexpress.com/winfx/2008/xaml/mvvm"`

## ViewModels

### POCO ViewModel (recommended default)
`virtual` properties get auto INotifyPropertyChanged; public methods become commands.
```csharp
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

public class MainViewModel
{
    public virtual string Title { get; set; }
    public virtual decimal Price { get; set; }

    public void Save() { /* auto-generates SaveCommand */ }
    public bool CanSave() => !string.IsNullOrEmpty(Title);

    public async Task LoadDataAsync() { /* auto-generates LoadDataAsyncCommand */ }

    public IMessageBoxService MessageBoxService
        => this.GetService<IMessageBoxService>();

    public static MainViewModel Create()
        => ViewModelSource.Create(() => new MainViewModel());
}
```

### ViewModelBase (when Messenger or manual commands needed)
```csharp
public class DetailViewModel : ViewModelBase
{
    public string Name
    {
        get => GetProperty(() => Name);
        set => SetProperty(() => Name, value);
    }
    public DelegateCommand SaveCommand { get; }
    public DetailViewModel()
    {
        SaveCommand = new DelegateCommand(() => Save(), () => !string.IsNullOrEmpty(Name));
    }
}
```

### Source Generator ViewModel (compile-time)
```csharp
using DevExpress.Mvvm.CodeGenerators;

[GenerateViewModel]
public partial class OrderViewModel
{
    [GenerateProperty] string _customerName;
    [GenerateCommand] void Submit() { }
    bool CanSubmit() => !string.IsNullOrEmpty(CustomerName);
}
```

## Commands
- `DelegateCommand` / `DelegateCommand<T>` — sync commands.
- `AsyncCommand` / `AsyncCommand<T>` — async commands (IsExecuting, cancellation).
```xml
<Button Content="Save" Command="{Binding SaveCommand}"/>
<dx:SimpleButton Content="Load" Command="{Binding LoadDataAsyncCommand}"
                 AsyncDisplayMode="WaitCancel"/>
```

## Services
Register in XAML, access in ViewModel via `GetService<T>()`.

| Service | Interface | Purpose |
|---------|-----------|---------|
| `DXMessageBoxService` | `IMessageBoxService` | Message box |
| `DialogService` | `IDialogService` | Modal dialog |
| `WindowService` | `IWindowService` | Open new window |
| `DispatcherService` | `IDispatcherService` | UI thread dispatch |
| `OpenFileDialogService` | `IOpenFileDialogService` | File open dialog |
| `SaveFileDialogService` | `ISaveFileDialogService` | File save dialog |
| `NotificationService` | `INotificationService` | Toast notifications |

```xml
<dxmvvm:Interaction.Behaviors>
    <dx:DXMessageBoxService/>
    <dxmvvm:DispatcherService/>
</dxmvvm:Interaction.Behaviors>
```

## Behaviors
```xml
<!-- Event to command -->
<dxmvvm:EventToCommand EventName="Loaded" Command="{Binding LoadCommand}"/>
<!-- Key binding -->
<dxmvvm:KeyToCommand Command="{Binding SearchCommand}" KeyGesture="Ctrl+F"/>
<!-- Confirmation -->
<dxmvvm:ConfirmationBehavior Command="{Binding DeleteCommand}"
    MessageText="Delete?" MessageTitle="Confirm"/>
```

## DXBinding
```xml
<TextBlock Text="{DXBinding 'Price * Quantity'}"/>
<TextBlock Foreground="{DXBinding 'Amount &lt; 0 ? `Red` : `Black`'}"/>
<TextBox Text="{DXBinding 'Name', Mode=TwoWay}"/>
```
Keywords: `@s` (Self), `@p` (TemplatedParent), `@e(name)` (ElementName), `@a(Type)` (FindAncestor).

## Messenger
```csharp
Messenger.Default.Send(new StatusMessage("Updated"));
Messenger.Default.Register<StatusMessage>(this, msg => Status = msg.Text);
```

## XAML DataContext Binding
```xml
<dx:ThemedWindow.DataContext>
    <dxmvvm:ViewModelSource Type="{x:Type local:MainViewModel}"/>
</dx:ThemedWindow.DataContext>
```

## Reference
- https://docs.devexpress.com/WPF/15112/mvvm-framework
