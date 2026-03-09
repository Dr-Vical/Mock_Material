# Oscilloscope Panel 설계 사양서

> **목적**: MonitorControlDialog(오실로스코프 패널)에 구현된 모든 기능의 상세 레퍼런스 문서.
> 다른 프로젝트에서 유사한 패널을 구축할 때 참고 자료로 사용.

---

## 1. 개요

| 항목 | 내용 |
|------|------|
| **UserControl** | `MonitorControlDialog` |
| **도킹 방식** | AvalonDock `LayoutAnchorable`에 도킹 (플로팅 가능) |
| **레이아웃** | 좌측: 8CH 차트 + 컨트롤 패널, 우측: Favorites 리스트 |
| **크기 조정** | `GridSplitter`로 좌우 비율 드래그 조정 |
| **차트 라이브러리** | ScottPlot 5 WPF (`WpfPlot`) |
| **MVVM** | CommunityToolkit.Mvvm — ViewModel 바인딩 + WeakReferenceMessenger |
| **테마** | 스타일 딕셔너리 연동 (Dark/Gray/Light 3-theme 지원) |

### 1.1 진입점

- DriveTreeView의 **Oscilloscope 버튼** 또는 **ControlPanel 버튼** 클릭
- `WeakReferenceMessenger`를 통해 `ShowMonitorControlMessage` 발행
- MainWindow에서 수신 후 AvalonDock `LayoutAnchorable`을 Show/Float

### 1.2 DataContext 전파

- AvalonDock `LayoutAnchorable` 내부 UserControl은 DataContext가 자동 상속되지 않음
- **해결**: `MainWindow.Loaded` 이벤트에서 `monitorControlPane.Content`에 명시적 DataContext 할당

---

## 2. 레이아웃 구조

### 2.1 전체 레이아웃 (3-Column Grid)

```
┌─────────────────────────┬───┬──────────────┐
│       Chart Area        │ S │  Favorites   │
│   (Column 0, 1*)        │ p │   (Column 2) │
│                         │ l │              │
│  ┌───────────────────┐  │ i │  ┌──────────┐│
│  │   Toolbar         │  │ t │  │  Header  ││
│  ├───────────────────┤  │ t │  ├──────────┤│
│  │                   │  │ e │  │          ││
│  │   WpfPlot Chart   │  │ r │  │  ListBox ││
│  │                   │  │   │  │          ││
│  ├───────────────────┤  │   │  │          ││
│  │   Scale Panel     │  │   │  │          ││
│  ├───────────────────┤  │   │  └──────────┘│
│  │   Control Panel   │  │   │              │
│  └───────────────────┘  │   │              │
└─────────────────────────┴───┴──────────────┘
```

| Column | 내용 | 기본 너비 | MinWidth |
|--------|------|-----------|----------|
| 0 | Chart Area (차트 + 스케일 + 컨트롤) | `*` (남는 공간) | 200px |
| 1 | GridSplitter | 3px (고정) | — |
| 2 | Favorites 패널 | 220px | 150px |

### 2.2 Chart Area 내부 (3-Row Grid)

| Row | 내용 | Height |
|-----|------|--------|
| 0 | Toolbar (버튼 바) | Auto |
| 1 | WpfPlot 차트 | `*` |
| 2 | Scale 패널 + StatusBar | Auto |

### 2.3 Scale 패널 (차트 하단)

- 그룹별 섹션으로 구성 (동적 생성)
- 각 섹션: 그룹 레이블 + Max TextBox + Min TextBox + AutoScale 버튼
- `RebuildScalePanelUI()`로 채널 변경 시 동적으로 재구성

---

## 3. 차트 (ScottPlot 5)

### 3.1 기본 설정

| 항목 | 값 |
|------|-----|
| 컨트롤 | `ScottPlot.WPF.WpfPlot` |
| 채널 수 | 최대 8채널 (CH1~CH8) |
| 데이터 타입 | `Signal` plot (등간격 시계열) |
| 버퍼 크기 | 500 포인트 |
| 갱신 주기 | `DispatcherTimer` 50ms (20 FPS) |
| 채널 색상 | `ChartCH1Brush` ~ `ChartCH8Brush` (스타일 딕셔너리 토큰) |

```csharp
// 차트 초기화 예시
var plot = wpfPlot.Plot;
plot.Add.Signal(channelData[i], sampleRate);
```

### 3.2 좌측 스케일 (Y축) 표기

동적 그룹핑 로직으로 Y축을 자동 관리한다.

**규칙:**
1. 같은 카테고리(Position, Velocity, Current 등)의 채널은 **하나의 Y축 공유**
2. 다른 카테고리면 **별도 Y축 자동 생성** (`plot.Axes.AddLeftAxis()`)
3. Y축 색상 = 해당 그룹의 **첫 번째 채널 색상**
4. 폰트 사이즈 = **10 통일**

```csharp
// 카테고리 추출
string category = GetChannelCategory(channelName);
// 예: "Target Position" → "Position"
//     "Actual Velocity" → "Velocity"
//     "Phase Current U" → "Current"

// 그룹별 Y축 생성
if (groupIndex == 0)
{
    axis = plot.Axes.Left;  // 기존 좌측 축 사용
}
else
{
    axis = plot.Axes.AddLeftAxis();  // 추가 축 생성
}
```

**카테고리 분류 로직 (`GetChannelCategory`)**:
- 채널명에서 접두어(Target/Actual/Phase 등) 제거 후 핵심 단어 추출
- Position, Velocity, Current, Torque, Voltage 등의 그룹으로 분류
- 분류 불가 시 채널명 자체를 카테고리로 사용

### 3.3 스케일 최대/최소 화면 표기

- 차트 하단 **Scale 패널**에 그룹별 Max/Min TextBox 배치
- 입력 후 **Enter 키** 또는 **포커스 아웃(LostFocus)** 시 축 범위 적용
- 그룹별 **Auto Scale 버튼** (MaterialDesign `ArrowExpandVertical` 아이콘)

```
┌─────────────────────────────────────────────┐
│ [Position ●]  Max: [  1000.0 ] [↕]         │
│               Min: [ -1000.0 ]              │
│ [Velocity ●]  Max: [  5000.0 ] [↕]         │
│               Min: [ -5000.0 ]              │
└─────────────────────────────────────────────┘
```

**TextBox 이벤트 처리:**
```csharp
// KeyDown — Enter 키 입력 시
if (e.Key == Key.Enter)
{
    ApplyScaleRange(groupIndex, maxValue, minValue);
    Keyboard.ClearFocus();  // 포커스 해제
}

// LostFocus — 포커스 이탈 시
ApplyScaleRange(groupIndex, maxValue, minValue);
```

### 3.4 오토 스케일

두 가지 모드:

| 모드 | 트리거 | 동작 |
|------|--------|------|
| **그룹별** | Scale 패널의 `[↕]` 버튼 | 해당 그룹 채널 데이터의 min/max ± 10% 마진 |
| **전체** | Toolbar의 `Auto` 버튼 | 모든 그룹에 대해 `AutoScaleGroup()` 호출 |

```csharp
private void AutoScaleGroup(int groupIndex)
{
    var channels = _groupChannels[groupIndex];
    double dataMin = double.MaxValue;
    double dataMax = double.MinValue;

    foreach (var ch in channels)
    {
        // 해당 채널 데이터에서 min/max 계산
        dataMin = Math.Min(dataMin, channelData[ch].Min());
        dataMax = Math.Max(dataMax, channelData[ch].Max());
    }

    double margin = (dataMax - dataMin) * 0.1;  // 10% 마진
    if (margin == 0) margin = 1.0;  // 데이터가 일정할 때 기본 마진

    axis.Min = dataMin - margin;
    axis.Max = dataMax + margin;
}
```

### 3.5 채널 색상 변경

채널 옆 **Ellipse** 클릭 시 **Popup 팔레트** (16색) 표시.

**Popup 동작:**
- `StaysOpen="True"` 설정 (자동 닫힘 방지)
- 외부 클릭 시 닫힘: `OnPreviewMouseDown`에서 Popup 영역 밖 클릭 감지
- 색상 선택 시 Popup 닫힘

**색상 선택 시 동기화 대상 (5곳):**

| 대상 | 속성 |
|------|------|
| Ellipse (색상 인디케이터) | `Fill` |
| ComboBox (채널 선택) | `Foreground` |
| Signal (차트 데이터) | `Color` |
| Scale 패널 섹션 | 그룹 레이블 `Foreground` |
| Y축 | `TickLabelStyle.ForeColor`, `Label.ForeColor` |

```csharp
private void ApplyChannelColor(int channelIndex, Color newColor)
{
    // 1. UI 요소 업데이트
    ellipses[channelIndex].Fill = new SolidColorBrush(newColor);
    comboBoxes[channelIndex].Foreground = new SolidColorBrush(newColor);

    // 2. ScottPlot Signal 색상
    signals[channelIndex].Color = newColor.ToScottPlotColor();

    // 3. Scale 패널 + Y축 (해당 그룹의 첫 번째 채널인 경우)
    UpdateGroupColors(channelIndex);

    // 4. 차트 리프레시
    wpfPlot.Refresh();
}
```

**16색 팔레트 구성:**
```
┌──┬──┬──┬──┐
│🔴│🟠│🟡│🟢│
├──┼──┼──┼──┤
│🔵│🟣│⚪│⚫│
├──┼──┼──┼──┤
│  │  │  │  │  (추가 8색 — 밝은/어두운 변형)
├──┼──┼──┼──┤
│  │  │  │  │
└──┴──┴──┴──┘
```

### 3.6 채널 선택 ComboBox

| 항목 | 설명 |
|------|------|
| 데이터 소스 | `ChannelOptions` 배열 — 모니터링 가능한 전체 파라미터 리스트 |
| Foreground | 해당 채널 색상과 동기화 |
| 선택 변경 이벤트 | `_channelNames[i]` 업데이트 → `RebuildScaleGroups()` 호출 |

```csharp
private void ChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    var comboBox = (ComboBox)sender;
    int channelIndex = GetChannelIndex(comboBox);
    string newChannelName = (string)comboBox.SelectedItem;

    _channelNames[channelIndex] = newChannelName;
    RebuildScaleGroups();  // Y축 그룹 재구성
}
```

### 3.7 동일 항목 스케일 합치기

채널명 기반으로 카테고리를 추출하고, 같은 카테고리끼리 그룹으로 묶는다.

**처리 흐름:**

```
채널 변경/초기화
    ↓
GetChannelCategory() — 각 채널의 카테고리 추출
    ↓
그룹핑 — 같은 카테고리 채널을 _groupChannels 리스트로 묶음
    ↓
RebuildScaleGroups() — Y축 재구성
    ↓
RebuildScalePanelUI() — Scale 패널 UI 동적 재생성
```

**핵심 데이터 구조:**
```csharp
// 그룹 채널 매핑
private List<List<int>> _groupChannels;
// 예: [[0, 2], [1], [3]]
//   그룹0: CH1(Position) + CH3(Position) → Y축 공유
//   그룹1: CH2(Velocity) → 독립 Y축
//   그룹2: CH4(Current) → 독립 Y축

// 그룹별 Y축 참조
private List<IAxis> _groupAxes;
```

**Y축 재구성 로직:**
```csharp
private void RebuildScaleGroups()
{
    // 1. 기존 추가 축 제거 (Axes.Left는 유지)
    foreach (var axis in _groupAxes.Skip(1))
        plot.Axes.Remove(axis);

    // 2. 채널 카테고리별 그룹핑
    var groups = new Dictionary<string, List<int>>();
    for (int i = 0; i < _channelNames.Length; i++)
    {
        string category = GetChannelCategory(_channelNames[i]);
        if (!groups.ContainsKey(category))
            groups[category] = new List<int>();
        groups[category].Add(i);
    }

    // 3. 그룹별 Y축 할당
    _groupChannels = groups.Values.ToList();
    _groupAxes = new List<IAxis>();

    for (int g = 0; g < _groupChannels.Count; g++)
    {
        IAxis axis = (g == 0)
            ? plot.Axes.Left
            : plot.Axes.AddLeftAxis();

        _groupAxes.Add(axis);

        // 축 색상 = 그룹 첫 번째 채널 색상
        int firstCh = _groupChannels[g][0];
        axis.TickLabelStyle.ForeColor = _channelColors[firstCh];

        // Signal에 축 할당
        foreach (int ch in _groupChannels[g])
            signals[ch].Axes.YAxis = axis;
    }

    // 4. Scale 패널 UI 재구성
    RebuildScalePanelUI();
}
```

---

## 4. 툴바 버튼

차트 상단 Toolbar에 배치된 버튼 목록.

| 버튼 | 아이콘 | 기능 | 핸들러 | 상세 동작 |
|------|--------|------|--------|-----------|
| **Single** | `Camera` | 단발 캡쳐 | `BtnSingle_Click` | 버퍼 1회 채운 후 자동 정지 |
| **Contin** | `Play` | 연속 캡쳐 | `BtnContinuous_Click` | 타이머 시작, Stop까지 계속 갱신 |
| **Stop** | `Stop` | 정지 | `BtnStop_Click` | 타이머 정지, 현재 데이터 유지 |
| **Auto** | `ArrowExpandAll` | 전체 오토스케일 | `BtnAutoScale_Click` | 모든 그룹 `AutoScaleGroup()` 호출 |
| **Setting** | `Cog` | 설정 다이얼로그 | `BtnOption_Click` | 샘플링 레이트, 트리거 조건 등 설정 |
| **Save** | `ContentSave` | 설정 저장 (INI) | `BtnSaveSetting_Click` | `ScopeSettingService.Save()` 호출 |
| **Load** | `FolderOpen` | 설정 불러오기 (INI) | `BtnLoadSetting_Click` | `ScopeSettingService.Load()` 호출 |

**버튼 상태 관리:**
- Single/Contin 실행 중: Stop만 활성화
- 정지 상태: Single/Contin 활성화, Stop 비활성화
- ToggleButton 스타일은 사용하지 않음 (일반 Button + 상태 플래그)

---

## 5. 설정 저장/불러오기

### 5.1 ScopeSettingService

INI 형식으로 채널 설정을 파일에 저장/불러오기.

**저장 경로:** `{exe폴더}/ScopeSetting/`

**INI 파일 구조:**
```ini
[General]
ChannelCount=4
SampleRate=1000
BufferSize=500

[Channel0]
Name=Target Position
Color=#FF4CAF50
Enabled=True
ScaleMax=1000.0
ScaleMin=-1000.0

[Channel1]
Name=Actual Velocity
Color=#FF2196F3
Enabled=True
ScaleMax=5000.0
ScaleMin=-5000.0

[Channel2]
Name=Phase Current U
Color=#FFFF9800
Enabled=False
ScaleMax=10.0
ScaleMin=-10.0

[Channel3]
Name=Bus Voltage
Color=#FFF44336
Enabled=True
ScaleMax=400.0
ScaleMin=0.0
```

**저장 항목:**

| 항목 | 설명 |
|------|------|
| `Name` | 채널에 할당된 파라미터 이름 |
| `Color` | 채널 색상 (ARGB hex) |
| `Enabled` | 채널 활성화 여부 |
| `ScaleMax` | Y축 최대값 |
| `ScaleMin` | Y축 최소값 |

### 5.2 파일 다이얼로그

```csharp
// 저장
var dialog = new SaveFileDialog
{
    InitialDirectory = scopeSettingFolder,
    Filter = "Scope Setting (*.ini)|*.ini",
    DefaultExt = ".ini"
};

// 불러오기
var dialog = new OpenFileDialog
{
    InitialDirectory = scopeSettingFolder,
    Filter = "Scope Setting (*.ini)|*.ini"
};
```

---

## 6. 좌우 패널 숨기기/표시

### 6.1 Chart 영역 (Oscilloscope)

| 항목 | 내용 |
|------|------|
| 토글 메시지 | `ToggleMonitorSectionMessage("Oscilloscope")` |
| 대상 요소 | `ChartToolbar`, `ChartArea`, `ChartStatusBar` |
| 숨기기 동작 | Visibility = `Collapsed`, 타이머 정지 |
| 표시 동작 | Visibility = `Visible`, 이전 상태에 따라 타이머 재시작 |

```csharp
private void OnToggleMonitorSection(ToggleMonitorSectionMessage msg)
{
    if (msg.Section == "Oscilloscope")
    {
        bool isVisible = ChartToolbar.Visibility == Visibility.Visible;
        var vis = isVisible ? Visibility.Collapsed : Visibility.Visible;

        ChartToolbar.Visibility = vis;
        ChartArea.Visibility = vis;
        ChartStatusBar.Visibility = vis;

        if (isVisible)
            _timer?.Stop();  // 숨기면 타이머 정지 (리소스 절약)
    }
}
```

### 6.2 Control Panel 영역

| 항목 | 내용 |
|------|------|
| 토글 메시지 | `ToggleMonitorSectionMessage("ControlPanel")` |
| 대상 요소 | `ControlPanelGrid` |
| 숨기기 동작 | Visibility = `Collapsed` |
| 표시 동작 | Visibility = `Visible` |

### 6.3 Favorites 패널

GridSplitter 기반 폭 조정으로 숨기기/표시 구현.

```csharp
// 숨기기
FavoritesColumn.Width = new GridLength(0);
GridSplitterElement.Visibility = Visibility.Hidden;

// 표시
FavoritesColumn.Width = new GridLength(220);
GridSplitterElement.Visibility = Visibility.Visible;
```

---

## 7. GridSplitter 패널 크기 조정

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" MinWidth="200" />      <!-- Chart -->
        <ColumnDefinition Width="3" />                       <!-- Splitter -->
        <ColumnDefinition Width="220" MinWidth="150" />      <!-- Favorites -->
    </Grid.ColumnDefinitions>

    <!-- Chart Area -->
    <Grid Grid.Column="0" ... />

    <!-- GridSplitter -->
    <GridSplitter Grid.Column="1"
                  Width="3"
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"
                  Background="{DynamicResource SurfaceVariantBrush}" />

    <!-- Favorites -->
    <Grid Grid.Column="2" ... />
</Grid>
```

| 속성 | 값 | 설명 |
|------|-----|------|
| Width | 3px | Splitter 너비 |
| HorizontalAlignment | Center | 중앙 정렬 |
| VerticalAlignment | Stretch | 전체 높이 |
| Background | `SurfaceVariantBrush` | 테마 연동 색상 |
| Chart MinWidth | 200px | 차트 영역 최소 너비 |
| Favorites MinWidth | 150px | Favorites 영역 최소 너비 |

---

## 8. Favorites 패널

### 8.1 목록 표시

ViewModel의 `FavoriteParameters` ObservableCollection에 바인딩.

**ListBox ItemTemplate 구조:**
```
┌─────────────────────────────┐
│ ★  FT-001  Target Position  │  1234.56
│ ★  FT-015  Actual Velocity  │  5678.90
│ ★  FT-023  Phase Current U  │     3.14
│ ★  FT-042  Bus Voltage      │   310.00
└─────────────────────────────┘
```

| 요소 | 바인딩 | 설명 |
|------|--------|------|
| Star (★) | `IsFavorite` | 항상 True (Favorites 목록이므로) |
| FT번호 | `ParameterId` | 파라미터 고유 번호 |
| 이름 | `Name` | 파라미터 이름 (loc 키 기반) |
| 값 | `Value` | 현재 값 (실시간 갱신) |

**카운트 텍스트 업데이트:**
```csharp
_viewModel.FavoriteParameters.CollectionChanged += (s, e) =>
{
    FavoritesCountText.Text = $"Favorites ({_viewModel.FavoriteParameters.Count})";
};
```

### 8.2 드래그 순서 변경

> **핵심**: WPF 표준 `DragDrop.DoDragDrop()` API는 ListBox/DataGrid의 마우스 캡쳐와 충돌하여 정상 동작하지 않는다. **Manual mouse tracking** 방식을 사용해야 한다.

**구현 방식:**

```csharp
private int _dragStartIndex = -1;
private bool _isDragging = false;
private Point _dragStartPoint;

// 1단계: 마우스 다운 — 시작 인덱스 기록
private void FavoritesList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    _dragStartIndex = GetItemIndexFromPoint(e.GetPosition(FavoritesList));
    _dragStartPoint = e.GetPosition(FavoritesList);
    _isDragging = false;
}

// 2단계: 마우스 이동 — 드래그 시작 감지
private void FavoritesList_PreviewMouseMove(object sender, MouseEventArgs e)
{
    if (e.LeftButton != MouseButtonState.Pressed || _dragStartIndex < 0)
        return;

    Point currentPoint = e.GetPosition(FavoritesList);
    double yDiff = Math.Abs(currentPoint.Y - _dragStartPoint.Y);

    if (yDiff > SystemParameters.MinimumVerticalDragDistance)
    {
        _isDragging = true;
        int currentIndex = GetItemIndexFromPoint(currentPoint);

        if (currentIndex >= 0 && currentIndex != _dragStartIndex)
        {
            // ObservableCollection.Move()로 항목 이동
            _viewModel.FavoriteParameters.Move(_dragStartIndex, currentIndex);
            _dragStartIndex = currentIndex;  // 인덱스 갱신
        }
    }
}

// 3단계: 마우스 업 — 드래그 종료
private void FavoritesList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    _dragStartIndex = -1;
    _isDragging = false;
}
```

**왜 DragDrop API를 사용하지 않는가:**
- `DragDrop.DoDragDrop()`을 호출하면 WPF가 모달 드래그 루프에 진입
- ListBox가 이미 마우스를 캡쳐하고 있어 충돌 발생
- 드래그 시작이 안 되거나, 시작 후 ListBox 선택이 꼬이는 문제
- Manual tracking이 가장 안정적

**주의사항:**
- `ObservableCollection.Move()`를 사용할 것 (`Remove()` + `Insert()` 대신)
- `Move()`는 단일 `CollectionChanged` 이벤트만 발생시켜 UI 깜빡임 방지
- `GetItemIndexFromPoint()`는 `VisualTreeHelper.HitTest()` 기반으로 구현

### 8.3 Favorite 저장/불러오기 (INI)

**FavoriteFileService** — `Favorite/` 폴더에 INI 형식 저장.

**저장 경로:** `{exe폴더}/Favorite/`

**INI 파일 구조:**
```ini
[Favorites]
Count=4

[Item0]
ParameterId=FT-001
Name=Target Position

[Item1]
ParameterId=FT-015
Name=Actual Velocity

[Item2]
ParameterId=FT-023
Name=Phase Current U

[Item3]
ParameterId=FT-042
Name=Bus Voltage
```

**진입점:**
- Action Panel의 **"Save Favorite"** 버튼 → `FavoriteFileService.Save()`
- Action Panel의 **"Load Favorite"** 버튼 → `FavoriteFileService.Load()`
- 앱 시작 시 자동 로드 (기본 파일명: `default.ini`)

### 8.4 전체 삭제

- **"Clear All"** 버튼 클릭
- `ShowClearFavoritesConfirmMessage` 발행
- 확인 다이얼로그(ConfirmExitDialog 재사용) 표시
- 확인 시 `FavoriteParameters.Clear()` 호출
- `FavoriteAnimationMessage(IsAdded: false)` → 트리 Favorites 노드에 감소 애니메이션

---

## 9. 컨트롤 패널

차트 하단에 배치되는 드라이브 제어 버튼 영역.

### 9.1 버튼 레이아웃

```
┌────────────────────────────────────────────────────┐
│ [Enable] [Disable] [Jog+] [Jog-] [ClearFault]     │
│ [Reset]  [TgtPos+] [TgtPos-] [ZeroSet] [MoveZero] │
├────────────────────────────────────────────────────┤
│ Target Position: [________]  Velocity: [________]  │
└────────────────────────────────────────────────────┘
```

### 9.2 버튼 상세

| 버튼 | 기능 | 전송 커맨드 |
|------|------|-------------|
| **Enable** | 서보 활성화 | SET 커맨드 |
| **Disable** | 서보 비활성화 | SET 커맨드 |
| **Jog+** | 정방향 조그 운전 | SET 커맨드 (누르는 동안) |
| **Jog-** | 역방향 조그 운전 | SET 커맨드 (누르는 동안) |
| **ClearFault** | 알람 해제 | SET 커맨드 |
| **Reset** | 드라이브 리셋 | SET 커맨드 |
| **TgtPos+** | 목표 위치 증가 | 입력필드 값 기반 |
| **TgtPos-** | 목표 위치 감소 | 입력필드 값 기반 |
| **ZeroSet** | 현재 위치를 0으로 설정 | SET 커맨드 |
| **MoveZero** | 원점 복귀 | SET 커맨드 |

### 9.3 입력 필드

| 필드 | 용도 | 단위 |
|------|------|------|
| Target Position | 목표 위치 입력 | pulse / mm / deg |
| Velocity | 속도 입력 | rpm / mm/s |

- 숫자 입력만 허용 (PreviewTextInput 검증)
- Enter 키 입력 시 값 전송
- 드라이브 미연결 시 비활성화 (IsEnabled=False)

---

## 10. 테마 연동

### 10.1 테마 변경 시 차트 업데이트

`ThemeChangedMessage` 수신 시 `RefreshChartTheme()` 호출.

```csharp
private void RefreshChartTheme()
{
    var plot = wpfPlot.Plot;

    // 배경색
    plot.FigureBackground.Color = GetThemeColor("BackgroundBrush");
    plot.DataBackground.Color = GetThemeColor("SurfaceBrush");

    // 축 색상
    plot.Axes.Bottom.TickLabelStyle.ForeColor = GetThemeColor("TextPrimary");
    plot.Axes.Bottom.MajorTickStyle.Color = GetThemeColor("TextSecondary");
    plot.Axes.Bottom.MinorTickStyle.Color = GetThemeColor("TextTertiary");

    // 그리드 색상
    plot.Grid.MajorLineColor = GetThemeColor("SurfaceVariantBrush");

    // Y축 (각 그룹별)
    foreach (var axis in _groupAxes)
    {
        axis.FrameLineStyle.Color = GetThemeColor("TextSecondary");
        // 채널 색상은 유지 (사용자 지정값 덮어쓰지 않음)
    }

    wpfPlot.Refresh();
}
```

### 10.2 GetThemeColor 헬퍼

스타일 딕셔너리의 `DynamicResource`에서 ScottPlot.Color로 변환.

```csharp
private ScottPlot.Color GetThemeColor(string resourceKey)
{
    var brush = (SolidColorBrush)Application.Current.Resources[resourceKey];
    var c = brush.Color;
    return new ScottPlot.Color(c.R, c.G, c.B, c.A);
}
```

**주의사항:**
- `FromHex()` 사용 금지 — 반드시 `GetThemeColor()` 헬퍼를 통해 스타일 딕셔너리에서 읽기
- 채널 색상(사용자 지정)은 테마 변경 시 덮어쓰지 않음
- 배경/축/그리드 색상만 테마에 따라 변경

### 10.3 적용되는 색상 토큰

| 차트 요소 | 리소스 키 | 용도 |
|-----------|-----------|------|
| Figure 배경 | `BackgroundBrush` | 차트 외부 배경 |
| Data 배경 | `SurfaceBrush` | 차트 데이터 영역 배경 |
| 축 텍스트 | `TextPrimary` | 축 레이블, 눈금 텍스트 |
| 주눈금 | `TextSecondary` | 주눈금 선 |
| 보조눈금 | `TextTertiary` | 보조눈금 선 |
| 그리드 | `SurfaceVariantBrush` | 그리드 라인 |
| 채널 1~8 | `ChartCH1Brush` ~ `ChartCH8Brush` | 초기 채널 색상 |

---

## 11. 주요 클래스/파일

| 파일 | 역할 | 비고 |
|------|------|------|
| `Views/MonitorControlDialog.xaml` | UI 레이아웃 (XAML) | Grid, WpfPlot, ListBox, 버튼 배치 |
| `Views/MonitorControlDialog.xaml.cs` | 차트 로직, 드래그, 색상 피커 | 코드-비하인드 (차트는 MVVM 한계로 코드-비하인드 필수) |
| `Services/ScopeSettingService.cs` | 스코프 설정 INI 저장/로드 | `{exe}/ScopeSetting/` 폴더 |
| `Services/FavoriteFileService.cs` | Favorite INI 저장/로드 | `{exe}/Favorite/` 폴더 |
| `ViewModels/MainWindowViewModel.cs` | `FavoriteParameters`, 메시지 처리 | ObservableCollection + WeakReferenceMessenger |
| `Themes/DarkColors.xaml` | 차트 테마 색상 (Dark) | `ChartCH1Brush` ~ `ChartCH8Brush` 포함 |
| `Themes/GrayColors.xaml` | 차트 테마 색상 (Gray) | 동일 키, Gray 테마 값 |
| `Themes/LightColors.xaml` | 차트 테마 색상 (Light) | 동일 키, Light 테마 값 |

### 11.1 코드-비하인드를 사용하는 이유

ScottPlot `WpfPlot` 컨트롤은 MVVM 바인딩을 완전히 지원하지 않는다:
- `Plot.Add.Signal()` 등의 API는 명령형 호출 필요
- 축 조작, 색상 변경, Refresh 등이 ViewModel에서 직접 불가
- **규칙**: 차트 관련 로직은 코드-비하인드에서, 데이터/상태는 ViewModel에서 관리

---

## 12. 메시지 (WeakReferenceMessenger)

### 12.1 메시지 목록

| 메시지 클래스 | 페이로드 | 발행자 | 수신자 | 용도 |
|---------------|----------|--------|--------|------|
| `ToggleMonitorSectionMessage` | `Section` (string) | DriveTreeView 버튼 | MonitorControlDialog | Oscilloscope/ControlPanel 영역 독립 토글 |
| `ShowMonitorControlMessage` | — | DriveTreeView, Ribbon | MainWindow | 스코프 다이얼로그 표시/포커스 |
| `FavoriteAnimationMessage` | `IsAdded` (bool) | CompareParameterPanel | DriveTreeView | 트리 Favorites 노드에 ±1 플로팅 애니메이션 |
| `ShowClearFavoritesConfirmMessage` | — | MonitorControlDialog | MainWindow | 전체삭제 확인 다이얼로그 표시 |
| `ThemeChangedMessage` | `ThemeName` (string) | MainWindow (테마 토글) | MonitorControlDialog | 차트 테마 색상 갱신 |

### 12.2 메시지 등록/해제 패턴

```csharp
// 등록 (생성자 또는 Loaded)
WeakReferenceMessenger.Default.Register<ToggleMonitorSectionMessage>(this, (r, m) =>
{
    ((MonitorControlDialog)r).OnToggleMonitorSection(m);
});

// 해제 (Unloaded)
WeakReferenceMessenger.Default.UnregisterAll(this);
```

### 12.3 `ToggleMonitorSectionMessage` 상세

```csharp
// Section 값에 따라 독립 토글
public class ToggleMonitorSectionMessage
{
    public string Section { get; }  // "Oscilloscope" 또는 "ControlPanel"

    public ToggleMonitorSectionMessage(string section)
    {
        Section = section;
    }
}
```

- **"Oscilloscope"**: 차트 Toolbar + Chart + StatusBar + Scale 패널 토글
- **"ControlPanel"**: 컨트롤 패널 그리드 토글
- 둘은 독립적으로 동작 — Oscilloscope만 표시, ControlPanel만 표시, 둘 다 표시, 둘 다 숨김 모두 가능

---

## 13. WPF 구현 시 주의사항

### 13.1 DragDrop API 충돌

> **문제**: `DragDrop.DoDragDrop()`은 ListBox/DataGrid의 마우스 캡쳐와 충돌한다.
>
> **해결**: Manual mouse tracking (`PreviewMouseLeftButtonDown` / `PreviewMouseMove` / `PreviewMouseLeftButtonUp`) 사용. 섹션 8.2 참조.

### 13.2 Popup 외부 클릭 닫기

> **문제**: `Popup.StaysOpen="False"`는 ComboBox 등과 상호작용 시 예기치 않게 닫힐 수 있다.
>
> **해결**: `StaysOpen="True"` + Window 레벨 `PreviewMouseDown` 핸들러에서 Popup 영역 외부 클릭 감지.

```csharp
private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    if (_activePopup != null && _activePopup.IsOpen)
    {
        // Popup 내부 클릭인지 확인
        var hitTest = VisualTreeHelper.HitTest(_activePopup.Child, e.GetPosition(_activePopup.Child));
        if (hitTest == null)
        {
            _activePopup.IsOpen = false;
            _activePopup = null;
        }
    }
}
```

### 13.3 ScottPlot Y축 관리

```csharp
// 기존 좌측 축 (항상 존재, 제거하면 안 됨)
var defaultAxis = plot.Axes.Left;

// 추가 Y축 생성
var newAxis = plot.Axes.AddLeftAxis();

// 축 제거 — Axes.Left는 절대 제거하지 않음
// 추가 축만 Remove
foreach (var axis in additionalAxes)
    plot.Axes.Remove(axis);

// Signal에 축 할당
signal.Axes.YAxis = targetAxis;
```

### 13.4 ObservableCollection.Move()

```csharp
// 권장 — 단일 이벤트 발생
collection.Move(oldIndex, newIndex);

// 비권장 — 2회 이벤트 발생, UI 깜빡임 유발
var item = collection[oldIndex];
collection.RemoveAt(oldIndex);
collection.Insert(newIndex, item);
```

### 13.5 Path.Data null 바인딩 에러

```xml
<!-- 문제: CustomIconPath가 null이면 GeometryConverter 에러 발생 -->
<Path Data="{Binding CustomIconPath}" />

<!-- 해결: TargetNullValue 설정 -->
<Path Data="{Binding CustomIconPath, TargetNullValue='M0 0'}" />
```

> `Visibility=Collapsed`이어도 바인딩은 평가됨 — null 바인딩 방어 필수.

### 13.6 AvalonDock DataContext 전파

```csharp
// LayoutAnchorable 내부 UserControl은 DataContext 자동 상속이 안 될 수 있음
// MainWindow.Loaded에서 명시적 할당 필요
private void MainWindow_Loaded(object sender, RoutedEventArgs e)
{
    if (monitorControlPane.Content is MonitorControlDialog dialog)
    {
        dialog.DataContext = this.DataContext;
    }
}
```

### 13.7 DataGrid Star 토글 (싱글클릭)

> **문제**: DataGrid는 첫 클릭을 셀 편집모드 진입에 사용 → ToggleButton이 더블클릭 필요.
>
> **해결**: `PreviewMouseLeftButtonDown` 핸들러에서 직접 토글.

```csharp
private void StarToggle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (sender is FrameworkElement fe && fe.DataContext is ParameterModel param)
    {
        param.IsFavorite = !param.IsFavorite;
        e.Handled = true;  // DataGrid의 셀 편집 진입 방지
    }
}
```

---

## 14. 의존성 다이어그램

```
MonitorControlDialog.xaml.cs
    ├── ScottPlot.WPF.WpfPlot          (차트 렌더링)
    ├── ScopeSettingService             (설정 저장/로드)
    ├── FavoriteFileService             (Favorite 저장/로드)
    ├── MainWindowViewModel             (FavoriteParameters, 상태)
    │   └── WeakReferenceMessenger      (메시지 버스)
    ├── Themes/*.xaml                   (색상 토큰)
    │   ├── ChartCH1Brush ~ CH8Brush
    │   ├── BackgroundBrush, SurfaceBrush
    │   └── TextPrimary, TextSecondary
    └── AvalonDock.LayoutAnchorable     (도킹 컨테이너)
```

---

## 15. 체크리스트 (새 프로젝트 적용 시)

새 프로젝트에서 오실로스코프 패널을 구현할 때 확인해야 할 항목:

- [ ] ScottPlot.WPF NuGet 패키지 설치 (5.x)
- [ ] CommunityToolkit.Mvvm NuGet 패키지 설치
- [ ] 스타일 딕셔너리에 `ChartCH1Brush` ~ `ChartCH8Brush` 토큰 등록
- [ ] `GetThemeColor()` 헬퍼 메서드 구현
- [ ] `ScopeSettingService` — INI 파일 저장/로드 서비스 구현
- [ ] `FavoriteFileService` — Favorite 저장/로드 서비스 구현
- [ ] WeakReferenceMessenger 메시지 클래스 정의
- [ ] AvalonDock DataContext 명시적 할당 코드 추가
- [ ] Popup 외부 클릭 닫기 핸들러 등록
- [ ] ListBox 드래그 → Manual mouse tracking 구현
- [ ] Path.Data 바인딩에 `TargetNullValue='M0 0'` 추가
- [ ] DataGrid Star 토글 — `PreviewMouseLeftButtonDown` 핸들러 추가
- [ ] 테마 변경 메시지 수신 → `RefreshChartTheme()` 호출
- [ ] 채널 색상 변경 시 5곳 동기화 확인 (Ellipse, ComboBox, Signal, Scale, Y축)
- [ ] Y축 그룹핑 로직 — `GetChannelCategory()`, `RebuildScaleGroups()` 구현
- [ ] 오토스케일 — 그룹별 + 전체 모드 구현
- [ ] 컨트롤 패널 버튼 — 드라이브 통신 커맨드 연결
