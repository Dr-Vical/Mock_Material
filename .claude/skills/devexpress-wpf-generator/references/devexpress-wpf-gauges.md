# DevExpress WPF Gauge Controls

## Overview
- Circular, Linear, Digital gauge controls for industrial dashboards.
- Predefined models/themes, ranges, markers, needles, level bars.
- Interactive (drag to change value).

## Key Classes
| Class | Description |
|-------|-------------|
| `CircularGaugeControl` | Circular gauge |
| `LinearGaugeControl` | Linear gauge |
| `DigitalGaugeControl` | Digital LED gauge |
| `StateIndicatorControl` | State indicator |
| `ArcScale` / `LinearScale` | Scale definitions |

- **xmlns**: `dxga="http://schemas.devexpress.com/winfx/2008/xaml/gauges"`
- **NuGet**: `DevExpress.Wpf.Gauges`

## Circular Gauge
```xml
<dxga:CircularGaugeControl>
    <dxga:CircularGaugeControl.Model>
        <dxga:CircularFullModel/>
    </dxga:CircularGaugeControl.Model>
    <dxga:CircularGaugeControl.Scales>
        <dxga:ArcScale StartValue="0" EndValue="100" MajorIntervalCount="10">
            <dxga:ArcScale.Needles>
                <dxga:ArcScaleNeedle Value="{Binding Temperature}"/>
            </dxga:ArcScale.Needles>
            <dxga:ArcScale.Ranges>
                <dxga:ArcScaleRange StartValue="0" EndValue="30" Fill="Green"/>
                <dxga:ArcScaleRange StartValue="30" EndValue="70" Fill="Yellow"/>
                <dxga:ArcScaleRange StartValue="70" EndValue="100" Fill="Red"/>
            </dxga:ArcScale.Ranges>
        </dxga:ArcScale>
    </dxga:CircularGaugeControl.Scales>
</dxga:CircularGaugeControl>
```

## Linear Gauge
```xml
<dxga:LinearGaugeControl>
    <dxga:LinearGaugeControl.Model>
        <dxga:LinearVerticalModel/>
    </dxga:LinearGaugeControl.Model>
    <dxga:LinearGaugeControl.Scales>
        <dxga:LinearScale StartValue="0" EndValue="100">
            <dxga:LinearScale.LevelBars>
                <dxga:LinearScaleLevelBar Value="{Binding Level}"/>
            </dxga:LinearScale.LevelBars>
        </dxga:LinearScale>
    </dxga:LinearGaugeControl.Scales>
</dxga:LinearGaugeControl>
```

## Digital Gauge
```xml
<dxga:DigitalGaugeControl>
    <dxga:DigitalGaugeControl.Model>
        <dxga:DigitalDefaultModel/>
    </dxga:DigitalGaugeControl.Model>
    <dxga:DigitalGaugeControl.Layers>
        <dxga:DigitalGaugeLayer Text="{Binding DisplayValue}"/>
    </dxga:DigitalGaugeControl.Layers>
</dxga:DigitalGaugeControl>
```

## Reference
- https://docs.devexpress.com/WPF/6188/controls-and-libraries/gauge-controls
