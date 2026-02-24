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

**Keywords:** "WPF 화면 만들어줘", "아발론독", "AvalonDock", "기본 WPF", "독 레이아웃"

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 8.0 Windows (WPF) |
| Docking | AvalonDock (Xceed.Wpf.AvalonDock) |
| MVVM | CommunityToolkit.Mvvm |
| Icons | Segoe Fluent Icons / Geometry Path Data |
| Theme | Custom Dark Theme (ResourceDictionary) |

---

## Design System (from Figma)

### Color Palette — Dark Theme

```xml
<!-- Backgrounds -->
<Color x:Key="AppBackground">#FF1E1E1E</Color>
<Color x:Key="PanelBackground">#FF252526</Color>
<Color x:Key="HeaderBackground">#FF2D2D30</Color>
<Color x:Key="RibbonBackground">#FF3C3C3C</Color>
<Color x:Key="ToolbarBackground">#FF333337</Color>

<!-- Borders -->
<Color x:Key="BorderDefault">#FF3F3F46</Color>
<Color x:Key="BorderLight">#FF555555</Color>
<Color x:Key="BorderSubtle">#FF2A2A2A</Color>

<!-- Text -->
<Color x:Key="TextPrimary">#FFCCCCCC</Color>
<Color x:Key="TextSecondary">#FF888888</Color>
<Color x:Key="TextMuted">#FF555555</Color>
<Color x:Key="TextDisabled">#FF444444</Color>
<Color x:Key="TextWhite">#FFFFFFFF</Color>

<!-- Accent Colors -->
<Color x:Key="AccentRed">#FFCC3333</Color>
<Color x:Key="AccentGreen">#FF4EC970</Color>
<Color x:Key="AccentBlue">#FF56B6C2</Color>
<Color x:Key="AccentYellow">#FFD4A843</Color>
<Color x:Key="AccentInfoBlue">#FF6D9CBE</Color>

<!-- Interactive States -->
<Color x:Key="SelectionBackground">#FF264F78</Color>
<Color x:Key="HoverBackground">#FF3E3E42</Color>
<Color x:Key="ActiveBackground">#FF505050</Color>
<Color x:Key="DarkRedHighlight">#FF6B1A1A</Color>
<Color x:Key="CloseButtonHover">#FFE81123</Color>
<Color x:Key="FocusBorder">#FF3794FF</Color>
```

### Typography

| Element | Size | Weight | Color |
|---------|------|--------|-------|
| Title Bar | 11px | Normal | TextSecondary (#AAA) |
| Menu Tab | 11px | Normal | TextSecondary (#999) |
| Menu Tab Active | 11px | Normal | TextWhite |
| Ribbon Label | 9px | Normal | TextSecondary (#AAA) |
| Ribbon Group Label | 9px | Normal | TextMuted (#777) |
| Tree Item | 11px | Normal | TextPrimary |
| Grid Cell | 11px | Normal | TextPrimary |
| Grid Header | 11px | Normal | TextWhite (on DarkRedHighlight bg) |
| Panel Header | 11px | Normal | TextPrimary |
| Status Bar | 9-10px | Normal | TextSecondary |
| Terminal | 10px | Mono | AccentGreen |

### Spacing & Sizing

| Element | Value |
|---------|-------|
| TitleBar Height | 28px |
| MenuBar Height | ~26px |
| RibbonBar Height | 70px |
| StatusBar Height | 22px |
| Panel Header Height | 24px |
| Border Radius | 3px |
| Icon Size (Large) | 22px |
| Icon Size (Small) | 12-16px |
| Icon Size (Tiny) | 10-11px |
| Tree Item Padding | 3px vertical, indent 14px/level |
| Grid Row Padding | 3px vertical |
| Button Padding | 5-6px vertical, 6px horizontal |

---

## Layout Structure

```
+----------------------------------------------------------+
| TitleBar (28px) — App Icon + Title + Min/Max/Close        |
+----------------------------------------------------------+
| MenuBar — Tab-style: File | Parameters | Tools | Views    |
+----------------------------------------------------------+
| RibbonBar (70px) — Grouped icon buttons + Status icons    |
+----------------------------------------------------------+
|           |                              |                |
| TreePanel | DataGrid / Content Area      | SidePanel     |
| (left)    | (center, resizable)          | (right)       |
| ~15%      | ~70%                         | ~8%           |
|           |                              |                |
+-----------+------------------------------+----------------+
| Controls  | Terminal                     | Alarm List     |
| (left)    | (center)                     | (right)        |
| ~15%      | ~45%                         | ~40%           |
+----------------------------------------------------------+
| StatusBar (22px) — Connected + Tab nav + Info text        |
+----------------------------------------------------------+
```

### AvalonDock Mapping

| Figma Component | AvalonDock Element |
|-----------------|-------------------|
| TreePanel (left) | `LayoutAnchorablePane` (Left) |
| DataGrid (center) | `LayoutDocumentPane` (Center) |
| SideButtons (right) | `LayoutAnchorablePane` (Right) |
| Controls (bottom-left) | `LayoutAnchorablePane` (Bottom) |
| Terminal (bottom-center) | `LayoutAnchorablePane` (Bottom) |
| Alarm (bottom-right) | `LayoutAnchorablePane` (Bottom) |
| FloatingWindow | AvalonDock auto-float (drag out) |
| Pop-out button | `ToggleAutoHide` or `Float` command |
| Dock-back | AvalonDock native dock-back |

---

## Component Patterns

### 1. PanelHeader (재사용 UserControl)

```xml
<!-- PanelHeader: accent icon + title + action buttons -->
<Border Background="{StaticResource HeaderBackgroundBrush}"
        BorderBrush="{StaticResource BorderDefaultBrush}"
        BorderThickness="0,0,0,1" Height="24" CornerRadius="3,3,0,0">
    <DockPanel LastChildFill="True">
        <!-- Right: Action buttons -->
        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Margin="0,0,2,0">
            <Button Style="{StaticResource PanelActionButton}" ToolTip="Pop out">
                <!-- ExternalLink icon path -->
            </Button>
            <Button Style="{StaticResource PanelActionButton}" ToolTip="Minimize">
                <!-- Minimize icon -->
            </Button>
            <Button Style="{StaticResource PanelActionButton}" ToolTip="Maximize">
                <!-- Maximize icon -->
            </Button>
            <Button Style="{StaticResource PanelCloseButton}" ToolTip="Close">
                <!-- X icon -->
            </Button>
        </StackPanel>
        <!-- Left: Icon + Title -->
        <StackPanel Orientation="Horizontal" Margin="8,0,0,0" VerticalAlignment="Center">
            <Path Data="{StaticResource IconPath}" Fill="{Binding AccentColor}"
                  Width="13" Height="13" Stretch="Uniform" Margin="0,0,5,0"/>
            <TextBlock Text="{Binding Title}" Foreground="{StaticResource TextPrimaryBrush}"
                       FontSize="11" VerticalAlignment="Center"/>
        </StackPanel>
    </DockPanel>
</Border>
```

### 2. TreePanel

```xml
<TreeView Background="{StaticResource PanelBackgroundBrush}"
          BorderThickness="0" FontSize="11"
          Foreground="{StaticResource TextPrimaryBrush}">
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Children}">
            <StackPanel Orientation="Horizontal" Margin="0,2">
                <Path Data="{Binding IconPath}" Fill="{Binding IconColor}"
                      Width="13" Height="13" Stretch="Uniform" Margin="0,0,5,0"/>
                <TextBlock Text="{Binding Label}"/>
            </StackPanel>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

### 3. DataGrid (Parameter Grid)

```xml
<DataGrid AutoGenerateColumns="False" Background="{StaticResource AppBackgroundBrush}"
          GridLinesVisibility="Horizontal" BorderThickness="0"
          RowHeight="22" FontSize="11"
          HeadersVisibility="Column"
          Foreground="{StaticResource TextPrimaryBrush}"
          HorizontalGridLinesBrush="{StaticResource BorderSubtleBrush}">
    <!-- Column header style: DarkRedHighlight background -->
    <DataGrid.ColumnHeaderStyle>
        <Style TargetType="DataGridColumnHeader">
            <Setter Property="Background" Value="{StaticResource DarkRedHighlightBrush}"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="FontSize" Value="11"/>
            <Setter Property="Padding" Value="8,5"/>
        </Style>
    </DataGrid.ColumnHeaderStyle>
    <!-- Row selection: SelectionBackground -->
    <DataGrid.RowStyle>
        <Style TargetType="DataGridRow">
            <Style.Triggers>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="{StaticResource SelectionBackgroundBrush}"/>
                </Trigger>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#FF2A2D2E"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </DataGrid.RowStyle>
</DataGrid>
```

### 4. StatusBar

```xml
<Border Background="{StaticResource ToolbarBackgroundBrush}" Height="22"
        BorderBrush="{StaticResource BorderDefaultBrush}" BorderThickness="0,1,0,0">
    <DockPanel>
        <!-- Left: Connection status -->
        <StackPanel DockPanel.Dock="Left" Orientation="Horizontal" Margin="8,0">
            <Ellipse Width="7" Height="7" Fill="{StaticResource AccentGreenBrush}" Margin="0,0,4,0"/>
            <TextBlock Text="Connected" FontSize="9" Foreground="{StaticResource TextSecondaryBrush}"/>
        </StackPanel>
        <!-- Right: Info text -->
        <TextBlock DockPanel.Dock="Right" FontSize="9" Margin="0,0,8,0"
                   Foreground="{StaticResource TextMutedBrush}" VerticalAlignment="Center"
                   Text="Drive Setup and Motion Activity"/>
        <!-- Center: Tab navigation -->
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center">
            <!-- Status tabs: Drive | Motor | Encoder | Tune | ServiceInfo -->
        </StackPanel>
    </DockPanel>
</Border>
```

### 5. RibbonBar (Custom, NOT System.Windows.Controls.Ribbon)

```xml
<Border Background="{StaticResource RibbonBackgroundBrush}" Height="70"
        BorderBrush="{StaticResource BorderLightBrush}" BorderThickness="0,0,0,1">
    <DockPanel>
        <!-- Right: Status icons (STOP, Srv ON, Srv OFF, Comm) -->
        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Margin="0,0,12,0">
            <!-- Status icon buttons -->
        </StackPanel>
        <!-- Left: Ribbon groups separated by vertical lines -->
        <StackPanel Orientation="Horizontal">
            <!-- Each group: vertical stack of icon buttons + group label at bottom -->
        </StackPanel>
    </DockPanel>
</Border>
```

---

## Button Styles

### PanelActionButton (header buttons: minimize, maximize, pop-out)
```xml
<Style x:Key="PanelActionButton" TargetType="Button">
    <Setter Property="Width" Value="18"/>
    <Setter Property="Height" Value="18"/>
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Foreground" Value="#FF777777"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="{StaticResource HoverBackgroundBrush}"/>
            <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### PanelCloseButton (red hover)
```xml
<Style x:Key="PanelCloseButton" TargetType="Button" BasedOn="{StaticResource PanelActionButton}">
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="{StaticResource CloseButtonHoverBrush}"/>
            <Setter Property="Foreground" Value="White"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### GridButton (bottom controls, side buttons)
```xml
<Style x:Key="GridButton" TargetType="Button">
    <Setter Property="Background" Value="#FF3E3E42"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="FontSize" Value="11"/>
    <Setter Property="Padding" Value="8,5"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderLightBrush}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="{StaticResource ActiveBackgroundBrush}"/>
        </Trigger>
        <Trigger Property="IsPressed" Value="True">
            <Setter Property="Background" Value="{StaticResource SelectionBackgroundBrush}"/>
            <Setter Property="BorderBrush" Value="{StaticResource FocusBorderBrush}"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

---

## Alarm/Log Item Types

| Type | Icon | Color |
|------|------|-------|
| Error | AlertCircle | AccentRed (#CC3333) |
| Warning | AlertTriangle | AccentYellow (#D4A843) |
| Info | Info | AccentInfoBlue (#6D9CBE) |

---

## File Naming Convention

| Type | Pattern | Example |
|------|---------|---------|
| View (XAML) | `{Name}View.xaml` | `MainShellView.xaml` |
| ViewModel | `{Name}ViewModel.cs` | `MainShellViewModel.cs` |
| Theme | `DarkTheme.xaml` | ResourceDictionary |
| UserControl | `{Name}Panel.xaml` | `TreePanel.xaml` |
| Styles | `{Name}Styles.xaml` | `ButtonStyles.xaml` |

---

## MVVM Pattern

```csharp
// ViewModel using CommunityToolkit.Mvvm
public partial class MainShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "RS Application Studio";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _activeTab = "Parameters";

    [RelayCommand]
    private void ServoOn() { /* ... */ }

    [RelayCommand]
    private void ServoOff() { /* ... */ }

    [RelayCommand]
    private void EmergencyStop() { /* ... */ }
}
```

---

## AvalonDock Theme

Use `Xceed.Wpf.AvalonDock.Themes.VS2013.Vs2013DarkTheme` as base, then override:

```xml
<avalonDock:DockingManager>
    <avalonDock:DockingManager.Theme>
        <avalonDockTheme:Vs2013DarkTheme/>
    </avalonDock:DockingManager.Theme>
    <avalonDock:LayoutRoot>
        <avalonDock:LayoutPanel Orientation="Vertical">
            <!-- Upper: Horizontal split -->
            <avalonDock:LayoutPanel Orientation="Horizontal">
                <avalonDock:LayoutAnchorablePane DockWidth="240" DockMinWidth="100">
                    <avalonDock:LayoutAnchorable Title="Drive Setup" ContentId="tree"/>
                </avalonDock:LayoutAnchorablePane>
                <avalonDock:LayoutDocumentPane>
                    <avalonDock:LayoutDocument Title="Parameters" ContentId="datagrid"/>
                </avalonDock:LayoutDocumentPane>
                <avalonDock:LayoutAnchorablePane DockWidth="80" DockMinWidth="50">
                    <avalonDock:LayoutAnchorable Title="Actions" ContentId="side"/>
                </avalonDock:LayoutAnchorablePane>
            </avalonDock:LayoutPanel>
            <!-- Lower: Horizontal split -->
            <avalonDock:LayoutPanel Orientation="Horizontal" DockHeight="180">
                <avalonDock:LayoutAnchorablePane DockWidth="180">
                    <avalonDock:LayoutAnchorable Title="Controls" ContentId="controls"/>
                </avalonDock:LayoutAnchorablePane>
                <avalonDock:LayoutAnchorablePane>
                    <avalonDock:LayoutAnchorable Title="Terminal" ContentId="terminal"/>
                </avalonDock:LayoutAnchorablePane>
                <avalonDock:LayoutAnchorablePane>
                    <avalonDock:LayoutAnchorable Title="Alarm" ContentId="alarm"/>
                </avalonDock:LayoutAnchorablePane>
            </avalonDock:LayoutPanel>
        </avalonDock:LayoutPanel>
    </avalonDock:LayoutRoot>
</avalonDock:DockingManager>
```

---

## Project-Specific: Drive Tree Structure

### Tree Node Hierarchy

드라이브 트리는 Online/Offline 루트 하위에 연결된 드라이브와 파라미터 노드로 구성된다.

```
Online (Root)
  └── {DriveName} (Drive, ModelName={driveType})
        ├── Drive               (DriveRoot — 메인 파라미터)
        ├── ECAT Homing         (v1.06+, !DCT)
        ├── Motor               (모터 설정)
        ├── PID Tuning          (PID 게인 튜닝)
        ├── Tuningless          (v1.20+, !DCT)
        ├── Resonant Suppression
        ├── Vibration Suppression (v1.20+, !DCT)
        ├── Bode Plot           (v1.06+, !DCT)
        ├── Encoders
        ├── Digital Inputs
        ├── Digital Outputs
        ├── Monitor             (MDM 실시간 모니터)
        ├── Oscilloscope        (UI-only, CSV 없음)
        ├── Faults
        ├── Fully Closed System (v2.10+, !DCT)
        │     ├── Load Side AqB Scale
        │     └── Load Side BiSS Scale
        ├── ServiceInfo
        ├── Control Panel
        ├── Group               (파라미터 그룹)
        │     ├── Group 0 : Basic
        │     ├── Group 1 : Gain
        │     ├── Group 2 : Velocity
        │     ├── Group 3 : Position
        │     ├── Group 4 : Current
        │     └── Group 5 : Auxiliary
        ├── ECAT Objects        (v2.05+, !DCT)
        │     ├── PDOMapping
        │     ├── Object 1~3, 5, 6
        │     └── Online (EngMode VIRTUAL, CSV 없음)
        └── ECAT OP Mode        (v2.05+, !DCT)
              ├── CSP / CSV / CST / HM / PP

Offline (Root)
  └── (오프라인 추가 드라이브 — 동일 노드 구조)
```

### 등록 드라이브 목록 (12종)

| DriveType | 대표 Product Code | 비고 |
|-----------|------------------|------|
| CSD7A | 300, 301, 310, 1300 | |
| CSD7AB | 1301, 1310, 1600 | |
| CSD7AS | 510, 2000 | |
| CSD7N | 305, 315 | 기준 드라이브 |
| CSD7NB | 1305, 1315, 1700 | |
| CSD7NI | 307, 317, 2300 | |
| CSD7NS | 515, 2100 | |
| CSD7Y | 302, 312, 1500 | |
| D8LMS | 600, 610, 2200 | |
| D8Q | 420, 2400 | |
| LMMT | 0, 210, 306 | DCT 모드, LMMT Setting 트리 추가 |
| RMD | 400 | |

### 트리 아이콘 매핑

| NodeKey | Icon 의미 | Path Data 용도 |
|---------|----------|---------------|
| Online | 링크 연결 | 체인 아이콘 |
| Offline | 링크 해제 | 끊어진 체인 |
| Drive | 기어 | 톱니바퀴 |
| DriveRoot | 속성 | 리스트 아이콘 |
| Motor | 설정 | 기어+렌치 |
| Tuning | 차트 | 라인 그래프 |
| DigitalIO | 스위치 | I/O 스위치 |
| AnalogIO | 파형 | Area 차트 |
| Fault | 경고 | 느낌표 삼각형 |
| Encoder | 다이얼 | 로터리 다이얼 |
| Info | 정보 | i 원 |
| BodePlot | 스플라인 | 곡선 차트 |
| Blackbox | 데이터 | 필드 입력 |
| Homing | 홈 | 집 아이콘 |
| EtherCAT | 글로브 | 지구본 |
| Node | 메뉴 | 햄버거 |
| Group | 리스트 | 불릿 리스트 |

### 트리 노드 선택 → 모듈 뷰 매핑

| NodeKey | 표시할 뷰 | 설명 |
|---------|----------|------|
| Monitor | MonitorView | 실시간 MDM 모니터 |
| Oscilloscope | OscilloscopeView | 오실로스코프 |
| Bode Plot | BodePlotView | 보드 플롯 |
| Blackbox | BlackboxView | 블랙박스 데이터 |
| Control Panel | ControlPanelView | 컨트롤 패널 |
| Faults | FaultListView | 결함 목록 |
| Group (하위) | GroupView | 그룹별 파라미터 |
| Service Info | ServiceInfoView | 서비스 정보 |
| (기타 모든 노드) | ParameterGridView | 파라미터 그리드 (기본) |

---

## Project-Specific: Menu & Ribbon Structure

### 메뉴 바 탭 (Tab-style)

기존 Shell 기준 4개 탭 + Figma 추가 탭:

| Tab | 기존 Shell | Figma 추가 |
|-----|-----------|-----------|
| Home (홈) | O | File → Home 통합 |
| Tools (도구) | O | O |
| View (보기) | O | Views + Floating Tools 통합 |
| Help (도움말) | O | — |
| Parameters | — | O (별도 탭으로 분리 가능) |
| Recording | — | O (오실로스코프 녹화) |

### Home 탭 — 리본 그룹

#### Drive 그룹 (드라이브)
| Button | Command | Size | 설명 |
|--------|---------|------|------|
| New Drive | NewDriveCommand | Large | 새 드라이브 추가 (타입 선택 팝업) |
| Open | OpenFileCommand | Large | 파일 열기 |
| Save | SaveFileCommand | Large | 저장 |
| Save As | SaveAsCommand | Large | 다른 이름 저장 |
| Close | CloseFileCommand | Large | 닫기 |
| Recent Files | OpenRecentFileCommand | Sub-menu | 최근 파일 4개 MRU |

#### Connection 그룹 (연결)
| Button | Command | 설명 |
|--------|---------|------|
| Port (ComboBox) | SelectedPort 바인딩 | 시리얼 포트 드롭다운 (EditWidth=140) |
| Refresh | RefreshPortsCommand | 포트 새로고침 |
| Connect | ConnectCommand | 연결 |
| Disconnect | DisconnectCommand | 연결 해제 |

#### Drive Control 그룹 (드라이브 제어)
| Button | Command | 설명 |
|--------|---------|------|
| Enable (Servo ON) | EnableDriveCommand | 서보 ON |
| E-Stop | EStopCommand | 비상 정지 |
| Reset | ResetDriveCommand | 리셋 |
| Auto Setup | DriveAutoSetupCommand | 오토 셋업 (모달 윈도우) |

#### Fault 그룹 (결함)
| Button | Command | 설명 |
|--------|---------|------|
| Clear Fault | ClearFaultCommand | 결함 해제 |
| Clear All | ClearAllFaultsCommand | 전체 결함 해제 |
| Factory Reset | FactoryResetCommand | 공장 초기화 |

#### Parameter 그룹 (파라미터)
| Button | Command | 설명 |
|--------|---------|------|
| Save Param | SaveParamCommand | 현재 파라미터 Flash 저장 |
| Save All | SaveAllParamCommand | 전체 파라미터 저장 |
| Batch Read | BatchReadCommand | 전체 일괄 읽기 |
| Undo | UndoCommand | 실행 취소 |
| Redo | RedoCommand | 다시 실행 |

### Tools 탭 — 리본 그룹

#### Diagnostic 그룹 (진단)
| Button | Command | 설명 |
|--------|---------|------|
| Control Panel | OpenControlPanelCommand | 컨트롤 패널 (비모달 싱글턴) |
| View Monitors | OpenChartPopupCommand | 모니터 차트 팝업 (비모달 싱글턴) |
| Drive Report | GenerateReportCommand | 드라이브 보고서 생성 |
| Gain Scheduling | GainSchedulingCommand | 게인 스케줄링 (모달) |

#### Database 그룹 (데이터베이스)
| Button | Command | 설명 |
|--------|---------|------|
| Motor DB | MotorDatabaseCommand | 모터 데이터베이스 |
| Import/Export | ImportExportCommand | 파라미터 가져오기/내보내기 |

#### Firmware 그룹 (펌웨어)
| Button | Command | 설명 |
|--------|---------|------|
| FW Upgrade | FirmwareUpgradeCommand | 펌웨어 업그레이드 |
| Batch Download | BatchDownloadCommand | 일괄 다운로드 |

#### Settings 그룹 (설정)
| Button | Command | 설명 |
|--------|---------|------|
| Serial Port | SerialPortSettingsCommand | 시리얼 포트 설정 (모달) |
| App Settings | AppSettingsCommand | 앱 설정 (모달) |

### View 탭 — 리본 그룹

#### Panels 그룹 (패널 표시/숨김, CheckItem)
| Toggle | Binding | 설명 |
|--------|---------|------|
| Drive Tree | IsDriveTreeVisible | 좌측 드라이브 트리 |
| Properties | IsPropertiesVisible | 우측 속성 패널 |
| Host Command | IsHostCmdVisible | 하단 호스트 명령 |
| Comm Log | IsCommLogVisible | 하단 통신 로그 |
| Error Log | IsErrorLogVisible | 하단 오류 로그 |

#### Language 그룹 (언어, 라디오 그룹)
- 한국어 (ko), English (en), 中文 (zh), 日本語 (ja)

#### Theme 그룹 (테마, 라디오 그룹)
- Win11 System, Win11 Light, Win11 Dark, Office 2019, VS 2019 Dark

### Help 탭

| Button | Command | 설명 |
|--------|---------|------|
| About | AboutCommand | 정보 (모달) |
| Release Notes | ReleaseNotesCommand | 릴리즈 노트 (모달) |

### 리본 우측 상태 표시 (Status Icons)

Figma에서 정의된 리본 우측 상태 아이콘:

| 아이콘 | 상태 | 색상 |
|--------|------|------|
| STOP | 비상정지 | AccentRed |
| Srv ON | 서보 ON | AccentGreen |
| Srv OFF | 서보 OFF | TextMuted |
| Comm | 통신 연결 | AccentBlue (연결) / TextMuted (미연결) |

---

## Project-Specific: Status Bar

```
[●Connected] | [Drive | Motor | Encoder | Tune | ServiceInfo] | "Drive Setup and Motion Activity"
```

| 영역 | 위치 | 내용 |
|------|------|------|
| 연결 상태 | Left | 녹색/빨간색 원 + "Connected"/"Disconnected" |
| 탭 네비게이션 | Center | Drive / Motor / Encoder / Tune / ServiceInfo |
| 정보 텍스트 | Right | 현재 모드 / 상태 텍스트 |
| Fault 표시 | Right | HasFault=true 시 빨간 원 + "Fault" 레이블 |

**상태 색상:**
- Connected: `#4CAF50` (AccentGreen)
- Disconnected: `#F44336` (AccentRed)
- Fault: `#FF5722`

---

## Project-Specific: Bottom Panels

### Host Command (하단 좌측)
- 수동 명령 입력 TextBox + 전송 버튼
- 명령 히스토리 리스트
- ASCII 프로토콜 명령 직접 전송 (SET, CHP, STR, MDM 등)

### Comm Log (하단 중앙, Terminal 스타일)
- 모노스페이스 폰트, AccentGreen 색상
- TX/RX 패킷 실시간 로그
- 자동 스크롤, 필터 기능

### Error Log (하단)
- 타임스탬프 + 아이콘(Error/Warning/Info) + 메시지
- 알람 유형별 색상 구분

---

## Project-Specific: Modal Windows

| 윈도우 | 호출 | 모달 여부 |
|--------|------|----------|
| AddDriveWindow | Home > New Drive | 모달 (드라이브 타입 12종 선택) |
| SerialPortSettingsWindow | Tools > Serial Port | 모달 |
| AppSettingsWindow | Tools > App Settings | 모달 |
| DriveAutoSetupWindow | Home > Auto Setup | 모달 |
| GainSchedulingWindow | Tools > Gain Scheduling | 모달 |
| AboutWindow | Help > About | 모달 |
| ReleaseNotesWindow | Help > Release Notes | 모달 |
| ChartPopupWindow | Tools > View Monitors | 비모달 싱글턴 |
| ControlPanelWindow | Tools > Control Panel | 비모달 싱글턴 |

---

## Project-Specific: Module List (9개)

| # | Module | 설명 | 트리 노드 매핑 |
|---|--------|------|---------------|
| 1 | ParameterGrid | 파라미터 그리드 (기본 뷰) | 대부분 노드 |
| 2 | Monitor | 실시간 MDM 모니터 | Monitor |
| 3 | Oscilloscope | 오실로스코프 | Oscilloscope |
| 4 | HostCommand | 호스트 명령/통신/에러 로그 | 하단 패널 3개 |
| 5 | ControlPanel | 컨트롤 패널 | Control Panel |
| 6 | MotorDatabase | 모터 데이터베이스 | 리본 > Motor DB |
| 7 | BodePlot | 보드 플롯 | Bode Plot |
| 8 | Blackbox | 블랙박스 | Blackbox |
| 9 | FirmwareUpgrade | FW 업그레이드 + Import/Export | 리본 > FW Upgrade |

---

## Important Rules

1. **DevExpress 사용 금지** — 모든 UI는 기본 WPF + AvalonDock으로 구현
2. **Dark Theme 기본** — 위 Color Palette 기준, Light Theme은 별도 요청 시만
3. **CornerRadius = 3px** — 모든 패널, 버튼, 입력 컨트롤
4. **아이콘은 Path Data** — Segoe Fluent Icons 또는 Geometry 경로 데이터 사용
5. **Grid Header = DarkRedHighlight** (#6B1A1A) — 테이블 헤더 배경색 통일
6. **Selection = Blue** (#264F78) — 행 선택, 트리 항목 선택 색상 통일
7. **Close 버튼 = Red hover** (#E81123) — 닫기 버튼만 빨간 호버
8. **Panel pop-out** — AvalonDock 기본 Float 기능 활용, 커스텀 FloatingWindow 불필요
9. **폰트 크기 일관성** — 컨텐츠 11px, 헤더/라벨 9-10px, 상태바 9px
10. **Figma 원본**: `https://www.figma.com/make/tu0ak6umVAkDImUgzHpCTg/Create-WPF-Main-Design`
11. **트리 노드 = DriveParameters CSV 기반** — 12개 드라이브 타입별 노드 동적 생성
12. **메뉴/리본 = RSWare.Shell 기존 구조** — Home(4그룹), Tools(4그룹), View(3그룹), Help(1그룹)
13. **다국어 지원** — ResourceDictionary 기반 ko/en/zh/ja 4개 언어
