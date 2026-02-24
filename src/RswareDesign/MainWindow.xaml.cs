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
            SwitchTheme(msg.ThemeName);
        });

        WeakReferenceMessenger.Default.Register<HueChangedMessage>(this, (_, msg) =>
        {
            UpdateThemeColors(msg.Hue, msg.Saturation);
        });

        WeakReferenceMessenger.Default.Register<FontSizeChangedMessage>(this, (_, msg) =>
        {
            UpdateFontSizes(msg.FontSize);
        });

        WeakReferenceMessenger.Default.Register<ShowAdminPasswordMessage>(this, (_, msg) =>
        {
            var dialog = new AdminPasswordDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                // Password verified — open admin feature
            }
        });
    }

    // ═══════════════════════════════════════════════════════════
    //  THEME SWITCHING (Dark / Gray / Light)
    // ═══════════════════════════════════════════════════════════

    private void SwitchTheme(string themeName)
    {
        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        // 1. Swap color dictionary
        ResourceDictionary? currentColors = null;
        foreach (var dict in mergedDicts)
        {
            var src = dict.Source?.OriginalString ?? "";
            if (src.Contains("DarkColors.xaml") || src.Contains("GrayColors.xaml") || src.Contains("LightColors.xaml"))
            {
                currentColors = dict;
                break;
            }
        }

        if (currentColors != null)
            mergedDicts.Remove(currentColors);

        var newSource = themeName switch
        {
            "Light" => "Themes/LightColors.xaml",
            "Gray"  => "Themes/GrayColors.xaml",
            _       => "Themes/DarkColors.xaml",
        };
        mergedDicts.Add(new ResourceDictionary
        {
            Source = new Uri(newSource, UriKind.Relative)
        });

        bool isDark = themeName != "Light";

        // 2. Switch MaterialDesign base theme
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }
        catch { /* Ignore */ }

        // 3. Switch Fluent.Ribbon theme
        try
        {
            ThemeManager.Current.ChangeTheme(app, isDark ? "Dark.Steel" : "Light.Steel");
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

        // 5. Re-apply accent colors with current hue/saturation
        var vm = DataContext as MainWindowViewModel;
        if (vm != null)
            UpdateThemeColors(vm.HueValue, vm.SaturationValue);
    }

    // ═══════════════════════════════════════════════════════════
    //  ACCENT COLOR (hue + saturation based icon/accent colors)
    // ═══════════════════════════════════════════════════════════

    private void UpdateThemeColors(double hue, double satPercent)
    {
        var res = Application.Current.Resources;
        var vm = DataContext as MainWindowViewModel;
        bool isDark = vm?.SelectedTheme != "Light";

        // Convert 0-100 slider → 0.0-1.0
        double sat = satPercent / 100.0;

        // Primary colors
        var primary = HslToColor(hue, sat, isDark ? 0.60 : 0.42);
        res["PrimaryBrush"] = new SolidColorBrush(primary);
        res["PrimaryHoverBrush"] = new SolidColorBrush(HslToColor(hue, sat, isDark ? 0.52 : 0.35));

        // Secondary = analogous hue (+20)
        double secHue = (hue + 20) % 360;
        double secSat = Math.Max(0, sat * 0.7);
        var secondary = HslToColor(secHue, secSat, isDark ? 0.75 : 0.55);
        res["SecondaryBrush"] = new SolidColorBrush(secondary);
        res["SecondaryHoverBrush"] = new SolidColorBrush(HslToColor(secHue, secSat, isDark ? 0.68 : 0.48));

        // RibbonItemBrush follows Secondary
        res["RibbonItemBrush"] = new SolidColorBrush(secondary);

        // Theme-aware derived colors
        res["StatusBarBrush"] = new SolidColorBrush(HslToColor(hue, sat * 0.8, isDark ? 0.38 : 0.42));
        res["BorderFocused"] = new SolidColorBrush(HslToColor(hue, secSat, isDark ? 0.65 : 0.48));
        res["SelectedRowBrush"] = new SolidColorBrush(HslToColor(hue, sat * 0.6, isDark ? 0.28 : 0.82));

        // Update MaterialDesign theme palette
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetPrimaryColor(primary);
            theme.SetSecondaryColor(secondary);
            paletteHelper.SetTheme(theme);
        }
        catch { /* Ignore */ }
    }

    // ═══════════════════════════════════════════════════════════
    //  FONT SIZE
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

    private void UpdateFontSizes(int baseMd)
    {
        double scale = baseMd / 12.0;
        var res = Application.Current.Resources;
        foreach (var (key, baseSize) in BaseFontSizes)
            res[key] = Math.Round(baseSize * scale, 1);
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
