# Oscilloscope & Control Panel 설계 명세서

> 본 문서는 서보 드라이브 파라미터 튜닝 애플리케이션의 **Oscilloscope(MonitorControlDialog)** 및 **Control Panel(ControlPanelView)** 패널을 다른 애플리케이션에서 동일하게 재현하기 위한 상세 설계 명세서이다.

---

## 1. 전체 아키텍처

### 1.1 패널 호스팅 구조

두 패널 모두 **AvalonDock의 LayoutAnchorable**로 호스팅되며, 독립적인 도킹/플로팅/숨김이 가능하다.

```
MainWindow (AvalonDock DockingManager)
├── LayoutRoot
│   └── LayoutPanel (Horizontal)
│       ├── LayoutAnchorablePane (Left: DriveTree, Width=250)
│       ├── LayoutPanel (Vertical: Center)
│       │   ├── LayoutDocumentPane (Editor)
│       │   └── LayoutAnchorablePane (Bottom: ErrorLog, Height=150)
│       └── LayoutAnchorablePane (Right: MonitorControlDialog, Width=1250)  ← Oscilloscope
│
└── LayoutRoot.RightSide
    └── LayoutAnchorSide
        └── LayoutAnchorGroup
            └── LayoutAnchorable (ControlPanel, FloatingWidth=620, FloatingHeight=380) ← Control Panel
```

### 1.2 패널 간 관계

| 항목 | MonitorControlDialog (Oscilloscope) | ControlPanelView (Control Panel) |
|------|--------------------------------------|----------------------------------|
| AvalonDock 위치 | 메인 레이아웃 오른쪽 패널 | RightSide AnchorGroup (AutoHide 영역) |
| 초기 상태 | 숨김 (startup 시 `Hide()` 호출) | 숨김 |
| 열기 방식 | "Oscilloscope" 메시지 수신 시 `Show()` + `ToggleChart()` | "ControlPanel" 메시지 수신 시 `Show()` + `Float()` |
| 닫기 동작 | `Closing` 이벤트에서 `Cancel=true` → `Hide()` | `Closing` 이벤트에서 `Cancel=true` → `Hide()` |
| ContentId | `"monitorControl"` | `"controlPanel"` |
| CanClose | True | True |
| CanFloat | True | True |
| 기능 | 실시간 차트 + 즐겨찾기 파라미터 리스트 | 축 상태 모니터 + 제어 버튼 + 입력 필드 |

### 1.3 메시징 시스템

패널 토글은 `WeakReferenceMessenger`를 통해 수행된다.

```
ToggleMonitorSectionMessage(string Section)
  - Section = "Oscilloscope" → MonitorControlDialog 표시/숨김 + 차트 토글
  - Section = "ControlPanel" → ControlPanelView 표시/숨김 + Float()
```

MainWindow에서 메시지를 수신하여 `ToggleMonitorSection(section)` 메서드로 처리한다.

### 1.4 닫기 시 동작 (Hide 패턴)

두 패널 모두 AvalonDock의 `Closing` 이벤트를 인터셉트하여 실제로 닫지 않고 숨긴다:

- **MonitorControlDialog 닫기**: 차트가 보이면 `ToggleChart()` 호출(타이머 정지) → `Hide()`
- **ControlPanelView 닫기**: `StopUpdating()` 호출(타이머 정지, 상태 초기화) → `Hide()`

### 1.5 DataContext 전파

AvalonDock의 `LayoutAnchorable` 내부 UserControl은 DataContext가 자동 상속되지 않을 수 있다. Oscilloscope 패널을 `Show()`할 때 명시적으로 `DataContext = DataContext` (MainWindow의 VM)를 할당해야 한다.

---

## 2. Oscilloscope (MonitorControlDialog) 상세 설계

### 2.1 전체 레이아웃

3-Column Grid 구조:

```
┌──────────────────────────────────────────────┐
│ Column 0: Chart 영역          │ 1 │ Column 2 │
│                                │ S │ Toggle + │
│ ┌─────────────────────────────┐│ p │ Favorite │
│ │ Row 0: Toolbar              ││ l │ Parameter│
│ │ ├─ Row1: Buttons + Progress ││ i ││          │
│ │ └─ Row2: Channel Selectors  ││ t │          │
│ ├─────────────────────────────┤│ t │          │
│ │ Row 1: Scale | Toggle |Chart││ e │          │
│ ├─────────────────────────────┤│ r │          │
│ │ Row 2: Status Bar           ││   │          │
│ └─────────────────────────────┘│   │          │
└──────────────────────────────────────────────┘
```

#### Grid 정의

| Column | Width | MinWidth | 설명 |
|--------|-------|----------|------|
| 0 | `*` | 200 | 차트 영역 (가변) |
| 1 | `Auto` | - | GridSplitter (4px) |
| 2 | `220` | 150 | Favorites 패널 |

Column 0 내부 Row 정의:

| Row | Height | 설명 |
|-----|--------|------|
| 0 | Auto | Toolbar (처음엔 Collapsed) |
| 1 | `*` | Chart 영역 (처음엔 Collapsed) |
| 2 | Auto | Status Bar (처음엔 Collapsed) |

### 2.2 초기 상태

- 배경: `BackgroundBrush`
- Toolbar, Chart, StatusBar 모두 `Visibility.Collapsed`로 시작
- "Oscilloscope" 토글 메시지 수신 시 `Visible`로 전환

### 2.3 Toolbar (Row 0)

**컨테이너**: `Border`
- Background: `SurfaceBrush`
- BorderBrush: `BorderDefault`
- BorderThickness: `0,0,0,1` (아래쪽 구분선)
- Padding: `6,3`

#### Row 1: 제어 버튼 + Progress Bar

`DockPanel` 레이아웃:

**오른쪽 (DockPanel.Dock="Right")**: 진행 표시
- `ProgressBar` (Width=100, Height=10): Foreground=`SecondaryBrush`, Background=`SurfaceVariantBrush`
- `TextBlock` "Collecting..." (FontSize=`FontSizeXS`, Foreground=`SecondaryBrush`)
- 둘 다 초기 `Visibility.Collapsed`

**왼쪽**: 버튼 5개 (`WrapPanel Horizontal`)

| 버튼 | 아이콘 (PackIcon) | 텍스트 | 동작 |
|------|-------------------|--------|------|
| Single | `ChartLine` | "Single" | 1회 수집 (30 tick, 프로그레스 바 표시) |
| Continuous | `Refresh` | "Continuous" | 연속 수집 (타이머 시작) |
| Stop | `Stop` | "Stop" | 수집 중지 |
| Auto Scale | `ArrowExpandVertical` | "Auto Scale" | 전체 축 자동 스케일 |
| *(Separator)* | - | - | `ToolBar.SeparatorStyleKey` |
| Setting | `Cog` | "Setting" | 설정 다이얼로그 (`OscilloscopeOptionDialog`) |

모든 버튼 공통:
- Padding: `Padding.Button.Small` (StaticResource)
- Margin: `0,0,4,0` (마지막 버튼은 `0`)
- 아이콘: Width=14, Height=14

#### Row 2: 채널 선택기

`WrapPanel Horizontal`, Margin=`0,4,0,0`

4개 채널(CH1~CH4) 각각의 구성:

```
StackPanel (Horizontal, Margin="0,0,6,0")
├── CheckBox (IsChecked=True) — 채널 활성화/비활성화
├── Ellipse (8x8) — 채널 색상 표시 (ChartCH1~4Brush)
└── ComboBox (Width=175) — 파라미터 선택
```

ComboBox 속성:
- Tag: 채널 인덱스 ("0"~"3")
- MaxDropDownHeight: 300
- FontSize: `FontSizeXS`
- Background: `SurfaceVariantBrush`
- Foreground: 해당 채널 색상 (`ChartCH1~4Brush`)
- BorderBrush: `BorderDefault`

ComboBox 항목 목록 (83개 파라미터): Motor Feedback Position, Master Position, Follower Position, Position Error 등 서보 드라이브 모니터링 파라미터 전체 목록 (코드 참조).

기본 선택값:

| CH | 인덱스 | 파라미터명 |
|----|--------|-----------|
| CH1 | 0 | Motor Feedback Position |
| CH2 | 1 | Master Position |
| CH3 | 6 | Velocity Feedback |
| CH4 | 5 | Velocity Command |

### 2.4 Chart 영역 (Row 1)

3-Column Grid 구조:

```
┌────────────┬──────┬──────────────────────┐
│ Scale Panel│Toggle│ ScottPlot Chart      │
│ (Width=148)│(18px)│ (Fill)               │
└────────────┴──────┴──────────────────────┘
  Column 0    Col 1         Column 2
```

#### 2.4.1 Scale Panel (Column 0)

**컨테이너**: `Border` (Width=148)
- Background: `SurfaceBrush`
- BorderBrush: `DividerBrush`
- BorderThickness: `0,0,1,0` (오른쪽 구분선)

내부: `ScrollViewer` (Padding=`6,4`) > `StackPanel`

**2개 그룹**: Position, Velocity

각 그룹의 구성:

```
Border
├── 왼쪽 색상 바: BorderThickness="3,0,0,0" + 그룹 색상
├── Background: SurfaceVariantBrush
├── CornerRadius: Radius.SM
├── Padding: 6,4
└── StackPanel
    ├── DockPanel (헤더)
    │   ├── [Right] Auto Scale 버튼 (ArrowExpandVertical 12x12)
    │   └── TextBlock: "Position (pulse)" 또는 "Velocity (rpm)"
    ├── Grid 2x2 (Min/Max 입력)
    │   ├── "Max" 레이블 + TextBox (TxtMax0/TxtMax1)
    │   └── "Min" 레이블 + TextBox (TxtMin0/TxtMin1)
    └── WrapPanel (채널 범례)
        ├── Ellipse(6x6) + "Cur" (현재값)
        └── Ellipse(6x6) + "Cmd" (명령값)
```

**그룹별 상세:**

| 그룹 | 색상 (왼쪽 바) | 채널 | 기본 Min | 기본 Max | 단위 |
|------|---------------|------|----------|----------|------|
| Position | `ChartCH1Brush` | CH1(Cur), CH2(Cmd) | -12000 | 12000 | pulse |
| Velocity | `ChartCH3Brush` | CH3(Cur), CH4(Cmd) | -5000 | 5000 | rpm |

Min/Max TextBox 공통 속성:
- FontFamily: `FontFamilyCode`
- FontSize: `FontSizeXS`
- Foreground: `ValueHighlightBrush`
- Background: `SurfaceBrush`
- BorderBrush: `BorderDefault`
- Padding: `3,1`
- Tag: 그룹 인덱스 ("0" 또는 "1")

#### 2.4.2 Scale Toggle 버튼 (Column 1)

- Width: 18
- Background: `SurfaceVariantBrush`
- BorderBrush: `DividerBrush`
- 아이콘: `ChevronLeft` → 접으면 `ChevronRight`
- Foreground: `TextSecondary`

동작: Scale Panel의 Visibility를 Visible/Collapsed 토글

#### 2.4.3 ScottPlot 차트 (Column 2)

**WpfPlot** 컨트롤 (ScottPlot.WPF)

차트 설정:
- FigureBackground: `BackgroundBrush`
- DataBackground: `SurfaceBrush`
- Grid MajorLine: `SurfaceVariantBrush`
- 축 라벨/틱: `TextSecondary`
- 프레임: `BorderDefault`
- X축 라벨: "Time (ms)"
- Y축 라벨: 없음 ("")

**신호 구성 (4채널):**

| CH | 색상 키 | 선 두께 | 기본 범례 텍스트 |
|----|---------|---------|----------------|
| CH1 | `ChartCH1Brush` | 1.5 | Motor Feedback Position |
| CH2 | `ChartCH2Brush` | 1.0 | Master Position |
| CH3 | `ChartCH3Brush` | 1.5 | Velocity Feedback |
| CH4 | `ChartCH4Brush` | 1.0 | Velocity Command |

**멀티 Y축 구성:**

| 그룹 | 채널 | Y축 | 색상 |
|------|------|-----|------|
| Group 0 (Position) | CH1, CH2 | 기본 Left 축 | `ChartCH1Brush` |
| Group 1 (Velocity) | CH3, CH4 | 추가 Left 축 | `ChartCH3Brush` |

범례(Legend):
- IsVisible: true
- Alignment: UpperRight
- BackgroundColor: `SurfaceVariantBrush`
- FontColor: `TextPrimary`
- OutlineColor: `BorderDefault`

### 2.5 Status Bar (Row 2)

**컨테이너**: `Border`
- Background: `SurfaceBrush`
- BorderBrush: `BorderDefault`
- BorderThickness: `0,1,0,0` (위쪽 구분선)
- Padding: `6,2`

내용: `StackPanel Horizontal`
- 고정 텍스트: "4 CH | 10 kHz | " (Foreground=`TextSecondary`, FontSize=`FontSizeXS`)
- 상태 텍스트 (TxtStatus): "Stopped" (Foreground=`SecondaryBrush`, FontSize=`FontSizeXS`)

### 2.6 GridSplitter (Column 1)

- Width: 4
- Background: `DividerBrush`
- Cursor: `SizeWE`
- 수평 정렬: Center
- 수직 정렬: Stretch

### 2.7 Favorites 패널 (Column 2)

2-Column Grid:

```
┌──────┬────────────────────────┐
│Toggle│ Favorites 콘텐츠      │
│(18px)│                        │
└──────┴────────────────────────┘
```

#### 2.7.1 Favorites Toggle 버튼

- Width: 18
- Background: `SurfaceVariantBrush`
- BorderBrush: `DividerBrush`
- 아이콘: `ChevronRight` (기본) → 접으면 `ChevronLeft`
- 아이콘 색상: `WarningBrush` (노란색/주황색 별 색상)

접기 동작:
1. 현재 Favorites Column 너비 저장
2. FavoritesInnerContent Visibility → Collapsed
3. FavoritesColumn Width → Auto, MinWidth → 0
4. GridSplitter Visibility → Collapsed

펼치기 동작:
1. FavoritesInnerContent Visibility → Visible
2. 저장된 너비 복원 (기본 220px)
3. FavoritesColumn MinWidth → 150
4. GridSplitter Visibility → Visible

#### 2.7.2 Favorites 콘텐츠

**헤더 바** (Border, BorderThickness=`0,0,0,1`, Padding=`8,5`):

`DockPanel`:
- [Right] 아이템 수 표시 (예: "(5 items)"), FontSize=`FontSizeXS`, Foreground=`TextSecondary`
- [Left] 전체 삭제 버튼 (Star 아이콘 14x14, Foreground=`WarningBrush`, Tooltip="Clear all favorites")
- 제목: "Favorite Parameter" (FontSize=`FontSizeSM`, FontWeight=SemiBold, Foreground=`WarningBrush`)

**리스트** (`ListBox`):
- Background: Transparent
- BorderThickness: 0
- Padding: `4,2`
- AllowDrop: True

**ListBoxItem 커스텀 스타일**:

| 속성 | 값 |
|------|-----|
| Background | Transparent |
| Foreground | `TextPrimary` |
| Padding | `5,3` |
| Margin | `0,1` |
| Cursor | Hand |
| AllowDrop | True |
| CornerRadius | `Radius.SM` |
| BorderBrush | `BorderDefault` |
| BorderThickness | 1 |

트리거:
- IsMouseOver → Background=`SurfaceVariantBrush`
- IsSelected → Background=`SelectedRowBrush`, BorderBrush=`PrimaryBrush`

**각 아이템 DataTemplate** (4-Column Grid):

| Column | Width | 내용 |
|--------|-------|------|
| 0 | Auto | Star ToggleButton (14x14) |
| 1 | 44 | ShortNumber (파라미터 번호) |
| 2 | `*` | Name (파라미터 이름) |
| 3 | 45 | Value (편집 가능 TextBox) |

Star ToggleButton:
- IsChecked=True → Star (Foreground=`WarningBrush`)
- IsChecked=False → StarOutline (Foreground=`TextSecondary`)
- 클릭 시: `PreviewMouseLeftButtonDown` 핸들러로 즐겨찾기 해제 → 리스트에서 제거

ShortNumber/Name TextBlock:
- FontFamily: `FontFamilyCode`
- FontSize: `FontSizeXS`
- TextTrimming: CharacterEllipsis

Value TextBox:
- FontFamily: `FontFamilyCode`
- FontSize: `FontSizeXS`
- Foreground: `ValueHighlightBrush`
- FontWeight: SemiBold
- Background: Transparent, BorderThickness: 0
- UpdateSourceTrigger: LostFocus

---

## 3. Control Panel (ControlPanelView) 상세 설계

### 3.1 전체 레이아웃

```
ScrollViewer (VerticalScrollBar=Auto)
└── StackPanel (Margin="8,6")
    ├── "Axis Status Monitor" 제목
    ├── 3-카드 Grid (Position | Velocity | Load)
    ├── 구분선
    ├── "Control" 제목 + Enable 상태 표시
    ├── UniformGrid (2열, 8버튼)
    ├── 구분선
    └── Input Fields Grid (3행 2열)
```

Background: `SurfaceBrush`

### 3.2 Axis Status Monitor

제목: "Axis Status Monitor" (FontSize=`FontSizeSM`, FontWeight=SemiBold, Foreground=`TextPrimary`)

3-Column Grid (각 카드 사이 4px 간격):

#### 3.2.1 Card 1: ACTUAL POSITION

```
Border (SurfaceVariantBrush, Radius.MD, Padding=8,6)
└── StackPanel
    ├── 제목: "ACTUAL POSITION" (FontSizeXS, TextSecondary, SemiBold)
    ├── 값 표시: "{값} mm" (FontSizeXXL, Bold, FontFamilyCode, SuccessBrush)
    ├── 프로그레스 바 (Height=4)
    │   ├── 트랙: BackgroundBrush, CornerRadius=2
    │   └── 채움: SuccessBrush, CornerRadius=2, HorizontalAlignment=Left
    └── 범위 표시 (오른쪽 정렬): "-1,000 ~ 3,000 mm" (FontSizeXS, TextSecondary, FontFamilyCode)
```

프로그레스 바 너비 계산:
```
posRatio = Clamp((currentPosition - minPosition) / (maxPosition - minPosition), 0, 1)
barWidth = posRatio * containerActualWidth
```

#### 3.2.2 Card 2: ACTUAL VELOCITY

```
Border (SurfaceVariantBrush, Radius.MD, Padding=8,6)
└── Grid (2열: 텍스트 | 아크 게이지)
    ├── Column 0: StackPanel
    │   ├── 제목: "ACTUAL VELOCITY" (FontSizeXS, TextSecondary, SemiBold)
    │   ├── 값 표시: "{값} rpm" (FontSizeXXL, Bold, FontFamilyCode, SuccessBrush)
    │   └── 최대값: "3,900 rpm" (FontSizeXS, TextSecondary, FontFamilyCode)
    └── Column 1: Canvas (48x48) — Arc Gauge
```

#### 3.2.3 Card 3: ACTUAL LOAD

```
Border (SurfaceVariantBrush, Radius.MD, Padding=8,6)
└── Grid (2열: 텍스트 | 수직 바)
    ├── Column 0: StackPanel
    │   ├── 제목: "ACTUAL LOAD" (FontSizeXS, TextSecondary, SemiBold)
    │   └── 값 표시: "{값} %" (FontSizeXXL, Bold, FontFamilyCode, TextPrimary)
    └── Column 1: 수직 바 (Width=28)
        ├── "300%" 레이블 (FontSize=8, TextSecondary)
        └── Border (Height=40, BackgroundBrush)
            └── Border (SecondaryBrush, VerticalAlignment=Bottom, Height=동적)
```

Load 바 높이 계산:
```
loadRatio = Clamp(loadPercent / 300, 0, 1)
barHeight = loadRatio * 40
```

Load 값 색상 조건:
- loadPercent > 100 → `ErrorBrush`
- loadPercent > 60 → `WarningBrush`
- 그 외 → `TextPrimary`

### 3.3 Control 버튼

헤더 `DockPanel`:
- [Right] 상태 표시: "● Disabled" (FontSizeXS, ErrorBrush, FontFamilyCode)
  - Enabled 시: "● Enabled" (SuccessBrush)
- [Left] 제목: "Control" (FontSizeSM, SemiBold, TextPrimary)

**UniformGrid (Columns=2)**, 총 8개 버튼:

| # | 텍스트 | 아이콘 | 배경 | 전경 | 동작 |
|---|--------|--------|------|------|------|
| 1 | Enable | Play | `SuccessBrush` | `TextOnPrimary` | 활성화 + 타이머 시작 |
| 2 | Disable | Stop | `ErrorBrush` | `TextOnPrimary` | 비활성화 + 타이머 정지 |
| 3 | Jog - | *(없음)* | Transparent | `TextPrimary` | 누르고 있는 동안 음방향 조그 |
| 4 | Jog + | *(없음)* | Transparent | `TextPrimary` | 누르고 있는 동안 양방향 조그 |
| 5 | Clr Fault | AlertRemove | Transparent | `TextPrimary` | 확인 다이얼로그 후 Fault 초기화 |
| 6 | Reset | Restart | Transparent | `TextPrimary` | 확인 다이얼로그 후 전체 초기화 |
| 7 | Zero Set | Numeric0CircleOutline | `PrimaryBrush` | `TextOnPrimary` | 확인 다이얼로그 후 위치 0 초기화 |
| 8 | Move Zero | HomeCircleOutline | `PrimaryBrush` | `TextOnPrimary` | 원점 복귀 |

버튼 공통 속성:
- Margin: `2,1`
- Padding: `4,4`
- Cursor: Hand
- 아이콘: Width=12, Height=12
- 텍스트: FontSize=`FontSizeXS`

Transparent 배경 버튼:
- BorderBrush: `BorderDefault`
- BorderThickness: 1

아이콘별 특수 색상:
- AlertRemove (Clr Fault): Foreground=`WarningBrush`
- Restart (Reset): Foreground=`SecondaryBrush`

### 3.4 Input Fields

3행 2열 Grid:

| 행 | 레이블 | 입력 | 기본값 |
|----|--------|------|--------|
| 0 | Tgt Speed : | TextBox | 500 |
| 1 | Tgt Pos : | TextBox A ↔ TextBox B (왕복 A/B) | -500 ↔ 2000 |
| 2 | Cur Limit : | TextBox | 100 |

레이블 공통: Foreground=`TextSecondary`, FontSize=`FontSizeXS`, Margin=`0,0,6,3`

TextBox 공통:
- Background: `SurfaceVariantBrush`
- Foreground: `ValueHighlightBrush`
- BorderBrush: `BorderDefault`
- FontFamily: `FontFamilyCode`
- FontSize: `FontSizeXS`
- Padding: `5,2`

Tgt Pos 행은 3-Column Grid:
- Column 0: TextBox A (Position A)
- Column 1: "↔" (TextSecondary, FontSizeXS, Margin=`4,0`)
- Column 2: TextBox B (Position B)

---

## 4. 동작 명세 (Behavioral Specifications)

### 4.1 타이머 기반 데이터 생성

#### 4.1.1 Oscilloscope 타이머

- 주기: **50ms** (`DispatcherTimer`)
- 매 tick마다: 3회 연속 데이터 shift + 생성 (`_phase` 3 증가)
- 데이터 shift: `Array.Copy`로 배열 왼쪽 1칸 이동 → 마지막에 새 데이터 삽입
- 포인트 수: **500** (PointCount)
- 샘플 주기: **0.1ms** (PeriodMs)

데이터 생성 함수 (채널별):
```
CH0 (Position Current):  sin(p*0.5) * 10000 + p*10 + noise*50
CH1 (Position Command):  sin(p*0.5) * 10000 + p*10 + noise*20 + 150
CH2 (Velocity Current):  cos(p) * 3500 + cos(p*2.3) * 500 + noise*30
CH3 (Velocity Command):  cos(p) * 3500 + cos(p*2.3) * 500 + noise*15 + 80

여기서: p = _phase * 0.04, noise = Random(-0.5 ~ 0.5)
```

#### 4.1.2 Control Panel 타이머

- 주기: **50ms** (`DispatcherTimer`)
- Enable 상태에서만 동작
- Jog 중: position += direction * jogSpeed(50), velocity = direction * 800 + noise*50
- 유휴 시: position += noise*2, velocity *= 0.9 + noise*10 (감쇠)

### 4.2 Position 클램핑

모든 위치 업데이트 시 `Math.Clamp(position, minPosition, maxPosition)` 적용:
- minPosition: **-1000** (기본값)
- maxPosition: **3000** (기본값)

### 4.3 Jog Press-and-Hold 동작

**마우스 다운/업 이벤트** 기반 (Click 이벤트가 아님):

```
PreviewMouseLeftButtonDown → _isJogging = true, _jogDirection = -1 또는 +1
PreviewMouseLeftButtonUp   → _isJogging = false, _jogDirection = 0
```

- Enable 상태에서만 Jog 시작 가능
- 마우스를 누르고 있는 동안 50ms 간격으로 위치 이동 (타이머가 이미 동작 중)
- 버튼에서 마우스를 떼면 즉시 정지

### 4.4 확인 다이얼로그 (ConfirmActionDialog)

`MessageBox` 대신 커스텀 스타일 다이얼로그 사용.

**다이얼로그 스펙**:
- Size: 380 x 180
- WindowStyle: None (제목 표시줄 없음)
- AllowsTransparency: True
- ResizeMode: NoResize
- 외곽: Border (SurfaceBrush, BorderDefault, Radius.LG, Shadow Depth3)
- 드래그 이동: MouseLeftButtonDown → DragMove()
- ESC: 닫기

**레이아웃**:
```
┌──────────────────────────────┐
│ [Icon] Title                 │
│                              │
│ Message text                 │
│                              │
│          [Cancel] [Confirm]  │
└──────────────────────────────┘
```

**호출 방법**: `ConfirmActionDialog.Ask(owner, title, message, icon, confirmText, confirmBrushKey)`
- 반환: `bool` (true = 확인, false = 취소)

각 버튼별 다이얼로그 설정:

| 버튼 | 제목 | 메시지 | 아이콘 | 확인 텍스트 | 색상 키 |
|------|------|--------|--------|------------|---------|
| Zero Set | "Zero Set" | "Zero Set 하시겠습니까?\n현재 위치가 0으로 초기화됩니다." | Numeric0CircleOutline | "Zero Set" | `PrimaryBrush` |
| Clr Fault | "Clear Fault" | "Clear Fault 하시겠습니까?\n현재 발생한 Fault를 초기화합니다." | AlertRemoveOutline | "Clear Fault" | `WarningBrush` |
| Reset | "Reset" | "Reset 하시겠습니까?\n모든 상태가 초기화됩니다." | RestartAlert | "Reset" | `ErrorBrush` |

### 4.5 Velocity Arc Gauge 그리기 알고리즘

Canvas (48x48)에 프로그래밍 방식으로 그린다.

**상수**:
- Canvas 크기: 48x48
- 중심점: (24, 24)
- 반지름: 20
- 선 두께: 5

**그리기 순서**:

1. **트랙 (배경 원)**: Ellipse (40x40), Stroke=`BackgroundBrush`, StrokeThickness=5, Fill=Transparent
2. **아크 (값 표시)**: ratio > 0.01일 때만 그림
   - ratio = Clamp(|velocity| / MaxVelocity, 0, 0.999)
   - 시작 각도: -90도 (12시 방향)
   - 스윕 각도: ratio * 360도 (시계 방향)
   - PathFigure + ArcSegment로 호 생성
   - IsLargeArc: sweepAngle > 180
   - StrokeStartLineCap/EndLineCap: Round
3. **중앙 텍스트**: "{ratio*100}%" (FontSize=9, FontFamilyCode, TextPrimary)
   - Measure 후 중앙 배치

**아크 색상 조건**:
- ratio > 0.8 → `ErrorBrush`
- ratio > 0.5 → `WarningBrush`
- 그 외 → `SecondaryBrush`

### 4.6 차트 캡처 모드

#### Single 모드

1. 이미 실행 중이면 무시
2. Progress 표시: 확정(isIndeterminate=false), Value=0
3. 상태: "Collecting..."
4. 별도 `DispatcherTimer` (30ms 간격) 시작
5. 매 tick: `_singleTicks++`, Progress.Value = min(ticks * 3.3, 100)
6. 30 tick 도달 시 (약 900ms):
   - 타이머 정지
   - 새 데이터 생성 (`GenerateInitialData`)
   - AutoScale + Refresh + UpdateScaleTextBoxes
   - Progress 숨김
   - 상태: "Complete"

#### Continuous 모드

1. 이미 실행 중이면 무시
2. 메인 타이머(50ms) 시작
3. Progress 표시: 비확정(isIndeterminate=true)
4. 상태: "Running"

#### Stop

1. Single 타이머가 있으면 정지 + null
2. 연속 모드 타이머 정지
3. Progress 숨김
4. 상태: "Stopped"

### 4.7 Scale Group Auto-Scaling 알고리즘

그룹별 자동 스케일:

1. 해당 그룹의 채널 데이터 전체를 순회하여 min/max 탐색
2. 비활성(IsVisible=false) 채널은 건너뜀
3. min >= max이면 return (데이터 없음)
4. range = max - min (최소 1)
5. 여유 10% 추가: min -= range * 0.1, max += range * 0.1
6. TextBox 업데이트 (정수, "F0" 형식)
7. Y축 Range 설정 + Refresh

**Scale 수동 입력**: TextBox의 `LostFocus` 또는 `Enter` 키로 적용
- min >= max이면 무시
- Tag 속성으로 그룹 인덱스 식별

### 4.8 Favorites 드래그 순서 변경

**구현 방식**: WPF 기본 Drag & Drop API

1. `PreviewMouseLeftButtonDown`: 드래그 시작점 저장, 소스 ListBoxItem 인덱스 기록
2. `PreviewMouseMove`: 최소 드래그 거리 초과 시 `DragDrop.DoDragDrop` 시작
   - DataObject 형식: `"FavDragIndex"` (int)
   - Effect: Move
3. `DragOver`: `"FavDragIndex"` 데이터 존재 시 Move 허용
4. `Drop`:
   - 소스 인덱스와 타겟 인덱스 결정
   - `ObservableCollection.Move(fromIndex, toIndex)` 호출
   - 타겟이 없으면 마지막 위치로 이동

**ListBoxItem 히트 테스트**:
```
VisualTreeHelper.GetParent()를 따라 올라가며 ListBoxItem 탐색
```

### 4.9 Favorites Delete 키 지원

`FavoritesListBox.KeyDown` 이벤트:
- `Key.Delete` + 선택된 아이템이 `Parameter`일 때
- 해당 파라미터의 `IsFavorite = false` 설정
- 메인 파라미터 리스트에서도 동기화
- `FavoriteParameters.Remove()` 호출
- `FavoriteAnimationMessage(false)` 전송

### 4.10 Favorites Star 토글

Star ToggleButton의 `PreviewMouseLeftButtonDown` 이벤트 (Click이 아님):
- DataContext에서 Parameter 가져옴
- `IsFavorite = false` 설정
- 메인 파라미터 리스트에서 일치하는 항목도 `IsFavorite = false`
- `FavoriteParameters.Remove()` 호출
- `FavoriteAnimationMessage(false)` 전송
- `e.Handled = true` (ListBox의 선택 동작 방지)

### 4.11 차트 테마 갱신

테마 전환 시 `RefreshChartTheme()` 호출 필요 (ScottPlot은 DynamicResource 미지원):
- FigureBackground, DataBackground, Grid, 축 색상, 범례 색상 모두 재설정
- 4채널 신호 색상 재설정
- 그룹별 Y축 색상 재설정

### 4.12 채널 가시성 변경

CheckBox Checked/Unchecked 이벤트:
- 해당 채널 Signal의 `IsVisible` 설정
- 전체 AutoScale + Refresh + UpdateScaleTextBoxes

### 4.13 채널 파라미터 변경

ComboBox SelectionChanged:
- Tag로 채널 인덱스 식별
- 채널명 업데이트 + Signal 범례 텍스트 변경
- Refresh (데이터는 변경 안 됨 - 목업)

---

## 5. 데이터 모델

### 5.1 Parameter 클래스

```csharp
public partial class Parameter : ObservableObject
{
    // 고정 속성
    public string FtNumber { get; set; }      // 예: "Ft-001"
    public string ShortNumber => FtNumber.Replace("Ft-", "");  // 예: "001"
    public string Name { get; set; }           // 파라미터 이름
    public string Unit { get; set; }           // 단위
    public string Default { get; set; }        // 기본값
    public string Min { get; set; }            // 최소값
    public string Max { get; set; }            // 최대값
    public string Access { get; set; }         // "r/w" 또는 "r"
    public string Group { get; set; }          // 그룹 분류
    public bool IsModified { get; set; }       // 수정 여부

    // Observable 속성 (UI 바인딩)
    [ObservableProperty]
    private string _value = "";                // 현재 값 (양방향 바인딩)

    [ObservableProperty]
    private bool _isFavorite;                  // 즐겨찾기 상태 (양방향 바인딩)
}
```

Favorites 리스트에서 사용하는 속성:
- `ShortNumber` (읽기 전용, 표시용)
- `Name` (읽기 전용, 표시용)
- `Value` (양방향 바인딩, LostFocus 트리거)
- `IsFavorite` (양방향 바인딩, Star 토글)

### 5.2 ViewModel (MainWindowViewModel) 관련 속성

```csharp
public ObservableCollection<Parameter> FavoriteParameters { get; }  // 즐겨찾기 파라미터 목록
public ObservableCollection<Parameter> Parameters { get; }          // 현재 표시 중인 파라미터 목록
```

즐겨찾기 동기화:
- IsFavorite 변경 시 `UpdateFavorites()` → FavoriteParameters에 복사본 추가/제거
- ClearAllFavorites: `ShowClearFavoritesConfirmMessage` 전송 → 확인 후 `ExecuteClearAllFavorites()`

---

## 6. 참조하는 색상/테마 토큰 전체 목록

### 6.1 Color/Brush 토큰

| 토큰 키 | 역할 | 사용 위치 |
|---------|------|----------|
| `BackgroundBrush` | 앱 배경 | MonitorControl 배경, 차트 FigureBackground, Position 바 트랙, Velocity 게이지 트랙, Load 바 트랙 |
| `SurfaceBrush` | 패널/카드 배경 | Toolbar, Scale Panel, Favorites, StatusBar, 차트 DataBackground, Scale TextBox 배경 |
| `SurfaceVariantBrush` | 보조 표면 | Scale 그룹 카드, 채널 ComboBox 배경, Scale Toggle 버튼, Favorites Toggle, ProgressBar 배경, 차트 Grid, ListBoxItem Hover, 상태 카드 배경 |
| `PrimaryBrush` | 주요 액센트 | ListBoxItem Selected Border, ZeroSet/MoveZero 버튼, 확인 다이얼로그 기본 |
| `SecondaryBrush` | 보조 액센트 | ProgressBar, Collecting 텍스트, 상태 텍스트, Load 바 채움, Velocity 아크 기본색, Reset 아이콘 |
| `ErrorBrush` | 오류/위험 | Disable 버튼, Disabled 상태 텍스트, Load 100%+ 색상, Velocity 아크 80%+, Reset 확인 색상 |
| `WarningBrush` | 경고 | Star 아이콘(활성), Favorites 제목, Favorites Toggle 아이콘, Clr Fault 아이콘, Load 60%+, Velocity 아크 50%+ |
| `SuccessBrush` | 성공/활성 | Enable 버튼, Enabled 상태 텍스트, Position 값, Position 바 채움, Velocity 값 |
| `TextPrimary` | 주 텍스트 | 제목, 파라미터 이름, Scale 그룹 제목 bold 부분, 차트 범례 폰트, Load 기본 색상, Velocity 게이지 중앙 % |
| `TextSecondary` | 보조 텍스트 | 레이블, 범위 표시, Scale "Max"/"Min", 채널 상태바, 축 색상, Star(비활성) |
| `TextOnPrimary` | 반전 텍스트 | 채움 배경 버튼(Enable/Disable/ZeroSet/MoveZero) 위 텍스트 |
| `ValueHighlightBrush` | 값 강조 | Scale Min/Max TextBox, Favorites Value TextBox, Input Field TextBox |
| `BorderDefault` | 기본 테두리 | Toolbar 하단, 채널 ComboBox, Scale TextBox, StatusBar 상단, ListBoxItem, 차트 프레임, 버튼 테두리 |
| `DividerBrush` | 구분선 | Scale Panel 오른쪽, GridSplitter, Favorites Toggle, Favorites 헤더, 영역 구분 Rectangle |
| `SelectedRowBrush` | 선택 행 배경 | ListBoxItem Selected Background |
| `ChartCH1Brush` | 차트 채널 1 | CH1 색상 점, CH1 ComboBox 글자, Position 그룹 왼쪽 바, Position Cur 범례 |
| `ChartCH2Brush` | 차트 채널 2 | CH2 색상 점, CH2 ComboBox 글자, Position Cmd 범례 |
| `ChartCH3Brush` | 차트 채널 3 | CH3 색상 점, CH3 ComboBox 글자, Velocity 그룹 왼쪽 바, Velocity Cur 범례 |
| `ChartCH4Brush` | 차트 채널 4 | CH4 색상 점, CH4 ComboBox 글자, Velocity Cmd 범례 |

### 6.2 Font 토큰

| 토큰 키 | 사용 위치 |
|---------|----------|
| `FontSizeXS` | 거의 모든 데이터 표시 텍스트, 레이블, 버튼 텍스트, 채널 ComboBox |
| `FontSizeSM` | 섹션 제목 ("Axis Status Monitor", "Control", "Favorite Parameter"), 단위 텍스트 |
| `FontSizeMD` | ConfirmActionDialog 메시지, 기본 폰트 크기 |
| `FontSizeLG` | ConfirmActionDialog 제목 |
| `FontSizeXXL` | 상태 모니터 값 (Position, Velocity, Load) |
| `FontFamilyCode` | 모든 숫자/값 표시, Scale TextBox, Favorites 항목, Input Field, 상태 값 |
| `FontFamilyUI` | ConfirmActionDialog 기본 폰트 |

### 6.3 Spacing/Size 토큰

| 토큰 키 | 사용 위치 |
|---------|----------|
| `Padding.Button.Small` | Toolbar 버튼 Padding |
| `Padding.Dialog` | ConfirmActionDialog 내부 Margin |
| `Radius.SM` | Scale 그룹 카드, ListBoxItem CornerRadius |
| `Radius.MD` | 상태 모니터 카드 CornerRadius |
| `Radius.LG` | ConfirmActionDialog 외곽 CornerRadius |
| `Size.IconXL` | ConfirmActionDialog 헤더 아이콘 크기 |
| `Margin.Inline` | ConfirmActionDialog 아이콘-제목 간격, Cancel-Confirm 간격 |
| `MaterialDesignShadowDepth3` | ConfirmActionDialog 그림자 |

### 6.4 MaterialDesign PackIcon 사용 목록

| 아이콘 Kind | 사용 위치 |
|------------|----------|
| ChartLine | Single 버튼 |
| Refresh | Continuous 버튼 |
| Stop | Stop 버튼, Disable 버튼 |
| ArrowExpandVertical | Auto Scale 버튼, Scale 그룹 AutoScale |
| Cog | Setting 버튼 |
| Star | Favorites Star (활성) |
| StarOutline | Favorites Star (비활성) |
| ChevronLeft | Scale Panel Toggle (열림 상태) |
| ChevronRight | Scale Panel Toggle (닫힘), Favorites Toggle (열림) |
| Play | Enable 버튼 |
| AlertRemove | Clr Fault 버튼 |
| AlertRemoveOutline | Clr Fault 확인 다이얼로그 |
| Restart | Reset 버튼 |
| RestartAlert | Reset 확인 다이얼로그 |
| Numeric0CircleOutline | Zero Set 버튼/다이얼로그 |
| HomeCircleOutline | Move Zero 버튼 |
| HelpCircleOutline | ConfirmActionDialog 기본 아이콘 |

---

## 7. 구현 시 주의사항

### 7.1 ScottPlot DynamicResource 미지원

ScottPlot은 WPF DynamicResource를 지원하지 않으므로, 테마 전환 시 반드시 `RefreshChartTheme()` 메서드를 수동 호출하여 모든 차트 색상을 재설정해야 한다. 색상 변환은 `GetThemeColor()` 헬퍼를 통해 `Application.Current.TryFindResource()`에서 `SolidColorBrush`를 읽어 ScottPlot.Color로 변환한다.

### 7.2 DataGrid Star Toggle 이슈

DataGrid 내에서 ToggleButton을 사용할 경우, 첫 클릭이 셀 편집모드 진입에 사용되어 더블클릭이 필요한 문제가 있다. 해결: `PreviewMouseLeftButtonDown` 핸들러에서 직접 `IsFavorite` 토글 + `e.Handled = true`.

### 7.3 AvalonDock DataContext 전파

LayoutAnchorable 내부 UserControl은 DataContext가 자동 상속되지 않을 수 있다. `Show()` 시점에 명시적으로 DataContext를 할당해야 한다.

### 7.4 Velocity Arc Gauge 수학

- 시작점: (-90도, 즉 12시 방향)
- 시계 방향으로 ratio * 360도 스윕
- 라디안 변환: angle * PI / 180
- 좌표: (cx + r*cos(rad), cy + r*sin(rad))
- IsLargeArc: sweepAngle > 180
- ratio 클램핑: 최대 0.999 (360도 완전 원 방지)

### 7.5 Position Progress Bar

코드-비하인드에서 Width를 직접 설정하는 방식:
- 컨테이너의 `ActualWidth`를 사용 (레이아웃 완료 후)
- ActualWidth가 0이하면 fallback 150px 사용

### 7.6 Load 퍼센트 계산

```
loadPercent = |currentVelocity / MaxVelocity| * 100
```

최대 300%까지 표시 가능 (MaxLoad = 300).

### 7.7 Clear All Favorites 흐름

1. Clear 버튼 클릭 → VM의 `ClearAllFavoritesCommand` 실행
2. VM에서 `ShowClearFavoritesConfirmMessage` 전송
3. MainWindow에서 수신 → `ConfirmActionDialog.Ask()` 호출
4. 확인 시 → `vm.ExecuteClearAllFavorites()` 호출
5. 현재 Parameters의 IsFavorite 모두 false → FavoriteParameters.Clear()

---

## 8. 상수 요약

### 8.1 Oscilloscope 상수

| 상수 | 값 | 설명 |
|------|-----|------|
| PointCount | 500 | 차트 데이터 포인트 수 |
| PeriodMs | 0.1 | 샘플 주기 (ms) |
| 타이머 주기 | 50ms | 데이터 업데이트 주기 |
| Single 타이머 주기 | 30ms | Single 캡처 진행 주기 |
| Single 완료 tick | 30 | 약 900ms |
| 채널 수 | 4 | CH1~CH4 |
| 그룹 수 | 2 | Position, Velocity |
| Scale Panel 너비 | 148px | 고정 |
| Toggle 버튼 너비 | 18px | Scale/Favorites 토글 |
| Favorites 기본 너비 | 220px | 접기/펴기 시 저장/복원 |
| Favorites 최소 너비 | 150px | 최소 크기 |
| GridSplitter 너비 | 4px | 리사이즈 핸들 |

### 8.2 Control Panel 상수

| 상수 | 값 | 설명 |
|------|-----|------|
| MaxVelocity | 3900 | 최대 속도 (rpm) |
| MaxLoad | 300 | 최대 부하 (%) |
| 타이머 주기 | 50ms | 상태 업데이트 주기 |
| minPosition | -1000 | 위치 최소 범위 |
| maxPosition | 3000 | 위치 최대 범위 |
| jogSpeed | 50 | Jog 이동 속도 (units/tick) |
| Jog 속도 | 800 | Jog 시 표시 속도 (rpm 근사) |
| Arc Gauge 크기 | 48x48 | Canvas 크기 |
| Arc 반지름 | 20 | 원호 반지름 |
| Arc 두께 | 5 | 선 두께 |
| Load 바 높이 | 40px | 수직 바 최대 높이 |
| Random seed | 99 | 재현 가능 난수 |

---

## 9. 파라미터 선택 목록 (전체)

Oscilloscope 채널 ComboBox에 표시되는 83개 파라미터 목록:

```
Motor Feedback Position, Master Position, Follower Position,
Position Error, Position Command Count Frequency,
Velocity Command, Velocity Feedback, Velocity Error,
Current Command, Current Feedback, Current Command (D-Axis),
U Phase Current, V Phase Current, W Phase Current,
Absolute Maximum Current Command, Commutation Angle, Mechanical Angle,
Shunt Power Limit Ratio, Instantaneous Shunt Power,
Motor Power Limit Ratio, Drive Power Limit Ratio,
Drive Utilization, Drive Enabled,
Absolute Rotations, Absolute Single Turn, Bus Voltage,
Velocity Command Offset, Current Command Offset, Motor Utilization,
Analog Command - Velocity, Analog Command - Current,
Current Feedback (RMS), Maximum Current Feedback (RMS),
TouchProbe Function, TouchProbe Status,
TouchProbe Position 1 Positive Value, TouchProbe Position 1 Negative Value,
TouchProbe Position 2 Positive Value, TouchProbe Position 2 Negative Value,
Touch Probe 1 Positive Edge Counter, Touch Probe 1 Negative Edge Counter,
Touch Probe 2 Positive Edge Counter, Touch Probe 2 Negative Edge Counter,
ECAT Homing Status, ECAT Homing Error,
High Pass Filtered Current Command Output (ANF),
High Pass Filtered Current Command Variance (ANF),
Resonance Estimation Frequency (ANF),
ABSS Data, ABSA Data, Hall Value,
ABSS Data (Linear-BiSS), ABSA Data (Linear-BiSS), ABSA Data (Linear-BiSS-CC),
Load and Motor Side Feedback Difference,
Digital I/O, Control Word, Status Word,
MO New Set Point, MO Set Acknowledged, MO Target Reached,
MO Halt Enabled, MO Buffer Empty,
PP Cmd Position, PP Cmd Velocity, PP Cmd Accel, PP Cmd Decel, PP Cmd Jerk,
PP Actual Position, PP Actual Velocity,
PP Feedback Position, PP Feedback Velocity, PP Position Offset,
MO Target Position, MO Target Velocity,
MO Feedback Position, MO Feedback Velocity, MO Feedback Torque,
MO Ether CMD Offset, MO OriginOffset,
Displacement in Control Cycle, Displacement for Encoder Direction,
Partial Progress Step, Displacement for Command Cut-off,
Theta Reference Angle, Overall Progress Step, Number of Stabilizing Count
```
