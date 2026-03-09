# Monitor & Control Panel 구현 프롬프트

## 목표
WPF Material Design 앱에서 AvalonDock 도킹 패널로 **Monitor & Control** 화면 구현.
좌측=8채널 실시간 차트 + 컨트롤 버튼, 우측=Favorites 리스트.

---

## 1. 구조

### 레이아웃 (MonitorControlDialog — UserControl)
```
[좌측: Chart + Controls]  [GridSplitter]  [우측: Favorites]
```
- **좌측 상단**: 8채널 ScottPlot 실시간 차트 + 채널 토글 CheckBox + Start/Stop + Auto Scale 버튼
- **좌측 하단**: 컨트롤 패널 (Enable/Disable/Jog+/Jog-/ClearFault/Reset/TgtPos+/TgtPos-/ZeroSet/MoveZero + 입력필드)
- **우측**: Favorites 리스트 (드래그 순서변경, Del키 삭제, 값 편집 가능)
- **GridSplitter**: 좌/우 폭 조절 가능 (MinWidth=150)

### AvalonDock 배치
- 메인 `LayoutPanel` 안의 `LayoutAnchorablePane`에 `LayoutAnchorable`로 등록
- `CanFloat=True`, `CanAutoHide=True`, `CanClose=True`, `DockWidth=500`
- **초기 숨김**: `Loaded` 이벤트에서 `monitorControlPane.Hide()` 호출 (XAML 속성으로 안됨)
- **⚠️ `LayoutRoot.RightSide` (auto-hide) 사용 금지** — Show() 호출 시 내용이 제대로 표시되지 않음

### 트리뷰 버튼
- DriveTreeView 하단에 Oscilloscope / Control Panel 2개 버튼 (A/B/C/D 토글 위)
- 각 버튼은 독립적으로 해당 섹션 토글 (`ToggleMonitorSectionMessage("Oscilloscope"/"ControlPanel")`)

### 섹션 토글 동작 규칙
- **Oscilloscope 버튼 1번째 클릭**: 패널 열림 + 차트 영역 표시 (정지 상태, Start 버튼)
- **Start 클릭**: 그래프 실시간 시작, Stop으로 변경
- **Oscilloscope 버튼 2번째 클릭**: 차트 영역 숨김 + 타이머 정지
- **Control Panel 열려 있으면**: 스코프만 닫고 패널은 유지
- **양쪽 모두 닫히면**: 패널 전체 Hide()
```csharp
// MainWindow.xaml.cs — 양쪽 섹션 모두 닫힘 확인
if (!monitor.IsChartVisible && !monitor.IsControlPanelVisible)
    monitorControlPane.Hide();
```

---

## 2. 핵심 구현 사항

### 8채널 ScottPlot 차트
- 채널: Continuous Current, Target Current, Velocity, Target Velocity, Current Position, Current Position 2, Error, Inposition
- `ChartCH1Brush`~`ChartCH8Brush` 토큰 사용 (3개 Colors.xaml 모두 등록 필수)
- `GetThemeColor("리소스키")` 헬퍼로 색상 읽기 (FromHex 절대 금지)
- DispatcherTimer 30ms 간격
- **⚠️ 자동 시작 금지** — 생성자에서 타이머 시작하지 않음. 사용자가 Start 클릭 시 시작
- **⚠️ 초기 Visibility**: ChartToolbar, MonitorPlot, ChartStatusBar, ControlPanel 모두 `Collapsed`
- Auto Scale 버튼: `Plot.Axes.AutoScale()` + `Refresh()`

### Favorites 연동
- **VM의 `ObservableCollection<Parameter> FavoriteParameters`에 직접 바인딩** (로컬 컬렉션 사용 금지)
- `DataContextChanged`에서 VM의 FavoriteParameters를 ListBox.ItemsSource에 할당
- `CollectionChanged` 이벤트로 카운트 텍스트 업데이트
- 전체삭제: Star 버튼 → `ShowClearFavoritesConfirmMessage` → MessageBox 확인 → `ExecuteClearAllFavorites()`

### Favorites 기능
- **드래그 순서변경**: PreviewMouseLeftButtonDown/Move/Drop/DragOver + `ObservableCollection.Move()`
- **Del키 삭제**: ListBox KeyDown 핸들러, 메인 파라미터의 IsFavorite도 연동 해제
- **값 편집**: TextBox (UpdateSourceTrigger=LostFocus), Value를 `[ObservableProperty]`로 선언 필수

### Value Min/Max 검증
- `OnParameterPropertyChanged`에서 Value 변경 감지 → `ClampValueIfNeeded()` 호출
- Min 미만 → Min으로 보정, Max 초과 → Max로 보정
- 보정 시 하단 Error Log (`StatusEntries`)에 `[Warning]` 항목 추가
```csharp
private void ClampValueIfNeeded(Parameter param)
{
    if (!double.TryParse(param.Value, out double val)) return;
    bool hasMin = double.TryParse(param.Min, out double min);
    bool hasMax = double.TryParse(param.Max, out double max);
    if (hasMin && val < min)
    {
        param.Value = min.ToString();
        StatusEntries.Add(new StatusEntry { ... "Value clamped to Min" ... });
    }
    else if (hasMax && val > max)
    {
        param.Value = max.ToString();
        StatusEntries.Add(new StatusEntry { ... "Value clamped to Max" ... });
    }
}
```

---

## 3. 반드시 지켜야 할 WPF 함정 회피

### ⚠️ DataGrid 내부 Star 토글 (가장 중요)
**`DataGridCheckBoxColumn` 사용.** WPF 내장 싱글클릭 체크박스 컬럼이 유일한 해결책.
커스텀 CheckBox 템플릿으로 별 아이콘 표시.

**올바른 구현:**
```xml
<DataGridCheckBoxColumn Header="★" Width="30"
                        Binding="{Binding IsFavorite, UpdateSourceTrigger=PropertyChanged}">
    <DataGridCheckBoxColumn.ElementStyle>
        <Style TargetType="CheckBox">
            <Setter Property="HorizontalAlignment" Value="Center" />
            <Setter Property="Cursor" Value="Hand" />
            <Setter Property="FocusVisualStyle" Value="{x:Null}" />
            <Setter Property="Validation.ErrorTemplate" Value="{x:Null}" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="CheckBox">
                        <Grid Background="Transparent">
                            <materialDesign:PackIcon x:Name="star" Kind="StarOutline"
                                Width="14" Height="14"
                                Foreground="{DynamicResource TextDisabled}"
                                HorizontalAlignment="Center" VerticalAlignment="Center" />
                        </Grid>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsChecked" Value="True">
                                <Setter TargetName="star" Property="Kind" Value="Star" />
                                <Setter TargetName="star" Property="Foreground"
                                        Value="{DynamicResource WarningBrush}" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </DataGridCheckBoxColumn.ElementStyle>
    <!-- EditingElementStyle도 동일한 템플릿 적용 필수 -->
</DataGridCheckBoxColumn>
```

**핵심 원리:**
- `DataGridCheckBoxColumn`은 WPF가 싱글클릭 토글을 보장하는 유일한 컬럼 타입
- DataGrid 레벨의 `IsReadOnly` 제거, 개별 컬럼에 `IsReadOnly="True"` 설정 (VALUE 컬럼은 제외)
- `FocusVisualStyle="{x:Null}"` + `Validation.ErrorTemplate="{x:Null}"` — **빨간 테두리 제거 필수**
- ElementStyle + EditingElementStyle **양쪽 모두** 동일 템플릿 적용

**실패한 시도들 (반복 금지):**
1. ❌ ToggleButton + IsChecked 바인딩 → DataGrid가 첫 클릭 가로챔
2. ❌ ToggleButton + PreviewMouseLeftButtonDown + e.Handled → 프로퍼티는 변경되나 비주얼 미갱신
3. ❌ ToggleButton + PreviewMouseLeftButtonDown + tb.IsChecked 명시 설정 → 여전히 미갱신
4. ❌ Button + DataTrigger (Style.Triggers) → DataGrid 셀 내부 DataTrigger 갱신 불안정
5. ❌ Button + 두 아이콘 Visibility 바인딩 (BoolToVis/InverseBoolToVis) → Visibility 바인딩 갱신 안됨
6. ❌ DataGrid PreviewMouseLeftButtonDown + 아이콘 직접 Visibility 조작 → 여전히 미갱신
7. ✅ **DataGridCheckBoxColumn + 커스텀 CheckBox 템플릿** → 확실한 해결

### ⚠️ DataGridCheckBoxColumn 빨간 테두리
체크박스 클릭 시 빨간 테두리(Validation Error Adorner)가 나타남.
**반드시** 아래 두 속성을 CheckBox 스타일에 추가:
```xml
<Setter Property="FocusVisualStyle" Value="{x:Null}" />
<Setter Property="Validation.ErrorTemplate" Value="{x:Null}" />
```

### ⚠️ AvalonDock 초기 숨김
- **실패**: `IsVisible="False"` XAML 속성 → 500px 빈 공간 차지
- **실패**: `LayoutRoot.RightSide` auto-hide → `Show()` 호출 시 내용물 표시 안됨
- **성공**: 메인 `LayoutPanel` 내 `LayoutAnchorablePane`에 배치 + `Loaded`에서 `Hide()` 호출
```csharp
Loaded += (_, _) => { monitorControlPane.Hide(); };
```

### ⚠️ AvalonDock DataContext 전파
LayoutAnchorable 내부 UserControl은 DataContext가 자동 상속 안 될 수 있음.
```csharp
// Show() 직후 명시적 할당
monitorControlPane.Show();
if (monitorControlPane.Content is MonitorControlDialog m)
    m.DataContext = DataContext;
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
- DataGrid VALUE 컬럼: `IsReadOnly` 제거 + `UpdateSourceTrigger=LostFocus`

### ⚠️ 마지막 패널 닫기 방지
CompareParameterPanel에 `CanCloseCheck` 콜백 추가:
```csharp
// CompareParameterPanel.xaml.cs
public Func<bool>? CanCloseCheck { get; set; }

private void OnCloseClick(object sender, RoutedEventArgs e)
{
    if (CanCloseCheck != null && !CanCloseCheck())
        return;  // 마지막 패널이면 애니메이션/닫기 차단
    // ... animate ...
}

// MainWindow.xaml.cs — 패널 생성 시
panel.CanCloseCheck = () =>
{
    int count = new[] { vm.IsPanelAVisible, vm.IsPanelBVisible,
                        vm.IsPanelCVisible, vm.IsPanelDVisible }.Count(v => v);
    return count > 1;
};
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
| `Views/MonitorControlDialog.xaml.cs` | 차트 설정, 타이머, 섹션 토글, Favorites 드래그/삭제, Auto Scale |
| `Views/CompareParameterPanel.xaml` | DataGrid Star 컬럼 (DataGridCheckBoxColumn 방식) |
| `Views/CompareParameterPanel.xaml.cs` | CanCloseCheck 콜백, 닫기 애니메이션, 컬럼 너비 |
| `Views/DriveTreeView.xaml` | Oscilloscope/ControlPanel 버튼 + 애니메이션 Canvas |
| `Views/DriveTreeView.xaml.cs` | 버튼 클릭 → ToggleMonitorSectionMessage + 애니메이션 |
| `MainWindow.xaml` | AvalonDock LayoutAnchorable 등록 (LayoutPanel 내부) |
| `MainWindow.xaml.cs` | ToggleMonitorSection, ShowClearFavoritesConfirm, Hide/Show, CanCloseCheck |
| `ViewModels/MainWindowViewModel.cs` | FavoriteParameters, ClearAllFavorites, ClampValueIfNeeded, 메시지 정의 |
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
