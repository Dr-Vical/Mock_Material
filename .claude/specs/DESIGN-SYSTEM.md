# Design System Specification — RswareDesign

> Last updated: 2026-02-24 | Migrated from Platform DevExpress theme bridge

## Summary

RswareDesign uses a **5-color constrained** design token system with MaterialDesignInXamlToolkit as the base UI library. All visual properties (colors, typography, spacing, border radius) are managed through ResourceDictionary style dictionaries that enable global Dark/Light theme switching. No hardcoded visual values in XAML.

---

## 1. Color System — 5-Role Constraint

### 1-1. Color Roles (MAX 5)

| Role | Purpose | Light Default | Dark Default |
|------|---------|--------------|-------------|
| **Primary** | Accent, active tabs, primary buttons, ribbon highlight | `#1976D2` (Blue 700) | `#90CAF9` (Blue 200) |
| **Secondary** | Selected items, secondary buttons, links | `#FF6F00` (Amber 900) | `#FFB74D` (Orange 300) |
| **Surface** | Cards, panels, tool windows, input fields | `#FFFFFF` | `#2D2D30` |
| **Background** | App background, sidebar, base layer | `#FAFAFA` | `#1E1E1E` |
| **Error** | Error states, danger buttons, fault indicators | `#D32F2F` (Red 700) | `#EF5350` (Red 400) |

### 1-2. Derived Semantic Colors (auto-calculated from 5 roles)

| Token | Light | Dark | Derived From |
|-------|-------|------|-------------|
| `PrimaryBrush` | #1976D2 | #90CAF9 | Primary |
| `PrimaryHoverBrush` | #1565C0 | #BBDEFB | Primary (darken/lighten 10%) |
| `PrimaryForegroundBrush` | #FFFFFF | #000000 | Contrast on Primary |
| `SecondaryBrush` | #FF6F00 | #FFB74D | Secondary |
| `SecondaryHoverBrush` | #E65100 | #FFE0B2 | Secondary (darken/lighten 10%) |
| `SurfaceBrush` | #FFFFFF | #2D2D30 | Surface |
| `SurfaceVariantBrush` | #F5F5F5 | #3C3C3C | Surface (slight shift) |
| `BackgroundBrush` | #FAFAFA | #1E1E1E | Background |
| `ErrorBrush` | #D32F2F | #EF5350 | Error |
| `WarningBrush` | #F9A825 | #FFD54F | Secondary-adjacent (fixed) |
| `SuccessBrush` | #2E7D32 | #66BB6A | Fixed green |
| `TextPrimary` | #212121 | #E0E0E0 | High contrast on Background |
| `TextSecondary` | #757575 | #9E9E9E | Medium contrast |
| `TextDisabled` | #BDBDBD | #616161 | Low contrast |
| `TextOnPrimary` | #FFFFFF | #000000 | Contrast on PrimaryBrush |
| `BorderDefault` | #E0E0E0 | #424242 | Surface-adjacent |
| `BorderFocused` | #1976D2 | #90CAF9 | Primary |
| `DividerBrush` | #E0E0E0 | #424242 | Surface-adjacent |
| `RibbonBackground` | #F5F5F5 | #252526 | Background variant |
| `TreeBackground` | #FFFFFF | #252526 | Surface |
| `GridHeaderBrush` | #E3F2FD | #37474F | Primary (very light/dark) |
| `GridAlternatingRowBrush` | #FAFAFA | #2A2A2E | Background variant |
| `StatusBarBrush` | #1976D2 | #007ACC | Primary |
| `StatusBarForeground` | #FFFFFF | #FFFFFF | Fixed white |

### 1-3. ThemeColorSet Class

```csharp
public record ThemeColorSet
{
    public Color Primary { get; init; }
    public Color Secondary { get; init; }
    public Color Surface { get; init; }
    public Color Background { get; init; }
    public Color Error { get; init; }
    public bool IsDark { get; init; }
}
```

Theme switching replaces the entire `ThemeColorSet` and recalculates all derived tokens.

---

## 2. Border Radius Tokens

| Token | Value | Usage |
|-------|-------|-------|
| `Radius.None` | 0 | Tables, tree items |
| `Radius.SM` | 2 | Input fields, small buttons |
| `Radius.MD` | 4 | Cards, panels, standard buttons |
| `Radius.LG` | 6 | Dialogs, floating panels |
| `Radius.XL` | 8 | Tooltips, notifications |

**Rule:** Maximum 5 radius values. No other radius values allowed.

---

## 3. Typography Tokens

### 3-1. Font Families

| Token | Value | Fallback |
|-------|-------|----------|
| `FontFamilyUI` | Segoe UI | Malgun Gothic |
| `FontFamilyCode` | Cascadia Code | Consolas |
| `FontFamilyMono` | Consolas | Courier New |

### 3-2. Font Sizes

| Token | Size (px) | Usage |
|-------|-----------|-------|
| `FontSizeXS` | 10 | Badges, captions |
| `FontSizeSM` | 11 | Status bar, small labels |
| `FontSizeMD` | 12 | Body text, DataGrid cells, menu items |
| `FontSizeLG` | 13 | Code, emphasized text |
| `FontSizeXL` | 14 | Headings, panel titles |
| `FontSizeXXL` | 16 | Section headings |
| `FontSize3XL` | 20 | Page titles |

### 3-3. Font Weights

| Token | Value | Usage |
|-------|-------|-------|
| `FontWeightRegular` | 400 | Body text |
| `FontWeightMedium` | 500 | Emphasized text |
| `FontWeightSemiBold` | 600 | Headings, active tabs |
| `FontWeightBold` | 700 | Titles |

---

## 4. Spacing Tokens

### 4-1. Base Unit: 4px

| Token | px | Usage |
|-------|-----|-------|
| `Space.0` | 0 | No spacing |
| `Space.1` | 4 | Inline gaps, icon-text spacing |
| `Space.2` | 8 | List item gaps, ToolWindow padding |
| `Space.3` | 12 | Panel padding, form field gaps |
| `Space.4` | 16 | Section padding |
| `Space.5` | 20 | Large section gaps |
| `Space.6` | 24 | Dialog padding |

### 4-2. Semantic Spacing (StaticResource)

| Key | Value | Usage |
|-----|-------|-------|
| `Padding.Panel` | 12 | Panel content padding |
| `Padding.ToolWindow` | 8 | AvalonDock tool window padding |
| `Padding.Dialog` | 24 | Dialog content padding |
| `Padding.Input` | 8,4 | Input field padding |
| `Padding.Button` | 12,6 | Standard button padding |
| `Padding.Button.Small` | 8,4 | Small/action panel button padding |
| `Padding.GridCell` | 6,3 | DataGrid cell padding |
| `Margin.ListItem` | 0,0,0,4 | Vertical list items |
| `Margin.FormField` | 0,0,0,12 | Form field spacing |
| `Margin.Section` | 0,0,0,24 | Between sections |
| `Margin.Inline` | 0,0,8,0 | Inline element spacing |

### 4-3. Fixed Sizes

| Key | Value | Usage |
|-----|-------|-------|
| `Size.StatusBarHeight` | 24 | Status bar |
| `Size.RibbonTabHeight` | 34 | Ribbon tab header |
| `Size.TreeItemHeight` | 28 | Tree view items |
| `Size.GridRowHeight` | 28 | DataGrid rows |
| `Size.GridHeaderHeight` | 32 | DataGrid header |
| `Size.ActionButtonHeight` | 32 | Right panel buttons |
| `Size.ActionButtonWidth` | 140 | Right panel buttons |
| `Size.SidebarWidth` | 250 | Default sidebar (tree) width |
| `Size.SidebarMinWidth` | 180 | Min sidebar width |
| `Size.SidebarMaxWidth` | 400 | Max sidebar width |
| `Size.ActionPanelWidth` | 160 | Right action panel width |
| `Size.ErrorPanelHeight` | 150 | Bottom error panel height |
| `Size.IconSM` | 14 | Small icons |
| `Size.IconMD` | 16 | Standard icons |
| `Size.IconLG` | 20 | Large icons |
| `Size.IconXL` | 24 | Extra large icons |

---

## 5. Style Dictionary Structure

### 5-1. ResourceDictionary Files

```
RswareDesign.UI.Themes/
├── Themes/
│   ├── DarkTheme.xaml          # Dark color token values
│   ├── LightTheme.xaml         # Light color token values
│   └── SharedTokens.xaml       # Theme-independent tokens (spacing, radius, sizes)
├── Styles/
│   ├── ButtonStyles.xaml       # All button variants
│   ├── DataGridStyles.xaml     # DataGrid, headers, cells, alternating rows
│   ├── TreeViewStyles.xaml     # TreeView, TreeViewItem, icons
│   ├── RibbonStyles.xaml       # Fluent.Ribbon overrides for Material look
│   ├── TextStyles.xaml         # TextBlock style presets
│   ├── InputStyles.xaml        # TextBox, ComboBox, CheckBox
│   ├── PanelStyles.xaml        # Borders, GroupBox, Expander
│   ├── DockingStyles.xaml      # AvalonDock theme overrides
│   └── StatusBarStyles.xaml    # StatusBar items
└── ThemeManager.cs             # Runtime theme switching logic
```

### 5-2. Theme Switching Mechanism

```csharp
public class ThemeManager : ObservableObject
{
    // 1. Remove current theme dictionary from MergedDictionaries
    // 2. Add new theme dictionary (DarkTheme.xaml or LightTheme.xaml)
    // 3. Update MaterialDesign BundledTheme (BaseTheme.Dark/Light)
    // 4. Update Fluent.Ribbon theme (Dark/Light)
    // 5. Send ThemeChangedMessage via WeakReferenceMessenger

    public void SetTheme(bool isDark);
    public void SetCustomColors(ThemeColorSet colorSet);
    public bool IsDark { get; }
}
```

### 5-3. Button Style Variants

| Key | Background | Foreground | Usage |
|-----|-----------|-----------|-------|
| `ButtonStyle.Primary` | PrimaryBrush | TextOnPrimary | Main actions (Save, Connect) |
| `ButtonStyle.Secondary` | Transparent + Border | TextPrimary | Secondary actions |
| `ButtonStyle.Ghost` | Transparent | TextPrimary | Toolbar, minimal UI |
| `ButtonStyle.Danger` | ErrorBrush | White | Delete, disconnect |
| `ButtonStyle.Action` | SurfaceBrush + Border | TextPrimary | Right panel buttons (Save Parameters, Revert) |
| `ButtonStyle.Icon` | Transparent | TextSecondary | Icon-only buttons |
| `ButtonStyle.Toggle` | Transparent/PrimaryBrush | TextPrimary/White | Views tab toggle buttons |

---

## 6. DataGrid Styling

### Parameter DataGrid Columns

| Column | Width | Binding | Editable |
|--------|-------|---------|----------|
| FT NUM | 80 | `FtNumber` | Never |
| PARAMETER | 200* | `Name` | Never |
| VALUE | 120 | `Value` | If AccessMode == ReadWrite |
| UNITS | 80 | `Unit` | Never |
| DEFAULT | 100 | `Default` | Never |
| MIN | 100 | `Min` | Never |
| MAX | 100 | `Max` | Never |

*200 with star sizing for remaining space.

### DataGrid Visual Rules
- Header: `GridHeaderBrush` background, `FontWeightSemiBold`
- Selected row: `PrimaryBrush` background with `TextOnPrimary`
- Alternating rows: `GridAlternatingRowBrush`
- ReadWrite cells: normal text with edit cursor on click
- ReadOnly cells: `TextSecondary` foreground
- Modified (unsaved) cells: `SecondaryBrush` foreground (orange)
- Out-of-range values: `ErrorBrush` foreground

---

## 7. Binding Type Reference

| Category | Binding | Reason |
|----------|---------|--------|
| Colors | `{DynamicResource TokenName}` | Runtime theme switching |
| Font family | `{DynamicResource FontFamilyUI}` | Runtime font switching |
| Font sizes | `{DynamicResource FontSizeMD}` | Runtime scaling |
| Spacing | `{StaticResource Padding.*}` | Compile-time (fixed values) |
| Margin | `{StaticResource Margin.*}` | Compile-time |
| CornerRadius | `{StaticResource Radius.*}` | Compile-time |
| Sizes | `{StaticResource Size.*}` | Compile-time |
| i18n strings | `{DynamicResource loc.*}` | Runtime language switch |
| Text styles | `{StaticResource TextStyle.*}` | Compile-time |
| Button styles | `{StaticResource ButtonStyle.*}` | Compile-time |

---

## 8. Prohibited Patterns

```
NEVER: Foreground="#CCCCCC"           → USE: Foreground="{DynamicResource TextPrimary}"
NEVER: FontSize="12"                 → USE: FontSize="{DynamicResource FontSizeMD}"
NEVER: FontFamily="Segoe UI"         → USE: FontFamily="{DynamicResource FontFamilyUI}"
NEVER: Background="#1E1E1E"          → USE: Background="{DynamicResource SurfaceBrush}"
NEVER: Padding="8"                   → USE: Padding="{StaticResource Padding.ToolWindow}"
NEVER: Margin="0,0,0,12"            → USE: Margin="{StaticResource Margin.FormField}"
NEVER: CornerRadius="4"             → USE: CornerRadius="{StaticResource Radius.MD}"
NEVER: Text="저장"                   → USE: Text="{DynamicResource loc.common.save}"
NEVER: Width="250"                   → USE: Width="{StaticResource Size.SidebarWidth}"
NEVER: new SolidColorBrush(#...)    → USE: FindResource("PrimaryBrush")
```

---

## Change History

| Date | Change |
|------|--------|
| 2026-02-24 | Initial spec — Material Design based, 5-color constraint, migrated from DevExpress theme bridge |
