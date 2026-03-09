using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using RswareDesign.Models;
using RswareDesign.Services;
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

    // Dynamic grouping — rebuilt when channel selection changes
    private List<List<int>> _groupChannels = [[0, 1], [2, 3]];
    private List<string> _groupNames = ["Position", "Velocity"];
    private List<IYAxis?> _groupYAxes = [null, null];

    // Dynamic scale panel TextBox references
    private readonly List<TextBox> _scaleMaxBoxes = [];
    private readonly List<TextBox> _scaleMinBoxes = [];

    // Per-channel colors (initialized from theme, updated by color picker)
    private readonly ScottPlot.Color[] _channelColors = new ScottPlot.Color[4];

    // Preset palette colors (the 8 ChartCH brush hex values)
    private static readonly System.Windows.Media.Color[] PaletteColors =
    [
        // Row 1: bright
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFEB3B"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4CAF50"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF29B6F6"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEF5350"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF9800"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFAB47BC"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF26A69A"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEC407A"),
        // Row 2: pastel / additional
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF81D4FA"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA5D6A7"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFCC80"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCE93D8"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF80CBC4"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF48FB1"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFB0"),
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFB0BEC5"),
    ];

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
        bool collapse = FavoritesInnerContent.Visibility == Visibility.Visible;
        if (collapse)
        {
            _savedFavoritesWidth = FavoritesColumn.Width;
            FavoritesInnerContent.Visibility = Visibility.Collapsed;
            FavoritesColumn.Width = GridLength.Auto;
            FavoritesColumn.MinWidth = 0;
            FavoritesSplitter.Visibility = Visibility.Collapsed;
            FavoritesToggleIcon.Kind = PackIconKind.ChevronLeft;
        }
        else
        {
            FavoritesInnerContent.Visibility = Visibility.Visible;
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
        plot.Axes.Bottom.TickLabelStyle.FontSize = 10;
        plot.Axes.Left.TickLabelStyle.ForeColor = axisColor;
        plot.Axes.Left.TickLabelStyle.FontSize = 10;
        plot.Axes.Bottom.MajorTickStyle.Color = axisColor;
        plot.Axes.Left.MajorTickStyle.Color = axisColor;
        plot.Axes.Bottom.FrameLineStyle.Color = frameColor;
        plot.Axes.Left.FrameLineStyle.Color = frameColor;
        plot.Axes.Right.FrameLineStyle.Color = frameColor;
        plot.Axes.Top.FrameLineStyle.Color = frameColor;

        float[] lineWidths = [1.5f, 1.0f, 1.5f, 1.0f];

        // Initialize channel colors from theme
        for (int ch = 0; ch < 4; ch++)
            _channelColors[ch] = GetThemeColor(_channelColorKeys[ch]);

        for (int ch = 0; ch < 4; ch++)
        {
            _signals[ch] = plot.Add.Signal(_channelData[ch], PeriodMs);
            _signals[ch]!.LegendText = _channelNames[ch];
            _signals[ch]!.Color = _channelColors[ch];
            _signals[ch]!.LineWidth = lineWidths[ch];
        }

        plot.Legend.IsVisible = true;
        plot.Legend.Alignment = Alignment.UpperRight;
        plot.Legend.BackgroundColor = GetThemeColor("SurfaceVariantBrush");
        plot.Legend.FontColor = GetThemeColor("TextPrimary");
        plot.Legend.OutlineColor = GetThemeColor("BorderDefault");

        // Build dynamic groups and scale panel
        RebuildScaleGroups();

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

    private void BtnSaveSetting_Click(object sender, RoutedEventArgs e)
    {
        ComboBox[] combos = [CmbCh1, CmbCh2, CmbCh3, CmbCh4];
        CheckBox[] checkboxes = [ChkCh1, ChkCh2, ChkCh3, ChkCh4];

        var channels = new List<ScopeChannelSetting>();
        for (int i = 0; i < 4; i++)
        {
            var spColor = _channelColors[i];
            var wpfColor = System.Windows.Media.Color.FromArgb(spColor.A, spColor.R, spColor.G, spColor.B);

            // Find scale values: locate which group contains this channel
            double scaleMax = 10000, scaleMin = -10000;
            for (int g = 0; g < _groupChannels.Count; g++)
            {
                if (_groupChannels[g].Contains(i))
                {
                    if (g < _scaleMaxBoxes.Count && double.TryParse(_scaleMaxBoxes[g].Text, out double mx))
                        scaleMax = mx;
                    if (g < _scaleMinBoxes.Count && double.TryParse(_scaleMinBoxes[g].Text, out double mn))
                        scaleMin = mn;
                    break;
                }
            }

            channels.Add(new ScopeChannelSetting
            {
                Name = combos[i].SelectedItem as string ?? _channelNames[i],
                Color = wpfColor.ToString(),
                Enabled = checkboxes[i].IsChecked == true,
                ScaleMax = scaleMax,
                ScaleMin = scaleMin
            });
        }

        var dlg = new SaveFileDialog
        {
            InitialDirectory = ScopeSettingService.GetScopeFolder(),
            Filter = "INI Files|*.ini",
            DefaultExt = ".ini",
            FileName = "ScopeSetting.ini"
        };

        if (dlg.ShowDialog() == true)
            ScopeSettingService.Save(dlg.FileName, channels);
    }

    private void BtnLoadSetting_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            InitialDirectory = ScopeSettingService.GetScopeFolder(),
            Filter = "INI Files|*.ini",
            DefaultExt = ".ini"
        };

        if (dlg.ShowDialog() != true) return;

        var channels = ScopeSettingService.Load(dlg.FileName);
        if (channels.Count == 0) return;

        ComboBox[] combos = [CmbCh1, CmbCh2, CmbCh3, CmbCh4];
        Ellipse[] ellipses = [EllipseCh1, EllipseCh2, EllipseCh3, EllipseCh4];
        CheckBox[] checkboxes = [ChkCh1, ChkCh2, ChkCh3, ChkCh4];

        int count = Math.Min(channels.Count, 4);
        for (int i = 0; i < count; i++)
        {
            var ch = channels[i];

            // Set ComboBox selection
            var idx = Array.IndexOf(ParameterItems, ch.Name);
            if (idx >= 0)
                combos[i].SelectedIndex = idx;

            // Update channel color
            try
            {
                var wpfColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ch.Color);
                var brush = new SolidColorBrush(wpfColor);
                _channelColors[i] = new ScottPlot.Color(wpfColor.R, wpfColor.G, wpfColor.B, wpfColor.A);
                ellipses[i].Fill = brush;
                combos[i].Foreground = brush;
                if (_signals[i] != null)
                    _signals[i]!.Color = _channelColors[i];
            }
            catch
            {
                // Ignore invalid color strings
            }

            // Set checkbox
            checkboxes[i].IsChecked = ch.Enabled;
        }

        // Rebuild groups and scale panel with new channel names
        RebuildScaleGroups();

        // Apply loaded scale values per channel -> per group
        for (int i = 0; i < count; i++)
        {
            var ch = channels[i];
            for (int g = 0; g < _groupChannels.Count; g++)
            {
                if (!_groupChannels[g].Contains(i)) continue;
                if (g < _scaleMaxBoxes.Count)
                    _scaleMaxBoxes[g].Text = ch.ScaleMax.ToString("F0");
                if (g < _scaleMinBoxes.Count)
                    _scaleMinBoxes[g].Text = ch.ScaleMin.ToString("F0");
                if (g < _groupYAxes.Count && _groupYAxes[g] is not null)
                    _groupYAxes[g]!.Range.Set(ch.ScaleMin, ch.ScaleMax);
                break;
            }
        }

        // Update group axis colors
        for (int g = 0; g < _groupYAxes.Count; g++)
        {
            if (_groupYAxes[g] is null || g >= _groupChannels.Count) continue;
            int firstCh = _groupChannels[g][0];
            var groupColor = _channelColors[firstCh];
            _groupYAxes[g]!.TickLabelStyle.ForeColor = groupColor;
            _groupYAxes[g]!.MajorTickStyle.Color = groupColor;
            _groupYAxes[g]!.FrameLineStyle.Color = groupColor;
        }

        MonitorPlot.Refresh();
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

        // Rebuild groups based on new channel names
        RebuildScaleGroups();

        MonitorPlot.Plot.Axes.AutoScale();
        MonitorPlot.Refresh();
        UpdateScaleTextBoxes();
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
    //  CHANNEL COLOR PICKER
    // ═══════════════════════════════════════════════════════════

    private int _colorPickerChannelIndex;

    private void ChannelColor_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        if (!int.TryParse(fe.Tag?.ToString(), out int chIndex)) return;

        _colorPickerChannelIndex = chIndex;

        // Populate swatches
        ColorSwatches.Children.Clear();
        foreach (var paletteColor in PaletteColors)
        {
            var swatch = new Ellipse
            {
                Width = 22,
                Height = 22,
                Fill = new SolidColorBrush(paletteColor),
                Margin = new Thickness(2),
                Cursor = Cursors.Hand,
                Tag = paletteColor
            };
            swatch.MouseLeftButtonDown += ColorSwatch_Click;
            ColorSwatches.Children.Add(swatch);
        }

        ColorPickerPopup.IsOpen = true;
        e.Handled = true;

        // Close popup when clicking outside
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, () =>
        {
            Mouse.Capture(this, CaptureMode.SubTree);
        });
    }

    private void CloseColorPickerIfOpen()
    {
        if (ColorPickerPopup.IsOpen)
        {
            ColorPickerPopup.IsOpen = false;
            Mouse.Capture(null);
        }
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        if (!ColorPickerPopup.IsOpen) return;

        // Check if click is inside the popup
        var popupChild = ColorPickerPopup.Child;
        if (popupChild != null)
        {
            var pos = e.GetPosition(popupChild);
            var rect = new Rect(0, 0, popupChild.RenderSize.Width, popupChild.RenderSize.Height);
            if (rect.Contains(pos)) return; // Inside popup — let it handle
        }

        // Click outside popup — close it
        CloseColorPickerIfOpen();
    }

    private void ColorSwatch_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Ellipse swatch) return;
        if (swatch.Tag is not System.Windows.Media.Color wpfColor) return;

        int chIndex = _colorPickerChannelIndex;
        var newBrush = new SolidColorBrush(wpfColor);
        var spColor = new ScottPlot.Color(wpfColor.R, wpfColor.G, wpfColor.B, wpfColor.A);

        // Update stored channel color
        _channelColors[chIndex] = spColor;

        // Update Ellipse fill
        Ellipse[] ellipses = [EllipseCh1, EllipseCh2, EllipseCh3, EllipseCh4];
        ellipses[chIndex].Fill = newBrush;

        // Update ComboBox foreground
        ComboBox[] combos = [CmbCh1, CmbCh2, CmbCh3, CmbCh4];
        combos[chIndex].Foreground = newBrush;

        // Update signal color
        if (_signals[chIndex] != null)
            _signals[chIndex]!.Color = spColor;

        // Update scale group colors
        UpdateScaleGroupColors();

        // Update group axis colors
        for (int g = 0; g < _groupChannels.Count; g++)
        {
            if (_groupYAxes[g] is null) continue;
            int firstCh = _groupChannels[g][0];
            var groupColor = _channelColors[firstCh];
            _groupYAxes[g]!.TickLabelStyle.ForeColor = groupColor;
            _groupYAxes[g]!.MajorTickStyle.Color = groupColor;
            _groupYAxes[g]!.FrameLineStyle.Color = groupColor;
        }

        MonitorPlot.Refresh();
        ColorPickerPopup.IsOpen = false;
        e.Handled = true;
    }

    // ═══════════════════════════════════════════════════════════
    //  DYNAMIC SCALE GROUPING
    // ═══════════════════════════════════════════════════════════

    private static string GetChannelCategory(string name)
    {
        string[] keywords = ["Position", "Velocity", "Current", "Angle", "Voltage", "Power"];
        foreach (var kw in keywords)
        {
            if (name.Contains(kw, StringComparison.OrdinalIgnoreCase))
                return kw;
        }
        return name;
    }

    private static string GetCategoryUnit(string category)
    {
        return category switch
        {
            "Position" => "pulse",
            "Velocity" => "rpm",
            "Current" => "A",
            "Angle" => "deg",
            "Voltage" => "V",
            "Power" => "W",
            _ => ""
        };
    }

    /// <summary>
    /// Rebuilds scale groups from current channel names, recreates Y-axes, and rebuilds the scale panel UI.
    /// </summary>
    private void RebuildScaleGroups()
    {
        var plot = MonitorPlot.Plot;

        // Determine categories for each channel
        var categories = new string[4];
        for (int ch = 0; ch < 4; ch++)
            categories[ch] = GetChannelCategory(_channelNames[ch]);

        // Group channels by category (preserving order of first appearance)
        var categoryOrder = new List<string>();
        var categoryChannels = new Dictionary<string, List<int>>();
        for (int ch = 0; ch < 4; ch++)
        {
            var cat = categories[ch];
            if (!categoryChannels.ContainsKey(cat))
            {
                categoryOrder.Add(cat);
                categoryChannels[cat] = [];
            }
            categoryChannels[cat].Add(ch);
        }

        _groupChannels = [];
        _groupNames = [];
        foreach (var cat in categoryOrder)
        {
            _groupChannels.Add(categoryChannels[cat]);
            _groupNames.Add(cat);
        }

        int groupCount = _groupChannels.Count;

        // Remove old additional Y-axes (keep Axes.Left as group 0)
        // Remove any previously added left axes beyond the default
        var existingLeftAxes = plot.Axes.GetAxes(ScottPlot.Edge.Left).ToList();
        foreach (var ax in existingLeftAxes)
        {
            if (ax != plot.Axes.Left)
                plot.Axes.Remove(ax);
        }

        // Create Y-axes
        _groupYAxes = [];
        for (int g = 0; g < groupCount; g++)
        {
            if (g == 0)
            {
                _groupYAxes.Add(plot.Axes.Left);
                int firstCh = _groupChannels[g][0];
                var color = _channelColors[firstCh];
                plot.Axes.Left.TickLabelStyle.ForeColor = color;
                plot.Axes.Left.TickLabelStyle.FontSize = 10;
                plot.Axes.Left.MajorTickStyle.Color = color;
                plot.Axes.Left.FrameLineStyle.Color = color;
            }
            else
            {
                var axis = plot.Axes.AddLeftAxis();
                int firstCh = _groupChannels[g][0];
                var color = _channelColors[firstCh];
                axis.Label.Text = "";
                axis.TickLabelStyle.ForeColor = color;
                axis.TickLabelStyle.FontSize = 10;
                axis.MajorTickStyle.Color = color;
                axis.FrameLineStyle.Color = color;
                _groupYAxes.Add(axis);
            }
        }

        // Assign each signal to its group's Y-axis
        for (int g = 0; g < groupCount; g++)
        {
            foreach (int ch in _groupChannels[g])
            {
                if (_signals[ch] != null)
                    _signals[ch]!.Axes.YAxis = _groupYAxes[g]!;
            }
        }

        // Rebuild the scale panel UI
        RebuildScalePanelUI();
    }

    /// <summary>
    /// Programmatically builds one Border per scale group in ScaleGroupsPanel.
    /// </summary>
    private void RebuildScalePanelUI()
    {
        ScaleGroupsPanel.Children.Clear();
        _scaleMaxBoxes.Clear();
        _scaleMinBoxes.Clear();

        for (int g = 0; g < _groupChannels.Count; g++)
        {
            int groupIndex = g;
            int firstCh = _groupChannels[g][0];

            // Get the WPF color for the first channel in this group
            var spColor = _channelColors[firstCh];
            var wpfColor = System.Windows.Media.Color.FromArgb(spColor.A, spColor.R, spColor.G, spColor.B);
            var groupBrush = new SolidColorBrush(wpfColor);

            // Outer Border with left accent
            var border = new Border
            {
                BorderThickness = new Thickness(3, 0, 0, 0),
                BorderBrush = groupBrush,
                CornerRadius = (CornerRadius)FindResource("Radius.SM"),
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 4),
            };
            if (TryFindResource("SurfaceVariantBrush") is Brush bgBrush)
                border.Background = bgBrush;

            var outerStack = new StackPanel();

            // Header: group name + auto-scale button
            var header = new DockPanel();
            var autoBtn = new Button
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(2),
                Cursor = Cursors.Hand,
                ToolTip = $"Auto Scale {_groupNames[g]}",
                Tag = groupIndex
            };
            DockPanel.SetDock(autoBtn, Dock.Right);
            var autoIcon = new PackIcon
            {
                Kind = PackIconKind.ArrowExpandVertical,
                Width = 12,
                Height = 12
            };
            if (TryFindResource("TextSecondary") is Brush tsb)
                autoIcon.Foreground = tsb;
            autoBtn.Content = autoIcon;
            autoBtn.Click += BtnAutoScaleGroup_Click;
            header.Children.Add(autoBtn);

            var headerText = new TextBlock
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            if (TryFindResource("FontSizeXS") is double fontXs)
                headerText.FontSize = fontXs;
            var runName = new System.Windows.Documents.Run(_groupNames[g])
            {
                FontWeight = FontWeights.SemiBold
            };
            if (TryFindResource("TextPrimary") is Brush tpb)
                runName.Foreground = tpb;
            headerText.Inlines.Add(runName);

            var unit = GetCategoryUnit(_groupNames[g]);
            if (!string.IsNullOrEmpty(unit))
            {
                var runUnit = new System.Windows.Documents.Run($" ({unit})");
                if (TryFindResource("TextSecondary") is Brush tsb2)
                    runUnit.Foreground = tsb2;
                headerText.Inlines.Add(runUnit);
            }
            header.Children.Add(headerText);
            outerStack.Children.Add(header);

            // Max/Min grid
            var grid = new Grid { Margin = new Thickness(0, 2, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Max label
            var maxLabel = new TextBlock { Text = "Max", VerticalAlignment = System.Windows.VerticalAlignment.Center };
            if (TryFindResource("FontSizeXS") is double fxs2) maxLabel.FontSize = fxs2;
            if (TryFindResource("TextSecondary") is Brush tsb3) maxLabel.Foreground = tsb3;
            Grid.SetRow(maxLabel, 0);
            Grid.SetColumn(maxLabel, 0);
            grid.Children.Add(maxLabel);

            // Max TextBox
            var maxBox = new TextBox
            {
                Text = "10000",
                Tag = groupIndex,
                Padding = new Thickness(3, 1, 3, 1),
                Margin = new Thickness(0, 0, 0, 1)
            };
            if (TryFindResource("FontFamilyCode") is FontFamily ff) maxBox.FontFamily = ff;
            if (TryFindResource("FontSizeXS") is double fxs3) maxBox.FontSize = fxs3;
            if (TryFindResource("ValueHighlightBrush") is Brush vhb) maxBox.Foreground = vhb;
            if (TryFindResource("SurfaceBrush") is Brush sb) maxBox.Background = sb;
            if (TryFindResource("BorderDefault") is Brush bd) maxBox.BorderBrush = bd;
            maxBox.LostFocus += ScaleValue_LostFocus;
            maxBox.KeyDown += ScaleValue_KeyDown;
            Grid.SetRow(maxBox, 0);
            Grid.SetColumn(maxBox, 1);
            grid.Children.Add(maxBox);
            _scaleMaxBoxes.Add(maxBox);

            // Min label
            var minLabel = new TextBlock { Text = "Min", VerticalAlignment = System.Windows.VerticalAlignment.Center };
            if (TryFindResource("FontSizeXS") is double fxs4) minLabel.FontSize = fxs4;
            if (TryFindResource("TextSecondary") is Brush tsb4) minLabel.Foreground = tsb4;
            Grid.SetRow(minLabel, 1);
            Grid.SetColumn(minLabel, 0);
            grid.Children.Add(minLabel);

            // Min TextBox
            var minBox = new TextBox
            {
                Text = "-10000",
                Tag = groupIndex,
                Padding = new Thickness(3, 1, 3, 1)
            };
            if (TryFindResource("FontFamilyCode") is FontFamily ff2) minBox.FontFamily = ff2;
            if (TryFindResource("FontSizeXS") is double fxs5) minBox.FontSize = fxs5;
            if (TryFindResource("ValueHighlightBrush") is Brush vhb2) minBox.Foreground = vhb2;
            if (TryFindResource("SurfaceBrush") is Brush sb2) minBox.Background = sb2;
            if (TryFindResource("BorderDefault") is Brush bd2) minBox.BorderBrush = bd2;
            minBox.LostFocus += ScaleValue_LostFocus;
            minBox.KeyDown += ScaleValue_KeyDown;
            Grid.SetRow(minBox, 1);
            Grid.SetColumn(minBox, 1);
            grid.Children.Add(minBox);
            _scaleMinBoxes.Add(minBox);

            outerStack.Children.Add(grid);

            // Channel color indicators
            var indicators = new WrapPanel { Margin = new Thickness(0, 3, 0, 0) };
            foreach (int ch in _groupChannels[g])
            {
                var chColor = _channelColors[ch];
                var chWpfColor = System.Windows.Media.Color.FromArgb(chColor.A, chColor.R, chColor.G, chColor.B);
                var chBrush = new SolidColorBrush(chWpfColor);

                var dot = new Ellipse
                {
                    Fill = chBrush,
                    Width = 6,
                    Height = 6,
                    Margin = new Thickness(0, 0, 2, 0),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                indicators.Children.Add(dot);

                // Short label from channel name
                var shortName = GetShortChannelLabel(ch);
                var lbl = new TextBlock
                {
                    Text = shortName,
                    Foreground = chBrush,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0)
                };
                if (TryFindResource("FontSizeXS") is double fxs6) lbl.FontSize = fxs6;
                indicators.Children.Add(lbl);
            }
            outerStack.Children.Add(indicators);

            border.Child = outerStack;
            ScaleGroupsPanel.Children.Add(border);
        }
    }

    /// <summary>
    /// Returns a short label for the channel indicator in the scale panel.
    /// </summary>
    private string GetShortChannelLabel(int chIndex)
    {
        var name = _channelNames[chIndex];
        // Extract a meaningful short label
        if (name.Contains("Feedback", StringComparison.OrdinalIgnoreCase)) return "Fbk";
        if (name.Contains("Command", StringComparison.OrdinalIgnoreCase)) return "Cmd";
        if (name.Contains("Error", StringComparison.OrdinalIgnoreCase)) return "Err";
        if (name.Contains("Master", StringComparison.OrdinalIgnoreCase)) return "Mst";
        if (name.Contains("Follower", StringComparison.OrdinalIgnoreCase)) return "Flw";
        if (name.Contains("Motor", StringComparison.OrdinalIgnoreCase)) return "Mtr";
        // Fallback: first 3 chars
        return name.Length > 3 ? name[..3] : name;
    }

    /// <summary>
    /// Updates the border and indicator colors in the scale panel to match current channel colors.
    /// </summary>
    private void UpdateScaleGroupColors()
    {
        if (ScaleGroupsPanel.Children.Count != _groupChannels.Count) return;

        for (int g = 0; g < _groupChannels.Count; g++)
        {
            if (ScaleGroupsPanel.Children[g] is not Border border) continue;

            int firstCh = _groupChannels[g][0];
            var spColor = _channelColors[firstCh];
            var wpfColor = System.Windows.Media.Color.FromArgb(spColor.A, spColor.R, spColor.G, spColor.B);
            border.BorderBrush = new SolidColorBrush(wpfColor);

            // Update channel indicator dots and labels
            if (border.Child is not StackPanel outerStack) continue;

            // The WrapPanel with indicators is the last child
            var indicators = outerStack.Children.OfType<WrapPanel>().FirstOrDefault();
            if (indicators == null) continue;

            int childIdx = 0;
            foreach (int ch in _groupChannels[g])
            {
                var chColor = _channelColors[ch];
                var chWpfColor = System.Windows.Media.Color.FromArgb(chColor.A, chColor.R, chColor.G, chColor.B);
                var chBrush = new SolidColorBrush(chWpfColor);

                // dot (Ellipse) then label (TextBlock)
                if (childIdx < indicators.Children.Count && indicators.Children[childIdx] is Ellipse dot)
                    dot.Fill = chBrush;
                childIdx++;
                if (childIdx < indicators.Children.Count && indicators.Children[childIdx] is TextBlock lbl)
                    lbl.Foreground = chBrush;
                childIdx++;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SCALE GROUP CONTROLS (dynamic)
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
        if (groupIndex >= _groupYAxes.Count || _groupYAxes[groupIndex] is null) return;
        if (groupIndex >= _groupChannels.Count) return;

        double min = double.MaxValue, max = double.MinValue;
        foreach (int ch in _groupChannels[groupIndex])
        {
            if (_signals[ch] is null || !_signals[ch]!.IsVisible) continue;
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

        if (groupIndex < _scaleMaxBoxes.Count)
            _scaleMaxBoxes[groupIndex].Text = max.ToString("F0");
        if (groupIndex < _scaleMinBoxes.Count)
            _scaleMinBoxes[groupIndex].Text = min.ToString("F0");

        _groupYAxes[groupIndex]!.Range.Set(min, max);
        MonitorPlot.Refresh();
    }

    private void ApplyScaleFromTextBoxes(int groupIndex)
    {
        if (groupIndex >= _groupYAxes.Count || _groupYAxes[groupIndex] is null) return;
        if (groupIndex >= _scaleMaxBoxes.Count || groupIndex >= _scaleMinBoxes.Count) return;

        if (!double.TryParse(_scaleMaxBoxes[groupIndex].Text, out double max)) return;
        if (!double.TryParse(_scaleMinBoxes[groupIndex].Text, out double min)) return;
        if (min >= max) return;

        _groupYAxes[groupIndex]!.Range.Set(min, max);
        MonitorPlot.Refresh();
    }

    private void UpdateScaleTextBoxes()
    {
        for (int g = 0; g < _groupYAxes.Count; g++)
        {
            if (_groupYAxes[g] is null) continue;
            if (g >= _scaleMaxBoxes.Count || g >= _scaleMinBoxes.Count) continue;
            var axisRange = _groupYAxes[g]!.Range;
            _scaleMaxBoxes[g].Text = axisRange.Max.ToString("F0");
            _scaleMinBoxes[g].Text = axisRange.Min.ToString("F0");
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

        // Refresh channel colors from theme (unless user has overridden)
        for (int ch = 0; ch < 4; ch++)
        {
            if (_signals[ch] != null)
                _signals[ch]!.Color = _channelColors[ch];
        }

        plot.Legend.BackgroundColor = GetThemeColor("SurfaceVariantBrush");
        plot.Legend.FontColor = GetThemeColor("TextPrimary");
        plot.Legend.OutlineColor = GetThemeColor("BorderDefault");

        // Update group axis colors
        for (int g = 0; g < _groupYAxes.Count; g++)
        {
            if (_groupYAxes[g] is null) continue;
            if (g >= _groupChannels.Count) continue;
            int firstCh = _groupChannels[g][0];
            var groupColor = _channelColors[firstCh];
            _groupYAxes[g]!.TickLabelStyle.ForeColor = groupColor;
            _groupYAxes[g]!.MajorTickStyle.Color = groupColor;
            _groupYAxes[g]!.FrameLineStyle.Color = groupColor;
        }

        // Rebuild scale panel to pick up new theme brushes
        RebuildScalePanelUI();
        UpdateScaleTextBoxes();

        MonitorPlot.Refresh();
    }


    // ═══════════════════════════════════════════════════════════
    //  DELETE FAVORITE WITH DEL KEY
    // ═══════════════════════════════════════════════════════════

    private void FavList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && FavoritesListBox.SelectedItem is Parameter param)
        {
            RemoveFavorite(param);
            e.Handled = true;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  STAR TOGGLE IN FAVORITES LIST
    // ═══════════════════════════════════════════════════════════

    private void FavStar_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        if (fe.DataContext is not Parameter param) return;

        // Toggle off -> remove from favorites
        param.IsFavorite = false;
        RemoveFavorite(param);
        e.Handled = true;
    }

    private void RemoveFavorite(Parameter param)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        // Un-star in the main parameter list too
        var mainParam = vm.Parameters.FirstOrDefault(p => p.FtNumber == param.FtNumber);
        if (mainParam != null)
            mainParam.IsFavorite = false;

        vm.FavoriteParameters.Remove(param);

        WeakReferenceMessenger.Default.Send(new FavoriteAnimationMessage(false));
    }

    // ═══════════════════════════════════════════════════════════
    //  DRAG-TO-REORDER FAVORITES (manual mouse tracking)
    // ═══════════════════════════════════════════════════════════

    private bool _isFavDragging;

    private void FavList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isFavDragging = false;
        _dragStartPoint = e.GetPosition(FavoritesListBox);

        var item = GetListBoxItemAtPoint(FavoritesListBox, _dragStartPoint);
        _dragSourceIndex = item != null
            ? FavoritesListBox.ItemContainerGenerator.IndexFromContainer(item)
            : -1;
    }

    private void FavList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragSourceIndex < 0 || e.LeftButton != MouseButtonState.Pressed) return;
        if (DataContext is not MainWindowViewModel vm) return;

        var pos = e.GetPosition(FavoritesListBox);
        var diff = pos.Y - _dragStartPoint.Y;

        if (!_isFavDragging && Math.Abs(diff) > SystemParameters.MinimumVerticalDragDistance)
            _isFavDragging = true;

        if (!_isFavDragging) return;

        var targetItem = GetListBoxItemAtPoint(FavoritesListBox, pos);
        if (targetItem == null) return;
        int targetIndex = FavoritesListBox.ItemContainerGenerator.IndexFromContainer(targetItem);

        if (targetIndex != _dragSourceIndex && targetIndex >= 0
            && targetIndex < vm.FavoriteParameters.Count)
        {
            vm.FavoriteParameters.Move(_dragSourceIndex, targetIndex);
            _dragSourceIndex = targetIndex;
            _dragStartPoint = pos;
        }
    }

    private void FavList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isFavDragging = false;
        _dragSourceIndex = -1;
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
