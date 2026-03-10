# Chart & Control Panel Technical Specification

> 다른 프로그램에서 동일한 기능을 구현하기 위한 기술 사양서
> Design/styling 제외, 기능/로직/구조만 기술

---

## 1. Oscilloscope (Real-time Chart)

### 1.1 라이브러리
- **ScottPlot 5.x** (WPF: `ScottPlot.WPF.WpfPlot`)
- Signal 타입: `ScottPlot.Plottables.Signal`

### 1.2 채널 구성

| 항목 | 값 |
|------|-----|
| 최대 채널 수 | 4 (팔레트는 8색 지원) |
| 기본 채널명 | Motor Feedback Position, Master Position, Velocity Feedback, Velocity Command |
| 채널 변경 | ComboBox에서 런타임 변경 가능 |

### 1.3 데이터 구조

```csharp
private const int PointCount = 500;        // 버퍼 크기
private const double PeriodMs = 0.1;       // 0.1ms/sample = 10kHz 샘플레이트
private readonly double[][] _channelData;  // double[4][500]
private int _phase;                        // 위상 카운터 (GeneratePoint에서 사용)
private readonly Random _rng = new(42);    // 시드 고정 난수
```

### 1.4 신호 생성 (시뮬레이션)

```csharp
private double GeneratePoint(int channel)
{
    double noise = _rng.NextDouble() - 0.5;
    double p = _phase * 0.04;  // 위상 → 라디안 변환 계수
    return channel switch
    {
        0 => Math.Sin(p * 0.5) * 10000 + p * 10 + noise * 50,         // Position Feedback
        1 => Math.Sin(p * 0.5) * 10000 + p * 10 + noise * 20 + 150,   // Position Command (offset +150)
        2 => Math.Cos(p) * 3500 + Math.Cos(p * 2.3) * 500 + noise * 30,      // Velocity Feedback
        3 => Math.Cos(p) * 3500 + Math.Cos(p * 2.3) * 500 + noise * 15 + 80, // Velocity Command (offset +80)
        _ => 0
    };
}
```

**신호 특성:**
- CH0/CH1 (Position): 저주파 정현파 (p*0.5) × 10000 + 선형 드리프트 (p*10)
- CH2/CH3 (Velocity): 이중 주파수 합성 (cos(p) + cos(p*2.3)) × 3500/500
- 노이즈: 채널별 다른 크기 (20~50)
- Command vs Feedback: 오프셋으로 차이 표현

### 1.5 실시간 업데이트 메커니즘

```
Timer Interval: 50ms (MonitorControlDialog) / 30ms (OscilloscopeDialog)
Shifts per tick: 3 (Monitor) / 5 (Oscilloscope)
```

**Shift-and-Append 패턴:**
```csharp
private void OnTimerTick(object? sender, EventArgs e)
{
    for (int s = 0; s < 3; s++)  // 틱당 3회 시프트
    {
        _phase++;
        for (int ch = 0; ch < 4; ch++)
        {
            // 왼쪽으로 1칸 시프트 (가장 오래된 데이터 제거)
            Array.Copy(_channelData[ch], 1, _channelData[ch], 0, PointCount - 1);
            // 새 데이터 추가
            _channelData[ch][PointCount - 1] = GeneratePoint(ch);
        }
    }
    MonitorPlot.Refresh();  // ScottPlot 화면 갱신
}
```

**가시 시간 범위:** 500 points × 0.1ms = 50ms

### 1.6 축 시스템

#### X축
- Label: `"Time (ms)"`
- 범위: 0 ~ (PointCount × PeriodMs) 동적 조정

#### Y축 (다중 축 - 카테고리 그룹핑)
```csharp
private static string GetChannelCategory(string name)
{
    string[] keywords = ["Position", "Velocity", "Current", "Angle", "Voltage", "Power"];
    foreach (var kw in keywords)
        if (name.Contains(kw, StringComparison.OrdinalIgnoreCase))
            return kw;
    return name;
}
```

- 채널명에서 카테고리 자동 감지
- 같은 카테고리 → 같은 Y축 공유
- 그룹 0: 기본 좌측 축 (`plot.Axes.Left`)
- 그룹 1~N: 추가 좌측 축 (`plot.Axes.AddLeftAxis()`)
- 결과: 1~4개 Y축이 카테고리별로 동적 생성

**스케일 자동 조정:**
```csharp
private void AutoScaleGroup(int groupIndex)
{
    double min = double.MaxValue, max = double.MinValue;
    foreach (int ch in _groupChannels[groupIndex])
    {
        if (_signals[ch] is null || !_signals[ch]!.IsVisible) continue;
        for (int i = 0; i < PointCount; i++)
        {
            min = Math.Min(min, _channelData[ch][i]);
            max = Math.Max(max, _channelData[ch][i]);
        }
    }
    if (min >= max) return;
    var range = max - min;
    if (range < 1) range = 1;
    min -= range * 0.1;  // 10% 마진
    max += range * 0.1;
    _groupYAxes[groupIndex]!.Range.Set(min, max);
}
```

**수동 스케일:**
```csharp
private void ApplyScaleFromTextBoxes(int groupIndex)
{
    if (!double.TryParse(maxBox.Text, out double max)) return;
    if (!double.TryParse(minBox.Text, out double min)) return;
    if (min >= max) return;
    _groupYAxes[groupIndex]!.Range.Set(min, max);
}
```

### 1.7 차트 설정 (Setup)

```csharp
private void SetupChart()
{
    var plot = MonitorPlot.Plot;

    // 배경색 (테마 딕셔너리에서 읽기)
    plot.FigureBackground.Color = GetThemeColor("BackgroundBrush");
    plot.DataBackground.Color = GetThemeColor("SurfaceBrush");
    plot.Grid.MajorLineColor = GetThemeColor("SurfaceVariantBrush");

    // 범례
    plot.Legend.IsVisible = true;
    plot.Legend.Alignment = Alignment.UpperRight;
    plot.Legend.BackgroundColor = GetThemeColor("SurfaceVariantBrush");
    plot.Legend.FontColor = GetThemeColor("TextPrimary");
    plot.Legend.OutlineColor = GetThemeColor("BorderDefault");

    // X축 설정
    plot.Axes.Bottom.Label.Text = "Time (ms)";

    // 채널별 Signal 등록
    for (int ch = 0; ch < 4; ch++)
    {
        var signal = plot.Add.Signal(_channelData[ch], 1.0 / PeriodMs);
        signal.LegendText = _channelNames[ch];
        signal.Color = GetChannelColor(ch);
        signal.LineWidth = 1.5f;
        _signals[ch] = signal;
    }
}
```

### 1.8 채널 색상 시스템

**테마 토큰 키:**
```
ChartCH1Brush ~ ChartCH8Brush
```

**색상 변환 헬퍼:**
```csharp
private static ScottPlot.Color GetThemeColor(string resourceKey)
{
    if (Application.Current.TryFindResource(resourceKey) is SolidColorBrush brush)
    {
        var c = brush.Color;
        return new ScottPlot.Color(c.R, c.G, c.B, c.A);
    }
    return ScottPlot.Colors.Gray;
}
```

**런타임 색상 변경 (팔레트):**
- 16색 팔레트 (8 bright + 8 pastel)
- 채널 색상 클릭 → Popup으로 팔레트 표시 → 선택 시 Signal.Color 업데이트
- 변경된 색상은 .ini 파일에 저장

### 1.9 수집 모드

| 모드 | 동작 |
|------|------|
| **Single** | 타이머 시작 → 30틱(≈900ms) 후 자동 정지, ProgressBar 표시 |
| **Continuous** | 타이머 시작 → Stop 버튼까지 계속 실행 |
| **Stop** | 타이머 정지, _isRunning = false |

### 1.10 설정 저장/복원

```csharp
public class ScopeChannelSetting
{
    public string Name { get; set; }     // 채널명
    public string Color { get; set; }    // #AARRGGBB
    public bool Enabled { get; set; }    // 채널 표시 여부
    public double ScaleMax { get; set; } // Y축 최대
    public double ScaleMin { get; set; } // Y축 최소
}
```
- INI 형식으로 저장 (4개 채널 섹션)
- BtnLoadSetting / BtnSaveSetting으로 트리거

---

## 2. Control Panel

### 2.1 상태값 (시뮬레이션 변수)

```csharp
private const double MaxVelocity = 3900;  // rpm 최대값
private const double MaxLoad = 300;       // % 최대값

private double _currentPosition;          // mm 단위
private double _currentVelocity;          // rpm 단위
private double _minPosition = -1000;      // 이동 범위 최소
private double _maxPosition = 3000;       // 이동 범위 최대
private double _jogSpeed = 50;            // 틱당 이동량 (position units)

private bool _isEnabled;                  // 서보 ON/OFF
private bool _isJogging;                  // 조그 진행 중
private int _jogDirection;                // -1 또는 +1
```

### 2.2 타이머 & 업데이트

```csharp
_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
```

**OnTimerTick 로직:**
```csharp
if (_isJogging)
{
    // 조그 중: 큰 이동
    _currentPosition += _jogDirection * _jogSpeed;  // 방향 × 속도
    _currentPosition = Math.Clamp(_currentPosition, _minPosition, _maxPosition);
    _currentVelocity = _jogDirection * 800 + (_rng.NextDouble() - 0.5) * 50;
}
else
{
    // 서보 ON + 대기: 미세 흔들림
    _currentPosition += (_rng.NextDouble() - 0.5) * 2;
    _currentPosition = Math.Clamp(_currentPosition, _minPosition, _maxPosition);
    _currentVelocity *= 0.9;  // 지수 감쇠 (0으로 수렴)
    _currentVelocity += (_rng.NextDouble() - 0.5) * 10;
}
```

### 2.3 Enable/Disable 시퀀스

```
Enable 클릭:
  1. _isEnabled = true
  2. _timer.Start()  ← 타이머 시작 = 시뮬레이션 시작
  3. 상태 표시: "● Enabled" (녹색)
  4. 테두리 애니메이션 시작

Disable 클릭:
  1. _isEnabled = false, _isJogging = false
  2. _timer.Stop()
  3. _currentVelocity = 0
  4. 상태 표시: "● Disabled" (빨간색)
  5. 테두리 애니메이션 정지
```

### 2.4 Enable 테두리 애니메이션

```csharp
// LinearGradientBrush + RotateTransform으로 회전하는 그라데이션 테두리
var brush = new LinearGradientBrush
{
    StartPoint = new Point(0, 0),
    EndPoint = new Point(1, 1),
    GradientStops = new GradientStopCollection
    {
        new(Colors.Transparent, 0.0),
        new(lightGreen, 0.25),          // alpha=140 (반투명)
        new(lightGreen, 0.5),
        new(Colors.Transparent, 0.75),
        new(Colors.Transparent, 1.0)
    },
    RelativeTransform = new RotateTransform(0, 0.5, 0.5)
};

// 6초에 360도 회전 (무한 반복)
var animation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(6))
{
    RepeatBehavior = RepeatBehavior.Forever
};
rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
```

### 2.5 Jog 제어 (Press & Hold)

```
MouseLeftButtonDown → _isJogging = true, _jogDirection = ±1
MouseLeftButtonUp   → _isJogging = false, _jogDirection = 0
```
- 버튼 누르는 동안만 이동 (PreviewMouseLeftButtonDown/Up 사용)
- 타이머 틱마다 _jogSpeed(50) × _jogDirection만큼 이동
- Enable 상태에서만 동작

### 2.6 모션 버튼 (Move ABS / Move Rel / Move Zero)

**Target Position 하이라이트 애니메이션:**
```csharp
private static void FlashTextBoxBorder(TextBox textBox, Color highlightColor, Color normalColor)
{
    var brush = new SolidColorBrush(normalColor);
    textBox.BorderBrush = brush;
    textBox.BorderThickness = new Thickness(2);

    var animation = new ColorAnimationUsingKeyFrames();
    animation.KeyFrames.Add(new LinearColorKeyFrame(highlightColor, TimeSpan.FromMilliseconds(200)));   // 0→200ms: 페이드인
    animation.KeyFrames.Add(new LinearColorKeyFrame(highlightColor, TimeSpan.FromMilliseconds(800)));   // 200→800ms: 유지
    animation.KeyFrames.Add(new LinearColorKeyFrame(normalColor, TimeSpan.FromMilliseconds(1200)));     // 800→1200ms: 페이드아웃

    animation.Completed += (_, _) =>
    {
        textBox.BorderThickness = new Thickness(1);  // 원래 두께 복원
        textBox.BorderBrush = new SolidColorBrush(normalColor);
    };

    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
}
```

**왕복 모드:**
- `ChkReciprocate` 체크됨 → Move ABS/Rel 클릭 시 TxtTgtPosA + TxtTgtPosB 모두 하이라이트
- 체크 안됨 → TxtTgtPosA만 하이라이트
- Move Zero → 즉시 position = 0

### 2.7 확인 다이얼로그 (ConfirmActionDialog)

```csharp
ConfirmActionDialog.Ask(
    owner: Window,
    title: string,           // "Zero Set" / "Clear Fault" / "Reset"
    message: string,         // 설명 메시지
    icon: PackIconKind,      // MaterialDesign 아이콘
    confirmText: string,     // 확인 버튼 텍스트
    confirmBrushKey: string  // "WarningBrush" / "ErrorBrush" → 테두리+버튼 색상
) → bool
```

| 동작 | 확인 브러시 |
|------|------------|
| Zero Set | WarningBrush (노란색) |
| Clear Fault | WarningBrush (노란색) |
| Reset | WarningBrush (노란색) |

### 2.8 Velocity Arc Gauge (원형 게이지)

```csharp
private void DrawVelocityArc(double velocity)
{
    // Canvas (48×48) 위에 매 틱마다 다시 그림
    double cx = 24, cy = 24;
    double radius = 20, thickness = 5;

    // 1. 배경 트랙 (전체 원)
    Ellipse(radius*2, stroke=BackgroundBrush, thickness=5)

    // 2. 값 아크 (비율만큼)
    double ratio = Math.Clamp(|velocity| / MaxVelocity, 0, 0.999);
    if (ratio > 0.01)
    {
        // 12시 방향(-90°)에서 시작, 시계방향으로 ratio×360° 그리기
        startAngle = -90°
        sweepAngle = ratio × 360°

        PathFigure + ArcSegment 사용
        isLargeArc = sweepAngle > 180°
        SweepDirection = Clockwise

        // 색상 임계값
        색상 = ratio > 0.8 → ErrorBrush (빨강)
             ratio > 0.5 → WarningBrush (노랑)
             else        → SecondaryBrush (기본)
    }

    // 3. 중앙 텍스트: 속도값(rpm) + "rpm" 단위
    TextBlock(|velocity|.ToString("F0"), FontSize=9, 중앙)
    TextBlock("rpm", FontSize=7, 중앙 아래)
}
```

### 2.9 상태 모니터 카드 3종

#### Position Card
```
표시값: _currentPosition.ToString("N2") + " mm"
Progress Bar: (_currentPosition - min) / (max - min) 비율로 너비 조절
범위 표시: "{min:N0} ~ {max:N0} mm"
```

#### Velocity Card
```
표시값: _currentVelocity.ToString("N0") + " rpm"
최대값: MaxVelocity(3900) rpm
Arc Gauge: DrawVelocityArc() 실시간 갱신
```

#### Load Card
```
계산: loadPercent = |velocity / MaxVelocity| × 100
표시값: loadPercent.ToString("F0") + " %"
막대: loadPercent / MaxLoad(300) 비율로 높이 조절 (최대 40px)
색상 임계값:
  > 100% → ErrorBrush (빨강)
  > 60%  → WarningBrush (노랑)
  else   → TextPrimary (기본)
```

### 2.10 입력 필드

| 필드 | 기본값 | 용도 |
|------|--------|------|
| Target Speed | 500 | 목표 속도 설정 |
| Target Position A | -500 | 이동 목표 위치 A (왕복 시작점) |
| Target Position B | 2000 | 이동 목표 위치 B (왕복 끝점) |
| Current Limit | 100 | 전류 제한값 |

---

## 3. 공통 패턴

### 3.1 테마 색상 읽기

```csharp
// WPF Brush 반환 (XAML 바인딩용)
private static Brush GetWpfBrush(string key)
{
    return Application.Current.TryFindResource(key) is Brush brush ? brush : Brushes.Gray;
}

// System.Windows.Media.Color 반환 (애니메이션용)
private static Color GetThemeColor(string key)
{
    return Application.Current.TryFindResource(key) is SolidColorBrush brush ? brush.Color : Colors.Gray;
}

// ScottPlot.Color 반환 (차트용)
private static ScottPlot.Color GetScottPlotColor(string resourceKey)
{
    if (Application.Current.TryFindResource(resourceKey) is SolidColorBrush brush)
    {
        var c = brush.Color;
        return new ScottPlot.Color(c.R, c.G, c.B, c.A);
    }
    return ScottPlot.Colors.Gray;
}
```

### 3.2 채널 색상 토큰

```
ChartCH1Brush: Yellow  (#FFFFEB3B) — Position 계열
ChartCH2Brush: Green   (#FF4CAF50) — Velocity 계열
ChartCH3Brush: Cyan    (#FF29B6F6) — Current 계열
ChartCH4Brush: Red     (#FFEF5350) — Error/Load 계열
ChartCH5Brush: Orange  (#FFFF9800)
ChartCH6Brush: Purple  (#FFAB47BC)
ChartCH7Brush: Teal    (#FF26A69A)
ChartCH8Brush: Pink    (#FFEC407A)
```

### 3.3 런타임 특성 요약

| 항목 | 값 |
|------|-----|
| 샘플 레이트 | 10 kHz (0.1ms/point) |
| 버퍼 크기 | 500 points = 50ms 시간 범위 |
| Chart 타이머 | 50ms (Monitor) / 30ms (Oscilloscope) |
| Control Panel 타이머 | 50ms |
| 틱당 시프트 | 3 (Monitor) / 5 (Oscilloscope) |
| Y축 수 | 1~4 (카테고리 자동 그룹핑) |
| 채널 수 | 4 활성 / 8 팔레트 |
| Arc Gauge 갱신 | 매 타이머 틱 (50ms) |
| 하이라이트 애니메이션 | 1200ms (200ms 페이드인 + 600ms 유지 + 400ms 페이드아웃) |
| Enable 테두리 회전 | 6초/360° (무한 반복) |

### 3.4 WeakReferenceMessenger 메시지

| 메시지 | 용도 |
|--------|------|
| `ToggleMonitorSectionMessage(Section)` | "Oscilloscope" 또는 "ControlPanel" 섹션 독립 토글 |
| `ShowClearFavoritesConfirmMessage` | Favorites 전체삭제 확인 |
| `FavoriteAnimationMessage(IsAdded)` | 트리 Favorites 노드에 +1/-1 플로팅 숫자 애니메이션 |

---

## 4. 구현 시 체크리스트

- [ ] ScottPlot 5.x NuGet 설치 (`ScottPlot.WPF`)
- [ ] 500-point double 배열 4개 할당
- [ ] Shift-and-Append 패턴으로 실시간 데이터 갱신
- [ ] DispatcherTimer 50ms 간격
- [ ] 카테고리별 Y축 자동 그룹핑
- [ ] Auto Scale: 10% 마진 포함
- [ ] Arc Gauge: PathGeometry + ArcSegment (12시 시작, 시계방향)
- [ ] Arc 색상 임계값: 50% → Warning, 80% → Error
- [ ] Jog: PreviewMouseLeftButtonDown/Up으로 press-and-hold 구현
- [ ] Position Clamp: min/max 범위 제한
- [ ] Velocity 감쇠: idle 시 ×0.9 지수 감쇠
- [ ] 확인 다이얼로그: severity별 테두리 색상
- [ ] 채널 색상 변경: 팔레트 Popup + INI 저장
- [ ] 스케일 수동 입력: TextBox → double.TryParse → Range.Set
