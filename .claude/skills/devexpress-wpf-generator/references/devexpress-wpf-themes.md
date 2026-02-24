# DevExpress WPF Themes & Appearance

## Overview
- 30+ built-in application themes, auto-applied to DevExpress + standard WPF controls.
- Default theme: `Office2019Colorful`. Customizable via WPF Theme Designer (free tool).

## Key Classes
| Class | Description |
|-------|-------------|
| `ApplicationThemeHelper` | Global theme management |
| `ThemeManager` | Container-level theme |
| `Theme` | Theme name constants |
| `ThemedWindow` | Theme-aware Window base |

- **NuGet**: `DevExpress.Wpf.Themes.All` or individual theme packages

## Apply Theme Globally (recommended)
```csharp
// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    ApplicationThemeHelper.ApplicationThemeName = Theme.Win11LightName;
    base.OnStartup(e);
}
```

## Theme List
| Family | Themes |
|--------|--------|
| Windows 11 | Win11Light, Win11Dark, Win11System |
| Office 2019 | Office2019Colorful, Office2019Black, Office2019White, Office2019System |
| VS 2019 | VS2019Light, VS2019Dark, VS2019Blue, VS2019System |
| Windows 10 | Win10Light, Win10Dark |

## Touch Mode
```xml
<dx:ThemeManager.ThemeName>Office2019Black;Touch</dx:ThemeManager.ThemeName>
```

## Runtime Theme Switch
```csharp
ApplicationThemeHelper.ApplicationThemeName = Theme.VS2019DarkName;
ApplicationThemeHelper.SaveApplicationThemeName();
```

## Custom Palette
```csharp
var palette = new ThemePalette("CustomTheme");
palette.SetColor("Foreground", Colors.DarkBlue);
palette.SetColor("Window", Colors.WhiteSmoke);
var theme = Theme.CreateTheme(palette, Theme.Office2019Colorful);
Theme.RegisterTheme(theme);
ApplicationThemeHelper.ApplicationThemeName = theme.Name;
```

## Container-Level Theme
```xml
<Grid dx:ThemeManager.ThemeName="Office2019Black">
    <!-- Controls use this theme -->
</Grid>
```

## Reference
- https://docs.devexpress.com/WPF/7406/common-concepts/themes
