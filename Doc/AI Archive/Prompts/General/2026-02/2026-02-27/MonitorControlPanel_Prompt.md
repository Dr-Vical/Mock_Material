# Monitor & Control Panel 구현 프롬프트

## 목표
WPF Material Design 앱에서 AvalonDock 플로팅 도킹 패널로 **Monitor & Control** 화면 구현.
좌측=8채널 실시간 차트 + 컨트롤 버튼, 우측=Favorites 리스트.

---

## 1. 구조

### 레이아웃 (MonitorControlDialog — UserControl)
```
[좌측: Chart + Controls]  [GridSplitter]  [우측: Favorites]
```
- **좌측 상단**: 8채널 ScottPlot 실시간 차트 + 채널 토글 CheckBox + Start/Stop 버튼
- **좌측 하단**: 컨트롤 패널 (Enable/Disable/Jog+/Jog-/ClearFault/Reset/TgtPos+/TgtPos-/ZeroSet/MoveZero + 입력필드)
- **우측**: Favorites 리스트 (드래그 순서변경, Del키 삭제, 값 편집 가능)
- **GridSplitter**: 좌/우 폭 조절 가능 (MinWidth=150)

### AvalonDock 배치
- `LayoutAnchorablePane`에 `LayoutAnchorable`로 등록
- `CanFloat=True`, `CanAutoHide=True`, `CanClose=True`, `IsVisible=False` (초기 숨김)
- 센터 패널 오른쪽에 도킹 (DockWidth=500)

### 트리뷰 버튼
- DriveTreeView 하단에 Oscilloscope / Control Panel 2개 버튼 (A/B/C/D 토글 위)
- 각 버튼은 독립적으로 해당 섹션 토글 (`ToggleMonitorSectionMessage("Oscilloscope"/"ControlPanel")`)

---

## 2. 핵심 구현 사항

### 8채널 ScottPlot 차트
- 채널: Continuous Current, Target Current, Velocity, Target Velocity, Current Position, Current Position 2, Error, Inposition
- `ChartCH1Brush`~`ChartCH8Brush` 토큰 사용 (3개 Colors.xaml 모두 등록 필수)
- `GetThemeColor("리소스키")` 헬퍼로 색상 읽기 (FromHex 절대 금지)
- DispatcherTimer 30ms 간격, 자동 시작

### Favorites 연동
- **VM의 `ObservableCollection<Parameter> FavoriteParameters`에 직접 바인딩** (로컬 컬렉션 사용 금지)
- `DataContextChanged`에서 VM의 FavoriteParameters를 ListBox.ItemsSource에 할당
- `CollectionChanged` 이벤트로 카운트 텍스트 업데이트
- 전체삭제: Star 버튼 → `ShowClearFavoritesConfirmMessage` → MessageBox 확인 → `ExecuteClearAllFavorites()`

### Favorites 기능
- **드래그 순서변경**: PreviewMouseLeftButtonDown/Move/Drop/DragOver + `ObservableCollection.Move()`
- **Del키 삭제**: ListBox KeyDown 핸들러, 메인 파라미터의 IsFavorite도 연동 해제
- **값 편집**: TextBox (UpdateSourceTrigger=LostFocus), Value를 `[ObservableProperty]`로 선언 필수

---

## 3. 반드시 지켜야 할 WPF 함정 회피

### ⚠️ DataGrid 내부 Star 토글 (가장 중요)
**절대 ToggleButton 사용 금지.** DataGrid는 셀 내부 ToggleButton의 IsChecked 바인딩을 제대로 갱신하지 않음.

**올바른 구현:**
```xml
<Button Click="StarToggle_Click" Focusable="False"
        Background="Transparent" BorderThickness="0" Cursor="Hand" Padding="2">
    <Grid>
        <materialDesign:PackIcon Kind="StarOutline" Width="14" Height="14"
                                 Foreground="{DynamicResource TextDisabled}"
                                 Visibility="{Binding IsFavorite, Converter={StaticResource InverseBoolToVis}}" />
        <materialDesign:PackIcon Kind="Star" Width="14" Height="14"
                                 Foreground="{DynamicResource WarningBrush}"
                                 Visibility="{Binding IsFavorite, Converter={StaticResource BoolToVis}}" />
    </Grid>
</Button>
```

**핵심 원리:**
- `Button` + `Focusable="False"` → DataGrid 셀 선택 간섭 없이 Click 즉시 발생
- 두 개의 PackIcon을 `Visibility` 바인딩으로 토글 → PropertyChanged에 확실히 반응
- **실패한 시도들 (반복 금지):**
  1. ❌ ToggleButton + IsChecked 바인딩 → DataGrid가 첫 클릭 가로챔
  2. ❌ ToggleButton + PreviewMouseLeftButtonDown + e.Handled → 프로퍼티는 변경되나 비주얼 미갱신
  3. ❌ ToggleButton + PreviewMouseLeftButtonDown + tb.IsChecked 명시 설정 → 여전히 미갱신
  4. ❌ Button + DataTrigger (Style.Triggers) → DataGrid 셀 내부 DataTrigger 갱신 불안정
  5. ✅ Button + 두 아이콘 Visibility 바인딩 → 확실한 해결

### ⚠️ AvalonDock DataContext 전파
LayoutAnchorable 내부 UserControl은 DataContext가 자동 상속 안 될 수 있음.
```csharp
// MainWindow.Loaded에서 명시적 할당
if (monitorControlPane.Content is MonitorControlDialog monitor)
    monitor.DataContext = DataContext;
```

### ⚠️ FtNumber 표시
- "Ft-1.01" → "1.01" 표시: Parameter에 `ShortNumber` 프로퍼티 추가
- `public string ShortNumber => FtNumber.Replace("Ft-", "");`
- DataGrid/Favorites 바인딩: `{Binding ShortNumber}`

### ⚠️ Favorites +1/-1 애니메이션
- DriveTreeView에 Canvas 오버레이 (Panel.ZIndex=100)
- `FavoriteAnimationMessage(bool IsAdded)` 메시지로 트리거
- Favorites TreeViewItem 찾기 → TransformToAncestor로 위치 계산
- +1은 위로 float, -1은 아래로 float (TranslateY + Opacity 애니메이션)

### ⚠️ Value 편집 가능하려면
Parameter.Value를 `[ObservableProperty]`로 선언해야 양방향 바인딩 작동:
```csharp
[ObservableProperty]
private string _value = "";
```

---

## 4. 메시지 체계

| 메시지 | 용도 |
|--------|------|
| `ToggleMonitorSectionMessage(string Section)` | "Oscilloscope" or "ControlPanel" 독립 토글 |
| `ShowClearFavoritesConfirmMessage` | Favorites 전체삭제 확인 다이얼로그 |
| `FavoriteAnimationMessage(bool IsAdded)` | 트리 +1/-1 플로팅 애니메이션 |
| `ComparePanelChangedMessage` | A/B/C/D 패널 토글 |

---

## 5. 파일 목록

| 파일 | 역할 |
|------|------|
| `Views/MonitorControlDialog.xaml` | UserControl — 차트+컨트롤+Favorites 레이아웃 |
| `Views/MonitorControlDialog.xaml.cs` | 차트 설정, 타이머, 섹션 토글, Favorites 드래그/삭제 |
| `Views/CompareParameterPanel.xaml` | DataGrid Star 컬럼 (Button+Visibility 방식) |
| `Views/CompareParameterPanel.xaml.cs` | StarToggle_Click 핸들러 |
| `Views/DriveTreeView.xaml` | Oscilloscope/ControlPanel 버튼 + 애니메이션 Canvas |
| `Views/DriveTreeView.xaml.cs` | 버튼 클릭 → ToggleMonitorSectionMessage + 애니메이션 |
| `MainWindow.xaml` | AvalonDock LayoutAnchorable 등록 |
| `MainWindow.xaml.cs` | ToggleMonitorSection, ShowClearFavoritesConfirm 핸들러 |
| `ViewModels/MainWindowViewModel.cs` | FavoriteParameters, ClearAllFavorites, 메시지 정의 |
| `Models/Parameter.cs` | ShortNumber, [ObservableProperty] Value/IsFavorite |
| `Themes/DarkColors.xaml` | ChartCH1~CH8Brush |
| `Themes/GrayColors.xaml` | ChartCH1~CH8Brush |
| `Themes/LightColors.xaml` | ChartCH1~CH8Brush |
| `Themes/Styles.xaml` | Size.Col.FtNum=45 |

---

## 6. 스타일 딕셔너리 필수 준수

- 모든 색상: `{DynamicResource BrushName}` (하드코딩 절대 금지)
- ScottPlot: `GetThemeColor("리소스키")` 헬퍼 패턴
- Ribbon: `RibbonLargeRipple`, `RibbonInlineLabel`, `RibbonDarkComboBox` 등 스타일 필수
- 5-role 색상 제약: Primary/Secondary/Surface/Background/Error 만 사용
