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

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _timer.Tick += OnTimerTick;

        // Start with chart & control panel hidden (shown via button click)
        ChartToolbar.Visibility = Visibility.Collapsed;
        MonitorPlot.Visibility = Visibility.Collapsed;
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
            MonitorPlot.Visibility = Visibility.Collapsed;
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
            MonitorPlot.Visibility = Visibility.Visible;
            ChartStatusBar.Visibility = Visibility.Visible;
        }
    }

    public void ToggleControlPanel()
    {
        ControlPanel.Visibility = ControlPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
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
        plot.Axes.Bottom.Label.Text = "Time (ms)";
        plot.Axes.Left.Label.Text = "Amplitude";
        plot.Axes.Bottom.Label.ForeColor = axisColor;
        plot.Axes.Left.Label.ForeColor = axisColor;
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

        plot.Axes.AutoScale();
        MonitorPlot.Refresh();
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
        for (int s = 0; s < 5; s++)
        {
            _phase++;
            for (int ch = 0; ch < 8; ch++)
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

        CheckBox[] checkboxes = [ChkCh1, ChkCh2, ChkCh3, ChkCh4, ChkCh5, ChkCh6, ChkCh7, ChkCh8];
        for (int i = 0; i < 8; i++)
            _signals[i]!.IsVisible = checkboxes[i].IsChecked == true;

        MonitorPlot.Plot.Axes.AutoScale();
        MonitorPlot.Refresh();
    }

    private void BtnAutoScale_Click(object sender, RoutedEventArgs e)
    {
        MonitorPlot.Plot.Axes.AutoScale();
        MonitorPlot.Refresh();
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
