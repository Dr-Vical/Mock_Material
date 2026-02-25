---
name: ui-dev
description: UI/XAML/Design System development workflow — Material Design WPF, CommunityToolkit.Mvvm, AvalonDock, Fluent.Ribbon, design token system, i18n automation
---

# UI Development Skill — Material Design WPF

**All reports, questions, and approval requests must be in Korean.**

When the user inputs `/ui-dev`, collect requirements interactively and execute the following workflow.

---

## Specification References (Required Reading)

| Spec | Purpose | When to Read |
|------|---------|-------------|
| `.claude/specs/DESIGN-SYSTEM.md` | Color tokens (5-role), typography, spacing, style dictionary | Step 2 — **mandatory** |
| `.claude/specs/I18N.md` | Key naming convention (ko/en), current key list | Step 2 — **mandatory** |
| `.claude/specs/ARCHITECTURE.md` | AvalonDock panels, DI, module structure | When classifying as NEW_VIEW |
| `.claude/specs/PARAMETER-SYSTEM.md` | CSV schema, DataGrid rules, parameter model | When modifying parameter views |

### UI-Specific Rules (this skill only)
- **Style Dictionary mandatory**: all UI references from `Themes/` files (Colors, Fonts, Styles, Buttons)
- **Design token mandatory**: no hardcoded colors (`#RRGGBB`), FontSize, Padding/Margin, FontFamily
- **i18n mandatory**: XAML `{DynamicResource loc.*}`, ViewModel `ILocalizationService.Get()`
- **Material Design controls**: use MaterialDesignInXamlToolkit styles (MaterialDesignRaisedButton, MaterialDesignDataGrid, etc.)
- **AvalonDock layout**: use LayoutAnchorable/LayoutDocument for dockable panels
- **Fluent.Ribbon**: use for ribbon menu (RibbonTabItem, RibbonGroupBox)
- **Ribbon inline controls**: ALL controls inside RibbonGroupBox MUST use predefined styles:
  - Labels: `Style="{StaticResource RibbonInlineLabel}"` (NEVER raw Foreground/FontSize/FontFamily)
  - Buttons: `Style="{StaticResource RibbonLargeRipple}"` / `RibbonLargeRippleToggle`
  - Icons: `Style="{StaticResource RibbonIconOpacityPulse}"` / `RibbonIconShake`
  - Button labels: `Style="{StaticResource RibbonButtonLabel}"`
  - ComboBox: `Style="{StaticResource RibbonDarkComboBox}"`
  - Inline panel: `Margin="{StaticResource Padding.RibbonInline}"`
  - Sizes: use `{StaticResource Size.IconLG}` etc., not raw numbers
- **CommunityToolkit.Mvvm**: ObservableObject, [ObservableProperty], [RelayCommand], WeakReferenceMessenger
- **Window base**: `<Window>` with MaterialDesign theme (NOT dx:ThemedWindow)
- **Color constraint**: MAX 5 color roles — STRICTLY ENFORCED:
  - Primary (`PrimaryBrush`) — accents, active elements
  - Secondary (`SecondaryBrush`, `RibbonItemBrush`) — highlights, ribbon icons
  - Surface (`SurfaceBrush`) — panel/card backgrounds
  - Background (`BackgroundBrush`) — app background
  - Error (`ErrorBrush`, `RibbonItemErrorBrush`) — error/danger states
- **NEVER define per-item colors** — all items in same category share ONE color from 5 roles
- **WPF Binding Safety** (runtime error prevention):
  - `Path.Data` with nullable binding → MUST add `TargetNullValue='M0 0'` (GeometryConverter cannot convert null)
  - Even `Visibility=Collapsed` parents still evaluate child bindings — null safety required
  - `Image.Source` with nullable path → use `TargetNullValue={x:Null}`
- **Style Dictionary file structure**:
  - `Themes/DarkColors.xaml`, `GrayColors.xaml`, `LightColors.xaml` — Color/brush definitions + Fluent.Ribbon overrides
  - `Themes/Fonts.xaml` — FontFamily, FontSize tokens
  - `Themes/Styles.xaml` — Spacing, sizes, icon effects, labels
  - `Themes/Buttons.xaml` — Button, ToggleButton, ComboBox styles
- **Fluent.Ribbon 배경 오버라이드**: Colors.xaml에 `Fluent.Ribbon.Brushes.*` 키 등록 (코드 할당 금지)
  - SwitchTheme 순서: MaterialDesign → Fluent.Ribbon → AvalonDock → Colors.xaml (LAST wins)
- **ScottPlot 색상**: `GetThemeColor("ResourceKey")` 헬퍼 패턴 사용 (`FromHex` 금지)
- **DropShadowEffect**: `Color="{Binding Color, Source={StaticResource BrushName}}"` (hex 금지)
- **Chart 채널 토큰**: `ChartCH1Brush`~`ChartCH4Brush` — Colors.xaml에 등록

---

## UI Development Request Classification

| Category | Code | Target | Risk Level |
|----------|------|--------|------------|
| New View Addition | `NEW_VIEW` | UserControl or standalone window | Medium |
| Existing View Modification | `MODIFY_VIEW` | XAML layout, binding, style changes | Low |
| Custom Control | `CONTROL` | Add reusable control | Medium |
| Design System Extension | `DESIGN_SYSTEM` | Token/style/theme addition | High |
| Navigation/Docking | `NAVIGATION` | Tree→Center switching, AvalonDock tabs | Medium |
| Dialog/Sub-screen | `DIALOG` | Modal/modeless dialog window | Low |

### UI Roadmap Reference
- Read `.claude/specs/UI-ROADMAP.md` for full development phases
- **Center view types (LayoutDocument tabs):** ParameterEditor, Monitor, Oscilloscope, ControlPanel, Faults, ServiceInfo
- **Tool panels (LayoutAnchorable):** DriveTree (Left), ActionPanel (Right), ErrorLog (Bottom)
- **Tree selection drives center content** — via TreeNodeSelectedMessage
- **ActionPanel is context-aware** — buttons change based on active center view

---

## Execution Procedure

### Step 0: Project Assessment

#### 0-1. Current State Check
- Existing views, ViewModels, themes in project
- Current NuGet packages installed

#### 0-2. Spec Reading
- Read `.claude/specs/DESIGN-SYSTEM.md` — verify token/style inventory
- Read `.claude/specs/I18N.md` — verify current key list

#### 0-3. Existing View Pattern Search
Find similar existing implementations as reference patterns.

#### 0-4. Status Summary Report

### Step 1: Requirements Collection and Classification

Ask the user: **"어떤 UI 개발을 진행할까요? 자유롭게 설명해주세요."**

### Step 2: Analysis and Design

#### 2-1. Existing Code Analysis
**Always read related code first** — do not design without reading code.

#### 2-2. Library Capability Check

This project uses 4 main UI libraries. Check in this order:

```
1. MaterialDesignInXamlToolkit — buttons, cards, icons, inputs, dialogs
   → Read: .claude/skills/material-wpf-generator/references/material-design-controls.md
2. Fluent.Ribbon — ribbon menu, tabs, groups
   → Read: .claude/skills/material-wpf-generator/references/fluent-ribbon.md
3. AvalonDock — docking, floating, tool windows, documents
   → Read: .claude/skills/material-wpf-generator/references/avalondock.md
4. ScottPlot — charts, real-time monitoring, oscilloscope
   → Read: .claude/skills/material-wpf-generator/references/scottplot.md
```

#### 2-3. Impact Analysis

| Analysis Item | Content |
|---------------|---------|
| Files to modify | Existing files needing changes |
| New files to create | Files to create |
| i18n keys to add | Keys to add to ko.json + en.json |
| Design token check | Whether needed tokens exist |

#### 2-4. Design Checklist

##### NEW_VIEW
- [ ] Design token usage (5-color, typography, spacing, radius)
- [ ] i18n (all text via `{DynamicResource loc.*}`)
- [ ] MVVM (ObservableObject, [ObservableProperty], [RelayCommand])
- [ ] AvalonDock integration (panel type, position)
- [ ] MaterialDesign control selection

##### MODIFY_VIEW
- [ ] Existing feature preservation
- [ ] Hardcoded values → design tokens
- [ ] Hardcoded strings → i18n keys

### Step 3: Implementation

#### Implementation Rules
- Implement in **file group units** (2-5 related files at a time)
- **Build verification** after each group
- **Progress report** after each group

#### Implementation Order
1. **i18n key addition** → ko.json, en.json
2. **ViewModel** → ObservableObject, commands, properties
3. **View (XAML)** → MaterialDesign styled, design tokens, i18n
4. **Integration** → AvalonDock panel registration
5. **Finalization** → build verification

### Step 4: Verification

#### Design Token Verification (auto-executed when XAML modified)
```bash
# Hardcoded pattern search
grep -rE '(#[0-9A-Fa-f]{6}|FontSize="[0-9]|Padding="[0-9]|Margin="[0-9]|FontFamily="[^{]|Foreground="#|Background="#|Text="[^{]|Content="[^{])' *.xaml
```

**Replacement guide:**
- Hardcoded color → `{DynamicResource TokenName}`
- Hardcoded FontSize → `FontSize="{DynamicResource FontSizeMD}"`
- Hardcoded spacing → `{StaticResource Padding.*}`
- Hardcoded string → `{DynamicResource loc.*}`

### Step 5: Completion

#### 5-1. Change Summary
#### 5-2. Spec Update Check
#### 5-3. git-flow Suggestion

---

## Quick Reference

### XAML Binding Rules
```xml
<!-- Colors: DynamicResource (runtime theme switch) -->
<TextBlock Foreground="{DynamicResource TextPrimary}" />
<!-- MaterialDesign styles -->
<Button Style="{StaticResource MaterialDesignRaisedButton}" />
<DataGrid Style="{StaticResource MaterialDesignDataGrid}" />
<!-- Custom styles -->
<Button Style="{StaticResource ButtonStyle.Primary}" />
<Border Padding="{StaticResource Padding.Panel}" CornerRadius="{StaticResource Radius.MD}" />
<!-- i18n -->
<TextBlock Text="{DynamicResource loc.menu.file}" />
<!-- PackIcon -->
<materialDesign:PackIcon Kind="ContentSave" />
```

### ViewModel Pattern
```csharp
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private bool _isConnected;

    [RelayCommand]
    private void Save() { }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task LoadAsync() { }
    private bool CanSave() => IsConnected;
}
```

### View Template
```xml
<UserControl xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             Background="{DynamicResource SurfaceBrush}">
    <Grid>
        <TextBlock Text="{DynamicResource loc.module.title}"
                   Foreground="{DynamicResource TextPrimary}"
                   FontSize="{DynamicResource FontSizeXL}" />
        <Button Content="{DynamicResource loc.common.save}"
                Style="{StaticResource MaterialDesignRaisedButton}"
                Command="{Binding SaveCommand}" />
    </Grid>
</UserControl>
```

---

## Prohibited Patterns

```
NEVER: Foreground="#CCCCCC"     → USE: {DynamicResource TextPrimary}
NEVER: FontSize="12"           → USE: {DynamicResource FontSizeMD}
NEVER: Background="#1E1E1E"    → USE: {DynamicResource SurfaceBrush}
NEVER: Text="저장"              → USE: {DynamicResource loc.common.save}
NEVER: BindableBase            → USE: ObservableObject
NEVER: DelegateCommand         → USE: [RelayCommand]
NEVER: IEventAggregator        → USE: WeakReferenceMessenger
NEVER: dx:ThemedWindow         → USE: Window (MaterialDesign themed)
NEVER: dxmvvm:ViewModelSource  → USE: DI from App.xaml.cs
```
