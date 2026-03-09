using System.IO;
using System.Text;

namespace RswareDesign.Services;

public class ScopeChannelSetting
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#FFFFFFFF";
    public bool Enabled { get; set; }
    public double ScaleMax { get; set; } = 10000;
    public double ScaleMin { get; set; } = -10000;
}

public static class ScopeSettingService
{
    public static string GetScopeFolder()
    {
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScopeSetting");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return folder;
    }

    public static void Save(string filePath, List<ScopeChannelSetting> channels)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[ScopeSetting]");
        sb.AppendLine($"Count={channels.Count}");
        sb.AppendLine();

        for (int i = 0; i < channels.Count; i++)
        {
            var ch = channels[i];
            sb.AppendLine($"[CH{i}]");
            sb.AppendLine($"Name={ch.Name}");
            sb.AppendLine($"Color={ch.Color}");
            sb.AppendLine($"Enabled={ch.Enabled}");
            sb.AppendLine($"ScaleMax={ch.ScaleMax}");
            sb.AppendLine($"ScaleMin={ch.ScaleMin}");
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    public static List<ScopeChannelSetting> Load(string filePath)
    {
        var result = new List<ScopeChannelSetting>();
        if (!File.Exists(filePath)) return result;

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        ScopeChannelSetting? current = null;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                var section = line[1..^1];
                if (section == "ScopeSetting") continue;
                if (section.StartsWith("CH"))
                {
                    current = new ScopeChannelSetting();
                    result.Add(current);
                }
                continue;
            }

            if (current == null) continue;

            var eqIdx = line.IndexOf('=');
            if (eqIdx < 0) continue;

            var key = line[..eqIdx].Trim();
            var val = line[(eqIdx + 1)..].Trim();

            switch (key)
            {
                case "Name": current.Name = val; break;
                case "Color": current.Color = val; break;
                case "Enabled": current.Enabled = val == "True"; break;
                case "ScaleMax":
                    if (double.TryParse(val, out double smax))
                        current.ScaleMax = smax;
                    break;
                case "ScaleMin":
                    if (double.TryParse(val, out double smin))
                        current.ScaleMin = smin;
                    break;
            }
        }

        return result;
    }

    public static string? GetDefaultFile()
    {
        var folder = GetScopeFolder();
        var files = Directory.GetFiles(folder, "*.ini");
        return files.Length > 0 ? files[0] : null;
    }
}
