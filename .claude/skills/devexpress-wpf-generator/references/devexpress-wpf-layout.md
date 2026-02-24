# DevExpress WPF Layout Controls

## Overview
- Form layout containers: LayoutControl, DataLayoutControl, TileLayoutControl, FlowLayoutControl.
- Auto-arrange items in rows/columns, auto-resize on window resize.

## Key Classes
| Class | Description |
|-------|-------------|
| `LayoutControl` | Auto-arrange container (row/column) |
| `DataLayoutControl` | Data-binding auto form generation |
| `LayoutItem` | Label + editor wrapper |
| `LayoutGroup` | Group items horizontally/vertically/tab |
| `TileLayoutControl` | Windows Modern UI tile layout |
| `FlowLayoutControl` | Flow-direction auto layout |

- **xmlns**: `dxlc="http://schemas.devexpress.com/winfx/2008/xaml/layoutcontrol"`
- **NuGet**: `DevExpress.Wpf.LayoutControl`

## Basic Form Layout
```xml
<dxlc:LayoutControl Orientation="Vertical" Padding="10">
    <dxlc:LayoutItem Label="Name">
        <dxe:TextEdit EditValue="{Binding Name, Mode=TwoWay}"/>
    </dxlc:LayoutItem>
    <dxlc:LayoutItem Label="Email">
        <dxe:TextEdit EditValue="{Binding Email, Mode=TwoWay}"/>
    </dxlc:LayoutItem>
    <dxlc:LayoutItem Label="Department">
        <dxe:ComboBoxEdit ItemsSource="{Binding Departments}"
                          EditValue="{Binding Department, Mode=TwoWay}"/>
    </dxlc:LayoutItem>
    <dxlc:LayoutGroup Orientation="Horizontal" HorizontalAlignment="Right">
        <dx:SimpleButton Content="Save" Command="{Binding SaveCommand}"/>
        <dx:SimpleButton Content="Cancel" Command="{Binding CancelCommand}"/>
    </dxlc:LayoutGroup>
</dxlc:LayoutControl>
```

## Grouped Layout
```xml
<dxlc:LayoutControl Orientation="Vertical">
    <dxlc:LayoutGroup Header="Personal Info" View="GroupBox">
        <dxlc:LayoutItem Label="First Name">
            <dxe:TextEdit EditValue="{Binding FirstName}"/>
        </dxlc:LayoutItem>
        <dxlc:LayoutItem Label="Last Name">
            <dxe:TextEdit EditValue="{Binding LastName}"/>
        </dxlc:LayoutItem>
    </dxlc:LayoutGroup>
    <dxlc:LayoutGroup Header="Address" View="GroupBox">
        <dxlc:LayoutItem Label="City">
            <dxe:TextEdit EditValue="{Binding City}"/>
        </dxlc:LayoutItem>
    </dxlc:LayoutGroup>
</dxlc:LayoutControl>
```

## DataLayoutControl (auto-generate from object)
```xml
<dxlc:DataLayoutControl CurrentItem="{Binding SelectedEmployee}"
                        AutoGenerateItems="True"/>
```

## Key Properties
**LayoutControl**: `Orientation` (Vertical/Horizontal), `Padding`, `ItemSpace`, `ItemLabelsAlignment`.
**LayoutItem**: `Label`, `LabelPosition` (Left/Top), `IsRequired`.
**LayoutGroup**: `Header`, `View` (Group/GroupBox/Tabs), `Orientation`, `IsCollapsible`.

## Reference
- https://docs.devexpress.com/WPF/6191/controls-and-libraries/layout-management/layout-control
