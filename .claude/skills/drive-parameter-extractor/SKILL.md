---
name: drive-parameter-extractor
description: >
  Extract drive parameters from C++ source files (Constants.h, Drive.cpp, Drive.h)
  or Excel parameter list documents for industrial servo drive applications,
  then generate structured parameter databases in CSV and Markdown formats.

  **Trigger this skill when the user says any of:**
  - "파라메터 항목 뽑아줘", "파라미터 추출해줘", "파라메터 분석해줘"
  - "parameter extraction", "extract parameters", "parameter analysis"
  - Provides C++ source files or Excel sheets from a servo drive project
  - Asks to build a parameter DB or list from drive source code or docs
  - Asks to compare parameters across multiple drives
---

# Drive Parameter Extractor Skill

Extract servo drive parameters from C++ source code or Excel parameter list
documents and generate structured output files (CSV, Markdown).
All output is in **English**.

---

## Overview

### Source Types

**Type A — C++ Source Files:**
| File | Content |
|---|---|
| `Constants.h` | `#define` macros mapping parameter names to numeric addresses |
| `Drive.cpp` | `Init()` method with `.Define()`, `.SetValue()`, `.SetRangeValidation()` etc. |
| `Drive.h` | Class member declarations showing parameter types |

**Type B — Excel Parameter List (e.g., CSD7 Parameter List xlsx):**
| Sheet | Content |
|---|---|
| `CSD7N Parameter List` | Right-side columns (col12+): Ft-G.NN, Digit, Name, Unit, Attr, Min, Max, Default |
| `MDM List` | Monitor data columns: No., Name, Unit, Remark |
| `CSD7N Object List` | EtherCAT objects (INDEX/ECAT parameters) |

**Type C — ECAT PDOMap Source Files (AddIndex pattern):**
| File | Content |
|---|---|
| `{Drive}PDOMap{N}.cpp` | `AddIndex()` calls defining EtherCAT object dictionary entries (0x1000–0x9FFF) |
| `{Drive}PDOMap{N}NodeMgr.cpp` | Grouping logic (subIndex=0 → named group, sub≥0→-1 transition → unnamed) |

> Object 2 파라미터 추출에 사용. `AddIndex(CONST, subIdx, "ro/rw", "TYPE", "NAME", hexa, default, [FW_VER])`
> FtNo: `CSD7N_PDO_200A_3` → `0x200A:03` (상수명에서 hex index 추출)
> 추출 도구: `tools/extract_object2.py` (Python, regex 기반)

Supported drives (12 types): CSD7A, CSD7AB, CSD7AS, CSD7N, CSD7NB, CSD7NI, CSD7NS, CSD7Y, D8LMS, D8Q, LMMT, RMD

---

## Parameter Number System

### ftx.xx Format

```
Ft-G.NN → SET number = G * 100 + NN
Ft-0.07 → SET 007
Ft-1.02 → SET 102
Ft-9.13 → SET 913
```

- `Ft-G.NN` is the hardware parameter address
- The SET number (3-digit zero-padded) is the protocol command index used with `SET`, `CHP`, `STR`

### Command Types

| Command | Purpose |
|---|---|
| `SET` | Read current value (data confirm) |
| `CHP` | Write to RAM (temporary) |
| `STR` | Write to Flash (permanent save) |
| `PDO` | EtherCAT PDO display-only (serial 직접 접근 불가) |

**Data Attribute → Command mapping:**

| Data Attribute | Applicable Commands |
|---|---|
| `Always` | SET/STR |
| `Servo Off` | SET/STR |
| `Drive Off=>On` | STR (requires power cycle) |
| `Power cycling` | STR |
| `Special` | SET only |

---

## Parameter Categories

### 1. SET Parameters (ftx.xx mapped) — PRIMARY FOCUS

- Plain `#define` integer macros or `stSETSTR` storage type
- Address format: `Ft-G.NN` → SET `G*100+NN`
- Include ALL of these in the main parameter DB

### 2. MDM Parameters — MONITORING DATA

- Monitor Data channel: No. 0–255
- Command = `MDM`
- Read-only, no Min/Max/Default
- Tree = `Monitor`, Group = `MDM`
- **메인 CSV에 합치기 가능**: `_MDM_Parameter.csv`를 별도로 유지하거나, 메인 `_Parameter.csv`에 직접 포함 가능
- 메인 CSV에 합칠 경우: MDM 행(Tree=Monitor, Command=MDM)을 CSV 끝에 추가
- **주의**: MDM Remark에 개행문자가 있으면 CSV 파싱 오류 발생 → 단일 행으로 정리 필수
- CSD7N은 이미 메인 CSV에 합침 완료. 다른 드라이브는 필요 시 순차 진행

### 3. INDEX / ECAT Parameters — OBJECT DICTIONARY

- EtherCAT object dictionary (0x1000–0x9FFF)
- Storage types: `stMDM`, `stMDE`, `stXETXHP`, `stINFO`, `stSTS`, `stSTE`, `stVER`
- In xlsx: `CSD7N Object List` sheet
- FtNo = `0xNNNN` 또는 `0xNNNN:SS` (hex 형식, Ft-G.NN 인코딩 아님)
- Command = `PDO` (display-only, serial 프로토콜로 직접 읽기/쓰기 불가)
- **메인 CSV에 포함**: `ECAT Objects/`, `ECAT OP Mode/` 트리로 계층 구조화
- Tree = `ECAT Objects/{ChildNode}`, `ECAT OP Mode/{ChildNode}`

---

## Column Definitions (14 mandatory)

> **Number = 트리별 UI 표시 순번 (1-based, per-tree reset)**
> - Number는 **각 Tree 내에서** 1부터 시작하는 표시 순번 (Tree가 바뀌면 다시 1부터)
> - 같은 파라미터가 여러 Tree에 중복 등장 가능 — 각 Tree에서 독립 Number 부여
> - composite sub-field(D0/D1/D2/D3)도 각각 별도 순번 부여
> - 하드웨어 통신 주소(SET번호)는 **FtNo** 컬럼에 `Ft-G.NN` 형식으로 기록
> - composite sub-field의 FtNo는 `Ft-G.NN/Dx` 형식 (예: `Ft-0.02/D3`)
> - 최종 UI 표시 순서는 `{Drive}NodeMgr.cpp` AddProperty 호출 순서가 결정 (drive-nodemgr-loader 스킬 참조)
> - NodeMgr에 없는 파라미터(숨김/미사용)는 해당 Tree의 마지막 Number 이후에 배치

| # | Column | Description | Format Example |
|---|---|---|---|
| 1 | **Tree** | 좌측 트리 노드 이름. `{Drive}NodeMgr.cpp` 기준. 계층 구조는 `/` 구분자 사용 (예: `Group/Group 0 : Basic`). 로더가 `/` 앞을 부모 노드, 뒤를 자식 노드로 파싱 | `Drive Root`, `Motor`, `Group/Group 0 : Basic`, `ECAT Objects/PDOMapping`, `Fully Closed System/Load Side AqB Scale` |
| 2 | **Group** | Tree 내 서브그룹 이름. 최상위 프로퍼티는 `(top-level)`, 접힌 그룹은 그룹명 | `(top-level)`, `Velocity Limits`, `Communications` |
| 3 | **FtNo** | Hardware SET address. Five forms: `Ft-G.NN` (regular), `Ft-G.NN/Dx` (composite digit), `Ft-G.NN/Bx` (bit), `0xNNNN` (ECAT hex), `0xNNNN:SS` (ECAT hex+subindex) | `Ft-0.07`, `Ft-0.02/D3`, `0x6040`, `0x1C12:01` |
| 4 | **Number** | Tree 내 표시 순번 (1-based, per-tree reset). Tree가 바뀌면 1부터 재시작. 서브그룹 순서는 이전 그룹에 이어서 연번. | `1`, `2`, `6`, `12` |
| 5 | **Name** | English display name | `Drive Address` |
| 6 | **ValueType** | `enum`, `int`, `float`, `composite`, `string`, `bool` | `int` |
| 7 | **Unit** | Engineering unit | `rpm`, `ms`, `A`, `%`, `-` |
| 8 | **Default** | Default value | `1`, `0x0020` |
| 9 | **Min** | Minimum value | `1` |
| 10 | **Max** | Maximum value | `247` |
| 11 | **Command** | `SET/STR`, `STR`, `SET`, `MDM` | `SET/STR` |
| 12 | **DataAttribute** | Always / Servo Off / Drive Off=>On / Power cycling / Special | `Always` |
| 13 | **Rsware** | RSWare support | `O`, `X` |
| 14 | **Remark** | Enum options ("; " separated) or notes | `0: Disable; 1: Enable` |

### FtNo Key Format

| Situation | FtNo format | Example |
|---|---|---|
| Regular parameter | `Ft-G.NN` | `Ft-0.07` |
| Composite digit sub-field | `Ft-G.NN/Dx` (x=0~3) | `Ft-0.02/D3` |
| Bit sub-field (Touch Probe Ft-3.21) | `Ft-G.NN/Bx` (x=bit pos) | `Ft-3.21/B0` |
| ECAT hex index | `0xNNNN` | `0x6040` |
| ECAT hex + subindex | `0xNNNN:SS` | `0x1C12:01` |

> **Hex FtNo (ECAT Objects/OP Mode):**
> - `0xNNNN` → paramIndex = NNNN (hex→int)
> - `0xNNNN:SS` → paramIndex = mainIndex * 256 + subIndex (서브인덱스 인코딩)
> - Command = `PDO` (display-only, serial 프로토콜로 직접 읽기/쓰기 불가)

> **Note on Touch Probe (Ft-3.21):** In Excel, this is stored as D0~D3 (4-digit composite).
> In C++ NodeMgr, each digit is expanded into individual bits (B0, B1, B4, B5, B8, B9, B12, B13, B14, B15).
> The **CSV uses Excel format** (D0~D3 keys), while NodeMgr order references these as `Ft-3.21/D0`~`Ft-3.21/D3`.

### UI Display Order — NodeMgr Tree Structure

**UI 표시 순서는 `{Drive}NodeMgr.cpp`의 AddProperty 호출 순서로 결정된다.**

각 트리 노드(`{Drive}NodeMgr.cpp`, `{Drive}Group0NodeMgr.cpp`, `{Drive}MotorNodeMgr.cpp` 등)는 **독립적인 프로퍼티 목록**을 가진다.

**동일 파라미터가 여러 트리에 중복 등장할 수 있다.** 예:
- `Ft-0.02/D3` (AC Line Loss Check) → Drive Root(top-level) + Group 0 Basic 양쪽에 존재
- `Ft-0.02/D0` (Fault and Disable Braking) → Drive Root > Stopping Functions + Group 0 Basic 양쪽에 존재

**CSD7N 트리 구조 예시:**
```
Drive Root (CSD7NNodeMgr.cpp)
├── (top-level): Name, AC Line Loss Check(D3), MotorModel, Command Polarity(D2), CurrentBias
├── Velocity Limits: VelLimitMode(D0), ManualVelLimit
├── Acceleration Limits: AccelLimits, Accel, AccelSub, Decel, DecelSub, SCurve
├── Communications: DriveAddress, AliasID, AliasIDEEPROM
├── Current Limits: CurrPosInt, CurrNegInt, CurrPosExt, CurrNegExt, ...
├── Speed Functions: SpeedWindow, UpToSpeed
├── Position Functions: InPositionSize, NearPositionSize, ...
├── Stopping Functions: Overtravel Stop Method(D1), CurrOvertravelLimit, Fault and Disable Braking(D0), ...
├── Auxiliary Function Selection 1: Emergency Stop Input(D3), ...
├── Display Monitoring: EnableDisplayMonitorValue(D3), DisplayNumber
├── RMS Current Load Factor: CumulativeTime, Threshold1, Threshold2
└── Shunt Resistor (v2.00+): ShuntResistor(D0), ExtValue, ExtCapacity

ECAT Homing (CSD7NHomingNodeMgr.cpp) → Number 1부터 재시작
Motor (CSD7NMotorNodeMgr.cpp) → Number 1부터 재시작
PID Tuning (CSD7NTuningNodeMgr.cpp) → Number 1부터 재시작
Tuningless (CSD7NTuninglessNodeMgr.cpp) → Number 1부터 재시작
Resonant Suppression (CSD7NResonantNodeMgr.cpp) → Number 1부터 재시작
Vibration Suppression (CSD7NVibrationNodeMgr.cpp) → Number 1부터 재시작
Bode Plot (CSD7NBodePlotNodeMgr.cpp) → Number 1부터 재시작
Encoders (CSD7NEncoderNodeMgr.cpp) → Number 1부터 재시작
...
Group 0~5 (CSD7NGroup0~5NodeMgr.cpp) → 각각 Number 1부터 재시작
Monitor (ECMonitorNodeMgr) → Number 1부터 재시작
```

**Number 부여 규칙:**
- Tree 내 top-level 프로퍼티: 1, 2, 3, 4, 5
- 첫 번째 서브그룹: 6, 7 (이전에 이어서)
- 두 번째 서브그룹: 8, 9, 10, ... (계속 이어서)
- Tree가 바뀌면: 다시 1부터

See `drive-nodemgr-loader` skill for full ordered list per drive type.

---

## Digit / Boolean Sub-field Rules

### Digit (Dx)

- 4-digit composite values split into 4 positions: D0 (bits 3..0), D1 (bits 7..4), D2 (bits 11..8), D3 (bits 15..12)
- Each digit is independently selectable (0–F range typically)
- In xlsx: column `Digit` shows `D0`/`D1`/`D2`/`D3`, column `Bit` shows `[3..0]`/`[7..4]` etc.
- **Digit order is NOT always D0→D1→D2→D3 sequential.** Some parameters skip positions (e.g., D0→D2). Always use the order as it appears in the Excel sheet.
- **Composite parent rows are NOT output.** Only the sub-field rows (each Dx) are emitted, each as an independent row with its own Number.
- FtNo for sub-field: `Ft-G.NN/Dx`

#### C++ StartPos → Dx 변환 규칙 (CRITICAL)

C++ `ECAttributeField` 생성자의 3번째 인자 `StartPos`는 **문자열 인덱스**(0=왼쪽=천의 자리)이다.
`Dx`는 **자릿수 의미**(D0=일의 자리=오른쪽, D3=천의 자리=왼쪽)이다.

```
Dx = 3 - StartPos

StartPos 0 (leftmost, thousands)  → D3
StartPos 1                        → D2
StartPos 2                        → D1
StartPos 3 (rightmost, ones)      → D0
```

**예시 (SET 2 = Ft-0.02, m_4BasicMode):**
```cpp
ECAttributeField(&m_BrakingMode, &m_4BasicMode, 3, 1);       // StartPos=3 → D0
ECAttributeField(&m_OvertravelStopMethod, &m_4BasicMode, 2, 1); // StartPos=2 → D1
ECAttributeField(&m_MotorDir, &m_4BasicMode, 1, 1);           // StartPos=1 → D2
ECAttributeField(&m_PowerInput, &m_4BasicMode, 0, 1);         // StartPos=0 → D3
```

> **주의**: C++ `StartPos`를 그대로 `Dx`로 사용하면 D0↔D3, D1↔D2가 반전된다. 반드시 `Dx = 3 - StartPos`로 변환할 것.

#### Display Name 추출 규칙 (CRITICAL)

C++ 소스에서 파라미터 이름을 추출할 때:
- **사용**: `SetName(IDS_XXX)` 또는 `SetName("...")` 에 설정된 실제 표시 이름
- **사용 금지**: 멤버 변수명 (`m_PowerInput`), getter 함수명 (`GetPowerInput()`)
- **SetName 우선순위**: Drive.cpp `Init()` → NodeMgr `AddProperty()` 순서. 둘 다 있으면 Drive.cpp가 최종 이름

```
멤버 변수명    getter 함수명        SetName() 표시 이름 (올바른 이름)
m_PowerInput   GetPowerInput()  →   "AC Line Loss Check"
m_MotorDir     GetMotorDir()    →   "Command Polarity"
m_BrakingMode  GetBrakingMode() →   "Fault and Disable Braking"
```

> 멤버 변수명과 표시 이름이 전혀 다른 경우가 많다. 반드시 `SetName()` 값을 사용할 것.

#### Drive.cpp SetName() 오버라이드 (전 드라이브 공통)

`{Drive}Drive.cpp`의 `Init()` 함수에서 `SetName(IDS_xxx)` 호출로 기본 이름을 UI 표시명으로 덮어쓴다.
`IDS_xxx` 매크로 뒤의 주석(`//"실제 문자열"`)이 실제 표시 이름.
12개 전 드라이브에 동일하게 적용되는 공통 패턴:

| SET | Member | Default Name | SetName() UI Display Name |
|-----|--------|-------------|--------------------------|
| 206 | m_Acceleration | Acceleration | Acceleration Time |
| 206 | m_AccelerationProxy | - | Acceleration (VIRTUAL proxy, UI에 표시) |
| (virtual) | m_AccelerationSub | - | Acceleration Time to Max Speed (VIRTUAL) |
| 207 | m_Deceleration | Deceleration | Deceleration Time |
| 207 | m_DecelerationProxy | - | Deceleration (VIRTUAL proxy, UI에 표시) |
| (virtual) | m_DecelerationSub | - | Deceleration Time to Max Speed (VIRTUAL) |
| 407 | m_CurrentPosInternal | Current Pos Internal | Positive Internal Current Limit |
| 408 | m_CurrentNegInternal | Current Neg Internal | Negative Internal Current Limit |
| 409 | m_CurrentPosExternal | Current Pos External | Positive External Current Limit |
| 410 | m_CurrentNegExternal | Current Neg External | Negative External Current Limit |
| 411 | m_CurrentOvertravelLimit | Current Overtravel Limit | Maximum Stopping Current |
| 412 | m_CurrentBias | Current Bias | Initial Current Bias |
| 218 | m_SpeedWindow | Speed Window | Velocity Window |
| 219 | m_UpToSpeed | Up To Speed | Up to Velocity |
| 205 | m_VelocityCommand | Velocity Command | Jog Velocity Command |
| 503 | m_BrakingSpeed | Braking Speed | Braking Application Velocity |
| 423 | m_RMSCumulativeTime | Cumulative Time | Current Feedback Squared Integrator Interval |
| 424 | m_RMSThreshold1 | Threshold 1 | RMS Current Feedback Threshold 1 |
| 425 | m_RMSThreshold2 | Threshold 2 | RMS Current Feedback Threshold 2 |
| 511 | m_PowerSagWarningEnable | Power Sag Warning Enable | Power Sag Warning |
| 513 | m_PowerSagCurrentReleaseTime | Power Sag Current Release Time | Power Sag Current Limit Release Time |
| 524 | m_ExternalShuntResistorValue | External Shunt Resistor Value | External Shunt Resistance |
| 525 | m_ExternalShuntResistorCapacity | External Shunt Resistor Capacity | External Shunt Resistor Power Rate |

> **VIRTUAL proxy/sub**: AccelerationProxy, DecelerationProxy는 UI에 표시되는 대리 엔트리.
> AccelerationSub, DecelerationSub는 VIRTUAL이며 자체 SET 번호 없음. FtNo는 `(virtual)`로 표기.

### B-field (Bx) — Touch Probe Only

- Ft-3.21 (Touch Probe) uses both D0~D3 composite format in Excel AND B0~B15 bit format in C++ NodeMgr
- **In CSV (Excel source):** stored as `Ft-3.21/D0`~`Ft-3.21/D3`
- **In C++ NodeMgr:** referenced as individual bits B0, B1, B4, B5, B8, B9, B12, B13, B14, B15

### Format in CSV

```csv
# Drive Root tree — Number는 이 트리 내에서 1부터 시작, 서브그룹은 이어서 연번
Tree,Group,FtNo,Number,Name,ValueType,Unit,Default,Min,Max,Command,DataAttribute,Rsware,Remark
Drive Root,(top-level),Ft-9.51,1,Drive Name,string,-,,,,SET/STR,Always,O,
Drive Root,(top-level),Ft-0.02/D3,2,AC Line Loss Check,enum,-,0,0,2,SET/STR,Servo Off,O,0: Enable; 1: Disable
Drive Root,(top-level),Ft-9.50,3,Motor Model,string,-,,,,STR,Power cycling,O,
Drive Root,(top-level),Ft-0.02/D2,4,Command Polarity,enum,-,0,0,1,SET/STR,Servo Off,O,0: Normal; 1: Inverted
Drive Root,(top-level),Ft-4.12,5,Initial Current Bias,int,-,0,,,SET/STR,Always,O,
Drive Root,Velocity Limits,Ft-2.17/D0,6,Velocity Limit Mode,enum,-,0,,,SET/STR,Always,O,
Drive Root,Velocity Limits,Ft-2.16,7,Manual Velocity Limit,int,rpm,,,,SET/STR,Always,O,
Drive Root,Acceleration Limits,Ft-0.09,8,Acceleration Limits,enum,-,0,,,SET/STR,Always,O,
...

# Group 0 Basic tree — 별도 트리이므로 Number 다시 1부터
# NodeMgr 순서: BrakingMode(StartPos=3→D0), OvertravelStopMethod(2→D1), MotorDir(1→D2), PowerInput(0→D3)
Group 0 Basic,(top-level),Ft-0.02/D0,1,Fault and Disable Braking,enum,-,0,0,3,SET/STR,Servo Off,O,0: Brake and Hold
Group 0 Basic,(top-level),Ft-0.02/D1,2,Overtravel Stop Method,enum,-,2,0,2,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.02/D2,3,Command Polarity,enum,-,0,0,1,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.02/D3,4,AC Line Loss Check,enum,-,0,0,2,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.03/D0,5,Off-Line Auto Tuning Mode,enum,-,0,,,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.03/D2,6,Off-Line Auto Tuning Velocity,enum,-,0,,,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.04,7,Inertia Ratio,int,%,,,,SET/STR,Always,O,
...
```

**Key rules:**
- **Number = Tree 내 표시 순번** — 각 Tree에서 1부터 시작, Tree가 바뀌면 리셋
- **서브그룹 순번은 이전에 이어서**: top-level 1~5 → 첫 서브그룹 6~7 → 다음 서브그룹 8~ ...
- **동일 파라미터 중복 가능**: 같은 FtNo가 여러 Tree에 등장할 수 있음 (각각 독립 Number)
- **FtNo = hardware SET address** (`Ft-G.NN`); composite sub-fields use `Ft-G.NN/Dx` format
- Composite sub-fields each get their own Number; parent composite row is NOT output
- Digit order follows NodeMgr AddProperty 순서 — NOT always D0→D1→D2→D3
- CSV는 Tree별로 정렬, Tree 내에서는 Number 오름차순

---

## Tree & Group Assignment — NodeMgr 기준

### Tree = 좌측 트리 노드

Tree 이름은 `{Drive}NodeMgr.cpp` 에서 `InsertItem()` + `new {Drive}XxxNodeMgr()` 로 생성되는 트리 노드명이다.
각 Tree는 독립적인 프로퍼티 목록을 가지며, Number는 Tree마다 1부터 재시작한다.

**CSD7N 기준 Tree 목록 (InsertItem 순서, IDS_ 상수 기준):**

| # | Tree (IDS_ 상수명) | Source File | 조건 |
|---|---|---|---|
| 1 | `Drive` (루트, 드라이브명) | `CSD7NNodeMgr.cpp` | 무조건 |
| 2 | `ECAT Homing` | `CSD7NHomingNodeMgr.cpp` | v1.06+ && !DCT |
| 3 | `Motor` | `CSD7NMotorNodeMgr.cpp` | 무조건 |
| | ↳ `Linear Motor Setup` | `CSD7NLinearNodeMgr.cpp` | v1.20+ && !DCT |
| | ↳ `Custom Rotary Motor Setup` | `CSD7NRotaryNodeMgr.cpp` | v1.20+ && !DCT |
| 4 | `PID Tuning` | `CSD7NTuningNodeMgr.cpp` | 무조건 |
| 5 | `Tuningless` | `CSD7NTuninglessNodeMgr.cpp` | v1.20+ && !DCT |
| 6 | `Resonant Suppression` | `CSD7NResonantNodeMgr.cpp` | 무조건 |
| 7 | `Vibration Suppression` | `CSD7NVibrationNodeMgr.cpp` | v1.20+ && !DCT |
| 8 | `Bode Plot` | `CSD7NBodePlotNodeMgr.cpp` | v1.06+ && !DCT |
| 9 | `Encoders` | `CSD7NEncoderNodeMgr.cpp` | 무조건 |
| 10 | `Digital Inputs` | `CSD7NDigInNodeMgr.cpp` | 무조건 |
| 11 | `Digital Outputs` | `CSD7NDigOutNodeMgr.cpp` | 무조건 |
| 12 | `Monitor` | `ECMonitorNodeMgr.cpp` | 무조건 |
| 13 | `Oscilloscope` | `CSD7NScopeNodeMgr.cpp` | 무조건, **UI-only** (CSV 파라미터 없음) |
| 14 | `Faults` | `CSD7NFaultsNodeMgr.cpp` | 무조건 |
| 15 | `Fully Closed System` | `CSD7NFullyClosedNodeMgr.cpp` | v2.10+ && !DCT |
| 16 | `ServiceInfo` | `CSD7NServiceInfoNodeMgr.cpp` | 무조건 |
| 17 | `Control Panel` | `CSD7NControlPanelNodeMgr.cpp` | 무조건 |
| 18 | `Group` (sub: 0~5) | `CSD7NGroupModeNodeMgr.cpp` | 무조건 |
| 19 | `ECAT Objects` | `CSD7NPDOMapModeNodeMgr.cpp` | v2.05+ && !DCT (sub: PDOMapping, Object 1~3,5,6, Online) |
| 20 | `ECAT OP Mode` | `CSD7NOperationModeNodeMgr.cpp` | v2.05+ && !DCT (sub: CSP, CSV, CST, HM, PP) |

> **Tree 이름은 `IDS_xxx` 문자열 리소스 상수값이 정확한 이름이다.**
> 예: `IDS_PID_TUNING` = "PID Tuning" (Tuning 아님), `IDS_ECAT_HOMING` = "ECAT Homing" (Homing 아님)

### Tree Hierarchy Convention (CSV `/` 구분자)

C++ RSWare에서 일부 트리 노드는 부모-자식 계층 구조를 가진다.
CSV의 Tree 컬럼에서 `/` 구분자로 표현하며, `CsvDriveParameterLoader.GetOrCreateNode()`가 자동 파싱한다.

```
CSV Tree 컬럼                        → UI 트리 구조
─────────────────────────────────     ──────────────
Group/Group 0 : Basic                 Group
Group/Group 1 : Gain                    ├── Group 0 : Basic
Group/Group 2 : Velocity                ├── Group 1 : Gain
Group/Group 3 : Position                ├── Group 2 : Velocity
Group/Group 4 : Current                 ├── Group 3 : Position
Group/Group 5 : Auxiliary               ├── Group 4 : Current
                                        └── Group 5 : Auxiliary

Fully Closed System/Load Side AqB    Fully Closed System
Fully Closed System/Load Side BiSS     ├── Load Side AqB Scale
                                        └── Load Side BiSS Scale

ECAT Objects/PDOMapping               ECAT Objects
ECAT Objects/Object 1                   ├── PDOMapping
ECAT Objects/Object 2                   ├── Object 1
ECAT Objects/Object 3                   ├── Object 2 (477 params, AddIndex)
ECAT Objects/Object 5                   ├── Object 3
ECAT Objects/Object 6                   ├── Object 5
                                        └── Object 6

ECAT OP Mode/CSP                      ECAT OP Mode
ECAT OP Mode/CSV                        ├── CSP
ECAT OP Mode/CST                        ├── CSV
ECAT OP Mode/HM                         ├── CST
ECAT OP Mode/PP                         ├── HM
                                        └── PP
```

**규칙:**
- `/` 앞 = 부모 노드 (NodeDefinition.Nodes에 등록)
- `/` 뒤 = 자식 노드 (부모의 NodeDefinition.Children에 등록)
- 부모 노드 자체에는 파라미터가 없을 수 있음 (빈 컨테이너)
- 한 단계 계층만 지원 (Parent/Child). 손자 노드 불가
- 계층 없는 노드 (Drive Root, Motor 등)는 `/` 없이 기존대로 최상위에 배치

### Group = Tree 내 서브그룹

Group은 Tree 내에서 `ECPropertyGroup` 으로 묶인 접힌(collapsed) 프로퍼티 그룹이다.

| Group | 의미 |
|---|---|
| `(top-level)` | Tree의 최상위 프로퍼티 (그룹에 속하지 않는 항목) |
| `Velocity Limits` | 접힌 서브그룹: 속도 제한 |
| `Acceleration Limits` | 접힌 서브그룹: 가감속 제한 |
| `Communications` | 접힌 서브그룹: 통신 |
| `Current Limits` | 접힌 서브그룹: 전류 제한 |
| `Stopping Functions` | 접힌 서브그룹: 정지 기능 |
| 기타... | NodeMgr.cpp의 `SetName()` 값 사용 |

> **서브그룹이 없는 Tree** (예: Group 0 Basic, Motor 등)는 Group을 `(top-level)`로 통일한다.

---

## Value Type Inference Rules

| Condition | Type |
|---|---|
| Has `Digit = Dx` sub-fields | `composite` |
| Has enum options listed (col30+) | `enum` |
| Name contains "label", "drive name", "scaling label" | `string` |
| Name contains "scaling data", "resistance", "inductance", "constant", "capacitance", "thermal", "mass" | `float` |
| Default value is float literal | `float` |
| Everything else | `int` |

---

## Excel Sheet Parsing (Type B)

### CSD7N / CSD7A Parameter List Sheet

Right-side columns (col12 onwards, 0-indexed):

| Col | Content |
|---|---|
| 12 | Group label (Group0, Group1…) |
| 13 | Rsware support (O/X) |
| 14 | Built-In support (O/X) |
| 15 | Para. No' = Ft-G.NN |
| 16 | Digit (D0/D1/D2/D3) |
| 17 | Bit ([3..0] etc.) |
| 18 | Parameter Name (English) — Ft-row use |
| 20 | Sub-parameter Name (English) — Digit sub-row use |
| 22 | Unit |
| 23 | Data Attribute |
| 24 | Min |
| 25 | Max |
| 26 | Default |
| 28 | Remark |
| 30–41 | Enum option strings (e.g., "0: Disable", "1: Enable") |

**Important:** col20 holds sub-field English name on Digit rows; col18 holds parent name on Ft rows.

### MDM List Sheet

| Col | Content |
|---|---|
| 0 | No. (integer) |
| 1 | Name |
| 2 | Unit |
| 3 | Remark |

---

## Output Files

### SET Parameters: `{DriveName}_Parameter.csv`

```csv
Tree,Group,FtNo,Number,Name,ValueType,Unit,Default,Min,Max,Command,DataAttribute,Rsware,Remark
Drive Root,(top-level),Ft-9.51,1,Drive Name,string,-,,,,SET/STR,Always,O,
Drive Root,(top-level),Ft-0.02/D3,2,AC Line Loss Check,enum,-,0,0,2,SET/STR,Servo Off,O,0: Enable; 1: Disable
Drive Root,(top-level),Ft-9.50,3,Motor Model,string,-,,,,STR,Power cycling,O,
Drive Root,(top-level),Ft-0.02/D2,4,Command Polarity,enum,-,0,0,1,SET/STR,Servo Off,O,
Drive Root,(top-level),Ft-4.12,5,Initial Current Bias,int,-,0,,,SET/STR,Always,O,
Drive Root,Velocity Limits,Ft-2.17/D0,6,Velocity Limit Mode,enum,-,0,,,SET/STR,Always,O,
Drive Root,Velocity Limits,Ft-2.16,7,Manual Velocity Limit,int,rpm,,,,SET/STR,Always,O,
...
Group 0 Basic,(top-level),Ft-0.02/D0,1,Fault and Disable Braking,enum,-,0,0,3,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.02/D1,2,Overtravel Stop Method,enum,-,2,0,2,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.02/D2,3,Command Polarity,enum,-,0,0,1,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.02/D3,4,AC Line Loss Check,enum,-,0,0,2,SET/STR,Servo Off,O,
Group 0 Basic,(top-level),Ft-0.03/D0,5,Off-Line Auto Tuning Mode,enum,-,0,,,SET/STR,Servo Off,O,
...
Motor,(top-level),Ft-9.50,1,Motor Model,string,-,,,,STR,Power cycling,O,
Motor,(top-level),Ft-9.01,2,Motor Resistance,float,ohm,,,,SET/STR,Always,O,
...

# 계층 트리 (Group 0~5는 Group/ 하위)
Group/Group 0 : Basic,(top-level),Ft-0.02/D0,1,Fault and Disable Braking,enum,-,0,0,3,SET/STR,Servo Off,O,0: Brake and Hold
Group/Group 0 : Basic,(top-level),Ft-0.02/D1,2,Overtravel Stop Method,enum,-,2,0,2,SET/STR,Servo Off,O,
...
Group/Group 1 : Gain,(top-level),Ft-1.00,1,Velocity Loop Gain,int,-,,,,SET/STR,Always,O,
...

# Digital Outputs (composite SET 파라미터)
Digital Outputs,(top-level),Ft-0.22/D3,1,Within In-Position Window(/P-COM),enum,-,0,0,7,SET/STR,Always,O,0: Unassigned; 1: Output 1; ...
Digital Outputs,(top-level),Ft-0.22/D2,2,Up to Velocity(/TG-ON),enum,-,0,0,7,SET/STR,Always,O,...
...

# ECAT Objects (hex FtNo, PDO command)
ECAT Objects/PDOMapping,(top-level),0x1C12:01,1,RxPDO 1st Assign,int,-,,,,PDO,Always,O,
ECAT Objects/PDOMapping,(top-level),0x1C12:02,2,RxPDO 2nd Assign,int,-,,,,PDO,Always,O,
...
ECAT Objects/Object 1,(top-level),0x6040,1,Control Word,int,-,,,,PDO,Always,O,
ECAT Objects/Object 1,(top-level),0x6041,2,Status Word,int,-,,,,PDO,ReadOnly,O,
...

# ECAT OP Mode (hex FtNo, PDO command)
ECAT OP Mode/CSP,(top-level),0x603F,1,Error Code,int,-,,,,PDO,ReadOnly,O,
ECAT OP Mode/CSP,(top-level),0x6040,2,Control Word,int,-,,,,PDO,Always,O,
...

# Monitor (MDM, 메인 CSV에 합침)
Monitor,MDM,,1,Velocity Feedback,int,RPM,,,,MDM,ReadOnly,O,
Monitor,MDM,,2,Velocity Command,int,RPM,,,,MDM,ReadOnly,O,
...
```

> - **Number** = Tree 내 표시 순번 (Tree마다 1부터 재시작)
> - **같은 FtNo가 여러 Tree에 중복 등장 가능** (예: Ft-0.02/D3은 Drive Root + Group 0 Basic 양쪽)
> - **FtNo** = 하드웨어 SET 주소; composite sub-field는 `Ft-G.NN/Dx`, ECAT는 `0xNNNN` 또는 `0xNNNN:SS`
> - Composite parent rows are NOT emitted — sub-fields only, each with own Number
> - CSV는 Tree별로 정렬, Tree 내에서 Number 오름차순
> - 계층 트리는 `Parent/Child` 형식 (로더가 `/` 기준으로 자동 파싱)

### MDM Parameters: `{DriveName}_MDM_Parameter.csv` (또는 메인 CSV에 합침)

MDM 데이터는 두 가지 방식으로 출력 가능:

**방식 A — 별도 파일 (기본):**
```csv
Tree,Group,FtNo,Number,Name,ValueType,Unit,Default,Min,Max,Command,DataAttribute,Rsware,Remark
Monitor,MDM,,0001,Velocity Feedback,int,RPM,,,,MDM,ReadOnly,O,
Monitor,MDM,,0002,Velocity Command,int,RPM,,,,MDM,ReadOnly,O,
```

**방식 B — 메인 CSV에 합치기 (권장):**
- MDM 행을 메인 `_Parameter.csv` 끝에 추가
- Tree=`Monitor`, Group=`MDM`, Command=`MDM`
- 로더가 자동으로 Monitor 노드 생성
- **주의**: Remark 컬럼에 개행문자(`\n`) 있으면 CSV 파싱 오류. 단일 행으로 정리 필수
- CSD7N은 방식 B 적용 완료 (CSD7N_MDM_Parameter.csv 삭제됨)

### Markdown Report: `{DriveName}_Parameter_Report.md`

```markdown
# {DriveName} Parameter Report
**Total Parameters:** N  **MDM Channels:** N

## Summary by Group
| Group | Count |
|---|---|
| Basic Setup | N |

## Parameter List
| Number | FtNo | Name | Type | Default | Min | Max | Unit | Command |
|---|---|---|---|---|---|---|---|---|

## Enum Options
| Number | FtNo | Name | Options |
|---|---|---|---|

## Notes
- stSETSTR parameters included (real ftx.xx parameters)
- MDM: monitoring channels, Command=MDM, ReadOnly
- ECAT/INDEX objects: included in main CSV under `ECAT Objects/` and `ECAT OP Mode/` trees
- [H] = Hidden parameter
```

---

## Multi-Drive Comparison

When processing multiple drives:
1. Extract each drive independently
2. Compare by Number + Name + Type + Range
3. Identical across all drives → `Main_Parameter.csv`
4. Drive-specific → `{DriveName}_Parameter.csv`

---

## Execution Procedure (2-Phase)

### Phase 1: 파라미터 순서 리스트 추출 (Order List)

**NodeMgr 소스 파일에서 AddProperty 호출 순서를 먼저 추출한다.**
상세 속성(Type, Min, Max, Default 등)은 이 단계에서 채우지 않는다.

1. C++ 레퍼런스 경로에서 대상 드라이브의 NodeMgr 파일 탐색
   - `{Drive}NodeMgr.cpp` (Drive Root)
   - `{Drive}Group0NodeMgr.cpp` ~ `{Drive}Group5NodeMgr.cpp`
   - `{Drive}MotorNodeMgr.cpp`, `{Drive}TuningNodeMgr.cpp`, `{Drive}EncoderNodeMgr.cpp` 등
2. 각 파일에서 `AddProperty()` / `AddGroup()` 호출 순서를 읽어 순서 리스트 생성
3. Tree별로 Number를 1부터 부여 (서브그룹은 이전에 이어서 연번)
4. **섹션 순서**: InsertItem() 순서(트리 구조 순서)대로 1→N 순차 배열. 섹션이 뒤섞이면 안 됨.
5. 출력: `{DriveName}_OrderList.md` — `DriveParameters/` 디렉토리에 저장

```markdown
## 1. Drive (CSD7NNodeMgr.cpp)
| Number | Group | FtNo | Name (SetName display name) |
|--------|-------|------|----------------------------|
| 1 | (top-level) | Ft-9.51 | Drive Name |
| 2 | (top-level) | Ft-0.02/D3 | AC Line Loss Check |
| 3 | (top-level) | Ft-9.50 | Motor Model |
| 4 | (top-level) | Ft-0.02/D2 | Command Polarity |
| 5 | (top-level) | Ft-4.12 | Initial Current Bias |
| 6 | Velocity Limits | Ft-2.17/D0 | Velocity Limit Mode |
| 7 | Velocity Limits | Ft-2.16 | Manual Velocity Limit |
...

## Group 0 : Basic (CSD7NGroup0NodeMgr.cpp)
| Number | Group | FtNo | Name (SetName display name) |
|--------|-------|------|----------------------------|
| 1 | (top-level) | Ft-0.02/D0 | Fault and Disable Braking |
| 2 | (top-level) | Ft-0.02/D1 | Overtravel Stop Method |
| 3 | (top-level) | Ft-0.02/D2 | Command Polarity |
| 4 | (top-level) | Ft-0.02/D3 | AC Line Loss Check |
...
```

> **Phase 1 Name 컬럼**: getter 함수명(`GetPowerInput()`)이 아닌 `SetName()` 표시 이름을 사용한다.
> **FtNo의 Dx**: `ECAttributeField`의 `StartPos` 파라미터에 `Dx = 3 - StartPos` 변환을 적용한다.

→ **사용자에게 순서 리스트를 먼저 제시하고 승인 받은 후 Phase 2 진행**

### Phase 2: 파라미터 상세 추출 (Detail Extraction)

Phase 1에서 확정된 순서 리스트를 기준으로 상세 속성을 채운다.

#### 자동화 도구 사용법

```bash
python tools/generate_drive_csv.py --drive {DRIVE_NAME}
```

**인자:**

| Arg | Default | Description |
|-----|---------|-------------|
| `--drive` | (필수) | 드라이브명 (CSD7N, CSD7A 등 12종) |
| `--orderlist` | `DriveParameters/{drive}_OrderList.md` | Phase 1 출력 파일 |
| `--excel` | `Doc/.xlsx/CSD7 Parameter List_20241111_Series B.xlsx` | Excel 원본 |
| `--sheet` | 자동 매핑 (아래 테이블) | Excel 시트명 |
| `--cpp-dir` | C++ ref 경로/`{drive}/` | C++ 소스 디렉토리 |
| `--output-dir` | `DriveParameters/` | 출력 디렉토리 |

**드라이브별 Excel 시트 매핑:**

| Drive | Sheet Name | Excel File |
|-------|-----------|------------|
| CSD7A, CSD7AB, CSD7AS, CSD7Y | `CSD7A Parameter List_20191224` | CSD7 Parameter List xlsx |
| CSD7N, CSD7NB, CSD7NI, CSD7NS | `CSD7N Parameter List_20241111` | CSD7 Parameter List xlsx |
| D8LMS, D8Q | 별도 Excel | D8 EtherCAT Object and Parameter List xlsx |
| LMMT, RMD | 별도 Excel 또는 C++ only | TBD |

#### 처리 파이프라인 (6단계)

1. **parse_orderlist** — OrderList.md 파싱 → `list[OrderEntry]`
   - `## N. TreeName (Source.cpp)` 섹션 헤더 감지
   - 테이블 행 → tree, group, ftno, number, name, version
   - `(virtual)` → skip, `XET-X.YY` → is_xet=True
   - `## Summary` 이전까지만 파싱

2. **parse_excel** — xlsx Parameter List → `dict[ftno_key → ExcelDetail]`
   - Parent 행: col15(FtNo) → key = `Ft-G.NN`
   - Digit 행: col16 → key = `Ft-G.NN/Dx`
   - Sub-field는 부모의 DataAttribute/Rsware/Unit 상속

3. **parse_mdm** — MDM List 시트 → `list[dict]`
   - No(col0), Name(col1), Unit(col2), Remark(col3)

4. **parse_drive_cpp** — Constants.h + Drive.cpp → `dict[ftno → CppOverride]`
   - `#define MACRO NUM` → FtNo 매핑
   - `.SetName()` → name override
   - `.SetValue()` → default (함수 호출은 `(dynamic)`)
   - `SetRangeValidation + SetMin/SetMax` → min/max
   - `ECAttributeField(child, parent, StartPos, width)` → `Dx = 3 - StartPos`

5. **merge_row** — OrderList × Excel × C++ → 14컬럼 CSV 행
   - Name: OrderList > C++ > Excel
   - Default/Min/Max: C++ > Excel
   - Command: DataAttribute에서 도출
   - ValueType: enum_opts 유무, name 패턴, default 형식으로 추론

6. **generate_report** — Summary markdown 생성

#### Data Source Priority

| Column | Primary | Fallback |
|--------|---------|----------|
| Tree, Group, Number | OrderList | - |
| FtNo | OrderList | Constants.h 검증 |
| Name | OrderList (=C++ SetName) | Excel col18/20 |
| ValueType | enum_opts 유무 / name 패턴 | int 기본 |
| Unit | Excel col22 | `-` |
| Default | C++ SetValue | Excel col26 |
| Min, Max | C++ SetRange | Excel col24/25 |
| Command | DataAttribute 도출 | SET/STR |
| DataAttribute | Excel col23 (sub-field는 부모 상속) | - |
| Rsware | Excel col13 (sub-field는 부모 상속) | - |
| Remark | Excel col30-41 (enum) | col28 |

#### 출력 파일

| File | Content |
|------|---------|
| `{Drive}_Parameter.csv` | SET + MDM + ECAT 파라미터 (14컬럼, Tree별 정렬). MDM/ECAT를 메인 CSV에 합침 |
| `{Drive}_MDM_Parameter.csv` | (옵셔널) MDM 모니터 채널 — 메인 CSV에 합치지 않을 경우만 생성 |
| `{Drive}_Parameter_Report.md` | 요약 보고서 |

#### 검증 체크리스트

1. `stdout` → 미매핑 FtNo 목록 확인
2. CSV 행 수 = OrderList 총 항목 - virtual 수
3. Composite sub-field (Ft-0.02/D0~D3) 정합성
4. 기존 CSV (`Doc/Drive 분류/`)와 교차 비교
5. Tree별 행 수가 OrderList Summary와 일치

#### 에이전트 호출 예시

```
# Phase 1이 완료된 드라이브에 대해 Phase 2 실행
python tools/generate_drive_csv.py --drive CSD7N

# 다른 Excel 사용 시
python tools/generate_drive_csv.py --drive D8LMS \
  --excel "Doc/.xlsx/D8 EtherCAT Object and Parameter List_V1.8.0.00_01_20231020.xlsx" \
  --sheet "D8LMS Parameter List"
```

---

## Important Notes

- **Non-ASCII / Korean text**: Strip from Name and Remark columns. Keep only ASCII printable.
- **Korean comments** in source code: Use for context, do not include in CSV output.
- `stSETSTR` parameters ARE real ftx.xx parameters — always include in SET DB
- `SetHidden(TRUE)` parameters: Include, mark with `[H]` suffix in Name
- `ATTRIBUTE_VIRTUAL`: UI-only sub-field (derived from composite parent digit); gets FtNo like `Ft-G.NN/Dx`. **단독 VIRTUAL 파라미터**(예: Index Number, AccelerationSub)는 FtNo = `(virtual)` 표기
- **Motor Status 그룹명**: Mixed case 사용 (`General`, `Electrical`, `Ratings`, `Feedback` — ALL CAPS 아님)
- **OrderList 섹션 순서**: 반드시 InsertItem() 순서 (트리 구조 순서)대로 1→N 순차 배열. 섹션이 뒤섞이면 안 됨
- Touch Probe (Ft-3.21): In Excel stored as D0~D3; in C++ NodeMgr expressed as B0~B15 bits. CSV uses Excel format (D0~D3).
- **Oscilloscope**: UI-only 스코프 뷰 노드. 고유 파라미터 없음 (다른 노드의 Tuning/Motor/Resonant 파라미터를 동적 참조). CSV에 행 추가 불필요.
- **Online** (ECAT Objects 하위): 모든 프로퍼티가 `ATTRIBUTE_VIRTUAL` + EngMode 전용. PPE 명령 기반 실시간 제어용. CSV에 행 추가 불필요.
- **ECAT Object 2**: `{Drive}PDOMap2.cpp`의 `AddIndex()` 패턴으로 추출 (Type C). 477개 항목 (0x2001~0x2F0A). FW 버전 조건부 항목 포함 (v2.10+, v2.14+).
- Parameter number gaps are normal (e.g., 0–32, then 100+)
- Duplicate numbers with different names = aliases; flag them
- `Dynamic` defaults (e.g., `m_X.GetInternalValue()`) → record as `(dynamic)`
- 12 drive types must each be extracted independently before cross-drive comparison
- **서브에이전트 모델**: Task 도구로 병렬 에이전트 호출 시 반드시 `model: "opus"` 를 지정한다. haiku/sonnet은 FtNo 매핑 정확도가 낮아 사용 금지.
- **C++ 원본 소스만 참조**: 기존 YAML 파일이나 이전에 생성된 CSV/MD 파일은 참고하지 않는다. C++ 원본 소스(`Constants.h`, `Drive.cpp`, `*NodeMgr.cpp`)와 Excel 문서만 기준으로 사용한다.
- **Composite Dx 변환 필수**: `ECAttributeField(child, parent, StartPos, size)` 에서 `Dx = 3 - StartPos` 변환을 반드시 적용한다. StartPos를 그대로 Dx로 사용하면 D0↔D3, D1↔D2가 반전된다.
- **Display Name = SetName() 값**: C++ 멤버 변수명(`m_PowerInput`)이나 getter(`GetPowerInput()`)가 아닌, `SetName(IDS_XXX)` 또는 `SetName("...")` 에 설정된 실제 표시 이름을 사용한다.
