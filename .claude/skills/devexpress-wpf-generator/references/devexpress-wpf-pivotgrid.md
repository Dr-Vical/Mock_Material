# DevExpress WPF Pivot Grid

## Overview
- Multi-dimensional data analysis pivot table.
- Cross-tab summary, sorting, grouping, filtering via drag-drop.
- Server/OLAP/In-Memory/Async data modes.

## Key Classes
| Class | Description |
|-------|-------------|
| `PivotGridControl` | Main pivot grid control |
| `PivotGridField` | Field definition |
| `DataSourceColumnBinding` | Data source column binding |

- **xmlns**: `dxpg="http://schemas.devexpress.com/winfx/2008/xaml/pivotgrid"`
- **NuGet**: `DevExpress.Wpf.PivotGrid`

## Basic XAML
```xml
<dxpg:PivotGridControl DataSource="{Binding SalesData}">
    <dxpg:PivotGridControl.Fields>
        <dxpg:PivotGridField Area="RowArea" FieldName="Category" Caption="Category"/>
        <dxpg:PivotGridField Area="ColumnArea" FieldName="Year" Caption="Year"/>
        <dxpg:PivotGridField Area="DataArea" FieldName="Revenue" Caption="Revenue">
            <dxpg:PivotGridField.DataBinding>
                <dxpg:DataSourceColumnBinding ColumnName="Revenue"
                    SummaryType="Sum"/>
            </dxpg:PivotGridField.DataBinding>
        </dxpg:PivotGridField>
        <dxpg:PivotGridField Area="FilterArea" FieldName="Region"/>
    </dxpg:PivotGridControl.Fields>
</dxpg:PivotGridControl>
```

## Field Areas
`RowArea`, `ColumnArea`, `DataArea`, `FilterArea`.

## Key Properties
**PivotGridControl**: `DataSource`, `ShowRowTotals`, `ShowColumnTotals`, `ShowColumnGrandTotals`.
**PivotGridField**: `Area`, `FieldName`, `Caption`, `SummaryType` (Sum/Count/Average/Min/Max).

## Reference
- https://docs.devexpress.com/WPF/7225/controls-and-libraries/pivot-grid
