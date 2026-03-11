using System.IO;
using System.Text;
using RswareDesign.Models;

namespace RswareDesign.Services;

public static class ParameterFileService
{
    /// <summary>
    /// Export all parameters to CSV file.
    /// Header: # Drive,Firmware metadata, then FtNumber,Value columns.
    /// </summary>
    public static int Export(string filePath, IEnumerable<Parameter> parameters,
        string driveType = "", string firmwareVersion = "")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Drive,{Escape(driveType)}");
        sb.AppendLine($"# Firmware,{Escape(firmwareVersion)}");
        sb.AppendLine($"# Exported,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("FtNumber,Value");

        int count = 0;
        foreach (var p in parameters)
        {
            if (string.IsNullOrEmpty(p.FtNumber)) continue;

            sb.AppendLine(string.Join(",", Escape(p.FtNumber), Escape(p.Value)));
            count++;
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        return count;
    }

    /// <summary>
    /// Import parameter overrides from CSV file.
    /// Returns (overrides dictionary, drive type, firmware version).
    /// </summary>
    public static (Dictionary<string, string> overrides, string drive, string firmware) Import(string filePath)
    {
        var result = new Dictionary<string, string>();
        string drive = "";
        string firmware = "";
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Parse metadata comments
            if (line.StartsWith("# Drive,"))
            {
                drive = line.Substring("# Drive,".Length).Trim();
                continue;
            }
            if (line.StartsWith("# Firmware,"))
            {
                firmware = line.Substring("# Firmware,".Length).Trim();
                continue;
            }
            if (line.StartsWith('#')) continue;

            var cols = ParseCsvLine(line);

            // Skip header row
            if (cols.Count > 0 && cols[0] == "FtNumber")
                continue;

            if (cols.Count < 2) continue;

            var ftNumber = cols[0].Trim();
            if (!string.IsNullOrEmpty(ftNumber))
                result[ftNumber] = cols[1];
        }

        return (result, drive, firmware);
    }

    /// <summary>
    /// Apply imported overrides: params in file → file value, params NOT in file → default value.
    /// Returns (restored, overridden) counts.
    /// </summary>
    public static (int restored, int overridden) ApplyImport(IList<Parameter> existing, Dictionary<string, string> overrides)
    {
        int restored = 0;
        int overridden = 0;

        foreach (var p in existing)
        {
            if (string.IsNullOrEmpty(p.FtNumber)) continue;

            if (overrides.TryGetValue(p.FtNumber, out var newValue))
            {
                // Parameter is in the file → set to file value
                if (p.Value != newValue)
                {
                    p.Value = newValue;
                    p.IsModified = true;
                    overridden++;
                }
            }
            else
            {
                // Parameter NOT in file → restore to default
                if (p.Value != p.Default)
                {
                    p.Value = p.Default;
                    p.IsModified = false;
                    restored++;
                }
            }
        }

        return (restored, overridden);
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
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
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                    current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result;
    }
}
