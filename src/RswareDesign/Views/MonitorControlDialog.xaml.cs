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
    private readonly ScottPlot.Plottables.Signal?[] _signals = new ScottPlot.Plottables.Signal?[8];

    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new(42);
    private double _phase;
    private bool _isRunning;

    private static readonly int[][] GroupChannels = [[0, 1], [2, 3], [4, 5], [6, 7]];
    private static readonly string[] GroupColorKeys = ["ChartCH1Brush", "ChartCH3Brush", "ChartCH5Brush", "ChartCH7Brush"];
    private readonly IYAxis?[] _groupYAxes = new IYAxis?[4];

    private Point _dragStartPoint;
    private int _dragSourceIndex = -1;

    private readonly string[] _channelNames =
    [
        "Continuous Current",
        "Target Current",
        "Velocity",
        "Target Velocity",
        "Current Position",
        "Current Position 2",
        "Error",
        "Inposition"
    ];

    private readonly string[] _channelColorKeys =
    [
        "ChartCH1Brush", "ChartCH2Brush", "ChartCH3Brush", "ChartCH4Brush",
        "ChartCH5Brush", "ChartCH6Brush", "ChartCH7Brush", "ChartCH8Brush"
    ];

    public MonitorControlDialog()
    {
        _channelData = new double[8][];
        for (int i = 0; i < 8; i++)
            _channelData[i] = new double[PointCount];

        InitializeComponent();

        GenerateInitialData();
        SetupChart();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += OnTimerTick;

        // Start with chart & control panel hidden (shown via button click)
        ChartToolbar.Visibility = Visibility.Collapsed;
        ChartArea.Visibility = Visibility.Collapsed;
        ChartStatusBar.Visibility = Visibility.Collapsed;
        ControlPanel.Visibility = Visibility.Collapsed;

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
    public bool IsControlPanelVisible => ControlPanel.Visibility == Visibility.Visible;

    public void ToggleChart()
    {
        if (IsChartVisible)
        {
            // Hide chart + stop timer
            ChartToolbar.Visibility = Visibility.Collapsed;
            ChartArea.Visibility = Visibility.Collapsed;
            ChartStatusBar.Visibility = Visibility.Collapsed;

            if (_isRunning)
            {
                _timer.Stop();
                _isRunning = false;
                StartStopIcon.Kind = PackIconKind.Play;
                StartStopText.Text = "Start";
                TxtStatus.Text = "Stopped";
            }
        }
        else
        {
            // Show chart area (stopped, Start button ready)
            ChartToolbar.Visibility = Visibility.Visible;
            ChartArea.Visibility = Visibility.Visible;
            ChartStatusBar.Visibility = Visibility.Visible;
        }
    }

    public void ToggleControlPanel()
    {
        ControlPanel.Visibility = ControlPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (ControlPanel.Visibility == Visibility.Visible)
            UpdateGauges();
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
            for (int ch = 0; ch < 8; ch++)
                _channelData[ch][i] = GeneratePoint(ch);
        }
    }

    private double GeneratePoint(int channel)
    {
        double noise = _rng.NextDouble() - 0.5;
        double p = _phase * 0.04;
        return channel switch
        {
            0 => Math.Sin(p) * 2000 + Math.Sin(p * 3.7) * 400 + noise * 30,
            1 => Math.Sin(p) * 1800 + Math.Sin(p * 2.1) * 200 + noise * 20,
            2 => Math.Cos(p) * 3500 + Math.Cos(p * 2.3) * 500 + noise * 30,
            3 => Math.Cos(p) * 3200 + Math.Cos(p * 1.8) * 300 + noise * 15,
            4 => Math.Sin(p * 0.5) * 10000 + p * 10 + noise * 50,
            5 => Math.Sin(p * 0.5) * 10000 + p * 10 + noise * 80 + 200,
            6 => Math.Sin(p * 2.5) * 300 + noise * 100,
            7 => Math.Abs(Math.Sin(p * 0.7)) < 0.1 ? 1 : 0,
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

        float[] lineWidths = [1.5f, 1.5f, 1.5f, 1.5f, 1.3f, 1.3f, 1.2f, 1.0f];

        for (int ch = 0; ch < 8; ch++)
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

        for (int g = 1; g < 4; g++)
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
        for (int g = 0; g < 4; g++)
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
            for (int ch = 0; ch < 8; ch++)
            {
                Array.Copy(_channelData[ch], 1, _channelData[ch], 0, PointCount - 1);
                _channelData[ch][PointCount - 1] = GeneratePoint(ch);
            }
        }
        MonitorPlot.Refresh();
        UpdateGauges();
    }

    private void Channel_Changed(object sender, RoutedEventArgs e)
    {
        if (_signals[0] is null) return;

        CheckBox[] checkboxes = [ChkCh1, ChkCh2, ChkCh3, ChkCh4, ChkCh5, ChkCh6, ChkCh7, ChkCh8];
        for (int i = 0; i < 8; i++)
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

    private void BtnStartStop_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
            TxtStatus.Text = "Stopped";
            StartStopIcon.Kind = PackIconKind.Play;
            StartStopText.Text = "Start";
        }
        else
        {
            _timer.Start();
            _isRunning = true;
            TxtStatus.Text = "Running";
            StartStopIcon.Kind = PackIconKind.Stop;
            StartStopText.Text = "Stop";
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

        TextBox[] maxBoxes = [TxtMax0, TxtMax1, TxtMax2, TxtMax3];
        TextBox[] minBoxes = [TxtMin0, TxtMin1, TxtMin2, TxtMin3];
        maxBoxes[groupIndex].Text = max.ToString("F0");
        minBoxes[groupIndex].Text = min.ToString("F0");

        _groupYAxes[groupIndex]!.Range.Set(min, max);
        MonitorPlot.Refresh();
    }

    private void ApplyScaleFromTextBoxes(int groupIndex)
    {
        if (_groupYAxes[groupIndex] is null) return;

        TextBox[] maxBoxes = [TxtMax0, TxtMax1, TxtMax2, TxtMax3];
        TextBox[] minBoxes = [TxtMin0, TxtMin1, TxtMin2, TxtMin3];

        if (!double.TryParse(maxBoxes[groupIndex].Text, out double max)) return;
        if (!double.TryParse(minBoxes[groupIndex].Text, out double min)) return;
        if (min >= max) return;

        _groupYAxes[groupIndex]!.Range.Set(min, max);
        MonitorPlot.Refresh();
    }

    private void UpdateScaleTextBoxes()
    {
        TextBox[] maxBoxes = [TxtMax0, TxtMax1, TxtMax2, TxtMax3];
        TextBox[] minBoxes = [TxtMin0, TxtMin1, TxtMin2, TxtMin3];

        for (int g = 0; g < 4; g++)
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

        for (int ch = 0; ch < 8; ch++)
        {
            if (_signals[ch] != null)
                _signals[ch]!.Color = GetThemeColor(_channelColorKeys[ch]);
        }

        plot.Legend.BackgroundColor = GetThemeColor("SurfaceVariantBrush");
        plot.Legend.FontColor = GetThemeColor("TextPrimary");
        plot.Legend.OutlineColor = GetThemeColor("BorderDefault");

        // Update group axis colors
        for (int g = 0; g < 4; g++)
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
    //  GAUGE INSTRUMENTS
    // ═══════════════════════════════════════════════════════════

    private static Brush GetWpfBrush(string key)
    {
        return Application.Current.TryFindResource(key) is Brush brush ? brush : Brushes.Gray;
    }

    private void UpdateGauges()
    {
        if (ControlPanel.Visibility != Visibility.Visible) return;

        int lastIdx = PointCount - 1;
        double velCur = _channelData[2][lastIdx];
        double velCmd = _channelData[3][lastIdx];
        double torCur = _channelData[0][lastIdx];
        double torCmd = _channelData[1][lastIdx];
        double posCur = _channelData[4][lastIdx];
        double posCmd = _channelData[5][lastIdx];

        DrawSemiCircleGauge(VelocityGauge, velCur, velCmd, -5000, 5000,
            "ChartCH3Brush", "ChartCH4Brush");
        DrawSemiCircleGauge(TorqueGauge, torCur, torCmd, -3000, 3000,
            "ChartCH1Brush", "ChartCH2Brush");
        DrawPositionBar(posCur, posCmd);

        TxtVelocityCur.Text = velCur.ToString("F0");
        TxtVelocityCmd.Text = $"Cmd: {velCmd:F0}";
        TxtVelocityCurLabel.Text = $"Cur: {velCur:F0}";

        TxtTorqueCur.Text = torCur.ToString("F0");
        TxtTorqueCmd.Text = $"Cmd: {torCmd:F0}";
        TxtTorqueCurLabel.Text = $"Cur: {torCur:F0}";

        TxtPositionCurLabel.Text = $"Cur: {posCur:F0}";
        TxtPositionCmdLabel.Text = $"Cmd: {posCmd:F0}";
    }

    private void DrawSemiCircleGauge(Canvas canvas, double curValue, double cmdValue,
        double minValue, double maxValue, string curBrushKey, string cmdBrushKey)
    {
        canvas.Children.Clear();

        double w = canvas.Width;
        double h = canvas.Height;
        double cx = w / 2;
        double cy = h - 2;
        double radius = Math.Min(cx - 5, h - 8);

        // Background track (full semicircle)
        canvas.Children.Add(CreateArcPath(cx, cy, radius, 0, 1,
            GetWpfBrush("SurfaceBrush"), 10));

        // Normalize: 0 = left (min), 0.5 = center (zero), 1 = right (max)
        double zeroNorm = (0 - minValue) / (maxValue - minValue);
        double cmdNorm = Math.Clamp((cmdValue - minValue) / (maxValue - minValue), 0.001, 0.999);
        double curNorm = Math.Clamp((curValue - minValue) / (maxValue - minValue), 0.001, 0.999);

        // Command arc (from zero to command, semi-transparent)
        double cmdStart = Math.Min(zeroNorm, cmdNorm);
        double cmdEnd = Math.Max(zeroNorm, cmdNorm);
        if (cmdEnd - cmdStart > 0.002)
        {
            var baseBrush = GetWpfBrush(cmdBrushKey);
            var cmdColor = baseBrush is SolidColorBrush scb ? scb.Color : System.Windows.Media.Colors.Gray;
            var cmdBrush = new SolidColorBrush(cmdColor) { Opacity = 0.4 };
            cmdBrush.Freeze();
            canvas.Children.Add(CreateArcPath(cx, cy, radius, cmdStart, cmdEnd, cmdBrush, 8));
        }

        // Current arc (from zero to current)
        double curStart = Math.Min(zeroNorm, curNorm);
        double curEnd = Math.Max(zeroNorm, curNorm);
        if (curEnd - curStart > 0.002)
        {
            canvas.Children.Add(CreateArcPath(cx, cy, radius, curStart, curEnd,
                GetWpfBrush(curBrushKey), 5));
        }

        // Needle line for current value
        double curAngle = Math.PI * (1 - curNorm);
        double nx = cx + radius * Math.Cos(curAngle);
        double ny = cy - radius * Math.Sin(curAngle);
        var needle = new System.Windows.Shapes.Line
        {
            X1 = cx, Y1 = cy, X2 = nx, Y2 = ny,
            Stroke = GetWpfBrush(curBrushKey),
            StrokeThickness = 1.5,
            StrokeEndLineCap = PenLineCap.Round
        };
        canvas.Children.Add(needle);

        // Center dot
        var dot = new System.Windows.Shapes.Ellipse
        {
            Width = 6, Height = 6,
            Fill = GetWpfBrush("TextPrimary")
        };
        Canvas.SetLeft(dot, cx - 3);
        Canvas.SetTop(dot, cy - 3);
        canvas.Children.Add(dot);
    }

    private static System.Windows.Shapes.Path CreateArcPath(
        double cx, double cy, double radius,
        double startNorm, double endNorm,
        Brush stroke, double thickness)
    {
        // Convert normalized 0..1 to angles: 0→π (left), 1→0 (right)
        double startAngle = Math.PI * (1 - startNorm);
        double endAngle = Math.PI * (1 - endNorm);

        var startPt = new Point(
            cx + radius * Math.Cos(startAngle),
            cy - radius * Math.Sin(startAngle));
        var endPt = new Point(
            cx + radius * Math.Cos(endAngle),
            cy - radius * Math.Sin(endAngle));

        bool isLargeArc = Math.Abs(startAngle - endAngle) > Math.PI;

        var figure = new PathFigure { StartPoint = startPt, IsFilled = false };
        figure.Segments.Add(new ArcSegment
        {
            Point = endPt,
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = isLargeArc
        });

        var geo = new PathGeometry();
        geo.Figures.Add(figure);

        return new System.Windows.Shapes.Path
        {
            Data = geo,
            Stroke = stroke,
            StrokeThickness = thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
    }

    private void DrawPositionBar(double curPos, double cmdPos)
    {
        PositionCanvas.Children.Clear();

        double width = PositionCanvas.ActualWidth;
        if (width <= 0) width = 200;
        double height = 32;

        // Calculate min/max from position data
        double minPos = double.MaxValue, maxPos = double.MinValue;
        for (int i = 0; i < PointCount; i++)
        {
            minPos = Math.Min(minPos, _channelData[4][i]);
            maxPos = Math.Max(maxPos, _channelData[4][i]);
        }
        double range = maxPos - minPos;
        if (range < 1) { range = 1000; minPos = curPos - 500; maxPos = curPos + 500; }

        // Track line
        var trackLine = new System.Windows.Shapes.Line
        {
            X1 = 0, Y1 = height / 2, X2 = width, Y2 = height / 2,
            Stroke = GetWpfBrush("BorderDefault"),
            StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        PositionCanvas.Children.Add(trackLine);

        // Command marker (dashed vertical line)
        double cmdNorm = Math.Clamp((cmdPos - minPos) / range, 0, 1);
        double cmdX = cmdNorm * width;
        var cmdLine = new System.Windows.Shapes.Line
        {
            X1 = cmdX, Y1 = 2, X2 = cmdX, Y2 = height - 2,
            Stroke = GetWpfBrush("ChartCH6Brush"),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 3, 2 }
        };
        PositionCanvas.Children.Add(cmdLine);

        // Current marker (triangle + vertical line)
        double curNorm = Math.Clamp((curPos - minPos) / range, 0, 1);
        double curX = curNorm * width;
        var curTriangle = new System.Windows.Shapes.Polygon
        {
            Points = new PointCollection
            {
                new Point(curX, 4),
                new Point(curX - 5, 14),
                new Point(curX + 5, 14)
            },
            Fill = GetWpfBrush("ChartCH5Brush")
        };
        PositionCanvas.Children.Add(curTriangle);

        var curLine = new System.Windows.Shapes.Line
        {
            X1 = curX, Y1 = 14, X2 = curX, Y2 = height - 2,
            Stroke = GetWpfBrush("ChartCH5Brush"),
            StrokeThickness = 2
        };
        PositionCanvas.Children.Add(curLine);

        // Update min/max labels
        TxtPositionMin.Text = minPos.ToString("F0");
        TxtPositionMax.Text = maxPos.ToString("F0");
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
