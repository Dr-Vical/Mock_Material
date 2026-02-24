# DevExpress WPF Tab Control

## Overview
- Tab-based navigation UI with multiple view modes.
- Tab drag-drop, close, add, data binding, MVVM support.
- Integration with ThemedWindow for tabbed windows.

## Key Classes
| Class | Description |
|-------|-------------|
| `DXTabControl` | Main tab control |
| `DXTabItem` | Individual tab item |

- **xmlns**: `dx="http://schemas.devexpress.com/winfx/2008/xaml/core"`
- **NuGet**: `DevExpress.Wpf.Core`

## Basic XAML
```xml
<dx:DXTabControl>
    <dx:DXTabItem Header="Dashboard">
        <local:DashboardView/>
    </dx:DXTabItem>
    <dx:DXTabItem Header="Reports">
        <local:ReportsView/>
    </dx:DXTabItem>
</dx:DXTabControl>
```

## Data-Bound Tabs (MVVM)
```xml
<dx:DXTabControl ItemsSource="{Binding Tabs}"
                  SelectedItem="{Binding SelectedTab}">
    <dx:DXTabControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Header}"/>
        </DataTemplate>
    </dx:DXTabControl.ItemTemplate>
    <dx:DXTabControl.ContentTemplate>
        <DataTemplate>
            <ContentControl Content="{Binding Content}"/>
        </DataTemplate>
    </dx:DXTabControl.ContentTemplate>
</dx:DXTabControl>
```

## Key Properties
**DXTabControl**: `TabContentCacheMode` (CacheAllTabs/CacheTabsOnSelecting/None), `HeaderLocation` (Top/Bottom/Left/Right), `AllowDragDrop`, `AllowItemClose`.
**DXTabItem**: `Header`, `IsSelected`, `AllowClose`.

## Reference
- https://docs.devexpress.com/WPF/7975/controls-and-libraries/layout-management/tab-control
