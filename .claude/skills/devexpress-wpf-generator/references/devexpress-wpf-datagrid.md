# DevExpress WPF Data Grid (GridControl)

## Overview
- 2D table for displaying/editing data with sorting, filtering, grouping, summary, editing, drag-drop, master-detail.
- Three view modes: TableView, CardView, TreeListView.
- Export to PDF/RTF/XLS and printing support.

## Key Classes
| Class | Description |
|-------|-------------|
| `GridControl` | Main grid control |
| `TableView` | Table layout view |
| `CardView` | Card layout view |
| `TreeListView` | Tree layout view |
| `GridColumn` | Column definition |

- **xmlns**: `dxg="http://schemas.devexpress.com/winfx/2008/xaml/grid"`
- **NuGet**: `DevExpress.Wpf.Grid.Core`

## Basic XAML
```xml
<dxg:GridControl ItemsSource="{Binding Customers}"
                 AutoGenerateColumns="AddNew"
                 EnableSmartColumnsGeneration="True">
    <dxg:GridControl.Columns>
        <dxg:GridColumn FieldName="ProductName" Header="Product"/>
        <dxg:GridColumn FieldName="UnitPrice" Header="Price"/>
        <dxg:GridColumn FieldName="Quantity" Header="Qty"/>
    </dxg:GridControl.Columns>
    <dxg:GridControl.View>
        <dxg:TableView AllowPerPixelScrolling="True"
                       ShowFixedTotalSummary="True"/>
    </dxg:GridControl.View>
</dxg:GridControl>
```

## Key Properties
**GridControl**: `ItemsSource`, `SelectedItem`, `CurrentItem`, `FilterString`, `AutoGenerateColumns` (None/AddNew/RemoveOld).
**GridColumn**: `FieldName`, `Header`, `Width`, `ReadOnly`, `AllowEditing`, `EditSettings`, `SortOrder`, `GroupIndex`, `Visible`, `CellTemplate`.
**TableView**: `ShowAutoFilterRow`, `ShowSearchPanel`, `ShowGroupPanel`, `AllowEditing`, `AllowPerPixelScrolling`, `ShowFixedTotalSummary`, `AllowDragDrop`.

## MVVM Binding
```xml
<dxg:GridControl ItemsSource="{Binding Items}"
                 SelectedItem="{Binding SelectedItem, Mode=TwoWay}">
    <dxg:GridControl.View>
        <dxg:TableView AllowEditing="True"
                       RowDoubleClickCommand="{Binding EditCommand}"/>
    </dxg:GridControl.View>
</dxg:GridControl>
```

## Column Editor Settings
```xml
<dxg:GridColumn FieldName="Category">
    <dxg:GridColumn.EditSettings>
        <dxe:ComboBoxEditSettings ItemsSource="{Binding Categories}"
                                   DisplayMember="Name" ValueMember="Id"/>
    </dxg:GridColumn.EditSettings>
</dxg:GridColumn>
```

## Filtering & Searching
```xml
<dxg:TableView ShowAutoFilterRow="True" ShowSearchPanel="True"/>
<dxg:GridColumn FieldName="Name" AutoFilterCriteria="Contains"
                ImmediateUpdateAutoFilter="True"/>
```

## Grouping & Summary
```xml
<dxg:GridColumn FieldName="Department" GroupIndex="0"/>
<dxg:GridControl.TotalSummary>
    <dxg:GridSummaryItem FieldName="Price" SummaryType="Sum"
                         DisplayFormat="Total: {0:C}"/>
</dxg:GridControl.TotalSummary>
```

## Master-Detail
```xml
<dxg:GridControl ItemsSource="{Binding Orders}">
    <dxg:GridControl.DetailDescriptor>
        <dxg:DataControlDetailDescriptor ItemsSourceBinding="{Binding OrderDetails}">
            <dxg:GridControl>
                <dxg:GridControl.Columns>
                    <dxg:GridColumn FieldName="Product"/>
                    <dxg:GridColumn FieldName="Quantity"/>
                </dxg:GridControl.Columns>
            </dxg:GridControl>
        </dxg:DataControlDetailDescriptor>
    </dxg:GridControl.DetailDescriptor>
</dxg:GridControl>
```

## Conditional Formatting
```xml
<dxg:TableView.FormatConditions>
    <dxg:FormatCondition FieldName="Price" Expression="[Price] &gt; 1000">
        <dxg:FormatCondition.Format>
            <dx:Format Foreground="Red" FontWeight="Bold"/>
        </dxg:FormatCondition.Format>
    </dxg:FormatCondition>
</dxg:TableView.FormatConditions>
```

## Key Events
`CustomColumnSort`, `CustomRowFilter`, `CellValueChanged`, `ValidateCell`, `ValidateRow`, `SelectionChanged`, `RowDoubleClick`.

## Reference
- https://docs.devexpress.com/WPF/6084/controls-and-libraries/data-grid
