# DevExpress WPF Tree List (TreeListControl)

## Overview
- Hierarchical tree+grid hybrid control for displaying/editing tree-structured data.
- Binding modes: Self-referential, ChildNodesPath, ChildNodesSelector, Unbound.
- Sorting, filtering, search, drag-drop, checkbox, summary support.

## Key Classes
| Class | Description |
|-------|-------------|
| `TreeListControl` | Main tree list control |
| `TreeListView` | Tree list view |
| `TreeListColumn` | Column (inherits GridColumn) |
| `TreeListNode` | Tree node |

- **xmlns**: `dxg="http://schemas.devexpress.com/winfx/2008/xaml/grid"`
- **NuGet**: `DevExpress.Wpf.Grid.Core`

## Basic XAML
```xml
<dxg:TreeListControl ItemsSource="{Binding Departments}"
                     AutoGenerateColumns="AddNew">
    <dxg:TreeListControl.Columns>
        <dxg:TreeListColumn FieldName="Name" Header="Department"/>
        <dxg:TreeListColumn FieldName="EmployeeCount" Header="Count"/>
    </dxg:TreeListControl.Columns>
    <dxg:TreeListControl.View>
        <dxg:TreeListView KeyFieldName="Id"
                          ParentFieldName="ParentId"
                          AutoExpandAllNodes="True"/>
    </dxg:TreeListControl.View>
</dxg:TreeListControl>
```

## Self-Referential Binding (flat list with Id/ParentId)
```xml
<dxg:TreeListView KeyFieldName="Id" ParentFieldName="ParentId"/>
```

## Hierarchical Binding (nested collections)
```xml
<dxg:TreeListView ChildNodesPath="Children" TreeDerivationMode="ChildNodesSelector"/>
```

## Key Properties
**TreeListControl**: `ItemsSource`, `SelectedItem`, `CurrentItem`, `AutoGenerateColumns`.
**TreeListView**: `KeyFieldName`, `ParentFieldName`, `ChildNodesPath`, `AutoExpandAllNodes`, `ShowCheckboxes`, `CheckBoxFieldName`, `AllowEditing`, `ShowSearchPanel`, `AllowDragDrop`.

## MVVM Binding
```xml
<dxg:TreeListControl ItemsSource="{Binding Departments}"
                     SelectedItem="{Binding SelectedDepartment, Mode=TwoWay}">
    <dxg:TreeListControl.View>
        <dxg:TreeListView KeyFieldName="Id" ParentFieldName="ParentId"
                          RowDoubleClickCommand="{Binding EditCommand}"/>
    </dxg:TreeListControl.View>
</dxg:TreeListControl>
```

## Checkbox Support
```xml
<dxg:TreeListView ShowCheckboxes="True" CheckBoxFieldName="IsSelected"
                  AllowRecursiveNodeChecking="True"/>
```

## Reference
- https://docs.devexpress.com/WPF/9599/controls-and-libraries/data-grid/tree-list
