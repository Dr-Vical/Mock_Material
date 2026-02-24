# DevExpress WPF Gantt Control

## Overview
- Project management Gantt chart: task tree + timeline chart.
- Task hierarchy, dependencies, resource assignment.
- Based on TreeListControl (inherits grid features).
- Critical path highlighting, timescale customization.

## Key Classes
| Class | Description |
|-------|-------------|
| `GanttControl` | Main Gantt control |
| `GanttView` | Gantt view (inherits TreeListView) |
| `GanttTaskMappings` | Task field mappings |
| `GanttResourceMappings` | Resource field mappings |
| `GanttPredecessorLinkMappings` | Dependency mappings |

- **xmlns**: `dxgn="http://schemas.devexpress.com/winfx/2008/xaml/gantt"`
- **NuGet**: `DevExpress.Wpf.Gantt`

## Basic XAML
```xml
<dxgn:GanttControl ItemsSource="{Binding Tasks}">
    <dxgn:GanttControl.View>
        <dxgn:GanttView ShowBaseline="True"
                          CriticalPathEnabled="True"
                          AutoExpandAllNodes="True">
            <dxgn:GanttView.TaskMappings>
                <dxgn:GanttTaskMappings Name="TaskName"
                    StartDate="Start" FinishDate="Finish"
                    Progress="PercentComplete" ParentId="ParentId" Id="Id"/>
            </dxgn:GanttView.TaskMappings>
            <dxgn:GanttView.PredecessorLinkMappings>
                <dxgn:GanttPredecessorLinkMappings PredecessorId="PredId"
                    LinkType="Type"/>
            </dxgn:GanttView.PredecessorLinkMappings>
        </dxgn:GanttView>
    </dxgn:GanttControl.View>
    <dxgn:GanttControl.Columns>
        <dxg:GridColumn FieldName="TaskName" Header="Task"/>
        <dxg:GridColumn FieldName="Start" Header="Start"/>
        <dxg:GridColumn FieldName="Finish" Header="Finish"/>
    </dxgn:GanttControl.Columns>
</dxgn:GanttControl>
```

## Key Properties
**GanttView**: `ShowBaseline`, `CriticalPathEnabled`, `AutoExpandAllNodes`.
**GanttTaskMappings**: `Name`, `StartDate`, `FinishDate`, `Progress`, `ParentId`, `Id`.

## Reference
- https://docs.devexpress.com/WPF/401464/controls-and-libraries/gantt-control
