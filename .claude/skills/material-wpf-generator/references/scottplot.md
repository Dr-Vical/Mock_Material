# ScottPlot 5 WPF Reference for Real-Time Monitoring & Oscilloscope Views

> Target: .NET 8 / WPF / CommunityToolkit.Mvvm / MaterialDesignInXamlToolkit
> Version: ScottPlot.WPF 5.1.x (latest stable: 5.1.57)

---

## 1. NuGet Package

```xml
<PackageReference Include="ScottPlot.WPF" Version="5.1.57" />
```

The `ScottPlot.WPF` package includes the core `ScottPlot` library as a dependency. No separate ScottPlot package reference is needed.

---

## 2. XMLNS Namespace in XAML

```xml
xmlns:scottPlot="clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF"
```

> **Important:** In ScottPlot 5, the namespace is `ScottPlot.WPF` (not `ScottPlot` as in v4). The assembly name is also `ScottPlot.WPF`.

---

## 3. WpfPlot Control -- XAML Usage

### Basic Declaration

```xml
<scottPlot:WpfPlot x:Name="WpfPlot1" />
```

### Inside a Grid Layout

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <scottPlot:WpfPlot x:Name="PlotPosition" Grid.Row="0" Margin="4" />
    <scottPlot:WpfPlot x:Name="PlotVelocity" Grid.Row="1" Margin="4" />
</Grid>
```

### Key Properties & Methods

| Member | Type | Description |
|--------|------|-------------|
| `Plot` | `ScottPlot.Plot` | The core Plot object -- all plot configuration goes through this |
| `Refresh()` | method | Request the control to re-render (call after data changes) |
| `UserInputProcessor` | object | Controls mouse/keyboard interaction (pan, zoom, etc.) |
| `UserInputProcessor.IsEnabled` | bool | Enable/disable all user interaction |

---

## 4. Plot Types for Servo Drive Monitoring

### 4.1 Signal Plot (High-Performance Time Series)

Best for **evenly-spaced data** (fixed sample rate). Renders millions of points efficiently.

```csharp
// Create data array
double[] velocityData = new double[10000];

// Add signal plot (assumes equal spacing on X axis)
var sig = WpfPlot1.Plot.Add.Signal(velocityData);
sig.Color = ScottPlot.Color.FromHex("#42A5F5");
sig.LineWidth = 1.5f;
sig.LegendText = "Velocity [rpm]";

// Customize sample rate (default period = 1.0)
sig.Data.Period = 0.001; // 1ms sample interval -> X axis in seconds

// Render only a portion of data
sig.Data.MinimumIndex = 0;
sig.Data.MaximumIndex = 5000;

WpfPlot1.Refresh();
```

### 4.2 Scatter Plot (Parameter Correlation)

For **X-Y paired data** where X values are not evenly spaced.

```csharp
double[] current = { 0.5, 1.0, 1.5, 2.0, 2.5, 3.0 };
double[] torque  = { 0.3, 0.6, 0.85, 1.1, 1.4, 1.7 };

var scatter = WpfPlot1.Plot.Add.Scatter(current, torque);
scatter.Color = ScottPlot.Color.FromHex("#EF5350");
scatter.LineWidth = 2;
scatter.MarkerSize = 6;
scatter.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
scatter.LegendText = "Torque vs Current";

WpfPlot1.Plot.Axes.Bottom.Label.Text = "Current [A]";
WpfPlot1.Plot.Axes.Left.Label.Text = "Torque [Nm]";

WpfPlot1.Refresh();
```

### 4.3 Multi-Axis (Velocity + Current on Same Chart)

Display different Y scales on left and right axes.

```csharp
// Velocity on left Y axis
double[] velocityData = GenerateVelocityData();
var sigVelocity = WpfPlot1.Plot.Add.Signal(velocityData);
sigVelocity.Axes.YAxis = WpfPlot1.Plot.Axes.Left;
sigVelocity.Color = ScottPlot.Color.FromHex("#42A5F5");
sigVelocity.LegendText = "Velocity [rpm]";

// Current on right Y axis
double[] currentData = GenerateCurrentData();
var sigCurrent = WpfPlot1.Plot.Add.Signal(currentData);
sigCurrent.Axes.YAxis = WpfPlot1.Plot.Axes.Right;
sigCurrent.Color = ScottPlot.Color.FromHex("#EF5350");
sigCurrent.LegendText = "Current [A]";

// Style axes with matching colors
WpfPlot1.Plot.Axes.Left.Label.Text = "Velocity [rpm]";
WpfPlot1.Plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex("#42A5F5");
WpfPlot1.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#42A5F5");

WpfPlot1.Plot.Axes.Right.Label.Text = "Current [A]";
WpfPlot1.Plot.Axes.Right.Label.ForeColor = ScottPlot.Color.FromHex("#EF5350");
WpfPlot1.Plot.Axes.Right.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#EF5350");

WpfPlot1.Refresh();
```

### 4.4 DataLogger -- Growing Dataset (Data Logging Mode)

For sensor data that continuously grows. X values must be monotonically increasing.

```csharp
// Add a DataLogger plottable
var logger = WpfPlot1.Plot.Add.DataLogger();
logger.Color = ScottPlot.Color.FromHex("#66BB6A");
logger.LegendText = "Position [pulse]";

// Enable automatic axis adjustment to keep new data in view
logger.ManageAxisLimits = true;

// Add data points as they arrive
logger.Add(x: 0.0, y: 100.0);
logger.Add(x: 0.1, y: 105.2);
logger.Add(x: 0.2, y: 112.8);
// ... continues growing

WpfPlot1.Refresh();
```

### 4.5 DataStreamer -- Fixed-Width Buffer (Oscilloscope Mode)

Fixed display width with circular buffer behavior. Old data scrolls/wraps out.

```csharp
// Create a DataStreamer displaying 1000 points with 1ms period
var streamer = WpfPlot1.Plot.Add.DataStreamer(points: 1000, period: 0.001);
streamer.Color = ScottPlot.Color.FromHex("#FFA726");
streamer.LegendText = "Current [A]";

// Enable automatic Y axis scaling
streamer.ManageAxisLimits = true;

// View mode options:
streamer.ViewScrollLeft();   // new data on right, old scrolls left (oscilloscope style)
// streamer.ViewWipeRight();  // new data overwrites from left to right (ECG style)

// Add new samples (single or batch)
streamer.Add(1.23);
streamer.AddRange(new double[] { 1.24, 1.25, 1.26 });

WpfPlot1.Refresh();
```

---

## 5. Real-Time Data Update Pattern

### 5.1 Timer-Based Update Architecture

Use **two timers**: one for data acquisition, one for rendering.

```csharp
public partial class MonitorView : UserControl
{
    private ScottPlot.Plottables.DataStreamer _streamerPosition;
    private ScottPlot.Plottables.DataStreamer _streamerVelocity;
    private ScottPlot.Plottables.DataStreamer _streamerCurrent;
    private ScottPlot.Plottables.DataStreamer _streamerTorque;

    private System.Timers.Timer _dataTimer;    // Data acquisition (background)
    private System.Windows.Threading.DispatcherTimer _renderTimer; // UI render

    public MonitorView()
    {
        InitializeComponent();
        SetupPlot();
        StartTimers();
    }

    private void SetupPlot()
    {
        _streamerPosition = PlotMain.Plot.Add.DataStreamer(points: 1000, period: 0.001);
        _streamerVelocity = PlotMain.Plot.Add.DataStreamer(points: 1000, period: 0.001);
        _streamerCurrent  = PlotMain.Plot.Add.DataStreamer(points: 1000, period: 0.001);
        _streamerTorque   = PlotMain.Plot.Add.DataStreamer(points: 1000, period: 0.001);

        _streamerPosition.ViewScrollLeft();
        _streamerVelocity.ViewScrollLeft();
        _streamerCurrent.ViewScrollLeft();
        _streamerTorque.ViewScrollLeft();
    }

    private void StartTimers()
    {
        // Data acquisition timer (10ms interval, background thread)
        _dataTimer = new System.Timers.Timer(10);
        _dataTimer.Elapsed += OnDataTimerElapsed;
        _dataTimer.Start();

        // Render timer (50ms = 20 FPS, UI thread)
        _renderTimer = new System.Windows.Threading.DispatcherTimer();
        _renderTimer.Interval = TimeSpan.FromMilliseconds(50);
        _renderTimer.Tick += OnRenderTimerTick;
        _renderTimer.Start();
    }

    private void OnDataTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        // Thread-safe data update using Plot.Sync lock
        lock (PlotMain.Plot.Sync)
        {
            double position = ReadPositionFromDrive();
            double velocity = ReadVelocityFromDrive();
            double current  = ReadCurrentFromDrive();
            double torque   = ReadTorqueFromDrive();

            _streamerPosition.Add(position);
            _streamerVelocity.Add(velocity);
            _streamerCurrent.Add(current);
            _streamerTorque.Add(torque);
        }
    }

    private void OnRenderTimerTick(object? sender, EventArgs e)
    {
        PlotMain.Refresh();
    }
}
```

### 5.2 Thread Safety Summary

| Rule | Detail |
|------|--------|
| **Lock on `Plot.Sync`** | Wrap all data modifications in `lock (wpfPlot.Plot.Sync) { ... }` |
| **Refresh on UI thread** | Call `WpfPlot.Refresh()` only from UI (dispatcher) thread |
| **Separate timers** | Use `System.Timers.Timer` for data, `DispatcherTimer` for render |
| **Never block render** | Keep data operations fast inside the lock |

### 5.3 Signal Plot with Dynamic Array Update

For manually managing data arrays with a Signal plot:

```csharp
private double[] _data = new double[10000];
private int _writeIndex = 0;
private ScottPlot.Plottables.Signal _signal;

private void SetupSignalPlot()
{
    _signal = WpfPlot1.Plot.Add.Signal(_data);
    _signal.Data.Period = 0.001; // 1kHz sample rate
    _signal.Data.MaximumIndex = 0; // initially show no data
}

private void AddDataPoint(double value)
{
    lock (WpfPlot1.Plot.Sync)
    {
        if (_writeIndex < _data.Length)
        {
            _data[_writeIndex] = value;
            _writeIndex++;
            _signal.Data.MaximumIndex = _writeIndex;
        }
    }
}
```

---

## 6. Oscilloscope-Style Features

### 6.1 Trigger Modes (Application-Level)

ScottPlot does not have built-in trigger logic. Implement in your ViewModel:

```csharp
public enum TriggerMode
{
    Continuous,   // Always running
    Single,       // Capture one buffer then stop
    Normal        // Capture on trigger condition, wait for next
}

public enum TriggerEdge
{
    Rising,
    Falling
}

private TriggerMode _triggerMode = TriggerMode.Continuous;
private TriggerEdge _triggerEdge = TriggerEdge.Rising;
private double _triggerLevel = 0.0;
private bool _triggered = false;
private bool _armed = true;

private void ProcessSample(double sample, double previousSample)
{
    switch (_triggerMode)
    {
        case TriggerMode.Continuous:
            _streamer.Add(sample);
            break;

        case TriggerMode.Single:
            if (_armed && CheckTrigger(sample, previousSample))
            {
                _triggered = true;
                _armed = false; // Disarm after single capture
            }
            if (_triggered)
                _streamer.Add(sample);
            break;

        case TriggerMode.Normal:
            if (!_triggered && CheckTrigger(sample, previousSample))
                _triggered = true;
            if (_triggered)
                _streamer.Add(sample);
            break;
    }
}

private bool CheckTrigger(double current, double previous)
{
    return _triggerEdge switch
    {
        TriggerEdge.Rising  => previous < _triggerLevel && current >= _triggerLevel,
        TriggerEdge.Falling => previous > _triggerLevel && current <= _triggerLevel,
        _ => false
    };
}
```

### 6.2 Time-Base Scaling

```csharp
// Change visible time window by adjusting DataStreamer point count and period
// Or use manual axis limits:

// Show 100ms window at 1kHz sample rate
WpfPlot1.Plot.Axes.SetLimitsX(left: 0, right: 0.1); // 0 to 100ms

// Show 1s window
WpfPlot1.Plot.Axes.SetLimitsX(left: 0, right: 1.0);

// Time/div scaling (e.g., 10ms/div with 10 divisions)
double timePerDiv = 0.010; // 10ms
int divisions = 10;
WpfPlot1.Plot.Axes.SetLimitsX(left: 0, right: timePerDiv * divisions);
```

### 6.3 Multiple Channels Overlay

```csharp
var ch1 = WpfPlot1.Plot.Add.DataStreamer(points: 1000, period: 0.001);
ch1.Color = ScottPlot.Color.FromHex("#42A5F5"); // Blue
ch1.LegendText = "CH1: Position";
ch1.ViewScrollLeft();

var ch2 = WpfPlot1.Plot.Add.DataStreamer(points: 1000, period: 0.001);
ch2.Color = ScottPlot.Color.FromHex("#EF5350"); // Red
ch2.LegendText = "CH2: Velocity";
ch2.ViewScrollLeft();

var ch3 = WpfPlot1.Plot.Add.DataStreamer(points: 1000, period: 0.001);
ch3.Color = ScottPlot.Color.FromHex("#66BB6A"); // Green
ch3.LegendText = "CH3: Current";
ch3.ViewScrollLeft();

var ch4 = WpfPlot1.Plot.Add.DataStreamer(points: 1000, period: 0.001);
ch4.Color = ScottPlot.Color.FromHex("#FFA726"); // Orange
ch4.LegendText = "CH4: Torque";
ch4.ViewScrollLeft();
```

### 6.4 Crosshair for Measurement

```csharp
// Add crosshair
var crosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);
crosshair.LineWidth = 1;
crosshair.LineColor = ScottPlot.Colors.Yellow;
crosshair.HorizontalLine.LinePattern = ScottPlot.LinePattern.Dotted;
crosshair.VerticalLine.LinePattern = ScottPlot.LinePattern.Dotted;

// Update crosshair position on mouse move
WpfPlot1.MouseMove += (s, e) =>
{
    // Get mouse position in plot coordinates
    var position = WpfPlot1.Plot.GetCoordinates(
        new ScottPlot.Pixel(e.GetPosition(WpfPlot1).X, e.GetPosition(WpfPlot1).Y));

    lock (WpfPlot1.Plot.Sync)
    {
        crosshair.Position = position;
    }

    WpfPlot1.Refresh();
};
```

### 6.5 Zoom and Pan Control

```csharp
// Enable/disable all interactions
WpfPlot1.UserInputProcessor.IsEnabled = true;  // or false

// Lock axes to prevent user zoom/pan (read-only oscilloscope)
WpfPlot1.Plot.Axes.AutoScale();
WpfPlot1.UserInputProcessor.IsEnabled = false;

// Manual zoom/pan in code
WpfPlot1.Plot.Axes.SetLimitsX(0, 1.0);       // Set X range
WpfPlot1.Plot.Axes.SetLimitsY(-10, 10);       // Set Y range
WpfPlot1.Plot.Axes.AutoScale();                // Auto-fit all data
WpfPlot1.Plot.Axes.AutoScaleX();               // Auto-fit X only
WpfPlot1.Plot.Axes.AutoScaleY();               // Auto-fit Y only
```

### 6.6 Trigger Level Line (Visual Indicator)

```csharp
var triggerLine = WpfPlot1.Plot.Add.HorizontalLine(triggerLevel);
triggerLine.Color = ScottPlot.Colors.Yellow;
triggerLine.LineWidth = 1;
triggerLine.LinePattern = ScottPlot.LinePattern.Dashed;
triggerLine.Text = "Trigger";
// triggerLine.IsDraggable = true; // Allow user to drag trigger level
```

---

## 7. Styling & Theming

### 7.1 Dark Theme Setup

```csharp
public static void ApplyDarkTheme(ScottPlot.Plot plot)
{
    // Figure and data area backgrounds
    plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");
    plot.DataBackground.Color = ScottPlot.Color.FromHex("#121212");

    // Axis colors (labels, ticks, frame)
    plot.Axes.Color(ScottPlot.Color.FromHex("#B0B0B0"));

    // Grid lines
    plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");
    plot.Grid.MinorLineColor = ScottPlot.Color.FromHex("#1A1A1A");

    // Legend styling
    plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#2D2D2D");
    plot.Legend.FontColor = ScottPlot.Color.FromHex("#E0E0E0");
    plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#555555");
}
```

### 7.2 Custom Color Palette for Multiple Channels

```csharp
// Define custom oscilloscope-style channel colors
public static class OscilloscopeColors
{
    // Standard oscilloscope channel colors
    public static readonly ScottPlot.Color CH1 = ScottPlot.Color.FromHex("#42A5F5"); // Blue
    public static readonly ScottPlot.Color CH2 = ScottPlot.Color.FromHex("#EF5350"); // Red
    public static readonly ScottPlot.Color CH3 = ScottPlot.Color.FromHex("#66BB6A"); // Green
    public static readonly ScottPlot.Color CH4 = ScottPlot.Color.FromHex("#FFA726"); // Orange
    public static readonly ScottPlot.Color CH5 = ScottPlot.Color.FromHex("#AB47BC"); // Purple
    public static readonly ScottPlot.Color CH6 = ScottPlot.Color.FromHex("#26C6DA"); // Cyan

    // Servo monitoring colors (semantic)
    public static readonly ScottPlot.Color Position = ScottPlot.Color.FromHex("#42A5F5");
    public static readonly ScottPlot.Color Velocity = ScottPlot.Color.FromHex("#66BB6A");
    public static readonly ScottPlot.Color Current  = ScottPlot.Color.FromHex("#EF5350");
    public static readonly ScottPlot.Color Torque   = ScottPlot.Color.FromHex("#FFA726");
}
```

### 7.3 Axis Label Styling

```csharp
// Left Y axis
plot.Axes.Left.Label.Text = "Velocity [rpm]";
plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex("#42A5F5");
plot.Axes.Left.Label.FontSize = 14;
plot.Axes.Left.Label.Bold = true;

// Bottom X axis
plot.Axes.Bottom.Label.Text = "Time [s]";
plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex("#B0B0B0");
plot.Axes.Bottom.Label.FontSize = 14;

// Tick label font color
plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#42A5F5");
plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#B0B0B0");

// Right Y axis (for dual-axis)
plot.Axes.Right.Label.Text = "Current [A]";
plot.Axes.Right.Label.ForeColor = ScottPlot.Color.FromHex("#EF5350");
```

### 7.4 Legend Configuration

```csharp
// Show legend
plot.ShowLegend();

// Position: Alignment enum (UpperLeft, UpperRight, LowerLeft, LowerRight, etc.)
plot.Legend.Alignment = ScottPlot.Alignment.UpperRight;
plot.Legend.Orientation = ScottPlot.Orientation.Vertical;

// Styling
plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#2D2D2D");
plot.Legend.FontColor = ScottPlot.Color.FromHex("#E0E0E0");
plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#555555");
plot.Legend.FontSize = 12;

// Manual legend items (for custom entries)
plot.Legend.ManualItems.Add(new ScottPlot.LegendItem
{
    LabelText = "Position [pulse]",
    LineColor = ScottPlot.Color.FromHex("#42A5F5"),
    LineWidth = 2
});
```

---

## 8. MVVM Integration

ScottPlot does not natively support XAML data binding for plot content. The recommended MVVM pattern uses a `ContentControl` binding or direct code-behind access.

### Pattern A: ContentControl Binding (Recommended)

**ViewModel:**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot.WPF;

public partial class MonitorViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private WpfPlot _plotControl;

    private ScottPlot.Plottables.DataStreamer _chPosition;
    private ScottPlot.Plottables.DataStreamer _chVelocity;
    private ScottPlot.Plottables.DataStreamer _chCurrent;
    private ScottPlot.Plottables.DataStreamer _chTorque;

    private System.Timers.Timer _dataTimer;
    private System.Windows.Threading.DispatcherTimer _renderTimer;

    [ObservableProperty]
    private bool _isRunning;

    public MonitorViewModel()
    {
        PlotControl = new WpfPlot();
        InitializePlot();
    }

    private void InitializePlot()
    {
        var plot = PlotControl.Plot;

        // Dark theme
        plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");
        plot.DataBackground.Color = ScottPlot.Color.FromHex("#121212");
        plot.Axes.Color(ScottPlot.Color.FromHex("#B0B0B0"));
        plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");

        // Add channels
        _chPosition = plot.Add.DataStreamer(points: 1000, period: 0.001);
        _chPosition.Color = ScottPlot.Color.FromHex("#42A5F5");
        _chPosition.LegendText = "Position";
        _chPosition.ViewScrollLeft();
        _chPosition.ManageAxisLimits = true;

        _chVelocity = plot.Add.DataStreamer(points: 1000, period: 0.001);
        _chVelocity.Color = ScottPlot.Color.FromHex("#66BB6A");
        _chVelocity.LegendText = "Velocity";
        _chVelocity.ViewScrollLeft();
        _chVelocity.ManageAxisLimits = true;

        // ... similar for current, torque

        // Legend
        plot.ShowLegend();
        plot.Legend.Alignment = ScottPlot.Alignment.UpperRight;
        plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#2D2D2D");
        plot.Legend.FontColor = ScottPlot.Color.FromHex("#E0E0E0");
        plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#555555");
    }

    [RelayCommand]
    private void StartMonitoring()
    {
        IsRunning = true;
        // Start timers...
    }

    [RelayCommand]
    private void StopMonitoring()
    {
        IsRunning = false;
        // Stop timers...
    }

    public void Dispose()
    {
        _dataTimer?.Dispose();
        _renderTimer?.Stop();
    }
}
```

**XAML:**

```xml
<UserControl
    xmlns:scottPlot="clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF">

    <!-- Bind WpfPlot from ViewModel -->
    <ContentControl Content="{Binding PlotControl, Mode=OneTime}" />
</UserControl>
```

### Pattern B: x:Name with Code-Behind Relay (Minimal Code-Behind)

**XAML:**

```xml
<UserControl x:Class="MyApp.Views.MonitorView"
    xmlns:scottPlot="clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF">

    <scottPlot:WpfPlot x:Name="WpfPlotMain" />
</UserControl>
```

**Code-Behind (Relay Only):**

```csharp
public partial class MonitorView : UserControl
{
    public MonitorView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is MonitorViewModel vm)
        {
            vm.Initialize(WpfPlotMain);
        }
    }
}
```

**ViewModel:**

```csharp
public partial class MonitorViewModel : ObservableObject
{
    private ScottPlot.WPF.WpfPlot? _plotControl;

    public void Initialize(ScottPlot.WPF.WpfPlot plotControl)
    {
        _plotControl = plotControl;
        SetupPlot();
    }

    private void SetupPlot()
    {
        // All plot configuration here in ViewModel
        var plot = _plotControl!.Plot;
        // ... configure plot ...
    }

    private void RefreshPlot()
    {
        _plotControl?.Refresh();
    }
}
```

---

## 9. Complete Example: 4-Channel Servo Monitor

### 9.1 XAML -- MonitorView.xaml

```xml
<UserControl
    x:Class="RswareDesign.Modules.Monitor.MonitorView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:md="http://materialdesigninxaml.net/winfx/xaml/themes"
    xmlns:scottPlot="clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF"
    mc:Ignorable="d"
    d:DesignHeight="600" d:DesignWidth="1200"
    Background="{DynamicResource SurfaceBrush}">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="8">
            <Button
                Style="{StaticResource MaterialDesignFlatButton}"
                Command="{Binding StartCommand}"
                IsEnabled="{Binding IsNotRunning}"
                Content="{DynamicResource loc.monitor.start}" />
            <Button
                Style="{StaticResource MaterialDesignFlatButton}"
                Command="{Binding StopCommand}"
                IsEnabled="{Binding IsRunning}"
                Content="{DynamicResource loc.monitor.stop}"
                Margin="8,0,0,0" />

            <Separator Style="{StaticResource {x:Static ToolBar.SeparatorStyleKey}}"
                       Margin="12,4" />

            <!-- Time/Div selector -->
            <TextBlock Text="{DynamicResource loc.monitor.timebase}"
                       VerticalAlignment="Center"
                       Foreground="{DynamicResource TextPrimary}" />
            <ComboBox
                SelectedItem="{Binding SelectedTimeBase}"
                ItemsSource="{Binding TimeBaseOptions}"
                Margin="4,0"
                MinWidth="100" />

            <Separator Style="{StaticResource {x:Static ToolBar.SeparatorStyleKey}}"
                       Margin="12,4" />

            <!-- Channel toggles -->
            <CheckBox Content="CH1" IsChecked="{Binding IsCh1Visible}"
                      Foreground="#42A5F5" Margin="4,0" />
            <CheckBox Content="CH2" IsChecked="{Binding IsCh2Visible}"
                      Foreground="#66BB6A" Margin="4,0" />
            <CheckBox Content="CH3" IsChecked="{Binding IsCh3Visible}"
                      Foreground="#EF5350" Margin="4,0" />
            <CheckBox Content="CH4" IsChecked="{Binding IsCh4Visible}"
                      Foreground="#FFA726" Margin="4,0" />
        </StackPanel>

        <!-- Plot Area -->
        <ContentControl Grid.Row="1"
                        Content="{Binding PlotControl, Mode=OneTime}"
                        Margin="4" />

        <!-- Status Bar -->
        <StatusBar Grid.Row="2"
                   Background="{DynamicResource SurfaceBrush}">
            <StatusBarItem>
                <TextBlock Foreground="{DynamicResource TextSecondary}">
                    <Run Text="{DynamicResource loc.monitor.cursor}" />
                    <Run Text="X:" />
                    <Run Text="{Binding CursorX, StringFormat=F4}" />
                    <Run Text="  Y:" />
                    <Run Text="{Binding CursorY, StringFormat=F4}" />
                </TextBlock>
            </StatusBarItem>
            <Separator />
            <StatusBarItem>
                <TextBlock Foreground="{DynamicResource TextSecondary}">
                    <Run Text="{DynamicResource loc.monitor.samplerate}" />
                    <Run Text="{Binding SampleRate, StringFormat=N0}" />
                    <Run Text="Hz" />
                </TextBlock>
            </StatusBarItem>
            <Separator />
            <StatusBarItem>
                <TextBlock Foreground="{DynamicResource TextSecondary}">
                    <Run Text="{DynamicResource loc.monitor.samples}" />
                    <Run Text="{Binding TotalSamples, StringFormat=N0}" />
                </TextBlock>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</UserControl>
```

### 9.2 ViewModel -- MonitorViewModel.cs

```csharp
using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;
using ScottPlot.WPF;

namespace RswareDesign.Modules.Monitor;

public partial class MonitorViewModel : ObservableObject, IDisposable
{
    // ── Plot Control (bound via ContentControl) ──
    [ObservableProperty]
    private WpfPlot _plotControl;

    // ── Channel Streamers ──
    private ScottPlot.Plottables.DataStreamer? _chPosition;
    private ScottPlot.Plottables.DataStreamer? _chVelocity;
    private ScottPlot.Plottables.DataStreamer? _chCurrent;
    private ScottPlot.Plottables.DataStreamer? _chTorque;

    // ── Crosshair ──
    private ScottPlot.Plottables.Crosshair? _crosshair;

    // ── Timers ──
    private System.Timers.Timer? _dataTimer;
    private DispatcherTimer? _renderTimer;

    // ── Observable Properties ──
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isNotRunning = true;
    [ObservableProperty] private bool _isCh1Visible = true;
    [ObservableProperty] private bool _isCh2Visible = true;
    [ObservableProperty] private bool _isCh3Visible = true;
    [ObservableProperty] private bool _isCh4Visible = true;
    [ObservableProperty] private string _cursorX = "0.0000";
    [ObservableProperty] private string _cursorY = "0.0000";
    [ObservableProperty] private int _sampleRate = 1000;
    [ObservableProperty] private long _totalSamples;
    [ObservableProperty] private string _selectedTimeBase = "100ms";

    public string[] TimeBaseOptions { get; } =
        { "10ms", "20ms", "50ms", "100ms", "200ms", "500ms", "1s", "2s", "5s" };

    // ── Channel Colors ──
    private static readonly ScottPlot.Color ColorPosition = ScottPlot.Color.FromHex("#42A5F5");
    private static readonly ScottPlot.Color ColorVelocity = ScottPlot.Color.FromHex("#66BB6A");
    private static readonly ScottPlot.Color ColorCurrent  = ScottPlot.Color.FromHex("#EF5350");
    private static readonly ScottPlot.Color ColorTorque   = ScottPlot.Color.FromHex("#FFA726");

    // ── Constructor ──
    public MonitorViewModel()
    {
        PlotControl = new WpfPlot();
        InitializePlot();
        SetupMouseTracking();
    }

    // ═══════════════════════════════════════════════════════════════
    // Plot Initialization
    // ═══════════════════════════════════════════════════════════════
    private void InitializePlot()
    {
        var plot = PlotControl.Plot;

        // ── Dark Theme ──
        plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");
        plot.DataBackground.Color   = ScottPlot.Color.FromHex("#121212");
        plot.Axes.Color(ScottPlot.Color.FromHex("#B0B0B0"));
        plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");
        plot.Grid.MinorLineColor = ScottPlot.Color.FromHex("#1A1A1A");

        // ── Axis Labels ──
        plot.Axes.Bottom.Label.Text = "Time [s]";
        plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex("#B0B0B0");
        plot.Axes.Left.Label.Text = "Value";
        plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex("#B0B0B0");

        // ── Channel 1: Position (Blue) ──
        int bufferSize = 1000;
        double period = 0.001; // 1kHz

        _chPosition = plot.Add.DataStreamer(points: bufferSize, period: period);
        _chPosition.Color = ColorPosition;
        _chPosition.LineWidth = 1.5f;
        _chPosition.LegendText = "CH1: Position [pulse]";
        _chPosition.ViewScrollLeft();
        _chPosition.ManageAxisLimits = true;

        // ── Channel 2: Velocity (Green) ──
        _chVelocity = plot.Add.DataStreamer(points: bufferSize, period: period);
        _chVelocity.Color = ColorVelocity;
        _chVelocity.LineWidth = 1.5f;
        _chVelocity.LegendText = "CH2: Velocity [rpm]";
        _chVelocity.ViewScrollLeft();
        _chVelocity.ManageAxisLimits = true;

        // ── Channel 3: Current (Red) ──
        _chCurrent = plot.Add.DataStreamer(points: bufferSize, period: period);
        _chCurrent.Color = ColorCurrent;
        _chCurrent.LineWidth = 1.5f;
        _chCurrent.LegendText = "CH3: Current [A]";
        _chCurrent.ViewScrollLeft();
        _chCurrent.ManageAxisLimits = true;

        // ── Channel 4: Torque (Orange) ──
        _chTorque = plot.Add.DataStreamer(points: bufferSize, period: period);
        _chTorque.Color = ColorTorque;
        _chTorque.LineWidth = 1.5f;
        _chTorque.LegendText = "CH4: Torque [Nm]";
        _chTorque.ViewScrollLeft();
        _chTorque.ManageAxisLimits = true;

        // ── Crosshair ──
        _crosshair = plot.Add.Crosshair(0, 0);
        _crosshair.LineWidth = 1;
        _crosshair.LineColor = ScottPlot.Color.FromHex("#FFEB3B");
        _crosshair.HorizontalLine.LinePattern = ScottPlot.LinePattern.Dotted;
        _crosshair.VerticalLine.LinePattern = ScottPlot.LinePattern.Dotted;

        // ── Legend ──
        plot.ShowLegend();
        plot.Legend.Alignment = ScottPlot.Alignment.UpperRight;
        plot.Legend.Orientation = ScottPlot.Orientation.Vertical;
        plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#2D2D30");
        plot.Legend.FontColor = ScottPlot.Color.FromHex("#E0E0E0");
        plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#555555");
        plot.Legend.FontSize = 11;
    }

    // ═══════════════════════════════════════════════════════════════
    // Mouse Tracking (Crosshair)
    // ═══════════════════════════════════════════════════════════════
    private void SetupMouseTracking()
    {
        PlotControl.MouseMove += (s, e) =>
        {
            var pos = e.GetPosition(PlotControl);
            var coords = PlotControl.Plot.GetCoordinates(
                new ScottPlot.Pixel((float)pos.X, (float)pos.Y));

            lock (PlotControl.Plot.Sync)
            {
                if (_crosshair is not null)
                    _crosshair.Position = coords;
            }

            CursorX = coords.X.ToString("F4");
            CursorY = coords.Y.ToString("F4");
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // Commands
    // ═══════════════════════════════════════════════════════════════
    [RelayCommand]
    private void Start()
    {
        IsRunning = true;
        IsNotRunning = false;

        // Data acquisition timer: 10ms (100 Hz data collection)
        _dataTimer = new System.Timers.Timer(10);
        _dataTimer.Elapsed += OnDataTimerElapsed;
        _dataTimer.AutoReset = true;
        _dataTimer.Start();

        // Render timer: 100ms (10 FPS) on UI thread
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _renderTimer.Tick += OnRenderTimerTick;
        _renderTimer.Start();
    }

    [RelayCommand]
    private void Stop()
    {
        IsRunning = false;
        IsNotRunning = true;

        _dataTimer?.Stop();
        _dataTimer?.Dispose();
        _dataTimer = null;

        _renderTimer?.Stop();
        _renderTimer = null;
    }

    // ═══════════════════════════════════════════════════════════════
    // Timer Handlers
    // ═══════════════════════════════════════════════════════════════
    private void OnDataTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        lock (PlotControl.Plot.Sync)
        {
            // Replace with actual drive data read
            double position = ReadFromDrive_Position();
            double velocity = ReadFromDrive_Velocity();
            double current  = ReadFromDrive_Current();
            double torque   = ReadFromDrive_Torque();

            _chPosition?.Add(position);
            _chVelocity?.Add(velocity);
            _chCurrent?.Add(current);
            _chTorque?.Add(torque);

            TotalSamples++;
        }
    }

    private void OnRenderTimerTick(object? sender, EventArgs e)
    {
        // Apply channel visibility
        if (_chPosition is not null) _chPosition.IsVisible = IsCh1Visible;
        if (_chVelocity is not null) _chVelocity.IsVisible = IsCh2Visible;
        if (_chCurrent  is not null) _chCurrent.IsVisible  = IsCh3Visible;
        if (_chTorque   is not null) _chTorque.IsVisible   = IsCh4Visible;

        PlotControl.Refresh();
    }

    // ═══════════════════════════════════════════════════════════════
    // Drive Data Read (Replace with actual implementation)
    // ═══════════════════════════════════════════════════════════════
    private static readonly Random _rng = new();
    private double _simTime;
    private double ReadFromDrive_Position() => Math.Sin(_simTime += 0.01) * 1000;
    private double ReadFromDrive_Velocity() => Math.Cos(_simTime) * 3000;
    private double ReadFromDrive_Current()  => Math.Sin(_simTime * 2) * 5 + _rng.NextDouble() * 0.5;
    private double ReadFromDrive_Torque()   => Math.Sin(_simTime * 1.5) * 2 + _rng.NextDouble() * 0.2;

    // ═══════════════════════════════════════════════════════════════
    // Channel Visibility Change Handlers
    // ═══════════════════════════════════════════════════════════════
    partial void OnIsCh1VisibleChanged(bool value) => PlotControl?.Refresh();
    partial void OnIsCh2VisibleChanged(bool value) => PlotControl?.Refresh();
    partial void OnIsCh3VisibleChanged(bool value) => PlotControl?.Refresh();
    partial void OnIsCh4VisibleChanged(bool value) => PlotControl?.Refresh();

    // ═══════════════════════════════════════════════════════════════
    // Cleanup
    // ═══════════════════════════════════════════════════════════════
    public void Dispose()
    {
        Stop();
    }
}
```

---

## 10. Quick Reference Table

### Plot Types

| Plot Type | Method | Use Case | Performance |
|-----------|--------|----------|-------------|
| Signal | `Plot.Add.Signal(double[])` | Fixed-rate evenly-spaced data | Excellent (millions of pts) |
| SignalXY | `Plot.Add.SignalXY(xs, ys)` | Ascending X, uneven spacing | Very good |
| SignalConst | `Plot.Add.SignalConst(double[])` | Immutable large datasets | Best |
| Scatter | `Plot.Add.Scatter(xs, ys)` | Arbitrary X-Y pairs | Good (up to ~100K pts) |
| DataLogger | `Plot.Add.DataLogger()` | Growing dataset logging | Good |
| DataStreamer | `Plot.Add.DataStreamer(pts, period)` | Fixed-window oscilloscope | Excellent |

### DataStreamer View Modes

| Method | Behavior |
|--------|----------|
| `ViewScrollLeft()` | New data on right, old data scrolls left (standard oscilloscope) |
| `ViewScrollRight()` | New data on left, old data scrolls right |
| `ViewWipeRight()` | New data overwrites left-to-right (ECG style) |
| `ViewWipeLeft()` | New data overwrites right-to-left |

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `ManageAxisLimits` | bool | Auto-adjust axes to show latest data |
| `IsVisible` | bool | Show/hide the plottable |
| `Color` | ScottPlot.Color | Line color |
| `LineWidth` | float | Line thickness |
| `LegendText` | string | Label shown in legend |
| `Data.Period` | double | X-axis spacing for Signal plots |
| `Data.MinimumIndex` | int | First visible data index |
| `Data.MaximumIndex` | int | Last visible data index |

### Color Construction

```csharp
ScottPlot.Color.FromHex("#42A5F5");              // From hex
ScottPlot.Color.FromColor(System.Drawing.Color.Blue);  // From System.Drawing
ScottPlot.Colors.Blue;                            // Named constant
new ScottPlot.Color(66, 165, 245);               // From RGB
new ScottPlot.Color(66, 165, 245, 128);          // From RGBA (semi-transparent)
```

### Axis Methods

```csharp
plot.Axes.SetLimitsX(left, right);    // Set X range
plot.Axes.SetLimitsY(bottom, top);    // Set Y range
plot.Axes.AutoScale();                 // Auto-fit all data
plot.Axes.AutoScaleX();                // Auto-fit X only
plot.Axes.AutoScaleY();                // Auto-fit Y only
plot.Axes.Color(color);                // Set all axis colors at once
```

---

## 11. Multi-Plot Layout (Separate Charts per Channel)

For oscilloscope views with separate plots stacked vertically:

### XAML

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />
        <RowDefinition Height="*" />
        <RowDefinition Height="*" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <ContentControl Grid.Row="0" Content="{Binding PlotPosition, Mode=OneTime}" Margin="2" />
    <ContentControl Grid.Row="1" Content="{Binding PlotVelocity, Mode=OneTime}" Margin="2" />
    <ContentControl Grid.Row="2" Content="{Binding PlotCurrent,  Mode=OneTime}" Margin="2" />
    <ContentControl Grid.Row="3" Content="{Binding PlotTorque,   Mode=OneTime}" Margin="2" />
</Grid>
```

### ViewModel

```csharp
[ObservableProperty] private WpfPlot _plotPosition;
[ObservableProperty] private WpfPlot _plotVelocity;
[ObservableProperty] private WpfPlot _plotCurrent;
[ObservableProperty] private WpfPlot _plotTorque;

public MultiPlotMonitorViewModel()
{
    PlotPosition = CreateChannelPlot("Position [pulse]", "#42A5F5");
    PlotVelocity = CreateChannelPlot("Velocity [rpm]",  "#66BB6A");
    PlotCurrent  = CreateChannelPlot("Current [A]",     "#EF5350");
    PlotTorque   = CreateChannelPlot("Torque [Nm]",     "#FFA726");
}

private WpfPlot CreateChannelPlot(string label, string hexColor)
{
    var wpfPlot = new WpfPlot();
    var plot = wpfPlot.Plot;
    var color = ScottPlot.Color.FromHex(hexColor);

    // Dark theme
    plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");
    plot.DataBackground.Color   = ScottPlot.Color.FromHex("#121212");
    plot.Axes.Color(ScottPlot.Color.FromHex("#B0B0B0"));
    plot.Grid.MajorLineColor    = ScottPlot.Color.FromHex("#333333");

    // Add streamer
    var streamer = plot.Add.DataStreamer(points: 1000, period: 0.001);
    streamer.Color = color;
    streamer.LineWidth = 1.5f;
    streamer.LegendText = label;
    streamer.ViewScrollLeft();
    streamer.ManageAxisLimits = true;

    // Axis label
    plot.Axes.Left.Label.Text = label;
    plot.Axes.Left.Label.ForeColor = color;

    return wpfPlot;
}
```

---

## 12. Performance Tips

| Tip | Detail |
|-----|--------|
| **Use DataStreamer** over Signal for live streaming | DataStreamer uses a circular buffer -- no array resizing |
| **Separate data + render timers** | Data at 1-10ms; render at 50-100ms (10-20 FPS is sufficient) |
| **Lock on Plot.Sync** | Prevents render corruption during data update |
| **Limit visible points** | Use `Data.MinimumIndex`/`MaximumIndex` for Signal plots |
| **Disable interaction when streaming** | `UserInputProcessor.IsEnabled = false` reduces overhead |
| **Use SignalConst for static data** | Immutable optimization for large reference datasets |
| **Batch AddRange** | Use `AddRange(double[])` instead of multiple `Add(double)` calls |

---

## Sources

- [ScottPlot WPF Quickstart](https://scottplot.net/quickstart/wpf/)
- [ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/)
- [Live Data -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/LiveData/)
- [DataLogger Quickstart](https://scottplot.net/cookbook/5/LiveData/DataLoggerQuickstart/)
- [DataStreamer Quickstart](https://scottplot.net/cookbook/5/LiveData/DataStreamerQuickstart/)
- [Dark Mode -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/Styling/DarkMode/)
- [Multiple Axes -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/MultiAxis/)
- [Crosshair -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/Crosshair/)
- [Legends -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/Legend/)
- [Signal Plot -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/Signal/)
- [MVVM and Data Binding -- ScottPlot FAQ](https://scottplot.net/faq/mvvm/)
- [Multi-Threaded/Async -- ScottPlot FAQ](https://scottplot.net/faq/async/)
- [Plot Live, Changing Data -- ScottPlot FAQ](https://scottplot.net/faq/live-data/)
- [NuGet: ScottPlot.WPF 5.1.57](https://www.nuget.org/packages/ScottPlot.WPF)
- [ScottPlot 5 API Reference](https://scottplot.net/api/5/)
- [Color Palettes -- ScottPlot 5](https://scottplot.net/cookbook/5/palettes/)
- [Styling Plots -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/Styling/)
- [Axis Customization -- ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/Styling/AxisCustom/)
- [User Control Configuration -- ScottPlot FAQ](https://scottplot.net/faq/configuration/)
- [ScottPlot GitHub - DataStreamer Demo](https://github.com/ScottPlot/ScottPlot/blob/main/src/ScottPlot5/ScottPlot5%20Demos/ScottPlot5%20WinForms%20Demo/Demos/DataStreamer.cs)
- [ScottPlot GitHub - DataLogger Demo](https://github.com/ScottPlot/ScottPlot/blob/main/src/ScottPlot5/ScottPlot5%20Demos/ScottPlot5%20WinForms%20Demo/Demos/DataLogger.cs)
