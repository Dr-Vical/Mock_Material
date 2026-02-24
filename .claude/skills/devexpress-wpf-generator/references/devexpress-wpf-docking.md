# DevExpress WPF Dock Layout Manager

## Overview
- Visual Studio-style docking window interface.
- Panel docking, floating, auto-hide, tab groups, MDI support.
- Runtime layout save/restore, MVVM binding support.

## Key Classes
| Class | Description |
|-------|-------------|
| `DockLayoutManager` | Root docking container |
| `LayoutPanel` | Individual dock panel |
| `LayoutGroup` | Horizontal/vertical panel group |
| `TabbedGroup` | Tab-style panel group |
| `DocumentPanel` | Document area panel |
| `DocumentGroup` | Document tab/MDI group |
| `FloatGroup` | Floating panel group |
| `AutoHideGroup` | Auto-hide panel group |

- **xmlns**: `dxdo="http://schemas.devexpress.com/winfx/2008/xaml/docking"`
- **NuGet**: `DevExpress.Wpf.Docking`

## Basic XAML
```xml
<dxdo:DockLayoutManager>
    <dxdo:LayoutGroup Orientation="Horizontal">
        <!-- Left panel -->
        <dxdo:LayoutPanel Caption="Explorer" ItemWidth="250">
            <TreeView/>
        </dxdo:LayoutPanel>

        <!-- Center document area -->
        <dxdo:DocumentGroup>
            <dxdo:DocumentPanel Caption="Document 1">
                <TextBox Text="Content"/>
            </dxdo:DocumentPanel>
        </dxdo:DocumentGroup>

        <!-- Right panel group (tabbed) -->
        <dxdo:TabbedGroup ItemWidth="300">
            <dxdo:LayoutPanel Caption="Properties">
                <ContentControl/>
            </dxdo:LayoutPanel>
            <dxdo:LayoutPanel Caption="Output">
                <TextBox IsReadOnly="True"/>
            </dxdo:LayoutPanel>
        </dxdo:TabbedGroup>
    </dxdo:LayoutGroup>

    <!-- Bottom auto-hide -->
    <dxdo:DockLayoutManager.AutoHideGroups>
        <dxdo:AutoHideGroup DockType="Bottom">
            <dxdo:LayoutPanel Caption="Error List"/>
        </dxdo:AutoHideGroup>
    </dxdo:DockLayoutManager.AutoHideGroups>
</dxdo:DockLayoutManager>
```

## Key Properties
**DockLayoutManager**: `FloatingMode` (Desktop/Widget), `AllowCustomization`, `DockItemClosing` event.
**LayoutPanel**: `Caption`, `AllowClose`, `AllowFloat`, `AllowDock`, `AllowHide`, `ShowCaption`, `ItemWidth`, `ItemHeight`.
**LayoutGroup**: `Orientation` (Horizontal/Vertical), `ItemWidth`, `ItemHeight`.
**DocumentGroup**: `ClosePageButtonShowMode`, `ShowDropDownButton`.

## Floating Panels
```xml
<dxdo:DockLayoutManager FloatingMode="Desktop">
    <dxdo:DockLayoutManager.FloatGroups>
        <dxdo:FloatGroup FloatLocation="300,200" FloatSize="400,300">
            <dxdo:LayoutPanel Caption="Floating Panel">
                <TextBlock Text="Floating content"/>
            </dxdo:LayoutPanel>
        </dxdo:FloatGroup>
    </dxdo:DockLayoutManager.FloatGroups>
</dxdo:DockLayoutManager>
```

## Layout Save/Restore
```csharp
// Save layout to XML string.
string layout = dockManager.GetLayoutAsXml();

// Restore layout.
dockManager.RestoreLayoutFromXml(layout);
```

## MVVM Binding
```xml
<dxdo:DockLayoutManager>
    <dxdo:LayoutGroup>
        <dxdo:DocumentGroup ItemsSource="{Binding Documents}"
                            ClosingCommand="{Binding CloseDocumentCommand}">
            <dxdo:DocumentGroup.ItemTemplate>
                <DataTemplate>
                    <ContentControl Content="{Binding}"/>
                </DataTemplate>
            </dxdo:DocumentGroup.ItemTemplate>
            <dxdo:DocumentGroup.ItemCaptionTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Title}"/>
                </DataTemplate>
            </dxdo:DocumentGroup.ItemCaptionTemplate>
        </dxdo:DocumentGroup>
    </dxdo:LayoutGroup>
</dxdo:DockLayoutManager>
```

## Reference
- https://docs.devexpress.com/WPF/6196/controls-and-libraries/layout-management/dock-windows
