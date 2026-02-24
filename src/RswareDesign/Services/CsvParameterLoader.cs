using RswareDesign.Models;
using System.IO;
using System.Text;

namespace RswareDesign.Services;

/// <summary>
/// Loads parameters from CSD7N_Parameter.csv for the mockup UI.
/// CSV columns: Tree, Group, FtNo, Number, Name, ValueType, Unit, Default, Min, Max, Command, DataAttribute, Rsware, Remark
/// </summary>
public static class CsvParameterLoader
{
    private static List<CsvRow>? _cachedRows;

    /// <summary>
    /// Maps tree NodeType to CSV Tree column value(s).
    /// </summary>
    private static readonly Dictionary<string, string> NodeTypeToCsvTree = new()
    {
        ["ModeConfig"]           = "Drive",
        ["Motor"]                = "Motor",
        ["PIDTuning"]            = "PID Tuning",
        ["Tuningless"]           = "Tuningless",
        ["ResonantSuppression"]  = "Resonant Suppression",
        ["VibrationSuppression"] = "Vibration Suppression",
        ["Encoders"]             = "Encoders",
        ["DigitalInputs"]        = "Digital Inputs",
        ["DigitalOutputs"]       = "Digital Outputs",
        ["ECATHoming"]           = "ECAT Homing",
        ["Monitor"]              = "Monitor",
        ["Oscilloscope"]         = "Bode Plot",
        ["Faults"]               = "Faults",
        ["FullyClosed"]          = "Fully Closed System",
        ["ServiceInfo"]          = "ServiceInfo",
        ["ControlPanel"]         = "Control Panel",
        ["Group0"]               = "Group/Group 0 : Basic",
        ["Group1"]               = "Group/Group 1 : Gain",
        ["Group2"]               = "Group/Group 2 : Velocity",
        ["Group3"]               = "Group/Group 3 : Position",
        ["Group4"]               = "Group/Group 4 : Current",
        ["Group5"]               = "Group/Group 5 : Auxiliary",
    };

    public static List<Parameter> LoadForNodeType(string nodeType)
    {
        var rows = GetAllRows();
        if (!NodeTypeToCsvTree.TryGetValue(nodeType, out var csvTree))
            csvTree = "ECAT Homing"; // default fallback

        // For "Fully Closed System", match startsWith (sub-trees)
        var filtered = csvTree == "Fully Closed System"
            ? rows.Where(r => r.Tree.StartsWith("Fully Closed System", StringComparison.OrdinalIgnoreCase))
            : rows.Where(r => r.Tree.Equals(csvTree, StringComparison.OrdinalIgnoreCase));

        return filtered.Select(r => new Parameter
        {
            FtNumber = r.FtNo,
            Name     = r.Name,
            Value    = r.Default,
            Unit     = r.Unit == "-" ? "" : r.Unit,
            Default  = r.Default,
            Min      = r.Min,
            Max      = r.Max,
            Access   = string.IsNullOrWhiteSpace(r.DataAttribute) ? "r/w" : r.DataAttribute,
            Group    = r.Group == "(top-level)" ? "" : r.Group,
        }).ToList();
    }

    private static List<CsvRow> GetAllRows()
    {
        if (_cachedRows != null) return _cachedRows;

        var csvPath = FindCsvPath();
        if (csvPath == null || !File.Exists(csvPath))
        {
            _cachedRows = [];
            return _cachedRows;
        }

        var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
        _cachedRows = [];

        // Skip header (line 0)
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (fields.Count < 14) continue;

            _cachedRows.Add(new CsvRow
            {
                Tree          = fields[0].Trim('\uFEFF'), // strip BOM
                Group         = fields[1],
                FtNo          = fields[2],
                Number        = fields[3],
                Name          = fields[4],
                ValueType     = fields[5],
                Unit          = fields[6],
                Default       = fields[7],
                Min           = fields[8],
                Max           = fields[9],
                Command       = fields[10],
                DataAttribute = fields[11],
                Rsware        = fields[12],
                Remark        = fields[13],
            });
        }

        return _cachedRows;
    }

    private static string? FindCsvPath()
    {
        // Try output directory first
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var candidate = Path.Combine(baseDir, "Parameters", "CSD7N_Parameter.csv");
        if (File.Exists(candidate)) return candidate;

        // Walk up from exe to find Parameters folder (development scenario)
        var dir = new DirectoryInfo(baseDir);
        for (int i = 0; i < 8 && dir?.Parent != null; i++)
        {
            dir = dir.Parent;
            candidate = Path.Combine(dir.FullName, "Parameters", "CSD7N_Parameter.csv");
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    /// <summary>
    /// RFC 4180 compliant CSV line parser (handles quoted fields with commas).
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                    inQuotes = true;
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                    current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields;
    }

    private class CsvRow
    {
        public string Tree { get; set; } = "";
        public string Group { get; set; } = "";
        public string FtNo { get; set; } = "";
        public string Number { get; set; } = "";
        public string Name { get; set; } = "";
        public string ValueType { get; set; } = "";
        public string Unit { get; set; } = "";
        public string Default { get; set; } = "";
        public string Min { get; set; } = "";
        public string Max { get; set; } = "";
        public string Command { get; set; } = "";
        public string DataAttribute { get; set; } = "";
        public string Rsware { get; set; } = "";
        public string Remark { get; set; } = "";
    }
}
