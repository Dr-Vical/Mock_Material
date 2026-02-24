using System.Windows;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using ScottPlot;

namespace RswareDesign.Views;

public partial class OscilloscopeDialog : Window
{
    private const int PointCount = 500;
    private const double PeriodMs = 0.1; // 10 kHz sample rate

    private readonly double[] _ch1Data = new double[PointCount];
    private readonly double[] _ch2Data = new double[PointCount];
    private readonly double[] _ch3Data = new double[PointCount];
    private readonly double[] _ch4Data = new double[PointCount];

    private ScottPlot.Plottables.Signal? _sigCh1;
    private ScottPlot.Plottables.Signal? _sigCh2;
    private ScottPlot.Plottables.Signal? _sigCh3;
    private ScottPlot.Plottables.Signal? _sigCh4;

    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new(42);
    private double _phase;
    private bool _isRunning;

    public OscilloscopeDialog()
    {
        InitializeComponent();

        GenerateInitialData();
        SetupChart();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _timer.Tick += OnTimerTick;
    }

    private void GenerateInitialData()
    {
        for (int i = 0; i < PointCount; i++)
        {
            _phase = i;
            _ch1Data[i] = GeneratePoint(0);
            _ch2Data[i] = GeneratePoint(1);
            _ch3Data[i] = GeneratePoint(2);
            _ch4Data[i] = GeneratePoint(3);
        }
    }

    private double GeneratePoint(int channel)
    {
        double noise = _rng.NextDouble() - 0.5;
        double p = _phase * 0.04;
        return channel switch
        {
            0 => Math.Sin(p) * 5000 + Math.Sin(p * 3.7) * 800 + noise * 50,
            1 => Math.Cos(p) * 3500 + Math.Cos(p * 2.3) * 500 + noise * 30,
            2 => Math.Sin(p * 2.5) * 1800 + Math.Sin(p * 7.1) * 400 + noise * 80,
            3 => Math.Sin(p * 0.7) * 300 + noise * 100,
            _ => 0
        };
    }

    private void SetupChart()
    {
        var plot = OscPlot.Plot;

        // Dark theme
        plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");
        plot.DataBackground.Color = ScottPlot.Color.FromHex("#252526");
        plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#3C3C3C");

        // Axes styling
        var axisColor = ScottPlot.Color.FromHex("#9E9E9E");
        plot.Axes.Bottom.Label.Text = "Time (ms)";
        plot.Axes.Left.Label.Text = "Amplitude";
        plot.Axes.Bottom.Label.ForeColor = axisColor;
        plot.Axes.Left.Label.ForeColor = axisColor;
        plot.Axes.Bottom.TickLabelStyle.ForeColor = axisColor;
        plot.Axes.Left.TickLabelStyle.ForeColor = axisColor;
        plot.Axes.Bottom.MajorTickStyle.Color = axisColor;
        plot.Axes.Left.MajorTickStyle.Color = axisColor;
        plot.Axes.Bottom.FrameLineStyle.Color = ScottPlot.Color.FromHex("#424242");
        plot.Axes.Left.FrameLineStyle.Color = ScottPlot.Color.FromHex("#424242");
        plot.Axes.Right.FrameLineStyle.Color = ScottPlot.Color.FromHex("#424242");
        plot.Axes.Top.FrameLineStyle.Color = ScottPlot.Color.FromHex("#424242");

        // CH1: Position (Yellow)
        _sigCh1 = plot.Add.Signal(_ch1Data, PeriodMs);
        _sigCh1.LegendText = "CH1 Position (counts)";
        _sigCh1.Color = ScottPlot.Color.FromHex("#FFEB3B");
        _sigCh1.LineWidth = 1.5f;

        // CH2: Velocity (Green)
        _sigCh2 = plot.Add.Signal(_ch2Data, PeriodMs);
        _sigCh2.LegendText = "CH2 Velocity";
        _sigCh2.Color = ScottPlot.Color.FromHex("#4CAF50");
        _sigCh2.LineWidth = 1.5f;

        // CH3: Current (Cyan)
        _sigCh3 = plot.Add.Signal(_ch3Data, PeriodMs);
        _sigCh3.LegendText = "CH3 Current (mA)";
        _sigCh3.Color = ScottPlot.Color.FromHex("#29B6F6");
        _sigCh3.LineWidth = 1.5f;

        // CH4: Position Error (Red)
        _sigCh4 = plot.Add.Signal(_ch4Data, PeriodMs);
        _sigCh4.LegendText = "CH4 Pos Error";
        _sigCh4.Color = ScottPlot.Color.FromHex("#EF5350");
        _sigCh4.LineWidth = 1.2f;

        // Legend
        plot.Legend.IsVisible = true;
        plot.Legend.Alignment = Alignment.UpperRight;
        plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#2D2D30");
        plot.Legend.FontColor = ScottPlot.Color.FromHex("#E0E0E0");
        plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#424242");

        plot.Axes.AutoScale();
        OscPlot.Refresh();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        // Shift 5 points per tick for visible flow
        for (int s = 0; s < 5; s++)
        {
            _phase++;
            Array.Copy(_ch1Data, 1, _ch1Data, 0, PointCount - 1);
            Array.Copy(_ch2Data, 1, _ch2Data, 0, PointCount - 1);
            Array.Copy(_ch3Data, 1, _ch3Data, 0, PointCount - 1);
            Array.Copy(_ch4Data, 1, _ch4Data, 0, PointCount - 1);

            _ch1Data[PointCount - 1] = GeneratePoint(0);
            _ch2Data[PointCount - 1] = GeneratePoint(1);
            _ch3Data[PointCount - 1] = GeneratePoint(2);
            _ch4Data[PointCount - 1] = GeneratePoint(3);
        }

        OscPlot.Refresh();
    }

    private void Channel_Changed(object sender, RoutedEventArgs e)
    {
        if (_sigCh1 is null) return;

        _sigCh1.IsVisible = ChkCh1.IsChecked == true;
        _sigCh2!.IsVisible = ChkCh2.IsChecked == true;
        _sigCh3!.IsVisible = ChkCh3.IsChecked == true;
        _sigCh4!.IsVisible = ChkCh4.IsChecked == true;

        OscPlot.Plot.Axes.AutoScale();
        OscPlot.Refresh();
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

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        Close();
    }
}
