# WPF AvalonDock UI Generator

Generate WPF UI screens using standard WPF controls + AvalonDock for docking/floating layout.
**DevExpress is NOT used.** All controls are native WPF or AvalonDock.

---

## Trigger

Use this skill when the user asks to:
- Create a WPF screen/window/view (non-DevExpress)
- Build a docking layout with AvalonDock
- Implement floating/dockable panels
- Generate XAML + ViewModel for standard WPF
- **Customize AvalonDock theme colors** (accent, header, overlay indicators)

**Keywords:** "WPF 화면 만들어줘", "아발론독", "AvalonDock", "기본 WPF", "독 레이아웃", "도킹 색상", "플로팅 색상"

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 8.0 Windows (WPF) |
| Docking | Dirkster.AvalonDock 4.72.1 |
| Theme | Dirkster.AvalonDock.Themes.VS2013 4.72.1 |
| MVVM | CommunityToolkit.Mvvm |
| Icons | Segoe Fluent Icons / Geometry Path Data |
| Theme | Custom Dark/Gray/Light Theme (ResourceDictionary) |

---

## XMLNS Declarations (Dirkster Fork — MUST USE)

**CRITICAL**: `http://schemas.xceed.com/wpf/xaml/avalondock` does NOT work with Dirkster's fork.

```xml
xmlns:avalonDock="clr-namespace:AvalonDock;assembly=AvalonDock"
xmlns:avalonDockLayout="clr-namespace:AvalonDock.Layout;assembly=AvalonDock"
xmlns:avalonDockTheme="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.VS2013"
xmlns:avalonDockControls="clr-namespace:AvalonDock.Controls;assembly=AvalonDock"
xmlns:avalonDockResKeys="clr-namespace:AvalonDock.Themes.VS2013.Themes;assembly=AvalonDock.Themes.VS2013"
```

---

## ★ Theme Color Customization (DictionaryTheme 방식)

### 원리

AvalonDock의 OverlayWindow(도킹 인디케이터)는 테마 리소스를 독립적으로 로드한다.
일반 Theme을 사용하면 매번 새 ResourceDictionary를 BAML에서 생성하여 외부 오버라이드가 무시됨.

**DictionaryTheme을 사용하면 같은 ResourceDictionary 인스턴스를 공유**하므로,
dictionary에 직접 오버라이드한 값이 OverlayWindow에도 전파된다.

### 구현 코드

```csharp
// Step 1: DictionaryTheme은 abstract → 서브클래스 필요
private sealed class RsDictionaryTheme : AvalonDock.Themes.DictionaryTheme
{
    public RsDictionaryTheme(ResourceDictionary rd) : base(rd) { }
}

// Step 2: 테마 ResourceDictionary 로드
private ResourceDictionary? _avalonThemeDict;

private void ApplyAvalonDockTheme(bool isDark = true)
{
    var themeUri = isDark
        ? new Uri("/AvalonDock.Themes.VS2013;component/DarkTheme.xaml", UriKind.Relative)
        : new Uri("/AvalonDock.Themes.VS2013;component/LightTheme.xaml", UriKind.Relative);
    _avalonThemeDict = new ResourceDictionary { Source = themeUri };

    // Step 3: Reflection으로 ResourceKeys 추출
    var rk = typeof(AvalonDock.Themes.VS2013.Themes.ResourceKeys);
    var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
    var keys = new Dictionary<string, object>();
    foreach (var prop in rk.GetProperties(bf))
        if (prop.GetValue(null) is { } key) keys[prop.Name] = key;
    foreach (var field in rk.GetFields(bf))
        if (field.GetValue(null) is { } key && !keys.ContainsKey(field.Name))
            keys[field.Name] = key;

    // Step 4: 색상 오버라이드 (themeDict에 직접!)
    var accent = new SolidColorBrush(Color.FromRgb(139, 26, 26)); accent.Freeze();
    var white = new SolidColorBrush(Colors.White); white.Freeze();

    void Set(string name, object value) {
        if (keys.TryGetValue(name, out var k)) _avalonThemeDict[k] = value;
    }

    // 마스터 악센트 (OverlayWindow DynamicResource가 이걸 참조)
    Set("ControlAccentColorKey", Color.FromRgb(139, 26, 26));
    Set("ControlAccentBrushKey", accent);

    // 도킹 헤더
    Set("ToolWindowCaptionActiveBackground", accent);
    Set("ToolWindowCaptionActiveText", white);

    // 플로팅 윈도우
    Set("FloatingWindowToolWindowBorder", accent);
    Set("FloatingWindowTitleBarBackground", accent);
    Set("FloatingWindowTitleBarText", white);

    // 도킹 인디케이터 (OverlayWindow)
    Set("DockingButtonForegroundBrushKey", accent);
    Set("PreviewBoxBorderBrushKey", accent);
    var previewBg = new SolidColorBrush(Color.FromArgb(128, 139, 26, 26)); previewBg.Freeze();
    Set("PreviewBoxBackgroundBrushKey", previewBg);

    // Step 5: DictionaryTheme으로 적용
    dockManager.Theme = new RsDictionaryTheme(_avalonThemeDict);
}
```

### 오버라이드 가능한 키 (ResourceKeys)

| Key | 대상 | 기본값 |
|-----|------|--------|
| `ControlAccentColorKey` | 마스터 악센트 Color | #1ba1e2 |
| `ControlAccentBrushKey` | 마스터 악센트 Brush | 파랑 |
| `ToolWindowCaptionActiveBackground` | 도킹 헤더 배경 | 파랑 |
| `ToolWindowCaptionActiveText` | 도킹 헤더 텍스트 | 흰색 |
| `ToolWindowCaptionActiveGrip` | 도킹 헤더 grip | 밝은 파랑 |
| `DocumentWellTabSelectedActiveBackground` | 선택 탭 배경 | 파랑 |
| `FloatingWindowToolWindowBorder` | 플로팅 테두리 | 파랑 |
| `FloatingWindowDocumentBorder` | 문서 플로팅 테두리 | 파랑 |
| `FloatingWindowTitleBarBackground` | 플로팅 타이틀 배경 | 파랑 |
| `FloatingWindowTitleBarText` | 플로팅 타이틀 텍스트 | 흰색 |
| `DockingButtonForegroundBrushKey` | 인디케이터 외곽선 | 파랑 |
| `DockingButtonBackgroundBrushKey` | 인디케이터 배경 | 반투명 |
| `DockingButtonForegroundArrowBrushKey` | 인디케이터 화살표 | 검정 |
| `DockingButtonStarBackgroundBrushKey` | 중앙 패널 배경 | 반투명 |
| `DockingButtonStarBorderBrushKey` | 중앙 패널 테두리 | 회색 |
| `PreviewBoxBorderBrushKey` | 드롭 프리뷰 테두리 | 파랑 |
| `PreviewBoxBackgroundBrushKey` | 드롭 프리뷰 배경 | 반투명 파랑 |

### 플로팅 윈도우 비주얼 트리 패치 (보조)

DictionaryTheme으로 대부분 해결되지만, 일부 하드코딩 색상은 비주얼 트리 패치 필요:

```csharp
// LayoutChanged/LayoutUpdated 이벤트에서 호출
// ⚠️ 재진입 가드 _isPatchingFloating 필수 (없으면 X 닫기 시 앱 멈춤)
// ⚠️ .ToList() 필수 (열거 중 컬렉션 변경 방지)
// ⚠️ OverlayWindow는 패치하지 말 것 (DictionaryTheme으로 해결됨)
```

### 실패한 방법들 (절대 사용하지 말 것)

| 방법 | 결과 |
|------|------|
| `dockManager.Resources[key]` | 도킹만 적용, 플로팅/인디케이터 무시 |
| `Application.Current.Resources[key]` | OverlayWindow 무시 |
| `BlueBrushs.xaml` 머지 | **도킹 기능 파괴** (인디케이터 사라짐) |
| OverlayWindow 비주얼 트리 패치 | 타이밍 안 맞음 (동적 생성/소멸) |

---

## XAML 테마 설정 (XAML에서는 하지 않음)

DictionaryTheme을 사용하려면 XAML에서 Theme을 설정하지 않고 코드-비하인드에서 설정:

```xml
<!-- ❌ XAML에서 Theme 직접 설정하지 않음 -->
<avalonDock:DockingManager x:Name="dockManager">
    <!-- Theme은 코드-비하인드에서 DictionaryTheme으로 설정 -->
    <avalonDock:DockingManager.Resources>
        <Style TargetType="{x:Type avalonDockControls:LayoutDocumentTabItem}">
            <Setter Property="Visibility" Value="Collapsed" />
        </Style>
    </avalonDock:DockingManager.Resources>
    <!-- ... -->
</avalonDock:DockingManager>
```

---

## DataContext 전파 주의

LayoutAnchorable 내부 UserControl은 DataContext가 자동 상속 안 될 수 있음:

```csharp
// MainWindow.Loaded에서 명시적 할당 필요
myPane.Content의 DataContext = this.DataContext;
```

---

## Layout Structure

```xml
<avalonDock:DockingManager x:Name="dockManager">
    <avalonDockLayout:LayoutRoot>
        <avalonDockLayout:LayoutPanel Orientation="Horizontal">
            <!-- Left: Tool pane -->
            <avalonDockLayout:LayoutAnchorablePane DockWidth="250">
                <avalonDockLayout:LayoutAnchorable Title="..." CanClose="False" CanFloat="True" />
            </avalonDockLayout:LayoutAnchorablePane>
            <!-- Center: Document pane -->
            <avalonDockLayout:LayoutDocumentPane>
                <avalonDockLayout:LayoutDocument Title="..." />
            </avalonDockLayout:LayoutDocumentPane>
        </avalonDockLayout:LayoutPanel>
    </avalonDockLayout:LayoutRoot>
</avalonDock:DockingManager>
```

---

## Important Rules

1. **DevExpress 사용 금지** — 모든 UI는 기본 WPF + AvalonDock으로 구현
2. **Dirkster 포크 사용** — Xceed 상용 버전이 아닌 `Dirkster.AvalonDock` 4.72.1
3. **CLR namespace 필수** — XML URI 스키마 사용 금지 (동작 안 함)
4. **DictionaryTheme 방식으로 테마 색상 변경** — XAML Theme 태그 사용 금지
5. **플로팅 윈도우 이벤트 핸들러에 재진입 가드 필수** — 없으면 앱 멈춤
6. **스타일 딕셔너리 토큰 사용** — 하드코딩 색상 금지
