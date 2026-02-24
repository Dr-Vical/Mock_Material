# Parameter System Specification — RswareDesign

> Last updated: 2026-02-24

## Summary

RswareDesign displays servo drive parameters in a center DataGrid panel. Parameters are loaded from CSV files (offline) or read from drives via Serial (online). Each parameter has read/write access attributes. Modified parameters are visually highlighted and saved via "Save Parameters" action.

---

## 1. CSV Schema

Source file: `Parameters/CSD7N_Parameter.csv`

| Column | Type | Description |
|--------|------|-------------|
| `Tree` | string | Tree node category (Drive, Motor, PID Tuning, etc.) |
| `Group` | string | Sub-group within tree node ((top-level), Velocity Limits, etc.) |
| `FtNo` | string | Firmware table number (Ft-0.02, Ft-9.50, XET-1.09, etc.) |
| `Number` | int | Parameter sequential number within tree node |
| `Name` | string | Parameter name (English) |
| `ValueType` | string | Data type: int, float, string, enum |
| `Unit` | string | Unit of measure (rpm, msec, %, -, etc.) |
| `Default` | string | Default value ((dynamic) for motor-dependent) |
| `Min` | string | Minimum value |
| `Max` | string | Maximum value |
| `Command` | string | Communication command (SET/STR, STR, XET) |
| `DataAttribute` | string | Write condition (Always, Servo Off, Drive Off=>On, etc.) |
| `Rsware` | string | RSWare support flag (O = supported, X = not shown) |
| `Remark` | string | Korean description, enum value definitions, version notes |

---

## 2. Parameter Entity Model

```csharp
public class Parameter
{
    public string TreeNode { get; set; }        // "Drive", "Motor", "PID Tuning", etc.
    public string Group { get; set; }           // "(top-level)", "Velocity Limits", etc.
    public string FtNumber { get; set; }        // "Ft-0.02/D3", "Ft-9.50"
    public int Number { get; set; }             // Sequential number
    public string Name { get; set; }            // "Drive Name", "Inertia Ratio"
    public ParameterValueType ValueType { get; set; }  // Int, Float, String, Enum
    public string Unit { get; set; }            // "rpm", "msec", "%"
    public string DefaultValue { get; set; }    // Default
    public string MinValue { get; set; }        // Min
    public string MaxValue { get; set; }        // Max
    public string CommandType { get; set; }     // "SET/STR", "STR", "XET"
    public string DataAttribute { get; set; }   // "Always", "Servo Off"
    public AccessMode Access { get; set; }      // ReadWrite or ReadOnly
    public string Remark { get; set; }          // Korean note, enum definitions

    // Runtime state
    public string CurrentValue { get; set; }    // Currently read/set value
    public string OriginalValue { get; set; }   // Value before editing
    public bool IsModified { get; set; }        // CurrentValue != OriginalValue
    public bool IsOutOfRange { get; set; }      // CurrentValue outside Min-Max
    public string[] EnumOptions { get; set; }   // Parsed from Remark for enum type
}

public enum AccessMode { ReadOnly, ReadWrite }
public enum ParameterValueType { Int, Float, String, Enum }
```

---

## 3. Tree-to-Parameter Mapping

When a tree node is selected, the center DataGrid shows parameters matching that tree node.

| Tree Node | CSV Tree Column | Filter |
|-----------|----------------|--------|
| Drive | `Tree == "Drive"` | All groups |
| ECAT Homing | `Tree == "ECAT Homing"` | All groups |
| Motor | `Tree == "Motor"` | All groups |
| PID Tuning | `Tree == "PID Tuning"` | All groups |
| Tuningless | `Tree == "Tuningless"` | All groups |
| Resonant Suppression | `Tree == "Resonant Suppression"` | All groups |
| Vibration Suppression | `Tree == "Vibration Suppression"` | All groups |
| Encoders | `Tree == "Encoders"` | All groups |
| Digital Inputs | `Tree == "Digital Inputs"` | All groups |
| Digital Outputs | `Tree == "Digital Outputs"` | All groups |
| Analog Outputs | `Tree == "Analog Outputs"` | All groups |
| Faults | `Tree == "Faults"` | All groups |
| Fully Closed System | `Tree == "Fully Closed System"` | All groups |
| Group 0-5 | `Tree == "Group"` + Group number | Filter by Group column |

### Group Display (Offline mode)

| Tree Node | Group Filter |
|-----------|-------------|
| Group 0 : Basic | Group number 0xx |
| Group 1 : Gain | Group number 1xx |
| Group 2 : Velocity | Group number 2xx |
| Group 3 : Position | Group number 3xx |
| Group 4 : Current | Group number 4xx |
| Group 5 : Auxiliary | Group number 5xx |

---

## 4. DataGrid Display Columns

### Standard Columns (always visible)

| Column | Header Key | Width | Binding | Editable |
|--------|-----------|-------|---------|----------|
| FT NUM | `loc.param.header.ftnum` | 80 | `FtNumber` | Never |
| PARAMETER | `loc.param.header.name` | * (remaining) | `Name` | Never |
| VALUE | `loc.param.header.value` | 120 | `CurrentValue` | If ReadWrite |
| UNITS | `loc.param.header.unit` | 80 | `Unit` | Never |
| DEFAULT | `loc.param.header.default` | 100 | `DefaultValue` | Never |
| MIN | `loc.param.header.min` | 80 | `MinValue` | Never |
| MAX | `loc.param.header.max` | 80 | `MaxValue` | Never |

### Visual Rules

| Condition | Visual Effect |
|-----------|--------------|
| Selected row | `PrimaryBrush` background |
| ReadWrite cell | Normal text, edit cursor on click |
| ReadOnly cell | `TextSecondary` foreground |
| Modified value (unsaved) | `SecondaryBrush` foreground (orange) |
| Out-of-range value | `ErrorBrush` foreground (red) |
| Enum type | ComboBox with EnumOptions in VALUE cell |
| String type | TextBox in VALUE cell |
| (dynamic) default | Display "(dynamic)" in gray |

---

## 5. Bottom Checkboxes

Below the DataGrid, 3 optional sections controlled by checkboxes:

| Checkbox | Key | Content |
|----------|-----|---------|
| Show Helps | `loc.action.showHelps` | Parameter description tooltip/panel |
| Show Status | `loc.action.showStatus` | Status values table (STATUS, VALUE, UNITS) |
| Show Commands | `loc.action.showCommands` | Command input + manual SET/STR |

### Status Panel (when Show Status is checked)

| Column | Example |
|--------|---------|
| STATUS | ECAT Homing Status, ECAT Homing Error |
| VALUE | 0:IDLE, No Error |
| UNITS | (unit if applicable) |

---

## 6. Right Action Panel Buttons

| Button | Key | Command | Condition |
|--------|-----|---------|-----------|
| Save Parameters | `loc.action.saveParam` | SaveParametersCommand | Online + HasModified |
| Revert | `loc.action.revert` | RevertCommand | HasModified |
| Setup... | `loc.action.setup` | ShowSetupCommand | Always |
| Simple/Detail | `loc.action.simpleDetail` | ToggleViewCommand | Always |
| Close | `loc.action.close` | CloseCommand | Always |
| Help | `loc.action.help` | ShowHelpCommand | Always |

---

## 7. Online vs Offline Mode

### Online Mode (Drive connected via Serial)
- Parameters read from drive via SET/STR commands
- VALUE column shows live drive values
- ReadWrite parameters can be edited and sent to drive
- "Save Parameters" writes to drive flash (EEPROM)

### Offline Mode (File-based)
- Parameters loaded from saved file (.rsw, .json, or .csv)
- VALUE column shows file-stored values
- ReadWrite parameters can be edited in-memory
- "Save" writes to file (not drive)
- Full parameter list available without connection

---

## Change History

| Date | Change |
|------|--------|
| 2026-02-24 | Initial spec — CSV schema, display rules, online/offline modes |
