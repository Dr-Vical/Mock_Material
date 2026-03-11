using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RswareDesign.Services;

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
    private double _currentLoad; // independent load simulation
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

    public void SetDriveIdentity(string driveId)
    {
        // Identity is shown via the parent Window title
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
            TxtEnableStatus.Text = LocalizationService.Get("loc.control.status.enabled");
            TxtEnableStatus.Foreground = GetWpfBrush("SuccessBrush");
            StartEnableBorderAnimation();
        }
        else
        {
            TxtEnableStatus.Text = LocalizationService.Get("loc.control.status.disabled");
            TxtEnableStatus.Foreground = GetWpfBrush("ErrorBrush");
            StopEnableBorderAnimation();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ENABLE BORDER ANIMATION (flowing green border)
    // ═══════════════════════════════════════════════════════════

    private void StartEnableBorderAnimation()
    {
        var greenColor = GetThemeColor("SuccessBrush");

        var lightGreen = Color.FromArgb(140, greenColor.R, greenColor.G, greenColor.B);

        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new(Colors.Transparent, 0.0),
                new(lightGreen, 0.25),
                new(lightGreen, 0.5),
                new(Colors.Transparent, 0.75),
                new(Colors.Transparent, 1.0)
            },
            RelativeTransform = new RotateTransform(0, 0.5, 0.5)
        };

        EnableBorder.BorderBrush = brush;

        var rotate = (RotateTransform)brush.RelativeTransform;
        var animation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(6))
        {
            RepeatBehavior = RepeatBehavior.Forever
        };
        rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
    }

    private void StopEnableBorderAnimation()
    {
        EnableBorder.BorderBrush = Brushes.Transparent;
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
        var owner = Window.GetWindow(this);
        if (!ConfirmActionDialog.Ask(owner,
                LocalizationService.Get("loc.control.confirm.zeroset.title"),
                LocalizationService.Get("loc.control.confirm.zeroset.msg"),
                MaterialDesignThemes.Wpf.PackIconKind.Numeric0CircleOutline,
                LocalizationService.Get("loc.control.confirm.zeroset.btn"), "WarningBrush"))
            return;

        _currentPosition = 0;
        UpdateStatusMonitor();
    }

    private void BtnClearFault_Click(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        if (!ConfirmActionDialog.Ask(owner,
                LocalizationService.Get("loc.control.confirm.clearfault.title"),
                LocalizationService.Get("loc.control.confirm.clearfault.msg"),
                MaterialDesignThemes.Wpf.PackIconKind.AlertRemoveOutline,
                LocalizationService.Get("loc.control.confirm.clearfault.btn"), "WarningBrush"))
            return;

        // Mock: just acknowledge
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        if (!ConfirmActionDialog.Ask(owner,
                LocalizationService.Get("loc.control.confirm.reset.title"),
                LocalizationService.Get("loc.control.confirm.reset.msg"),
                MaterialDesignThemes.Wpf.PackIconKind.RestartAlert,
                LocalizationService.Get("loc.control.confirm.reset.btn"), "WarningBrush"))
            return;

        _isEnabled = false;
        _isJogging = false;
        _timer.Stop();
        _currentPosition = 0;
        _currentVelocity = 0;
        UpdateEnableStatus();
        UpdateStatusMonitor();
    }

    // ═══════════════════════════════════════════════════════════
    //  MOTION BUTTONS (Move ABS / Move Rel / Move Zero)
    // ═══════════════════════════════════════════════════════════

    private void BtnMoveAbs_Click(object sender, RoutedEventArgs e)
    {
        FlashTargetPositionBorder();
    }

    private void BtnMoveRel_Click(object sender, RoutedEventArgs e)
    {
        FlashTargetPositionBorder();
    }

    private void BtnMoveZero_Click(object sender, RoutedEventArgs e)
    {
        _currentPosition = 0;
        UpdateStatusMonitor();
    }

    private void FlashTargetPositionBorder()
    {
        var highlightColor = GetThemeColor("PrimaryBrush");
        var normalColor = GetThemeColor("BorderDefault");

        // Flash TxtTgtPosA (always)
        FlashTextBoxBorder(TxtTgtPosA, highlightColor, normalColor);

        // Flash TxtTgtPosB only if 왕복 is checked
        if (ChkReciprocate.IsChecked == true)
            FlashTextBoxBorder(TxtTgtPosB, highlightColor, normalColor);
    }

    private static void FlashTextBoxBorder(TextBox textBox, Color highlightColor, Color normalColor)
    {
        var brush = new SolidColorBrush(normalColor);
        textBox.BorderBrush = brush;
        // Keep BorderThickness at 1 to avoid layout shift
        textBox.BorderThickness = new Thickness(1);

        var animation = new ColorAnimationUsingKeyFrames();
        animation.KeyFrames.Add(new LinearColorKeyFrame(highlightColor, TimeSpan.FromMilliseconds(200)));
        animation.KeyFrames.Add(new LinearColorKeyFrame(highlightColor, TimeSpan.FromMilliseconds(800)));
        animation.KeyFrames.Add(new LinearColorKeyFrame(normalColor, TimeSpan.FromMilliseconds(1200)));
        animation.Completed += (_, _) =>
        {
            textBox.BorderBrush = new SolidColorBrush(normalColor);
        };

        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
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
            // Load: 100~140% when jogging
            _currentLoad += (120 - _currentLoad) * 0.15 + (_rng.NextDouble() - 0.5) * 8;
            _currentLoad = Math.Clamp(_currentLoad, 100, 140);
        }
        else
        {
            // Enabled but idle: tiny fluctuation only
            _currentPosition += (_rng.NextDouble() - 0.5) * 2;
            _currentPosition = Math.Clamp(_currentPosition, _minPosition, _maxPosition);
            _currentVelocity *= 0.9; // decay toward zero
            _currentVelocity += (_rng.NextDouble() - 0.5) * 10;
            // Load: decay to 5~15% when idle
            _currentLoad += (10 - _currentLoad) * 0.1 + (_rng.NextDouble() - 0.5) * 3;
            _currentLoad = Math.Clamp(_currentLoad, 0, 300);
        }

        UpdateStatusMonitor();
    }

    // ═══════════════════════════════════════════════════════════
    //  STATUS MONITOR UPDATE
    // ═══════════════════════════════════════════════════════════

    private void UpdateStatusMonitor()
    {
        // Position card — show actual value (can be negative)
        TxtPositionValue.Text = _currentPosition.ToString("N2");

        // Progress bar: map position within [min, max] range
        double range = _maxPosition - _minPosition;
        double posRatio = range > 0 ? Math.Clamp((_currentPosition - _minPosition) / range, 0, 1) : 0;
        double barMaxWidth = PositionBarGrid.ActualWidth;
        if (barMaxWidth <= 0) barMaxWidth = 150;
        PositionProgressBar.Width = posRatio * barMaxWidth;

        // Target position red marker
        if (double.TryParse(TxtTgtPosA.Text, out double targetPos))
        {
            double tgtRatio = range > 0 ? Math.Clamp((targetPos - _minPosition) / range, 0, 1) : 0;
            TargetPositionMarker.Margin = new Thickness(tgtRatio * barMaxWidth - 1, 0, 0, 0);
            TargetPositionMarker.Visibility = Visibility.Visible;
        }

        // Velocity card — arc gauge
        TxtVelocityValue.Text = (_currentVelocity >= 0 ? "+" : "") + _currentVelocity.ToString("N0");
        TxtVelocityMin.Text = $"-{MaxVelocity:N0}";
        TxtVelocityMax.Text = $"{MaxVelocity:N0} rpm";

        double targetSpeed = double.TryParse(TxtTargetSpeed.Text, out var ts) ? ts : 500;
        DrawSpeedArcGauge(_currentVelocity, targetSpeed);

        // Load card — use independent _currentLoad
        double loadPercent = _currentLoad;
        TxtLoadValue.Text = loadPercent.ToString("F0");
        double loadRatio = Math.Clamp(loadPercent / MaxLoad, 0, 1);
        LoadBarFill.Height = loadRatio * 80; // bar height is now 80

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
    //  VELOCITY NEEDLE GAUGE (0 at top, -max left, +max right)
    // ═══════════════════════════════════════════════════════════

    private static readonly SolidColorBrush NeedleBrush = CreateFrozen(Color.FromRgb(200, 200, 60));
    private static readonly SolidColorBrush NeedleCmdBrush = CreateFrozen(Color.FromRgb(100, 185, 220));

    private static SolidColorBrush CreateFrozen(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    private void DrawSpeedArcGauge(double velocity, double targetSpeed)
    {
        SpeedArcCanvas.Children.Clear();

        double cx = 50, cy = 48;
        double arcR = 38;
        // 0 at top (angle 0), -max at left (-135°), +max at right (+135°)
        // Value v maps to angle: (v / MaxVelocity) * 135°
        double totalSweep = 270; // from -135° to +135°

        Point ToPoint(double angleDeg, double r)
        {
            double rad = angleDeg * Math.PI / 180;
            return new Point(cx + r * Math.Sin(rad), cy - r * Math.Cos(rad));
        }

        double ValueToAngle(double v) => Math.Clamp(v / MaxVelocity, -1, 1) * 135;

        var trackBrush = GetWpfBrush("SurfaceBrush");
        var tickBrush = GetWpfBrush("TextSecondary");

        // 1. Arc track (background)
        var startPt = ToPoint(-135, arcR);
        var endPt = ToPoint(135, arcR);
        var fig = new PathFigure { StartPoint = startPt, IsClosed = false };
        fig.Segments.Add(new ArcSegment(endPt, new Size(arcR, arcR), 0, true, SweepDirection.Clockwise, true));
        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        SpeedArcCanvas.Children.Add(new Path
        {
            Stroke = trackBrush, StrokeThickness = 5, Data = geo,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // 2. Tick marks (major every 1/6 of range)
        for (int i = 0; i <= 6; i++)
        {
            double tickAngle = -135 + (totalSweep * i / 6.0);
            var p1 = ToPoint(tickAngle, arcR - 3);
            var p2 = ToPoint(tickAngle, arcR + 5);
            SpeedArcCanvas.Children.Add(new Line
            {
                X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y,
                Stroke = tickBrush, StrokeThickness = 1
            });
        }
        // Minor ticks
        for (int i = 0; i < 12; i++)
        {
            double tickAngle = -135 + (totalSweep * i / 12.0);
            var p1 = ToPoint(tickAngle, arcR - 1);
            var p2 = ToPoint(tickAngle, arcR + 3);
            SpeedArcCanvas.Children.Add(new Line
            {
                X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y,
                Stroke = trackBrush, StrokeThickness = 0.5
            });
        }

        // 3. Cmd needle (target speed, thin, light blue)
        double cmdAngle = ValueToAngle(targetSpeed);
        var cmdTip = ToPoint(cmdAngle, arcR - 6);
        var cmdBase = ToPoint(cmdAngle, 8);
        SpeedArcCanvas.Children.Add(new Line
        {
            X1 = cmdBase.X, Y1 = cmdBase.Y, X2 = cmdTip.X, Y2 = cmdTip.Y,
            Stroke = NeedleCmdBrush, StrokeThickness = 1.5,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // 4. Current needle (velocity, thick, yellow-green)
        double curAngle = ValueToAngle(velocity);
        var curTip = ToPoint(curAngle, arcR - 6);
        var curBase = ToPoint(curAngle, 6);
        SpeedArcCanvas.Children.Add(new Line
        {
            X1 = curBase.X, Y1 = curBase.Y, X2 = curTip.X, Y2 = curTip.Y,
            Stroke = NeedleBrush, StrokeThickness = 2,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        });

        // 5. Center dot
        var dot = new System.Windows.Shapes.Ellipse
        {
            Width = 6, Height = 6, Fill = GetWpfBrush("TextPrimary")
        };
        Canvas.SetLeft(dot, cx - 3);
        Canvas.SetTop(dot, cy - 3);
        SpeedArcCanvas.Children.Add(dot);

        // Update Cmd/Cur label
        TxtVelocityCmdCur.Text = $"Cmd: {targetSpeed:N0}  Cur: {velocity:N0}";
    }

    private static Brush GetWpfBrush(string key)
    {
        return Application.Current.TryFindResource(key) is Brush brush ? brush : Brushes.Gray;
    }

    private static Color GetThemeColor(string key)
    {
        return Application.Current.TryFindResource(key) is SolidColorBrush brush ? brush.Color : Colors.Gray;
    }
}
