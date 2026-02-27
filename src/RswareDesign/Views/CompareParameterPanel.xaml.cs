using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using RswareDesign.Models;

namespace RswareDesign.Views;

public partial class CompareParameterPanel : UserControl
{
    public static readonly DependencyProperty PanelLabelProperty =
        DependencyProperty.Register(nameof(PanelLabel), typeof(string), typeof(CompareParameterPanel), new PropertyMetadata(""));

    public static readonly DependencyProperty HeaderBrushProperty =
        DependencyProperty.Register(nameof(HeaderBrush), typeof(Brush), typeof(CompareParameterPanel),
            new PropertyMetadata(null));

    public Brush? HeaderBrush
    {
        get => (Brush?)GetValue(HeaderBrushProperty);
        set => SetValue(HeaderBrushProperty, value);
    }

    public static readonly DependencyProperty LabelBrushProperty =
        DependencyProperty.Register(nameof(LabelBrush), typeof(Brush), typeof(CompareParameterPanel),
            new PropertyMetadata(null));

    public Brush? LabelBrush
    {
        get => (Brush?)GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public string PanelLabel
    {
        get => (string)GetValue(PanelLabelProperty);
        set => SetValue(PanelLabelProperty, value);
    }

    public static readonly DependencyProperty LoadedCountProperty =
        DependencyProperty.Register(nameof(LoadedCount), typeof(int), typeof(CompareParameterPanel), new PropertyMetadata(0));

    public int LoadedCount
    {
        get => (int)GetValue(LoadedCountProperty);
        set => SetValue(LoadedCountProperty, value);
    }

    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(CompareParameterPanel), new PropertyMetadata(0));

    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public static readonly DependencyProperty ActionButtonsProperty =
        DependencyProperty.Register(nameof(ActionButtons), typeof(List<ActionButton>), typeof(CompareParameterPanel),
            new PropertyMetadata(new List<ActionButton>(), OnActionButtonsChanged));

    public List<ActionButton> ActionButtons
    {
        get => (List<ActionButton>)GetValue(ActionButtonsProperty);
        set => SetValue(ActionButtonsProperty, value);
    }

    public event EventHandler? CloseRequested;
    public Func<bool>? CanCloseCheck { get; set; }

    private DispatcherTimer? _countTimer;

    private static readonly string[] ColumnWidthKeys =
    [
        "Size.Col.Favorite", "Size.Col.FtNum", "Size.Col.Parameter", "Size.Col.Value",
        "Size.Col.Units", "Size.Col.Default", "Size.Col.Min", "Size.Col.Max"
    ];

    public CompareParameterPanel()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyColumnWidths();
    }

    private void ApplyColumnWidths()
    {
        for (int i = 0; i < ColumnWidthKeys.Length && i < parameterGrid.Columns.Count; i++)
        {
            if (Application.Current.TryFindResource(ColumnWidthKeys[i]) is double w)
                parameterGrid.Columns[i].Width = new DataGridLength(w);
        }
    }

    /// <summary>
    /// Starts a counting animation from 0 to TotalCount.
    /// Call after setting TotalCount and adding to visual tree.
    /// </summary>
    public void StartLoadingAnimation()
    {
        if (TotalCount <= 0) return;

        LoadedCount = 0;

        // Calculate interval: complete in ~1.5 seconds, min 15ms per tick
        int total = TotalCount;
        int step = Math.Max(1, total / 60); // ~60 ticks over duration
        int intervalMs = Math.Max(15, 1500 / Math.Max(1, total / step));

        _countTimer?.Stop();
        _countTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
        _countTimer.Tick += (_, _) =>
        {
            int next = LoadedCount + step;
            if (next >= total)
            {
                LoadedCount = total;
                _countTimer.Stop();
                _countTimer = null;
            }
            else
            {
                LoadedCount = next;
            }
        };
        _countTimer.Start();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        // Block close if this is the last panel
        if (CanCloseCheck != null && !CanCloseCheck())
            return;

        // Animate out, then fire close
        RenderTransform = new ScaleTransform(1, 1);
        RenderTransformOrigin = new Point(0.5, 0.5);

        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = ease };
        var shrinkX = new DoubleAnimation(1, 0.85, TimeSpan.FromMilliseconds(200)) { EasingFunction = ease };
        var shrinkY = new DoubleAnimation(1, 0.85, TimeSpan.FromMilliseconds(200)) { EasingFunction = ease };

        fadeOut.Completed += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        BeginAnimation(OpacityProperty, fadeOut);
        ((ScaleTransform)RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, shrinkX);
        ((ScaleTransform)RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, shrinkY);
    }

    private static void OnActionButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CompareParameterPanel panel)
            panel.RebuildActionButtons();
    }

    public void RebuildActionButtons()
    {
        actionItemsControl.Items.Clear();
        var buttons = ActionButtons;
        if (buttons == null || buttons.Count == 0) return;

        foreach (var ab in buttons)
        {
            if (ab.IsSeparator)
            {
                actionItemsControl.Items.Add(new Separator
                {
                    Margin = new Thickness(0, 4, 0, 4),
                    Background = Application.Current.TryFindResource("DividerBrush") as Brush,
                });
                continue;
            }

            var icon = new PackIcon
            {
                Kind = Enum.TryParse<PackIconKind>(ab.IconKind, out var kind) ? kind : PackIconKind.Help,
                Width = 14, Height = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
            };

            var label = new TextBlock
            {
                Text = ab.Label,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = (double)(Application.Current.TryFindResource("FontSizeXS") ?? 10.0),
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(icon);
            sp.Children.Add(label);

            var btn = new Button
            {
                Content = sp,
                Height = (double)(Application.Current.TryFindResource("Size.ActionButtonHeight") ?? 28.0),
                Margin = new Thickness(0, 2, 0, 2),
                Cursor = System.Windows.Input.Cursors.Hand,
                HorizontalContentAlignment = HorizontalAlignment.Left,
            };

            switch (ab.Style)
            {
                case "Primary":
                    btn.Background = Application.Current.TryFindResource("PrimaryBrush") as Brush;
                    btn.Foreground = Application.Current.TryFindResource("PrimaryForegroundBrush") as Brush ?? Brushes.White;
                    btn.BorderThickness = new Thickness(0);
                    break;
                case "Secondary":
                    btn.Background = Application.Current.TryFindResource("SecondaryBrush") as Brush;
                    btn.Foreground = Application.Current.TryFindResource("BackgroundBrush") as Brush ?? Brushes.Black;
                    btn.BorderThickness = new Thickness(0);
                    break;
                default: // Outlined
                    btn.Background = Brushes.Transparent;
                    btn.Foreground = Application.Current.TryFindResource("TextPrimary") as Brush ?? Brushes.White;
                    btn.BorderBrush = Application.Current.TryFindResource("BorderDefault") as Brush;
                    btn.BorderThickness = new Thickness(1);
                    break;
            }

            actionItemsControl.Items.Add(btn);
        }
    }
}
