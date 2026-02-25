using RswareDesign.Models;

namespace RswareDesign.Services;

public static class ActionButtonRegistry
{
    private static readonly ActionButton Sep = new() { IsSeparator = true };

    public static List<ActionButton> GetForNodeType(string nodeType) => nodeType switch
    {
        // Parameter config nodes: full read/write/save + compare/export/revert
        "ModeConfig" or "Motor" or "PIDTuning" or "Tuningless"
            or "ResonantSuppression" or "VibrationSuppression"
            or "Encoders" or "DigitalInputs" or "DigitalOutputs"
            or "AnalogOutputs" or "ECATHoming" or "FullyClosed" =>
        [
            new() { Label = "Read All",  IconKind = "DatabaseArrowDown", Style = "Primary" },
            new() { Label = "Write All", IconKind = "DatabaseArrowUp",   Style = "Primary" },
            new() { Label = "Save",      IconKind = "ContentSaveAll",    Style = "Secondary" },
            Sep,
            new() { Label = "Compare",   IconKind = "CompareHorizontal", Style = "Outlined" },
            new() { Label = "Export",    IconKind = "Export",             Style = "Outlined" },
            new() { Label = "Revert",    IconKind = "UndoVariant",       Style = "Outlined" },
        ],

        // Monitor/Oscilloscope: read-only + refresh
        "Monitor" or "Oscilloscope" =>
        [
            new() { Label = "Read All",  IconKind = "DatabaseArrowDown", Style = "Primary" },
            new() { Label = "Refresh",   IconKind = "Refresh",           Style = "Secondary" },
            Sep,
            new() { Label = "Export",    IconKind = "Export",             Style = "Outlined" },
        ],

        // Faults/Service: read + clear + export
        "Faults" =>
        [
            new() { Label = "Read All",     IconKind = "DatabaseArrowDown", Style = "Primary" },
            new() { Label = "Clear Faults", IconKind = "AlertRemove",       Style = "Secondary" },
            Sep,
            new() { Label = "Export Log",   IconKind = "Export",            Style = "Outlined" },
        ],

        "ServiceInfo" =>
        [
            new() { Label = "Read All",  IconKind = "DatabaseArrowDown", Style = "Primary" },
            Sep,
            new() { Label = "Export",    IconKind = "Export",             Style = "Outlined" },
        ],

        // Control Panel: enable/disable/jog
        "ControlPanel" =>
        [
            new() { Label = "Enable",   IconKind = "Play",  Style = "Primary" },
            new() { Label = "Disable",  IconKind = "Stop",  Style = "Secondary" },
            Sep,
            new() { Label = "Jog +",    IconKind = "ArrowRight",     Style = "Outlined" },
            new() { Label = "Jog -",    IconKind = "ArrowLeft",      Style = "Outlined" },
            new() { Label = "Home",     IconKind = "Home",           Style = "Outlined" },
        ],

        // Group nodes (offline): read/write/save + compare/export
        "Group0" or "Group1" or "Group2" or "Group3" or "Group4" or "Group5" =>
        [
            new() { Label = "Read All",  IconKind = "DatabaseArrowDown", Style = "Primary" },
            new() { Label = "Write All", IconKind = "DatabaseArrowUp",   Style = "Primary" },
            new() { Label = "Save",      IconKind = "ContentSaveAll",    Style = "Secondary" },
            Sep,
            new() { Label = "Compare",   IconKind = "CompareHorizontal", Style = "Outlined" },
            new() { Label = "Export",    IconKind = "Export",             Style = "Outlined" },
        ],

        // Default (container nodes, etc.): minimal
        _ =>
        [
            new() { Label = "Read All",  IconKind = "DatabaseArrowDown", Style = "Primary" },
        ],
    };
}
