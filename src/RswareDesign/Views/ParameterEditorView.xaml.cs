using System;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using RswareDesign.ViewModels;

namespace RswareDesign.Views;

public partial class ParameterEditorView : UserControl
{
    /// <summary>
    /// Responsive column layout specs.
    /// Priority 0: FT NUM, PARAMETER, VALUE — kept visible, font scales down last
    /// Priority 1: UNITS, DEFAULT, MIN, MAX — shrinks/hides first
    /// </summary>
    private static readonly ColSpec[] Specs =
    [
        new(80,  50,  0),   // FT NUM
        new(220, 100, 0),   // PARAMETER
        new(120, 60,  0),   // VALUE
        new(100, 0,   1),   // UNITS
        new(120, 0,   1),   // DEFAULT
        new(100, 0,   1),   // MIN
        new(100, 0,   1),   // MAX
    ];

    private double _baseFontSize;
    private bool _isRecalculating;

    private const double MinFontScale = 0.72;
    private const double HideThreshold = 30;
    private const double ScrollBarReserve = 20;

    public ParameterEditorView()
    {
        InitializeComponent();
        parameterGrid.Loaded += OnGridLoaded;

        // Listen for global font scale changes
        WeakReferenceMessenger.Default.Register<FontScaleChangedMessage>(this, (r, _) =>
        {
            ((ParameterEditorView)r).OnFontScaleChanged();
        });
    }

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        _baseFontSize = parameterGrid.FontSize > 0 ? parameterGrid.FontSize : 11;
        parameterGrid.SizeChanged += (_, args) =>
        {
            if (args.WidthChanged)
                RecalculateColumns();
        };
        RecalculateColumns();
    }

    private void OnFontScaleChanged()
    {
        // Re-read base font size from current resources (reflects global scale)
        if (Application.Current.Resources["FontSizeSM"] is double scaledBase)
            _baseFontSize = scaledBase;

        RecalculateColumns();
    }

    private void RecalculateColumns()
    {
        if (_isRecalculating) return;
        _isRecalculating = true;

        try
        {
            var cols = parameterGrid.Columns;
            if (cols.Count != Specs.Length) return;

            double available = parameterGrid.ActualWidth - ScrollBarReserve;
            if (available <= 0) return;

            // Sum desired widths by priority group
            double p0Sum = 0, p1Sum = 0;
            foreach (var s in Specs)
            {
                if (s.Priority == 0) p0Sum += s.Desired;
                else p1Sum += s.Desired;
            }

            double total = p0Sum + p1Sum;

            if (available >= total)
            {
                // ── Comfortable: all columns at desired width (don't over-stretch) ──
                for (int i = 0; i < Specs.Length; i++)
                {
                    cols[i].Visibility = Visibility.Visible;
                    cols[i].Width = new DataGridLength(Specs[i].Desired);
                }
                parameterGrid.FontSize = _baseFontSize;
            }
            else if (available > p0Sum)
            {
                // ── Squeeze secondary: P0 at desired, P1 shrinks proportionally ──
                double p1Space = available - p0Sum;
                double ratio = p1Space / p1Sum;

                for (int i = 0; i < Specs.Length; i++)
                {
                    if (Specs[i].Priority == 0)
                    {
                        cols[i].Visibility = Visibility.Visible;
                        cols[i].Width = new DataGridLength(Specs[i].Desired);
                    }
                    else
                    {
                        double w = Specs[i].Desired * ratio;
                        if (w < HideThreshold)
                        {
                            cols[i].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            cols[i].Visibility = Visibility.Visible;
                            cols[i].Width = new DataGridLength(w);
                        }
                    }
                }
                parameterGrid.FontSize = _baseFontSize;
            }
            else
            {
                // ── Squeeze primary: P1 hidden, P0 shrinks + font reduces ──
                for (int i = 0; i < Specs.Length; i++)
                {
                    if (Specs[i].Priority == 1)
                        cols[i].Visibility = Visibility.Collapsed;
                }

                double scale = Math.Clamp(available / p0Sum, MinFontScale, 1.0);

                for (int i = 0; i < Specs.Length; i++)
                {
                    if (Specs[i].Priority == 0)
                    {
                        cols[i].Visibility = Visibility.Visible;
                        cols[i].Width = new DataGridLength(
                            Math.Max(Specs[i].Min, Specs[i].Desired * scale));
                    }
                }

                parameterGrid.FontSize = _baseFontSize * Math.Max(MinFontScale, scale);
            }
        }
        finally
        {
            _isRecalculating = false;
        }
    }

    private readonly record struct ColSpec(double Desired, double Min, int Priority);
}
