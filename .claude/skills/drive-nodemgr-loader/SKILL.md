---
name: drive-nodemgr-loader
description: >
  Design and implement the parameter loading sequence for industrial servo drive software (.NET/C#).
  The loading order must follow the {Drive}NodeMgr.cpp AddProperty call sequence — NOT parameter number order.
  Use this skill when implementing or modifying parameter read/load logic, building a parameter
  loading service, or generating the ordered parameter list for UI display in RSWare or similar projects.

  **Trigger this skill when the user says any of:**
  - "파라메터 로딩 로직 만들어줘", "파라메터 읽기 순서", "노드매니저 순서"
  - "parameter loading order", "NodeMgr order", "AddProperty sequence"
  - Asks to implement batch-read / load-all parameters in drive order
  - Asks which parameters to read and in what sequence for a specific drive type
---

# Drive NodeMgr Parameter Loader Skill

Implement the parameter loading/read sequence for servo drives based on
`{Drive}NodeMgr.cpp` `AddProperty` call order.

**All output is in English.**

---

## Core Concept

### Why NodeMgr Order Matters

The `{Drive}NodeMgr.cpp` file defines the UI property tree for each drive type.
Its `AddProperty` call sequence is the **canonical display order** used in the
original C++ RSWare application. When implementing a .NET equivalent:

- **Batch read** (load all params): issue `SET` commands in NodeMgr order
- **UI grid display**: rows appear in NodeMgr order, not by SET number
- **VIRTUAL parameters** (ATTRIBUTE_VIRTUAL): do NOT send a SET command — their
  value is derived from a composite parent parameter's digit at runtime

### Number vs. FtNo vs. Display Position

| Concept | Column | Source | Purpose |
|---|---|---|---|
| **Number** | `Number` | NodeMgr `AddProperty` order (1-based, unique) | **UI display sequence** — row order in parameter grid, CSV sort key |
| **FtNo** | `FtNo` | `Ft-G.NN → SET G*100+NN` | **Hardware communication address** — used in `SET`/`CHP`/`STR` commands |
| composite sub-field FtNo | `FtNo` | `Ft-G.NN/Dx` | Parent address + digit position |

**Key rules:**
- Number is **globally unique** across entire CSV — never duplicated
- composite sub-fields (D0/D1/D2/D3) each get their own Number
- VIRTUAL parameters also get a Number (displayed in UI), but no independent FtNo SET address
- SET 951 (Drive Name) has Number=0001 (first in UI)
- SET 007 (Drive Address) appears at UI position 18 in CSD7N
- Parameters not shown on screen get large Number values (9001+)

---

## VIRTUAL Parameter Handling

`ATTRIBUTE_VIRTUAL` parameters are sub-fields of composite SET parameters that are
**exposed directly as UI properties** without an independent SET address.

| VIRTUAL FtNo | Parent SET | Digit | Description |
|---|---|---|---|
| Ft-0.02/D3 | SET 002 | D3 | AC Line Loss Check |
| Ft-0.02/D2 | SET 002 | D2 | Command Polarity |
| Ft-0.02/D1 | SET 002 | D1 | Overtravel Stop Method |
| Ft-0.02/D0 | SET 002 | D0 | Fault and Disable Braking |
| Ft-0.05/D0 | SET 005 | D0 | E-Stop Input |
| Ft-0.05/D1 | SET 005 | D1 | Gain Change Enable |
| Ft-0.06/D0 | SET 006 | D0 | Shunt Resistor Connection |
| Ft-2.17/D0 | SET 217 | D0 | Velocity Limit Mode |
| Ft-2.17/D1 | SET 217 | D1 | Field Weakening Mode |
| Ft-3.00/D0 | SET 300 | D0 | Gear Ratio Setting Mode |
| Ft-3.00/D1 | SET 300 | D1 | Encoder Output Forward Direction |
| Ft-3.41/D0 | SET 341 | D0 | Position Error Monitoring Method |
| Ft-4.28/D3 | SET 428 | D3 | Current Limit Band |
| Ft-5.10/D3 | SET 510 | D3 | Enable Display Monitor Value |

**Loading rule for VIRTUAL:**
1. Read the parent composite SET parameter (e.g., `SET 002`)
2. Extract the relevant digit position (D0/D1/D2/D3) from the 4-digit value
3. Display the extracted digit value as the VIRTUAL property's value
4. On write: pack the digit back into the 4-digit composite and write with `CHP`/`STR`

---

## CSD7N — Complete Ordered Parameter List

Source files:
- `CSD7NNodeMgr.cpp` (Drive node: Ft-9.51, Ft-9.50)
- `CSD7NGroup0NodeMgr.cpp` (Basic Setup)
- `CSD7NGroup1NodeMgr.cpp` (Gain / Tuning)
- `CSD7NGroup2NodeMgr.cpp` (Velocity Control)
- `CSD7NGroup3NodeMgr.cpp` (Position Control)
- `CSD7NGroup4NodeMgr.cpp` (Current Control)
- `CSD7NGroup5NodeMgr.cpp` (System / Brake / Misc)

### Drive Node Top (CSD7NNodeMgr.cpp)

| Number | FtNo | Name | SET# |
|---|---|---|---|
| 0001 | Ft-9.51 | Drive Name | 951 |
| 0002 | Ft-9.50 | Motor Model | 950 |

### Group 0 — Basic Setup (CSD7NGroup0NodeMgr.cpp)

| Number | FtNo | Name | Notes |
|---|---|---|---|
| 0003 | Ft-0.02/D3 | AC Line Loss Check | VIRTUAL from SET 002 |
| 0004 | Ft-0.02/D2 | Command Polarity | VIRTUAL from SET 002 |
| 0005 | Ft-0.02/D1 | Overtravel Stop Method | VIRTUAL from SET 002 |
| 0006 | Ft-0.02/D0 | Fault and Disable Braking | VIRTUAL from SET 002 |
| 0007 | Ft-0.03/D3 | Offline Tuning Mode | VIRTUAL from SET 003 |
| 0008 | Ft-0.03/D1 | Autotuning Speed | VIRTUAL from SET 003 |
| 0009 | Ft-0.04 | Inertia Ratio | SET 004 |
| 0010 | Ft-0.05/D3 | Single Turn Absolute | v1.06+ |
| 0011 | Ft-0.05/D2 | Abs Single Turn Reset | v1.06+ |
| 0012 | Ft-0.05/D1 | Gain Change Enable | VIRTUAL from SET 005 |
| 0013 | Ft-0.05/D0 | E-Stop Input | VIRTUAL from SET 005 |
| 0014 | Ft-0.06/D0 | Shunt Resistor | V2.00+ |
| 0015 | Ft-0.06/D1 | ABS Homing Completed | V2.02+ |
| 0016 | Ft-0.06/D2 | Mode of Gain Switch | |
| 0017 | Ft-0.06/D3 | Absolute Feedback Transfer Type | |
| 0018 | Ft-0.07 | Drive Address | SET 007 |
| 0019 | Ft-0.10 | Input Signal Allocation 1 | SET 010 |
| 0020 | Ft-0.11 | Input Signal Allocation 2 | SET 011 |
| 0021 | Ft-0.12 | Input Signal Allocation 3 | SET 012 |
| 0022 | Ft-0.13 | Input Signal Allocation 4 | SET 013 |
| 0023 | Ft-0.14 | Input Signal Allocation 5 | SET 014 |
| 0024 | Ft-0.15 | Input Signal Allocation 6 | SET 015 |
| 0025 | Ft-0.16 | Input Signal Allocation 7 | SET 016 |
| 0026 | Ft-0.17 | Input Signal Allocation 8 | SET 017 |
| 0027 | Ft-0.18 | Input Signal Allocation 9 | SET 018 |
| 0028 | Ft-0.19 | Input Signal Allocation 10 | SET 019 |
| 0029 | Ft-0.22 | Output Signal Allocation 1 | SET 022 |
| 0030 | Ft-0.23 | Output Signal Allocation 2 | SET 023 |
| 0031 | Ft-0.24 | Output Signal Allocation 3 | SET 024 |
| 0032 | Ft-0.33 | Alias ID | SET 033, conditional |
| 0033 | Ft-0.34 | Load Side BiSS Protocol | SET 034 |

### Group 1 — Gain / Tuning (CSD7NGroup1NodeMgr.cpp)

| Number | FtNo | Name |
|---|---|---|
| 0034 | Ft-1.00 | Velocity Response Level |
| 0035 | Ft-1.01 | System Gain |
| 0036 | Ft-1.02 | 1st Velocity P Gain |
| 0037 | Ft-1.03 | 1st Velocity I Gain |
| 0038 | Ft-1.04 | Velocity I Gain Mode |
| 0039 | Ft-1.05 | Velocity I Gain Disable Threshold |
| 0040 | Ft-1.06 | Velocity D Gain |
| 0041 | Ft-1.07 | Position P Gain |
| 0042 | Ft-1.10 | Gain Switch Delay Time |
| 0043 | Ft-1.11 | Gain Switch Level |
| 0044 | Ft-1.12 | Gain Switch Hysteresis |
| 0045 | Ft-1.13 | Gain Switch Position Time |
| 0046 | Ft-1.14 | 2nd Velocity P Gain |
| 0047 | Ft-1.15 | 2nd Velocity I Gain |
| 0048 | Ft-1.16 | 2nd Position Kp Gain |
| 0049 | Ft-1.17 | 3rd Velocity P Gain |
| 0050 | Ft-1.18 | 3rd Velocity I Gain |
| 0051 | Ft-1.19 | 3rd Position Kp Gain |
| 0052 | Ft-1.20 | 4th Velocity P Gain |
| 0053 | Ft-1.21 | 4th Velocity I Gain |
| 0054 | Ft-1.22 | 4th Position Kp Gain |
| 0055 | Ft-1.23 | Tuning Mode Selection |
| 0056 | Ft-1.23/D3 | Inertia Ratio Estimation Sensitivity |
| 0057 | Ft-1.24 | Tuningless Gain |
| 0058 | Ft-1.25 | Load Inertia Ratio Select |
| 0059 | Ft-1.26 | Load Inertia Ratio |
| 0060 | Ft-1.27 | Smart Tuning Positioning Mode |
| 0061 | Ft-1.28 | Cmd Smoothing Filter BW Interlocking |
| 0062 | Ft-1.29 | Viscous Friction Compensator |
| 0063 | Ft-1.30 | Online Parameter Estimation |
| 0064 | Ft-1.31 | Viscous Friction Coefficient |
| 0065 | Ft-1.32 | Tuningless Fine Gain |
| 0066 | Ft-1.33 | Tuningless Feedforward Gain |

### Group 2 — Velocity Control (CSD7NGroup2NodeMgr.cpp)

| Number | FtNo | Name |
|---|---|---|
| 0067 | Ft-2.02 | Filter Bandwidth |
| 0068 | Ft-2.03 | Velocity Error Filter |
| 0069 | Ft-2.04 | Velocity Feed Forward |
| 0070 | Ft-2.05 | Velocity Command |
| 0071 | Ft-2.06 | Acceleration |
| 0072 | Ft-2.07 | Deceleration |
| 0073 | Ft-2.08 | S-Curve Time |
| 0074 | Ft-2.16 | Manual Velocity Limit |
| 0075 | Ft-2.17/D0 | Velocity Limit Mode |
| 0076 | Ft-2.17/D1 | Field Weakening Mode |
| 0077 | Ft-2.18 | Speed Window |
| 0078 | Ft-2.19 | Up to Speed |
| 0079 | Ft-2.21 | Test Run Dwell Period |
| 0080 | Ft-2.25 | Linear Overspeed Level |
| 0081 | Ft-2.26 | Linear Velocity Error Limit |
| 0082 | Ft-2.27 | Velocity Cmd Smoothing Filter |
| 0083 | Ft-2.28 | Path Tracking Mode Vel LPF BW |
| 0084 | Ft-2.29 | Path Tracking Mode Vel FF LPF BW |
| 0085 | Ft-2.31/D1 | POC Enable |
| 0086 | Ft-2.31/D2 | POC BPF Upper Cutoff Order Select |
| 0087 | Ft-2.32 | POC BPF Upper Cutoff Frequency |
| 0088 | Ft-2.33 | POC BPF Lower Cutoff Frequency |
| 0089 | Ft-2.34 | POC Kd Gain |
| 0090 | Ft-2.35 | POC LPF |
| 0091 | Ft-2.36 | Field Weakening Start Velocity |
| 0092 | Ft-2.37 | Maximum Extended Velocity |
| 0093 | Ft-2.38 | Field Weakening Current Cmd Gain |

### Group 3 — Position Control (CSD7NGroup3NodeMgr.cpp)

| Number | FtNo | Name |
|---|---|---|
| 0094 | Ft-3.00/D1 | Encoder Output Forward Direction |
| 0095 | Ft-3.00/D0 | Gear Ratio Setting Mode |
| 0096 | Ft-3.01 | Position Filter Bandwidth |
| 0097 | Ft-3.02 | Position FF Gain |
| 0098 | Ft-3.03 | Position FF Filter Bandwidth |
| 0099 | Ft-3.04 | Moving Average Filter |
| 0100 | Ft-3.05 | Gear Ratio (Master) |
| 0101 | Ft-3.06 | Gear Ratio (Follower) |
| 0102 | Ft-3.11 | Encoder Output Pulses |
| 0103 | Ft-3.12 | Motor Pulses |
| 0104 | Ft-3.14 | 1st Damping Frequency |
| 0105 | Ft-3.15 | 1st Damping Ratio |
| 0106 | Ft-3.16 | 2nd Damping Frequency |
| 0107 | Ft-3.17 | 2nd Damping Ratio |
| 0108 | Ft-3.18 | In-Position Size |
| 0109 | Ft-3.19 | Near Position Size |
| 0110 | Ft-3.20 | Following Error Limit |
| 0111 | Ft-3.21/D0 | Touch Probe (D0) |
| 0112 | Ft-3.21/D1 | Touch Probe Mode (D1) |
| 0113 | Ft-3.21/D2 | Touch Probe Source (D2) |
| 0114 | Ft-3.21/D3 | Touch Probe Edge (D3) |
| 0115 | Ft-3.34 | Outer Loop P Gain |
| 0116 | Ft-3.36 | In-Position Hold Time |
| 0117 | Ft-3.41/D0 | Position Error Monitoring Method |
| 0118 | Ft-3.43/D0 | Encoder Signal Filter Enable |
| 0119 | Ft-3.43/D1 | Path Tracking Input Smoothing Filter |
| 0120 | Ft-3.22/D3 | Vibration Auto Detection |
| 0121 | Ft-3.22/D2 | Vibration Filter Selection |

### Group 4 — Current Control (CSD7NGroup4NodeMgr.cpp)

| Number | FtNo | Name |
|---|---|---|
| 0122 | Ft-4.02 | Current Filter Bandwidth |
| 0123 | Ft-4.03 | 2nd Current Bandwidth |
| 0124 | Ft-4.04 | 3rd Current Bandwidth |
| 0125 | Ft-4.05 | 4th Current Bandwidth |
| 0126 | Ft-4.06 | Current Gain |
| 0127 | Ft-4.07 | Current Limit + (Internal) |
| 0128 | Ft-4.08 | Current Limit - (Internal) |
| 0129 | Ft-4.09 | Current Limit + (External) |
| 0130 | Ft-4.10 | Current Limit - (External) |
| 0131 | Ft-4.11 | Current Overtravel Limit |
| 0132 | Ft-4.12 | Current Bias |
| 0133 | Ft-4.13 | Resonant Freq Suppression 1st |
| 0134 | Ft-4.14 | Notch Width 1st |
| 0135 | Ft-4.15 | Notch Depth 1st |
| 0136 | Ft-4.16 | Resonant Freq Suppression 2nd |
| 0137 | Ft-4.17 | Notch Width 2nd |
| 0138 | Ft-4.18 | Notch Depth 2nd |
| 0139 | Ft-4.19 | Resonant Freq Suppression 3rd |
| 0140 | Ft-4.20 | Notch Width 3rd |
| 0141 | Ft-4.21 | Notch Depth 3rd |
| 0142 | Ft-4.29 | Resonant Freq Suppression 4th |
| 0143 | Ft-4.30 | Notch Width 4th |
| 0144 | Ft-4.31 | Notch Depth 4th |
| 0145 | Ft-4.32 | Resonant Freq Suppression 5th |
| 0146 | Ft-4.33 | Notch Width 5th |
| 0147 | Ft-4.34 | Notch Depth 5th |
| 0148 | Ft-4.22 | ANF Enable |
| 0149 | Ft-4.23 | RMS Load Factor Cumulative Time |
| 0150 | Ft-4.24 | RMS Load Factor Threshold 1 |
| 0151 | Ft-4.25 | RMS Load Factor Threshold 2 |
| 0152 | Ft-4.26/D0 | Bode Plot Function Enable |
| 0153 | Ft-4.26/D1 | Bode Plot Driving Mode |
| 0154 | Ft-4.26/D2 | Bode Plot Excitation Amplitude |
| 0155 | Ft-4.27 | Current Command LBF Bandwidth |
| 0156 | Ft-4.28/D0 | Interlocking Gain Tuning Mode |
| 0157 | Ft-4.28/D1 | Interlocking Tuningless Mode |
| 0158 | Ft-4.28/D2 | Current Command Feedforward |
| 0159 | Ft-4.28/D3 | Current Limit Band |
| 0160 | Ft-4.35 | Main Current Regulator Max BW |
| 0161 | Ft-4.36 | DDC Gain |
| 0162 | Ft-4.37 | Sliding Surface Slope |
| 0163 | Ft-4.38/D0 | DDC Interlocking |
| 0164 | Ft-4.38/D1 | Sliding Surface Slope Interlocking |
| 0165 | Ft-4.38/D2 | Outer Loop P Gain Interlocking |
| 0166 | Ft-4.38/D3 | Current POC Enable |
| 0167 | Ft-4.39/D3 | DDC Q Filter Order Select |
| 0168 | Ft-4.41 | DDC Q Value |
| 0169 | Ft-4.43 | Cascade DDC 2nd LPF Cutoff Freq |
| 0170 | Ft-4.44 | Current POC Kr Filter |
| 0171 | Ft-4.45 | Current POC BPF Upper Cutoff |
| 0172 | Ft-4.46 | Current POC BPF Lower Cutoff |
| 0173 | Ft-4.47 | Current POC Kd Gain |
| 0174 | Ft-4.48 | Current POC Operation Delay |

### Group 5 — System / Brake / Misc (CSD7NGroup5NodeMgr.cpp)

| Number | FtNo | Name |
|---|---|---|
| 0175 | Ft-5.00 | Brake Inactive Delay |
| 0176 | Ft-5.01 | Disable Delay |
| 0177 | Ft-5.02 | Brake Active Delay |
| 0178 | Ft-5.03 | Braking Speed |
| 0179 | Ft-5.04 | AC Line Loss Delay |
| 0180 | Ft-5.09 | Motor Overload Detect |
| 0181 | Ft-5.10/D0 | Display Number |
| 0182 | Ft-5.10/D3 | Enable Display Monitor Value |
| 0183 | Ft-5.11 | Power Sag Warning Enable |
| 0184 | Ft-5.12 | Power Sag Current Limit |
| 0185 | Ft-5.13 | Power Sag Release Time |
| 0186 | Ft-5.14 | ECAT Abs Origin Offset |
| 0187 | Ft-5.15 | ECAT Homing Method |
| 0188 | Ft-5.16 | ECAT Homing Time Out |
| 0189 | Ft-5.17 | ECAT Homing Offset |
| 0190 | Ft-5.18 | ECAT Homing Speed 1 |
| 0191 | Ft-5.19 | ECAT Homing Speed 2 |
| 0192 | Ft-5.20 | ECAT Homing Acceleration |
| 0193 | Ft-5.21/D0 | Conv EEPROM Write Enable |
| 0194 | Ft-5.21/D2 | Angle Search Moving Count |
| 0195 | Ft-5.21/D3 | Angle Search Enable |
| 0196 | Ft-5.22/D0 | Encoder Feedback Forward Direction |
| 0197 | Ft-5.24 | External Shunt Resistor Value |
| 0198 | Ft-5.25 | External Shunt Resistor Capacity |
| 0199 | Ft-5.26 | STO Circuit Verification Mode |
| 0200 | Ft-5.30 | PWM Frequency Select |
| 0201 | Ft-5.31 | BiSS Commutation Angle |
| 0202 | Ft-5.32 | BiSS Master Clock Frequency |
| 0203 | Ft-5.37 | NOT Polarity |
| 0204 | Ft-5.38 | POT Polarity |
| 0205 | Ft-5.39/D0 | Home Sensor Polarity |
| 0206 | Ft-5.39/D1 | Encoder Direction Setup Enable |
| 0207 | Ft-5.39/D2 | Encoder Direction Setup Finish |
| 0208 | Ft-5.40 | Short Angle Search Cur Cmd Ratio |
| 0209 | Ft-5.41 | Short Angle Search Cur Cmd Max Time |
| 0210 | Ft-5.42 | Short Angle Search Cur Cmd LBF BW |
| 0211 | Ft-5.43 | Short Angle Search Zero Moving PW |
| 0212 | Ft-5.44 | Short Angle Search Stop Pulse Count |
| 0213 | Ft-5.45 | Short Angle Search Stop Time |
| 0214 | Ft-5.46 | Short Angle Search Stop Time Limit |
| 0215 | Ft-5.47 | Current Command Target Angle |
| 0216 | Ft-5.48 | Current Command Target Interval Angle |
| 0217 | Ft-5.57 | Detection Level Motor Overload |

---

## Implementation Pattern — C# (.NET 8)

### 1. Composite Digit Extraction

```csharp
static int ExtractDigit(int compositeValue, int digitPos)
    => (compositeValue >> (digitPos * 4)) & 0xF;

static int PackDigit(int compositeValue, int digitPos, int newDigit)
{
    int mask = 0xF << (digitPos * 4);
    return (compositeValue & ~mask) | ((newDigit & 0xF) << (digitPos * 4));
}
```

```
SET 002 response: "0021"
Value = 0x0021
D0 [bits  3..0] = 0x1 → Fault and Disable Braking = 1
D1 [bits  7..4] = 0x2 → Overtravel Stop Method    = 2
D2 [bits 11..8] = 0x0 → Command Polarity          = 0
D3 [bits 15..12]= 0x0 → AC Line Loss Check        = 0
```

### 2. Batch Read Service

```csharp
public class DriveParameterLoader
{
    private readonly IMotorDriver _driver;

    public async Task<Dictionary<int, string>> LoadAllParametersAsync(
        int deviceId,
        IEnumerable<ParameterDefinition> orderedParams,
        CancellationToken ct = default)
    {
        var results = new Dictionary<int, string>();
        var setNumbersToRead = orderedParams
            .Where(p => !p.IsVirtual)
            .Select(p => p.SetNumber)
            .Distinct();

        foreach (int setNo in setNumbersToRead)
        {
            try
            {
                string value = await _driver.ReadParameterAsync(deviceId, setNo, ct);
                results[setNo] = value;
                await Task.Delay(10, ct);
            }
            catch (TimeoutException)
            {
                results[setNo] = "(timeout)";
            }
        }
        return results;
    }

    // Resolve VIRTUAL field (composite digit)
    public static string ResolveVirtual(
        Dictionary<int, string> loaded,
        int parentSetNo,
        int digitPos)
    {
        if (!loaded.TryGetValue(parentSetNo, out string? raw) || string.IsNullOrEmpty(raw))
            return "";

        if (!int.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out int composite))
            if (!int.TryParse(raw, out composite))
                return "";

        int digit = (composite >> (digitPos * 4)) & 0xF;
        return digit.ToString();
    }
}
```

### 3. CSV-based Parameter Loading

When loading from `CSD7N_Parameter.csv`:

```csharp
// Parameters are sorted by Number (ascending) — use directly
// For VIRTUAL (composite sub-field): FtNo contains "/D{x}" suffix
// Example: Ft-0.02/D3 → SetNumber=2, DigitPos=3

public static (int SetNo, int DigitPos) ParseFtNo(string ftNo)
{
    var match = Regex.Match(ftNo, @"Ft-(\d+)\.(\d+)(?:/D(\d+))?");
    if (!match.Success) return (0, -1);
    int g = int.Parse(match.Groups[1].Value);
    int nn = int.Parse(match.Groups[2].Value);
    int setNo = g * 100 + nn;
    int digitPos = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : -1;
    return (setNo, digitPos);
}
```

---

## Adding a New Drive Type

When implementing for a drive other than CSD7N:

1. **Read `{Drive}NodeMgr.cpp`** — find the `Init()` or `Create()` function
2. **Scan all `m_Properties.AddProperty(...)` and `pGroup->AddProperty(...)` calls** in order
3. **For each call**: identify the member variable → look up its `.Define(MACRO)` in `{Drive}Drive.cpp` → resolve macro in `{Drive}Constants.h` to get SET number
4. **Mark VIRTUAL**: any member declared as `ATTRIBUTE_VIRTUAL` in Drive.h or with no `.Define()` call
5. **Build the ordered SET list** (deduplicate — only add composite parents once)
6. **Update `CSD7N_NODEMGR_ORDER`** in `extract_params.py` with the new drive's order

---

## Important Rules

- **Never sort by SET number** — UI order is NodeMgr order, not numeric order
- **VIRTUAL params require no SET read** — only composite parent needs to be read
- **Read composite parents first** (SET 002, SET 005) before resolving VIRTUALs
- **ECPropertyGroup** headers are UI-only — they add no SET reads
- **Version Info** (Firmware, CPLD) use special VER command, not SET
- **Conditional groups** may only appear for certain hardware/firmware configs
- **Same SET number may appear multiple times** in NodeMgr as different VIRTUAL sub-fields — only read once, decode digits separately
- **Touch Probe (Ft-3.21)**: Excel CSV uses D0~D3 keys; C++ NodeMgr uses B0~B15 bit notation. CSV is authoritative.
