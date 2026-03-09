using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace RswareDesign.Views;

public partial class ControlPanelView : UserControl
{
    private const double MaxVelocity = 3900;
    private const double MaxLoad = 300;

    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new(99);

    private bool _isEnabled;
    private bool _isJogging;
    private int _jogDirection; // -1 or +1

    private double _currentPosition;
    private double _currentVelocity;
    private double _minPosition = -1000;
    private double _maxPosition = 3000;
    private double _jogSpeed = 50; // position units per tick

    public ControlPanelView()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += OnTimerTick;

        UpdateStatusMonitor();
        UpdateRangeLabel();
    }

    public void StartUpdating()
    {
        // Just show the panel — timer starts when Enable is clicked
        UpdateStatusMonitor();
    }

    public void StopUpdating()
    {
        _timer.Stop();
        _isEnabled = false;
        _isJogging = false;
        UpdateEnableStatus();
    }

    // ═══════════════════════════════════════════════════════════
    //  ENABLE / DISABLE
    // ═══════════════════════════════════════════════════════════

    private void BtnEnable_Click(object sender, RoutedEventArgs e)
    {
        if (_isEnabled) return;
        _isEnabled = true;
        _timer.Start();
        UpdateEnableStatus();
    }

    private void BtnDisable_Click(object sender, RoutedEventArgs e)
    {
        if (!_isEnabled) return;
        _isEnabled = false;
        _isJogging = false;
        _timer.Stop();
        _currentVelocity = 0;
        UpdateEnableStatus();
        UpdateStatusMonitor();
    }

    private void UpdateEnableStatus()
    {
        if (_isEnabled)
        {
            TxtEnableStatus.Text = "● Enabled";
            TxtEnableStatus.Foreground = GetWpfBrush("SuccessBrush");
        }
        else
        {
            TxtEnableStatus.Text = "● Disabled";
            TxtEnableStatus.Foreground = GetWpfBrush("ErrorBrush");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  JOG (press and hold)
    // ═══════════════════════════════════════════════════════════

    private void BtnJogMinus_Down(object sender, MouseButtonEventArgs e)
    {
        if (!_isEnabled) return;
        _isJogging = true;
        _jogDirection = -1;
    }

    private void BtnJogPlus_Down(object sender, MouseButtonEventArgs e)
    {
        if (!_isEnabled) return;
        _isJogging = true;
        _jogDirection = 1;
    }

    private void BtnJog_Up(object sender, MouseButtonEventArgs e)
    {
        _isJogging = false;
        _jogDirection = 0;
    }

    // ═══════════════════════════════════════════════════════════
    //  CONFIRMATION DIALOGS (ZeroSet, ClearFault, Reset)
    // ═══════════════════════════════════════════════════════════

    private void BtnZeroSet_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Zero Set 하시겠습니까?",
            "Zero Set",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        _currentPosition = 0;
        UpdateStatusMonitor();
    }

    private void BtnClearFault_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Clear Fault 하시겠습니까?",
            "Clear Fault",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Mock: just acknowledge
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Reset 하시겠습니까?",
            "Reset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        _isEnabled = false;
        _isJogging = false;
        _timer.Stop();
        _currentPosition = 0;
        _currentVelocity = 0;
        UpdateEnableStatus();
        UpdateStatusMonitor();
    }

    // ═══════════════════════════════════════════════════════════
    //  TIMER TICK
    // ═══════════════════════════════════════════════════════════

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!_isEnabled) return;

        if (_isJogging)
        {
            // Jog: significant movement
            _currentPosition += _jogDirection * _jogSpeed;
            _currentPosition = Math.Clamp(_currentPosition, _minPosition, _maxPosition);
            _currentVelocity = _jogDirection * 800 + (_rng.NextDouble() - 0.5) * 50;
        }
        else
        {
            // Enabled but idle: tiny fluctuation only
            _currentPosition += (_rng.NextDouble() - 0.5) * 2;
            _currentPosition = Math.Clamp(_currentPosition, _minPosition, _maxPosition);
            _currentVelocity *= 0.9; // decay toward zero
            _currentVelocity += (_rng.NextDouble() - 0.5) * 10;
        }

        UpdateStatusMonitor();
    }

    // ═══════════════════════════════════════════════════════════
    //  STATUS MONITOR UPDATE
    // ═══════════════════════════════════════════════════════════

    private void UpdateStatusMonitor()
    {
        double loadPercent = Math.Abs(_currentVelocity / MaxVelocity) * 100;

        // Position card — show actual value (can be negative)
        TxtPositionValue.Text = _currentPosition.ToString("N2");

        // Progress bar: map position within [min, max] range
        double range = _maxPosition - _minPosition;
        double posRatio = range > 0 ? Math.Clamp((_currentPosition - _minPosition) / range, 0, 1) : 0;
        double barMaxWidth = PositionBarGrid.ActualWidth;
        if (barMaxWidth <= 0) barMaxWidth = 150;
        PositionProgressBar.Width = posRatio * barMaxWidth;

        // Velocity card
        TxtVelocityValue.Text = _currentVelocity.ToString("N0");
        TxtVelocityMax.Text = $"{MaxVelocity:N0} rpm";
        DrawVelocityArc(_currentVelocity);

        // Load card
        TxtLoadValue.Text = loadPercent.ToString("F0");
        double loadRatio = Math.Clamp(loadPercent / MaxLoad, 0, 1);
        LoadBarFill.Height = loadRatio * 40;

        if (loadPercent > 100)
            TxtLoadValue.Foreground = GetWpfBrush("ErrorBrush");
        else if (loadPercent > 60)
            TxtLoadValue.Foreground = GetWpfBrush("WarningBrush");
        else
            TxtLoadValue.Foreground = GetWpfBrush("TextPrimary");
    }

    private void UpdateRangeLabel()
    {
        TxtPositionRange.Text = $"{_minPosition:N0} ~ {_maxPosition:N0} mm";
    }

    // ═══════════════════════════════════════════════════════════
    //  VELOCITY ARC GAUGE
    // ═══════════════════════════════════════════════════════════

    private void DrawVelocityArc(double velocity)
    {
        VelocityArcGauge.Children.Clear();

        double size = 48;
        double cx = size / 2;
        double cy = size / 2;
        double radius = 20;
        double thickness = 5;

        var trackEllipse = new System.Windows.Shapes.Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = GetWpfBrush("BackgroundBrush"),
            StrokeThickness = thickness,
            Fill = Brushes.Transparent
        };
        Canvas.SetLeft(trackEllipse, cx - radius);
        Canvas.SetTop(trackEllipse, cy - radius);
        VelocityArcGauge.Children.Add(trackEllipse);

        double ratio = Math.Clamp(Math.Abs(velocity) / MaxVelocity, 0, 0.999);
        if (ratio > 0.01)
        {
            double startAngle = -90;
            double sweepAngle = ratio * 360;
            double endAngle = startAngle + sweepAngle;

            double startRad = startAngle * Math.PI / 180;
            double endRad = endAngle * Math.PI / 180;

            var startPt = new Point(cx + radius * Math.Cos(startRad), cy + radius * Math.Sin(startRad));
            var endPt = new Point(cx + radius * Math.Cos(endRad), cy + radius * Math.Sin(endRad));

            bool isLargeArc = sweepAngle > 180;

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

            var arcBrush = ratio > 0.8 ? GetWpfBrush("ErrorBrush") :
                           ratio > 0.5 ? GetWpfBrush("WarningBrush") :
                                         GetWpfBrush("SecondaryBrush");

            var arcPath = new System.Windows.Shapes.Path
            {
                Data = geo,
                Stroke = arcBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            VelocityArcGauge.Children.Add(arcPath);
        }

        var pctText = new TextBlock
        {
            Text = $"{ratio * 100:F0}%",
            FontSize = 9,
            FontFamily = Application.Current.TryFindResource("FontFamilyCode") as FontFamily,
            Foreground = GetWpfBrush("TextPrimary"),
            TextAlignment = TextAlignment.Center
        };
        pctText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(pctText, cx - pctText.DesiredSize.Width / 2);
        Canvas.SetTop(pctText, cy - pctText.DesiredSize.Height / 2);
        VelocityArcGauge.Children.Add(pctText);
    }

    private static Brush GetWpfBrush(string key)
    {
        return Application.Current.TryFindResource(key) is Brush brush ? brush : Brushes.Gray;
    }
}
