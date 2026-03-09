using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using RswareDesign.Models;
using RswareDesign.ViewModels;
using ScottPlot;

namespace RswareDesign.Views;

public partial class MonitorControlDialog : UserControl
{
    private const int PointCount = 500;
    private const double PeriodMs = 0.1;

    private readonly double[][] _channelData;
    private readonly ScottPlot.Plottables.Signal?[] _signals = new ScottPlot.Plottables.Signal?[4];

    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new(42);
    private double _phase;
    private bool _isRunning;

    private static readonly int[][] GroupChannels = [[0, 1], [2, 3]];
    private static readonly string[] GroupColorKeys = ["ChartCH1Brush", "ChartCH3Brush"];
    private readonly IYAxis?[] _groupYAxes = new IYAxis?[2];

    private Point _dragStartPoint;
    private int _dragSourceIndex = -1;
    private bool _singleMode;
    private int _singleTicks;
    private DispatcherTimer? _singleTimer;

    private static readonly string[] ParameterItems =
    [
        "Motor Feedback Position", "Master Position", "Follower Position",
        "Position Error", "Position Command Count Frequency",
        "Velocity Command", "Velocity Feedback", "Velocity Error",
        "Current Command", "Current Feedback", "Current Command (D-Axis)",
        "U Phase Current", "V Phase Current", "W Phase Current",
        "Absolute Maximum Current Command", "Commutation Angle", "Mechanical Angle",
        "Shunt Power Limit Ratio", "Instantaneous Shunt Power",
        "Motor Power Limit Ratio", "Drive Power Limit Ratio",
        "Drive Utilization", "Drive Enabled",
        "Absolute Rotations", "Absolute Single Turn", "Bus Voltage",
        "Velocity Command Offset", "Current Command Offset", "Motor Utilization",
        "Analog Command - Velocity", "Analog Command - Current",
        "Current Feedback (RMS)", "Maximum Current Feedback (RMS)",
        "TouchProbe Function", "TouchProbe Status",
        "TouchProbe Position 1 Positive Value", "TouchProbe Position 1 Negative Value",
        "TouchProbe Position 2 Positive Value", "TouchProbe Position 2 Negative Value",
        "Touch Probe 1 Positive Edge Counter", "Touch Probe 1 Negative Edge Counter",
        "Touch Probe 2 Positive Edge Counter", "Touch Probe 2 Negative Edge Counter",
        "ECAT Homing Status", "ECAT Homing Error",
        "High Pass Filtered Current Command Output (ANF)",
        "High Pass Filtered Current Command Variance (ANF)",
        "Resonance Estimation Frequency (ANF)",
        "ABSS Data", "ABSA Data", "Hall Value",
        "ABSS Data (Linear-BiSS)", "ABSA Data (Linear-BiSS)", "ABSA Data (Linear-BiSS-CC)",
        "Load and Motor Side Feedback Difference",
        "Digital I/O", "Control Word", "Status Word",
        "MO New Set Point", "MO Set Acknowledged", "MO Target Reached",
        "MO Halt Enabled", "MO Buffer Empty",
        "PP Cmd Position", "PP Cmd Velocity", "PP Cmd Accel", "PP Cmd Decel", "PP Cmd Jerk",
        "PP Actual Position", "PP Actual Velocity",
        "PP Feedback Position", "PP Feedback Velocity", "PP Position Offset",
        "MO Target Position", "MO Target Velocity",
        "MO Feedback Position", "MO Feedback Velocity", "MO Feedback Torque",
        "MO Ether CMD Offset", "MO OriginOffset",
        "Displacement in Control Cycle", "Displacement for Encoder Direction",
        "Partial Progress Step", "Displacement for Command Cut-off",
        "Theta Reference Angle", "Overall Progress Step", "Number of Stabilizing Count"
    ];

    private readonly string[] _channelNames =
    [
        "Motor Feedback Position",
        "Master Position",
        "Velocity Feedback",
        "Velocity Command"
    ];

    private readonly string[] _channelColorKeys =
    [
        "ChartCH1Brush", "ChartCH2Brush", "ChartCH3Brush", "ChartCH4Brush"
    ];

    public MonitorControlDialog()
    {
        _channelData = new double[4][];
        for (int i = 0; i < 4; i++)
            _channelData[i] = new double[PointCount];

        InitializeComponent();

        InitChannelComboBoxes();
        GenerateInitialData();
        SetupChart();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += OnTimerTick;

        // Start with chart hidden (shown via button click)
        ChartToolbar.Visibility = Visibility.Collapsed;
        ChartArea.Visibility = Visibility.Collapsed;
        ChartStatusBar.Visibility = Visibility.Collapsed;

        // Bind favorites to VM when DataContext is set
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainWindowViewModel oldVm)
        {
            oldVm.FavoriteParameters.CollectionChanged -= OnFavoritesCollectionChanged;
        }

        if (e.NewValue is MainWindowViewModel vm)
        {
            FavoritesListBox.ItemsSource = vm.FavoriteParameters;
            FavCountText.Text = $"({vm.FavoriteParameters.Count} items)";
            vm.FavoriteParameters.CollectionChanged += OnFavoritesCollectionChanged;
        }
    }

    private void OnFavoritesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            FavCountText.Text = $"({vm.FavoriteParameters.Count} items)";
    }

    // ═══════════════════════════════════════════════════════════
    //  SECTION TOGGLE (Oscilloscope / ControlPanel)
    // ═══════════════════════════════════════════════════════════

    public bool IsChartVisible => ChartToolbar.Visibility == Visibility.Visible;

    public void ToggleChart()
    {
        if (IsChartVisible)
        {
            ChartToolbar.Visibility = Visibility.Collapsed;
            ChartArea.Visibility = Visibility.Collapsed;
            ChartStatusBar.Visibility = Visibility.Collapsed;

            if (_isRunning)
            {
                _timer.Stop();
                _isRunning = false;
                ShowCollectingProgress(false);
                TxtStatus.Text = "Stopped";
            }
        }
        else
        {
            ChartToolbar.Visibility = Visibility.Visible;
            ChartArea.Visibility = Visibility.Visible;
            ChartStatusBar.Visibility = Visibility.Visible;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SCALE PANEL / FAVORITES COLLAPSE TOGGLE
    // ═══════════════════════════════════════════════════════════

    private void ToggleScalePanel_Click(object sender, RoutedEventArgs e)
    {
        bool collapse = ScalePanel.Visibility == Visibility.Visible;
        ScalePanel.Visibility = collapse ? Visibility.Collapsed : Visibility.Visible;
        ScaleToggleIcon.Kind = collapse ? PackIconKind.ChevronRight : PackIconKind.ChevronLeft;
    }

    private GridLength _savedFavoritesWidth = new(220);

    private void ToggleFavoritesPanel_Click(object sender, RoutedEventArgs e)
    {
        bool collapse = FavoritesContent.Visibility == Visibility.Visible;
        if (collapse)
        {
            _savedFavoritesWidth = FavoritesColumn.Width;
            FavoritesContent.Visibility = Visibility.Collapsed;
            FavoritesColumn.Width = new GridLength(0);
            FavoritesColumn.MinWidth = 0;
            FavoritesSplitter.Visibility = Visibility.Collapsed;
            FavoritesToggleIcon.Kind = PackIconKind.ChevronLeft;
        }
        else
        {
            FavoritesContent.Visibility = Visibility.Visible;
            FavoritesColumn.Width = _savedFavoritesWidth;
            FavoritesColumn.MinWidth = 150;
            FavoritesSplitter.Visibility = Visibility.Visible;
            FavoritesToggleIcon.Kind = PackIconKind.ChevronRight;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CLEAR ALL FAVORITES
    // ═══════════════════════════════════════════════════════════

    private void ClearAllFavorites_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.ClearAllFavoritesCommand.Execute(null);
    }

    // ═══════════════════════════════════════════════════════════
    //  CHART DATA & SETUP
    // ═══════════════════════════════════════════════════════════

    private void GenerateInitialData()
    {
        for (int i = 0; i < PointCount; i++)
        {
            _phase = i;
            for (int ch = 0; ch < 4; ch++)
                _channelData[ch][i] = GeneratePoint(ch);
        }
    }

    private double GeneratePoint(int channel)
    {
        double noise = _rng.NextDouble() - 0.5;
        double p = _phase * 0.04;
        return channel switch
        {
            0 => Math.Sin(p * 0.5) * 10000 + p * 10 + noise * 50,              // Position Current
            1 => Math.Sin(p * 0.5) * 10000 + p * 10 + noise * 20 + 150,        // Position Command (tracking)
            2 => Math.Cos(p) * 3500 + Math.Cos(p * 2.3) * 500 + noise * 30,    // Velocity Current
            3 => Math.Cos(p) * 3500 + Math.Cos(p * 2.3) * 500 + noise * 15 + 80, // Velocity Command (tracking)
            _ => 0
        };
    }

    private void SetupChart()
    {
        var plot = MonitorPlot.Plot;

        plot.FigureBackground.Color = GetThemeColor("BackgroundBrush");
        plot.DataBackground.Color = GetThemeColor("SurfaceBrush");
        plot.Grid.MajorLineColor = GetThemeColor("SurfaceVariantBrush");

        var axisColor = GetThemeColor("TextSecondary");
        var frameColor = GetThemeColor("BorderDefault");
        plot.Axes.Bottom.Label.Text ="Time (ms)";
        plot.Axes.Left.Label.Text ="";
        plot.Axes.Bottom.Label.ForeColor =axisColor;
        plot.Axes.Left.Label.ForeColor =axisColor;
        plot.Axes.Bottom.TickLabelStyle.ForeColor = axisColor;
        plot.Axes.Left.TickLabelStyle.ForeColor = axisColor;
        plot.Axes.Bottom.MajorTickStyle.Color = axisColor;
        plot.Axes.Left.MajorTickStyle.Color = axisColor;
        plot.Axes.Bottom.FrameLineStyle.Color = frameColor;
        plot.Axes.Left.FrameLineStyle.Color = frameColor;
        plot.Axes.Right.FrameLineStyle.Color = frameColor;
        plot.Axes.Top.FrameLineStyle.Color = frameColor;

        float[] lineWidths = [1.5f, 1.0f, 1.5f, 1.0f];

        for (int ch = 0; ch < 4; ch++)
        {
            _signals[ch] = plot.Add.Signal(_channelData[ch], PeriodMs);
            _signals[ch]!.LegendText = _channelNames[ch];
            _signals[ch]!.Color = GetThemeColor(_channelColorKeys[ch]);
            _signals[ch]!.LineWidth = lineWidths[ch];
        }

        plot.Legend.IsVisible = true;
        plot.Legend.Alignment = Alignment.UpperRight;
        plot.Legend.BackgroundColor = GetThemeColor("SurfaceVariantBrush");
        plot.Legend.FontColor = GetThemeColor("TextPrimary");
        plot.Legend.OutlineColor = GetThemeColor("BorderDefault");

        // ── Multi-axis: each group gets its own Y-axis ──
        _groupYAxes[0] = plot.Axes.Left;
        var currentAxisColor = GetThemeColor(GroupColorKeys[0]);
        plot.Axes.Left.TickLabelStyle.ForeColor = currentAxisColor;
        plot.Axes.Left.MajorTickStyle.Color = currentAxisColor;

        for (int g = 1; g < 2; g++)
        {
            var axis = plot.Axes.AddLeftAxis();
            var color = GetThemeColor(GroupColorKeys[g]);
            axis.Label.Text ="";
            axis.TickLabelStyle.ForeColor = color;
            axis.TickLabelStyle.FontSize = 9;
            axis.MajorTickStyle.Color = color;
            axis.FrameLineStyle.Color = color;
            _groupYAxes[g] = axis;
        }

        // Assign each signal to its group's Y-axis
        for (int g = 0; g < 2; g++)
            foreach (int ch in GroupChannels[g])
                _signals[ch]!.Axes.YAxis = _groupYAxes[g]!;

        plot.Axes.AutoScale();
        MonitorPlot.Refresh();
        UpdateScaleTextBoxes();
    }

    private static ScottPlot.Color GetThemeColor(string resourceKey)
    {
        if (Application.Current.TryFindResource(resourceKey) is SolidColorBrush brush)
        {
            var c = brush.Color;
            return new ScottPlot.Color(c.R, c.G, c.B, c.A);
        }
        return ScottPlot.Colors.Gray;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        for (int s = 0; s < 3; s++)
        {
            _phase++;
            for (int ch = 0; ch < 4; ch++)
            {
                Array.Copy(_channelData[ch], 1, _channelData[ch], 0, PointCount - 1);
                _channelData[ch][PointCount - 1] = GeneratePoint(ch);
            }
        }
        MonitorPlot.Refresh();
    }

    private void Channel_Changed(object sender, RoutedEventArgs e)
    {
        if (_signals[0] is null) return;

        CheckBox[] checkboxes = [ChkCh1, ChkCh2, ChkCh3, ChkCh4];
        for (int i = 0; i < 4; i++)
            _signals[i]!.IsVisible = checkboxes[i].IsChecked == true;

        MonitorPlot.Plot.Axes.AutoScale();
        MonitorPlot.Refresh();
        UpdateScaleTextBoxes();
    }

    private void BtnAutoScale_Click(object sender, RoutedEventArgs e)
    {
        MonitorPlot.Plot.Axes.AutoScale();
        MonitorPlot.Refresh();
        UpdateScaleTextBoxes();
    }

    private void BtnSingle_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning) return;

        ShowCollectingProgress(true, isIndeterminate: false);
        CollectingProgress.Value = 0;
        TxtStatus.Text = "Collecting...";

        _singleTicks = 0;
        _singleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _singleTimer.Tick += (_, _) =>
        {
            _singleTicks++;
            CollectingProgress.Value = Math.Min(_singleTicks * 3.3, 100);

            if (_singleTicks >= 30)
            {
                _singleTimer!.Stop();
                _singleTimer = null;

                GenerateInitialData();
                MonitorPlot.Plot.Axes.AutoScale();
                MonitorPlot.Refresh();
                UpdateScaleTextBoxes();

                ShowCollectingProgress(false);
                TxtStatus.Text = "Complete";
            }
        };
        _singleTimer.Start();
    }

    private void BtnContinuous_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning) return;
        _singleMode = false;
        _timer.Start();
        _isRunning = true;
        ShowCollectingProgress(true, isIndeterminate: true);
        TxtStatus.Text = "Running";
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        if (_singleTimer != null)
        {
            _singleTimer.Stop();
            _singleTimer = null;
        }
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
        }
        ShowCollectingProgress(false);
        TxtStatus.Text = "Stopped";
    }

    private void BtnOption_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OscilloscopeOptionDialog
        {
            Owner = Window.GetWindow(this)
        };
        dialog.ShowDialog();
    }

    private void InitChannelComboBoxes()
    {
        ComboBox[] combos = [CmbCh1, CmbCh2, CmbCh3, CmbCh4];
        int[] defaults = [0, 1, 6, 5]; // Motor Feedback Position, Master Position, Velocity Feedback, Velocity Command
        for (int i = 0; i < 4; i++)
        {
            combos[i].ItemsSource = ParameterItems;
            combos[i].SelectedIndex = defaults[i];
        }
    }

    private void ChannelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cb) return;
        if (!int.TryParse(cb.Tag?.ToString(), out int chIndex)) return;
        if (_signals[chIndex] is null) return;
        if (cb.SelectedItem is not string name) return;

        _channelNames[chIndex] = name;
        _signals[chIndex]!.LegendText = name;
        MonitorPlot.Refresh();
    }

    private void ShowCollectingProgress(bool show, bool isIndeterminate = false)
    {
        if (show)
        {
            CollectingProgress.Visibility = Visibility.Visible;
            TxtCollecting.Visibility = Visibility.Visible;
            CollectingProgress.IsIndeterminate = isIndeterminate;
            if (!isIndeterminate) CollectingProgress.Value = 0;
        }
        else
        {
            CollectingProgress.Visibility = Visibility.Collapsed;
            TxtCollecting.Visibility = Visibility.Collapsed;
            CollectingProgress.IsIndeterminate = false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SCALE GROUP CONTROLS
    // ═══════════════════════════════════════════════════════════

    private void BtnAutoScaleGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        if (!int.TryParse(fe.Tag?.ToString(), out int groupIndex)) return;
        AutoScaleGroup(groupIndex);
    }

    private void ScaleValue_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (!int.TryParse(tb.Tag?.ToString(), out int groupIndex)) return;
        ApplyScaleFromTextBoxes(groupIndex);
    }

    private void ScaleValue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ScaleValue_LostFocus(sender, e);
            Keyboard.ClearFocus();
        }
    }

    private void AutoScaleGroup(int groupIndex)
    {
        if (_groupYAxes[groupIndex] is null) return;

        double min = double.MaxValue, max = double.MinValue;
        foreach (int ch in GroupChannels[groupIndex])
        {
            if (!_signals[ch]!.IsVisible) continue;
            for (int i = 0; i < PointCount; i++)
            {
                min = Math.Min(min, _channelData[ch][i]);
                max = Math.Max(max, _channelData[ch][i]);
            }
        }
        if (min >= max) return;
        var range = max - min;
        if (range < 1) range = 1;
        min -= range * 0.1;
        max += range * 0.1;

        TextBox[] maxBoxes = [TxtMax0, TxtMax1];
        TextBox[] minBoxes = [TxtMin0, TxtMin1];
        maxBoxes[groupIndex].Text = max.ToString("F0");
        minBoxes[groupIndex].Text = min.ToString("F0");

        _groupYAxes[groupIndex]!.Range.Set(min, max);
        MonitorPlot.Refresh();
    }

    private void ApplyScaleFromTextBoxes(int groupIndex)
    {
        if (_groupYAxes[groupIndex] is null) return;

        TextBox[] maxBoxes = [TxtMax0, TxtMax1];
        TextBox[] minBoxes = [TxtMin0, TxtMin1];

        if (!double.TryParse(maxBoxes[groupIndex].Text, out double max)) return;
        if (!double.TryParse(minBoxes[groupIndex].Text, out double min)) return;
        if (min >= max) return;

        _groupYAxes[groupIndex]!.Range.Set(min, max);
        MonitorPlot.Refresh();
    }

    private void UpdateScaleTextBoxes()
    {
        TextBox[] maxBoxes = [TxtMax0, TxtMax1];
        TextBox[] minBoxes = [TxtMin0, TxtMin1];

        for (int g = 0; g < 2; g++)
        {
            if (_groupYAxes[g] is null) continue;
            var axisRange = _groupYAxes[g]!.Range;
            maxBoxes[g].Text = axisRange.Max.ToString("F0");
            minBoxes[g].Text = axisRange.Min.ToString("F0");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CHART THEME (call after theme switch)
    // ═══════════════════════════════════════════════════════════

    public void RefreshChartTheme()
    {
        if (_signals[0] is null) return;
        var plot = MonitorPlot.Plot;

        plot.FigureBackground.Color = GetThemeColor("BackgroundBrush");
        plot.DataBackground.Color = GetThemeColor("SurfaceBrush");
        plot.Grid.MajorLineColor = GetThemeColor("SurfaceVariantBrush");

        var axisColor = GetThemeColor("TextSecondary");
        var frameColor = GetThemeColor("BorderDefault");
        plot.Axes.Bottom.Label.ForeColor =axisColor;
        plot.Axes.Left.Label.ForeColor =axisColor;
        plot.Axes.Bottom.TickLabelStyle.ForeColor = axisColor;
        plot.Axes.Left.TickLabelStyle.ForeColor = axisColor;
        plot.Axes.Bottom.MajorTickStyle.Color = axisColor;
        plot.Axes.Left.MajorTickStyle.Color = axisColor;
        plot.Axes.Bottom.FrameLineStyle.Color = frameColor;
        plot.Axes.Left.FrameLineStyle.Color = frameColor;
        plot.Axes.Right.FrameLineStyle.Color = frameColor;
        plot.Axes.Top.FrameLineStyle.Color = frameColor;

        for (int ch = 0; ch < 4; ch++)
        {
            if (_signals[ch] != null)
                _signals[ch]!.Color = GetThemeColor(_channelColorKeys[ch]);
        }

        plot.Legend.BackgroundColor = GetThemeColor("SurfaceVariantBrush");
        plot.Legend.FontColor = GetThemeColor("TextPrimary");
        plot.Legend.OutlineColor = GetThemeColor("BorderDefault");

        // Update group axis colors
        for (int g = 0; g < 2; g++)
        {
            if (_groupYAxes[g] is null) continue;
            var groupColor = GetThemeColor(GroupColorKeys[g]);
            _groupYAxes[g]!.TickLabelStyle.ForeColor = groupColor;
            _groupYAxes[g]!.MajorTickStyle.Color = groupColor;
            _groupYAxes[g]!.FrameLineStyle.Color = groupColor;
        }

        MonitorPlot.Refresh();
    }


    // ═══════════════════════════════════════════════════════════
    //  DELETE FAVORITE WITH DEL KEY
    // ═══════════════════════════════════════════════════════════

    private void FavList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && FavoritesListBox.SelectedItem is Parameter param)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                // Un-star in the main parameter list too
                var mainParam = vm.Parameters.FirstOrDefault(p => p.FtNumber == param.FtNumber);
                if (mainParam != null)
                    mainParam.IsFavorite = false;

                vm.FavoriteParameters.Remove(param);
            }
            e.Handled = true;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  DRAG-TO-REORDER FAVORITES
    // ═══════════════════════════════════════════════════════════

    private void FavList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        var item = GetListBoxItemAtPoint(FavoritesListBox, e.GetPosition(FavoritesListBox));
        _dragSourceIndex = item != null ? FavoritesListBox.ItemContainerGenerator.IndexFromContainer(item) : -1;
    }

    private void FavList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _dragSourceIndex < 0)
            return;

        var currentPos = e.GetPosition(null);
        var diff = _dragStartPoint - currentPos;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            var data = new DataObject("FavDragIndex", _dragSourceIndex);
            DragDrop.DoDragDrop(FavoritesListBox, data, DragDropEffects.Move);
            _dragSourceIndex = -1;
        }
    }

    private void FavList_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent("FavDragIndex")
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void FavList_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("FavDragIndex")) return;
        if (DataContext is not MainWindowViewModel vm) return;

        int fromIndex = (int)e.Data.GetData("FavDragIndex")!;
        var targetItem = GetListBoxItemAtPoint(FavoritesListBox, e.GetPosition(FavoritesListBox));
        int toIndex = targetItem != null
            ? FavoritesListBox.ItemContainerGenerator.IndexFromContainer(targetItem)
            : vm.FavoriteParameters.Count - 1;

        if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex) return;

        vm.FavoriteParameters.Move(fromIndex, toIndex);
    }

    private static ListBoxItem? GetListBoxItemAtPoint(ListBox listBox, Point point)
    {
        var element = listBox.InputHitTest(point) as DependencyObject;
        while (element != null)
        {
            if (element is ListBoxItem item)
                return item;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}
