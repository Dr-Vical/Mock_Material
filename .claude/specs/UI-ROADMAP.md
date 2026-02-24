# UI Mockup Development Roadmap — RswareDesign

> Last updated: 2026-02-24 | Focus: UI 완벽 구성 (백엔드는 이후 단계)

## Current State (완료)

| View | 상태 | 설명 |
|------|------|------|
| MainWindow.xaml | Done | Ribbon 5탭 + Quick Actions + StatusBar + AvalonDock 3분할 |
| DriveTreeView | Done | 2단 트리 (Online/Offline), 아이콘 바인딩 |
| ParameterEditorView | Done | 7열 DataGrid, 반응형 컬럼, 폰트 스케일링 |
| ActionPanelView | Done | 6버튼 (Read/Write/Save/Compare/Export/Revert) |
| ErrorLogView | Done | 3열 상태 DataGrid |
| OscilloscopeDialog | Done | ScottPlot 4채널, 실시간 애니메이션 |
| AdminPasswordDialog | Done | 비밀번호 다이얼로그 |
| Theme System | Done | Dark/Light 전환, Hue 기반 색상, 폰트 스케일링 |

---

## Architecture: 핵심 변경 사항

### 1. 트리 선택 → 센터 화면 전환 (Navigation)

```
트리 노드 선택
  → TreeNodeSelectedMessage 발행
    → MainWindow 수신
      → AvalonDock DocumentPane 내 탭 전환 또는 콘텐츠 교체
      → ActionPanel 버튼 세트 변경
```

### 2. ViewModel 분리

| ViewModel | View | 역할 |
|-----------|------|------|
| MainWindowViewModel | MainWindow | Shell, 리본, 상태바 |
| DriveTreeViewModel | DriveTreeView | 트리 탐색, 노드 선택 |
| ParameterEditorViewModel | ParameterEditorView | 파라미터 그리드 |
| MonitorViewModel | MonitorView | 실시간 모니터링 |
| OscilloscopeViewModel | OscilloscopeView | 오실로스코프 (도킹) |
| ControlPanelViewModel | ControlPanelView | 드라이브 제어 |
| FaultsViewModel | FaultsView | 폴트 이력 |
| ServiceInfoViewModel | ServiceInfoView | 드라이브 서비스 정보 |
| ActionPanelViewModel | ActionPanelView | 컨텍스트 액션 버튼 |
| ErrorLogViewModel | ErrorLogView | 에러/상태 로그 |

### 3. AvalonDock 탭 구조 (센터 영역)

```
DocumentPane
├── [Parameter Editor] ← 기본 탭 (트리 노드별 파라미터)
├── [Monitor]          ← ScottPlot 실시간 차트
├── [Oscilloscope]     ← ScottPlot 오실로스코프
├── [Control Panel]    ← 서보 On/Off, Jog, E-Stop
├── [Faults]           ← 폴트 이력 리스트
└── [Service Info]     ← 드라이브 서비스 정보
```

---

## Phase 0: 인프라 기반 — Navigation & ViewModel 분리

**목표:** 트리 선택 → 센터 화면 전환 메커니즘 구축

| Task | 파일 | 설명 | 의존성 |
|------|------|------|--------|
| 0-1 | TreeNodeSelectedMessage | 메시지 정의 (NodeId, NodeType, NodeName) | 없음 |
| 0-2 | DriveTreeViewModel | 트리 전용 VM, SelectedNode → 메시지 발행 | 0-1 |
| 0-3 | NavigationService | 센터 DocumentPane 탭 전환/생성/포커스 로직 | 0-1 |
| 0-4 | MainWindow.xaml 개편 | AvalonDock MVVM 바인딩 (DocumentsSource, AnchorablesSource) | 0-3 |
| 0-5 | ViewModel 분리 | MainWindowViewModel → 각 View별 VM 분리 | 0-2 |

**예상 소요:** 4-6시간
**병렬 가능:** 0-1 + 0-2 동시 → 0-3 → 0-4 + 0-5 동시

---

## Phase 1: 코어 화면 보강 (병렬 가능)

### 1A: 리본 메뉴 완성
**목표:** 모든 리본 버튼에 실질적 Command 연결 + 누락 기능 추가

| Task | 설명 |
|------|------|
| 1A-1 | File 탭: New/Open/Save/SaveAs 다이얼로그 연결 (FileDialog) |
| 1A-2 | Tools 탭: ReScan → 실제 COM 포트 스캔 목업, Com Setting 다이얼로그 |
| 1A-3 | Views 탭: 패널 토글 → AvalonDock 패널 Show/Hide 연동 |
| 1A-4 | Help 탭: Release List 다이얼로그 |
| 1A-5 | Admin 탭: 비밀번호 인증 → Motor DB / Param Setting 화면 열기 |

### 1B: 트리 구조 → 센터 연동
**목표:** 트리 노드 선택 시 센터 화면 내용 변경

| Task | 설명 |
|------|------|
| 1B-1 | DriveTreeView에 SelectedItem 바인딩 추가 |
| 1B-2 | 노드 타입별 파라미터 세트 매핑 (Mode Config, Motor, PID Tuning 등) |
| 1B-3 | 센터 DocumentPane 타이틀 자동 업데이트 |
| 1B-4 | ParameterEditorView에 샘플 데이터 노드별 분리 (13개 노드 타입) |

### 1C: 에러 로그 보강
**목표:** 타임스탬프, 심각도 아이콘, 자동 스크롤

| Task | 설명 |
|------|------|
| 1C-1 | ErrorLogEntry 모델 (Timestamp, Severity, Source, Message) |
| 1C-2 | 심각도별 아이콘 + 색상 (Info=Primary, Warning=Warning, Error=Error) |
| 1C-3 | 자동 스크롤 + Clear 버튼 |
| 1C-4 | 에러 발생 시 ErrorLog 패널 자동 표시 |

**Phase 1 예상 소요:** 6-8시간
**병렬:** 1A, 1B, 1C 모두 독립 — 3개 동시 진행 가능

---

## Phase 2: 센터 4종 화면 (병렬 가능)

### 2A: MonitorView — 실시간 모니터링
| 항목 | 내용 |
|------|------|
| 레이아웃 | 2x2 ScottPlot 차트 (Position, Velocity, Current, Torque) |
| 컨트롤 | Start/Stop, 채널 토글, 시간 범위 선택 |
| 데이터 | DataStreamer (1000pt circular buffer) + 목업 생성기 |
| 스타일 | 다크테마 차트 색상, 디자인 토큰 준수 |

### 2B: ControlPanelView — 드라이브 제어
| 항목 | 내용 |
|------|------|
| 레이아웃 | 상단: 상태 표시, 중앙: 제어 버튼, 하단: Jog 컨트롤 |
| 컨트롤 | Servo ON/OFF, E-Stop, Reset, Jog Forward/Reverse |
| 상태 | LED 인디케이터 (Servo Ready, In Motion, Fault) |
| 스타일 | E-Stop은 ErrorBrush, Servo ON은 PrimaryBrush |

### 2C: FaultsView — 폴트 이력
| 항목 | 내용 |
|------|------|
| 레이아웃 | DataGrid (Code, Message, Timestamp, Severity) + 상단 툴바 |
| 컨트롤 | Clear All, Export, Filter by severity |
| 데이터 | 10-15개 샘플 폴트 (Overcurrent, Overvoltage, Encoder Error 등) |
| 스타일 | 심각도별 행 색상 (Error=ErrorBrush, Warning=WarningBrush) |

### 2D: ServiceInfoView — 드라이브 정보
| 항목 | 내용 |
|------|------|
| 레이아웃 | 2열 PropertyGrid 스타일 (Label : Value) |
| 항목 | Drive Name, FW Version, Serial Number, Motor Type, Encoder Type |
| 항목 (추가) | Operating Hours, Boot Count, Last Fault, Production Date |
| 스타일 | MaterialDesign Card + 디자인 토큰 |

**Phase 2 예상 소요:** 8-12시간
**병렬:** 2A, 2B, 2C, 2D 모두 독립 — 4개 동시 진행 가능

---

## Phase 3: 도킹 & 멀티 화면 구조

**목표:** 여러 화면 동시 표시, 플로팅, 탭 전환

| Task | 설명 |
|------|------|
| 3-1 | Oscilloscope를 LayoutDocument 탭으로 전환 (다이얼로그 → 도킹) |
| 3-2 | ControlPanel 플로팅 윈도우 지원 (CanFloat=True) |
| 3-3 | Monitor + Oscilloscope 동시 표시 (수직/수평 분할) |
| 3-4 | BodePlotView 추가 (ScottPlot 주파수 응답) |
| 3-5 | 레이아웃 저장/복원 (XmlLayoutSerializer) |
| 3-6 | Views 탭 토글버튼 → 패널 Show/Hide 완전 연동 |

**예상 소요:** 6-8시간
**의존성:** Phase 0 (Navigation) + Phase 2 (센터 화면들) 완료 후

---

## Phase 4: 액션 패널 컨텍스트 연동

**목표:** 트리/센터 화면에 따라 액션 버튼 변경

| 컨텍스트 | 액션 버튼 |
|----------|----------|
| Parameter Editor | Read All, Write All, Save to Flash, Compare, Export, Revert |
| Monitor | Start, Stop, Channel Setup, Export CSV |
| Oscilloscope | Trigger, Single, Auto, Export Image |
| Control Panel | Servo ON, Servo OFF, Reset Fault, Home |
| Faults | Clear All, Clear Selected, Export |
| Service Info | Refresh, Export |

| Task | 설명 |
|------|------|
| 4-1 | ActionPanelViewModel 생성 — 컨텍스트별 버튼 세트 |
| 4-2 | CenterViewChangedMessage — 센터 탭 전환 시 액션 변경 |
| 4-3 | ActionPanelView XAML — ItemsControl 기반 동적 버튼 렌더링 |
| 4-4 | 각 컨텍스트별 아이콘 + 스타일 적용 |

**예상 소요:** 4-6시간
**의존성:** Phase 0 + Phase 2 완료 후

---

## Phase 5: 서브 화면 & 다이얼로그

### 5A: 경고/확인 다이얼로그
| 다이얼로그 | 트리거 | 설명 |
|-----------|--------|------|
| ConfirmWriteDialog | Write All 버튼 | "N개 파라미터를 드라이브에 기록합니다" |
| ConfirmRevertDialog | Revert 버튼 | "변경사항을 원래값으로 되돌립니다" |
| SaveConfirmDialog | 창 닫기/파일 변경 시 | "저장하지 않은 변경사항이 있습니다" |
| OverwriteConfirmDialog | Save 파일 존재 시 | "기존 파일을 덮어씁니다" |

### 5B: 설정 다이얼로그
| 다이얼로그 | 내용 |
|-----------|------|
| ComSettingDialog | COM Port, Baud Rate, Data Bits, Stop Bits, Parity |
| MotorDbDialog | 모터 데이터베이스 조회/편집 (Admin) |
| ParamSettingDialog | 파라미터 설정 편집 (Admin) |
| ReleaseListDialog | 릴리즈 노트 목록 |
| AppSettingsDialog | 테마, 폰트, 언어, 기본 설정 |

### 5C: 유닛 화면
| 화면 | 설명 |
|------|------|
| UnitConversionView | 단위 변환 (rpm ↔ rad/s, counts ↔ rev 등) |
| UnitSettingsDialog | 표시 단위 설정 (SI/Imperial/Custom) |

**예상 소요:** 8-10시간
**병렬:** 5A, 5B, 5C 독립 진행 가능

---

## Phase 6: 통합 검증 & 폴리시

| Task | 설명 |
|------|------|
| 6-1 | 전체 디자인 토큰 준수 검증 (hardcoded 값 탐색) |
| 6-2 | 모든 화면 Dark/Light 테마 전환 확인 |
| 6-3 | 폰트 스케일링 전체 화면 확인 |
| 6-4 | Hue 슬라이더 색상 전환 확인 |
| 6-5 | 1920x1080 기준 레이아웃 확인 |
| 6-6 | 모든 버튼 hover/pressed/disabled 상태 확인 |
| 6-7 | AvalonDock 드래그/플로팅/도킹 확인 |
| 6-8 | 빌드 0 error / 0 warning 확인 |

**예상 소요:** 4-6시간

---

## Parallel Execution Map

```
Phase 0 (인프라) ──────────────────────┐
                                       │
Phase 1A (리본)    ─┐                  │
Phase 1B (트리연동) ─┤ 병렬 ←──────────┘
Phase 1C (에러로그) ─┘
                     │
                     ▼
Phase 2A (Monitor)     ─┐
Phase 2B (ControlPanel) ─┤ 병렬
Phase 2C (Faults)        ─┤
Phase 2D (ServiceInfo)   ─┘
                          │
              ┌───────────┤
              ▼           ▼
Phase 3 (도킹)    Phase 4 (액션연동) ← 병렬
              │           │
              └─────┬─────┘
                    ▼
Phase 5A (경고)    ─┐
Phase 5B (설정)    ─┤ 병렬
Phase 5C (유닛)    ─┘
                    │
                    ▼
Phase 6 (통합검증) ────── 완료
```

## Total Estimate: 40-56시간 (병렬 활용 시 단축 가능)

---

## Design Rules (모든 Phase 공통)

1. **디자인 토큰 필수** — hardcoded 색상/폰트/간격 절대 금지
2. **5-role 색상 제한** — Primary, Secondary, Surface, Background, Error만 사용
3. **스타일 사전 참조** — Colors.xaml → Fonts.xaml → Styles.xaml → Buttons.xaml
4. **CommunityToolkit.Mvvm** — ObservableObject, [ObservableProperty], [RelayCommand]
5. **메시징** — WeakReferenceMessenger (inter-view communication)
6. **리본 스타일** — RibbonLargeRipple, RibbonIconOpacityPulse 등 사전정의 스타일만
7. **AvalonDock** — CLR namespace, CanClose=False (기본 패널), ContentId 필수
8. **DevExpress 절대 금지**
