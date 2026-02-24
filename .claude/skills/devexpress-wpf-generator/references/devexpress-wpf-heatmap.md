# DevExpress WPF Heatmap Control

## Overview
- Visualize relationships between two variables using color gradients.
- DataSourceAdapter and MatrixAdapter modes.
- Axis, legend, color scale, cell labels, tooltips.
- Zoom/scroll, cell highlight, cell selection, print/export.

## Key Classes
| Class | Description |
|-------|-------------|
| `HeatmapControl` | Main heatmap control |
| `HeatmapDataSourceAdapter` | External data source adapter |
| `HeatmapMatrixAdapter` | 2D array matrix adapter |
| `HeatmapPaletteColorProvider` | Palette color provider |
| `HeatmapRangeColorProvider` | Range-based color provider |

- **xmlns**: `dxht="http://schemas.devexpress.com/winfx/2008/xaml/charts/heatmap"`
- **NuGet**: `DevExpress.Wpf.Charts`

## DataSource Adapter
```xml
<dxht:HeatmapControl>
    <dxht:HeatmapControl.DataAdapter>
        <dxht:HeatmapDataSourceAdapter DataSource="{Binding HeatmapData}"
            XArgumentDataMember="Month" YArgumentDataMember="Product"
            ValueDataMember="Sales"/>
    </dxht:HeatmapControl.DataAdapter>
    <dxht:HeatmapControl.ColorProvider>
        <dxht:HeatmapPaletteColorProvider>
            <dxht:HeatmapPaletteColorProvider.Palette>
                <dxht:HeatmapPalette>
                    <dxht:HeatmapPaletteItem Color="Blue" Value="0"/>
                    <dxht:HeatmapPaletteItem Color="Red" Value="100"/>
                </dxht:HeatmapPalette>
            </dxht:HeatmapPaletteColorProvider.Palette>
        </dxht:HeatmapPaletteColorProvider>
    </dxht:HeatmapControl.ColorProvider>
</dxht:HeatmapControl>
```

## Key Properties
**HeatmapControl**: `LabelsVisibility`, `ToolTipEnabled`, `EnableSelection`.
**HeatmapDataSourceAdapter**: `XArgumentDataMember`, `YArgumentDataMember`, `ValueDataMember`.

## Reference
- https://docs.devexpress.com/WPF/401640/controls-and-libraries/charts-suite/heatmap
