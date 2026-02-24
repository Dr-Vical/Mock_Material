# DevExpress WPF Diagram Control

## Overview
- Create, display, edit diagrams with 130+ built-in shapes.
- Auto-layout (Tree, Sugiyama/Layered), Visio-style themes.
- Data binding, MVVM support, print/export (PNG, JPEG, BMP).

## Key Classes
| Class | Description |
|-------|-------------|
| `DiagramControl` | Diagram canvas (code-based) |
| `DiagramDesignerControl` | Visual designer (with toolbox) |
| `DiagramShape` | Shape node |
| `DiagramConnector` | Connector line |
| `DiagramContainer` | Group container |

- **xmlns**: `dxdiag="http://schemas.devexpress.com/winfx/2008/xaml/diagram"`
- **NuGet**: `DevExpress.Wpf.Diagram`

## Basic XAML
```xml
<dxdiag:DiagramControl>
    <dxdiag:DiagramShape Id="start" Position="100,50"
        Width="120" Height="60" Content="Start"
        Shape="{x:Static dxdiag:BasicShapes.Ellipse}"/>
    <dxdiag:DiagramShape Id="process" Position="100,200"
        Width="120" Height="60" Content="Process"
        Shape="{x:Static dxdiag:BasicShapes.Rectangle}"/>
    <dxdiag:DiagramConnector BeginItemId="start" EndItemId="process"
        Type="RightAngle"/>
</dxdiag:DiagramControl>
```

## Data-Bound Diagram (MVVM)
```xml
<dxdiag:DiagramControl>
    <dxmvvm:Interaction.Behaviors>
        <dxdiag:DiagramDataBindingBehavior
            ItemsSource="{Binding Nodes}"
            ItemTemplateSelector="{StaticResource NodeSelector}"
            ConnectorsSource="{Binding Connections}"
            ConnectorFromMember="SourceId" ConnectorToMember="TargetId"/>
    </dxmvvm:Interaction.Behaviors>
</dxdiag:DiagramControl>
```

## Reference
- https://docs.devexpress.com/WPF/115216/controls-and-libraries/diagram-control
