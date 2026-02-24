# DevExpress WPF Charts Suite

## Overview
- High-performance 2D/3D chart control for data visualization.
- Bar, Line, Area, Pie, Financial, Radar, Polar, Funnel, Waterfall chart types.
- Zoom/scroll, crosshair, real-time update support.

## Key Classes
| Class | Description |
|-------|-------------|
| `ChartControl` | 2D chart control |
| `Chart3DControl` | 3D chart (Surface, 3D Bar) |
| `XYDiagram2D` | 2D coordinate diagram |
| `Series` | Series base class |
| `Legend` | Chart legend |

- **xmlns**: `dxc="http://schemas.devexpress.com/winfx/2008/xaml/charts"`
- **NuGet**: `DevExpress.Wpf.Charts`

## Basic Bar Chart
```xml
<dxc:ChartControl>
    <dxc:ChartControl.Diagram>
        <dxc:XYDiagram2D>
            <dxc:XYDiagram2D.Series>
                <dxc:BarSideBySideSeries2D DisplayName="Sales"
                    ArgumentDataMember="Month" ValueDataMember="Amount"
                    DataSource="{Binding SalesData}"/>
            </dxc:XYDiagram2D.Series>
        </dxc:XYDiagram2D>
    </dxc:ChartControl.Diagram>
    <dxc:ChartControl.Legend>
        <dxc:Legend Visibility="Visible"/>
    </dxc:ChartControl.Legend>
</dxc:ChartControl>
```

## Line Chart with Multiple Series
```xml
<dxc:ChartControl>
    <dxc:ChartControl.Diagram>
        <dxc:XYDiagram2D>
            <dxc:XYDiagram2D.Series>
                <dxc:LineSeries2D DisplayName="Revenue"
                    ArgumentDataMember="Date" ValueDataMember="Revenue"
                    DataSource="{Binding Data}"/>
                <dxc:LineSeries2D DisplayName="Profit"
                    ArgumentDataMember="Date" ValueDataMember="Profit"
                    DataSource="{Binding Data}"/>
            </dxc:XYDiagram2D.Series>
        </dxc:XYDiagram2D>
    </dxc:ChartControl.Diagram>
</dxc:ChartControl>
```

## Pie Chart
```xml
<dxc:ChartControl>
    <dxc:ChartControl.Diagram>
        <dxc:SimpleDiagram2D>
            <dxc:SimpleDiagram2D.Series>
                <dxc:PieSeries2D ArgumentDataMember="Category"
                    ValueDataMember="Share" DataSource="{Binding Data}"
                    LabelsVisibility="True"/>
            </dxc:SimpleDiagram2D.Series>
        </dxc:SimpleDiagram2D>
    </dxc:ChartControl.Diagram>
</dxc:ChartControl>
```

## Key Properties
**ChartControl**: `AnimationMode`, `CrosshairEnabled`, `ToolTipEnabled`.
**Series**: `ArgumentDataMember`, `ValueDataMember`, `DataSource`, `LabelsVisibility`.
**XYDiagram2D**: `EnableAxisXNavigation`, `EnableAxisYNavigation` (zoom/scroll).

## Reference
- https://docs.devexpress.com/WPF/6229/controls-and-libraries/charts-suite
