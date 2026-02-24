# DevExpress WPF TreeMap Control

## Overview
- Visualize hierarchical data as nested rectangles (area proportional to value).
- Grouping, colorizers, layout algorithms, tooltips, selection.

## Key Classes
| Class | Description |
|-------|-------------|
| `TreeMapControl` | Main treemap control |
| `TreeMapFlatDataAdapter` | Flat data binding |
| `TreeMapHierarchicalDataAdapter` | Hierarchical data binding |
| `TreeMapPaletteColorizer` | Palette colorizer |

- **xmlns**: `dxtm="http://schemas.devexpress.com/winfx/2008/xaml/treemap"`
- **NuGet**: `DevExpress.Wpf.TreeMap`

## Flat Data Binding
```xml
<dxtm:TreeMapControl>
    <dxtm:TreeMapControl.DataAdapter>
        <dxtm:TreeMapFlatDataAdapter DataSource="{Binding SalesData}"
            ValueDataMember="Revenue" LabelDataMember="ProductName">
            <dxtm:TreeMapFlatDataAdapter.GroupDefinitions>
                <dxtm:TreeMapGroupDefinition GroupDataMember="Category"/>
            </dxtm:TreeMapFlatDataAdapter.GroupDefinitions>
        </dxtm:TreeMapFlatDataAdapter>
    </dxtm:TreeMapControl.DataAdapter>
    <dxtm:TreeMapControl.Colorizer>
        <dxtm:TreeMapPaletteColorizer/>
    </dxtm:TreeMapControl.Colorizer>
</dxtm:TreeMapControl>
```

## Layout Algorithms
`SquarifiedLayoutAlgorithm` (default), `SliceAndDiceLayoutAlgorithm`, `StripedLayoutAlgorithm`.

## Reference
- https://docs.devexpress.com/WPF/116694/controls-and-libraries/treemap-control
