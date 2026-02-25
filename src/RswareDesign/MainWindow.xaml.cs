using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ControlzEx.Theming;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using RswareDesign.Services;
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

        WeakReferenceMessenger.Default.Register<ComparePanelChangedMessage>(this, (_, msg) =>
        {
            UpdateComparePanels();
        });

        WeakReferenceMessenger.Default.Register<TreeNodeSelectedMessage>(this, (_, msg) =>
        {
            // Refresh action buttons on all existing panels when tree selection changes
            var newButtons = ActionButtonRegistry.GetForNodeType(msg.NodeType);
            foreach (var panel in _comparePanels.Values)
            {
                panel.ActionButtons = newButtons;
                panel.RebuildActionButtons();
            }
        });

        WeakReferenceMessenger.Default.Register<ShowExitConfirmMessage>(this, (_, _) =>
        {
            var dialog = new ConfirmExitDialog { Owner = this };
            if (dialog.ShowDialog() == true)
                Application.Current.Shutdown();
        });

        // Initialize Panel A on startup + apply window chrome
        Loaded += (_, _) =>
        {
            UpdateComparePanels();
            ApplyWindowChrome(isDark: true); // default is dark theme
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  COMPARE PANEL MANAGEMENT (A/B/C/D)
    // ═══════════════════════════════════════════════════════════

    private readonly Dictionary<string, CompareParameterPanel> _comparePanels = new();
    private readonly HashSet<string> _newlyAddedPanels = new();

    private void UpdateComparePanels()
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        var panelStates = new Dictionary<string, bool>
        {
            ["A"] = vm.IsPanelAVisible,
            ["B"] = vm.IsPanelBVisible,
            ["C"] = vm.IsPanelCVisible,
            ["D"] = vm.IsPanelDVisible,
        };

        // Enforce minimum 1 panel
        int visibleCount = panelStates.Values.Count(v => v);
        if (visibleCount == 0)
        {
            // Find the panel that was just turned off and turn it back on
            // (this shouldn't normally happen due to VM guard, but just in case)
            vm.IsPanelAVisible = true;
            return;
        }

        // Remove panels that are no longer visible
        foreach (var id in _comparePanels.Keys.ToList())
        {
            if (!panelStates.GetValueOrDefault(id))
            {
                _comparePanels.Remove(id);
            }
        }

        // Add panels that should be visible
        foreach (var (id, visible) in panelStates)
        {
            if (visible && !_comparePanels.ContainsKey(id))
            {
                int total = vm.Parameters.Count;
                string nodeType = vm.SelectedNodeType;
                var panel = new CompareParameterPanel
                {
                    PanelLabel = id,
                    DataContext = vm,
                    HeaderBrush = Application.Current.TryFindResource($"Panel{id}Brush") as Brush,
                    LabelBrush = Application.Current.TryFindResource($"Panel{id}Accent") as Brush,
                    TotalCount = total,
                    LoadedCount = 0,
                    ActionButtons = ActionButtonRegistry.GetForNodeType(nodeType),
                };
                panel.CloseRequested += (s, _) =>
                {
                    int currentVisible = new[] { vm.IsPanelAVisible, vm.IsPanelBVisible, vm.IsPanelCVisible, vm.IsPanelDVisible }.Count(v => v);
                    if (currentVisible <= 1) return;

                    switch (id)
                    {
                        case "A": vm.IsPanelAVisible = false; break;
                        case "B": vm.IsPanelBVisible = false; break;
                        case "C": vm.IsPanelCVisible = false; break;
                        case "D": vm.IsPanelDVisible = false; break;
                    }
                };
                _comparePanels[id] = panel;
                _newlyAddedPanels.Add(id);
            }
        }

        RebuildCenterLayout();
    }

    private void RebuildCenterLayout()
    {
        centerPanelGrid.Children.Clear();
        centerPanelGrid.RowDefinitions.Clear();
        centerPanelGrid.ColumnDefinitions.Clear();

        var orderedPanels = _comparePanels.OrderBy(p => p.Key).ToList();
        int count = orderedPanels.Count;

        if (count == 0) return;

        if (count == 1)
        {
            centerPanelGrid.Children.Add(orderedPanels[0].Value);
        }
        else if (count == 2)
        {
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(orderedPanels[0].Value, 0);
            Grid.SetColumn(orderedPanels[1].Value, 1);
            centerPanelGrid.Children.Add(orderedPanels[0].Value);
            centerPanelGrid.Children.Add(orderedPanels[1].Value);
        }
        else
        {
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            centerPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            centerPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            int[] cols = [0, 1, 0, 1];
            int[] rows = [0, 0, 1, 1];
            for (int i = 0; i < count && i < 4; i++)
            {
                Grid.SetColumn(orderedPanels[i].Value, cols[i]);
                Grid.SetRow(orderedPanels[i].Value, rows[i]);
                centerPanelGrid.Children.Add(orderedPanels[i].Value);
            }
        }

        // Animate newly added panels (fade-in + slight scale)
        foreach (var (id, panel) in orderedPanels)
        {
            if (_newlyAddedPanels.Contains(id))
            {
                panel.Opacity = 0;
                panel.RenderTransform = new ScaleTransform(0.95, 0.95);
                panel.RenderTransformOrigin = new Point(0.5, 0.5);

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                var scaleX = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                var scaleY = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                panel.BeginAnimation(OpacityProperty, fadeIn);
                ((ScaleTransform)panel.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
                ((ScaleTransform)panel.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);

                // Start loading count animation after fade-in
                panel.StartLoadingAnimation();
            }
        }
        _newlyAddedPanels.Clear();
    }

    // ═══════════════════════════════════════════════════════════
    //  THEME SWITCHING (Dark / Gray / Light)
    // ═══════════════════════════════════════════════════════════

    private void SwitchTheme(string themeName)
    {
        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        bool isDark = themeName != "Light";

        // 1. Switch MaterialDesign base theme (FIRST — adds its own resource dicts)
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }
        catch { /* Ignore */ }

        // 2. Switch Fluent.Ribbon theme (Gray uses Dark.Red to match red accent)
        try
        {
            var fluentTheme = themeName switch
            {
                "Gray"  => "Dark.Red",
                "Light" => "Light.Steel",
                _       => "Dark.Steel",
            };
            ThemeManager.Current.ChangeTheme(app, fluentTheme);
        }
        catch { /* Ignore */ }

        // 3. Switch AvalonDock theme
        try
        {
            dockManager.Theme = isDark
                ? new AvalonDock.Themes.Vs2013DarkTheme()
                : new AvalonDock.Themes.Vs2013LightTheme();
        }
        catch { /* Ignore */ }

        // 4. Swap our color dictionary LAST — overrides Fluent.Ribbon defaults
        //    (Colors.xaml includes Fluent.Ribbon.Brushes.* override keys)
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

        // 5. Re-apply accent colors with current hue/saturation
        var vm = DataContext as MainWindowViewModel;
        if (vm != null)
            UpdateThemeColors(vm.HueValue, vm.SaturationValue);

        // 6. Update title bar color to match new theme
        ApplyWindowChrome(isDark);
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
    //  TITLE BAR & WINDOW CHROME (Windows 11 DWM)
    // ═══════════════════════════════════════════════════════════

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_CAPTION_COLOR = 35;
    private const int DWMWA_TEXT_COLOR = 36;
    private const int DWMWCP_ROUND = 2;

    private void ApplyWindowChrome(bool isDark)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;

        // Rounded corners
        int cornerPref = DWMWCP_ROUND;
        DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));

        // Immersive dark mode (affects title bar button icons)
        int darkMode = isDark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

        // Caption (title bar) color — COLORREF = 0x00BBGGRR
        var bgBrush = Application.Current.TryFindResource("BackgroundBrush") as SolidColorBrush;
        if (bgBrush != null)
        {
            var c = bgBrush.Color;
            int colorRef = c.R | (c.G << 8) | (c.B << 16);
            DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, sizeof(int));
        }

        // Title text color
        var textBrush = Application.Current.TryFindResource("TextPrimary") as SolidColorBrush;
        if (textBrush != null)
        {
            var c = textBrush.Color;
            int colorRef = c.R | (c.G << 8) | (c.B << 16);
            DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref colorRef, sizeof(int));
        }
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
