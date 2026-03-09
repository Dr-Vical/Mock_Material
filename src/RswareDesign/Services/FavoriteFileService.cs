using System.IO;
using System.Text;
using RswareDesign.Models;

namespace RswareDesign.Services;

public static class FavoriteFileService
{
    public static string GetFavoriteFolder()
    {
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Favorite");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return folder;
    }

    public static void Save(string filePath, IEnumerable<Parameter> favorites)
    {
        var list = favorites.ToList();
        var sb = new StringBuilder();
        sb.AppendLine("[Favorite]");
        sb.AppendLine($"Count={list.Count}");
        sb.AppendLine();

        for (int i = 0; i < list.Count; i++)
        {
            var p = list[i];
            sb.AppendLine($"[{i}]");
            sb.AppendLine($"FtNumber={p.FtNumber}");
            sb.AppendLine($"Name={p.Name}");
            sb.AppendLine($"Value={p.Value}");
            sb.AppendLine($"Unit={p.Unit}");
            sb.AppendLine($"Default={p.Default}");
            sb.AppendLine($"Min={p.Min}");
            sb.AppendLine($"Max={p.Max}");
            sb.AppendLine($"IsFavorite=True");
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    public static List<Parameter> Load(string filePath)
    {
        var result = new List<Parameter>();
        if (!File.Exists(filePath)) return result;

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        Parameter? current = null;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Section header like [0], [1], [Favorite]
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                var section = line[1..^1];
                if (section == "Favorite") continue;
                if (int.TryParse(section, out _))
                {
                    current = new Parameter { IsFavorite = true };
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
                case "FtNumber": current.FtNumber = val; break;
                case "Name": current.Name = val; break;
                case "Value": current.Value = val; break;
                case "Unit": current.Unit = val; break;
                case "Default": current.Default = val; break;
                case "Min": current.Min = val; break;
                case "Max": current.Max = val; break;
                case "IsFavorite": current.IsFavorite = val == "True"; break;
            }
        }

        return result;
    }

    public static string? GetDefaultFile()
    {
        var folder = GetFavoriteFolder();
        var files = Directory.GetFiles(folder, "*.ini");
        return files.Length > 0 ? files[0] : null;
    }
}
