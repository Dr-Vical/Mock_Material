# DevExpress WPF Scheduler

## Overview
- Outlook-inspired calendar/scheduling control.
- Day, Week, WorkWeek, Month, Timeline, FullWeek views.
- Appointment CRUD, drag-drop, resize, recurrence, reminders.
- Resource-based grouping and filtering, MVVM support.

## Key Classes
| Class | Description |
|-------|-------------|
| `SchedulerControl` | Main scheduler control |
| `DataSource` | Data source container |
| `AppointmentMappings` | Appointment field mappings |
| `ResourceMappings` | Resource field mappings |
| `DayView` / `WeekView` / `MonthView` | View types |
| `TimelineView` | Timeline view |

- **xmlns**: `dxsch="http://schemas.devexpress.com/winfx/2008/xaml/scheduling"`
- **NuGet**: `DevExpress.Wpf.Scheduling`

## Basic XAML
```xml
<dxsch:SchedulerControl ActiveViewType="WorkWeek"
                         GroupType="Resource">
    <dxsch:SchedulerControl.DataSource>
        <dxsch:DataSource AppointmentsSource="{Binding Appointments}"
                          ResourcesSource="{Binding Resources}">
            <dxsch:DataSource.AppointmentMappings>
                <dxsch:AppointmentMappings Start="StartTime" End="EndTime"
                    Subject="Title" Description="Notes"
                    AllDay="IsAllDay" ResourceId="RoomId"
                    RecurrenceInfo="RecurrenceXml"/>
            </dxsch:DataSource.AppointmentMappings>
            <dxsch:DataSource.ResourceMappings>
                <dxsch:ResourceMappings Id="Id" Caption="Name"/>
            </dxsch:DataSource.ResourceMappings>
        </dxsch:DataSource>
    </dxsch:SchedulerControl.DataSource>

    <dxsch:SchedulerControl.Views>
        <dxsch:DayView/>
        <dxsch:WorkWeekView/>
        <dxsch:WeekView/>
        <dxsch:MonthView/>
        <dxsch:TimelineView/>
    </dxsch:SchedulerControl.Views>
</dxsch:SchedulerControl>
```

## Key Properties
**SchedulerControl**: `ActiveViewType`, `GroupType` (None/Date/Resource), `Start`, `SelectedAppointments`.
**DayView**: `ShowWorkTimeOnly`, `WorkTime`, `TimeScale`.
**MonthView**: `WeekCount`, `ShowWeekNumbers`.

## MVVM Commands
```xml
<dxsch:SchedulerControl AppointmentAdding="{Binding AddCommand}"
                         AppointmentEditing="{Binding EditCommand}"
                         AppointmentRemoving="{Binding RemoveCommand}"/>
```

## Reference
- https://docs.devexpress.com/WPF/113556/controls-and-libraries/scheduler
