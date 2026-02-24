using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using RswareDesign.ViewModels;
using RswareDesign.Views;

namespace RswareDesign;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<OpenOscilloscopeMessage>(this, (_, _) =>
        {
            var dialog = new OscilloscopeDialog { Owner = this };
            dialog.ShowDialog();
        });

        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (_, msg) =>
        {
            SwitchTheme(msg.IsDark);
        });
    }

    // ═══════════════════════════════════════════════════════════
    //  THEME SWITCHING (Dark ↔ Light)
    // ═══════════════════════════════════════════════════════════

    private void SwitchTheme(bool isDark)
    {
        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        // 1. Swap color dictionary (DarkColors ↔ LightColors)
        ResourceDictionary? currentColors = null;
        foreach (var dict in mergedDicts)
        {
            var src = dict.Source?.OriginalString ?? "";
            if (src.Contains("DarkColors.xaml") || src.Contains("LightColors.xaml"))
            {
                currentColors = dict;
                break;
            }
        }

        if (currentColors != null)
            mergedDicts.Remove(currentColors);

        var newSource = isDark ? "Themes/DarkColors.xaml" : "Themes/LightColors.xaml";
        mergedDicts.Add(new ResourceDictionary
        {
            Source = new Uri(newSource, UriKind.Relative)
        });

        // 2. Switch MaterialDesign base theme
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }
        catch { /* Ignore if theme API differs */ }

        // 3. Switch Fluent.Ribbon theme
        try
        {
            ThemeManager.Current.ChangeTheme(app, isDark ? "Dark.Blue" : "Light.Blue");
        }
        catch { /* Ignore */ }

        // 4. Switch AvalonDock theme
        try
        {
            dockManager.Theme = isDark
                ? new AvalonDock.Themes.Vs2013DarkTheme()
                : new AvalonDock.Themes.Vs2013LightTheme();
        }
        catch { /* Ignore */ }

        // 5. Re-apply hue slider accent colors if changed from default
        UpdateThemeColors(HueSlider.Value);
    }

    // ═══════════════════════════════════════════════════════════
    //  FONT SCALE SLIDER (global font size)
    // ═══════════════════════════════════════════════════════════

    private static readonly Dictionary<string, double> BaseFontSizes = new()
    {
        ["FontSizeXS"]  = 10,
        ["FontSizeSM"]  = 11,
        ["FontSizeMD"]  = 12,
        ["FontSizeLG"]  = 13,
        ["FontSizeXL"]  = 14,
        ["FontSizeXXL"] = 16,
        ["FontSize3XL"] = 20,
    };

    private void FontScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;

        double scale = e.NewValue / 100.0;
        var res = Application.Current.Resources;

        foreach (var (key, baseSize) in BaseFontSizes)
        {
            res[key] = Math.Round(baseSize * scale, 1);
        }

        FontScaleLabel.Text = $"{e.NewValue:F0}%";
    }

    // ═══════════════════════════════════════════════════════════
    //  HUE SLIDER (accent color tone change)
    // ═══════════════════════════════════════════════════════════

    private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        UpdateThemeColors(e.NewValue);
    }

    private void UpdateThemeColors(double hue)
    {
        var res = Application.Current.Resources;
        var vm = DataContext as MainWindowViewModel;
        bool isDark = vm?.IsDarkTheme ?? true;

        // Primary colors
        var primary = HslToColor(hue, 0.86, 0.46);
        res["PrimaryBrush"] = new SolidColorBrush(primary);
        res["PrimaryHoverBrush"] = new SolidColorBrush(HslToColor(hue, 0.86, 0.38));

        // Secondary = analogous hue (+35)
        double secHue = (hue + 35) % 360;
        var secondary = HslToColor(secHue, 0.80, 0.65);
        res["SecondaryBrush"] = new SolidColorBrush(secondary);
        res["SecondaryHoverBrush"] = new SolidColorBrush(HslToColor(secHue, 0.80, 0.58));

        // RibbonItemBrush follows Secondary
        res["RibbonItemBrush"] = new SolidColorBrush(secondary);

        // Theme-aware derived colors
        res["StatusBarBrush"] = new SolidColorBrush(HslToColor(hue, 1.0, isDark ? 0.40 : 0.45));
        res["BorderFocused"] = new SolidColorBrush(HslToColor(hue, 0.55, isDark ? 0.72 : 0.50));
        res["SelectedRowBrush"] = new SolidColorBrush(HslToColor(hue, 0.50, isDark ? 0.25 : 0.82));

        // Update MaterialDesign theme palette
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetPrimaryColor(primary);
            theme.SetSecondaryColor(secondary);
            paletteHelper.SetTheme(theme);
        }
        catch { /* Ignore if theme API differs */ }
    }

    // ═══════════════════════════════════════════════════════════
    //  HSL → Color Conversion
    // ═══════════════════════════════════════════════════════════

    private static Color HslToColor(double h, double s, double l)
    {
        h %= 360;
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        double m = l - c / 2;

        double r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255));
    }
}
