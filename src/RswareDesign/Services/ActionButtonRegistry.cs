using System.IO;
using RswareDesign.Models;

namespace RswareDesign.Services;

public static class ActionButtonRegistry
{
    private static readonly Dictionary<string, List<ActionButton>> _cache = new();
    private static bool _loaded;

    public static List<ActionButton> GetForNodeType(string nodeType)
    {
        EnsureLoaded();

        if (_cache.TryGetValue(nodeType, out var buttons))
            return buttons;

        if (_cache.TryGetValue("_default", out var fallback))
            return fallback;

        return new List<ActionButton> { new() { Label = "Refresh", IconKind = "Refresh", Style = "Primary" } };
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ActionButtons.csv");
        if (!File.Exists(csvPath)) return;

        foreach (var line in File.ReadLines(csvPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var cols = trimmed.Split(new[] { ',' }, 4);
            if (cols.Length < 2) continue;

            var node = cols[0].Trim();
            if (node == "NodeType") continue; // skip header

            if (!_cache.ContainsKey(node))
                _cache[node] = new List<ActionButton>();

            var label = cols.Length > 1 ? cols[1].Trim() : "";
            var iconKind = cols.Length > 2 ? cols[2].Trim() : "";
            var style = cols.Length > 3 ? cols[3].Trim() : "Primary";

            if (label == "---")
            {
                _cache[node].Add(new ActionButton { IsSeparator = true });
            }
            else
            {
                _cache[node].Add(new ActionButton
                {
                    Label = label,
                    IconKind = string.IsNullOrEmpty(iconKind) ? "Help" : iconKind,
                    Style = string.IsNullOrEmpty(style) ? "Primary" : style,
                });
            }
        }
    }

    /// <summary>
    /// Force reload from CSV (e.g., after file change).
    /// </summary>
    public static void Reload()
    {
        _cache.Clear();
        _loaded = false;
    }
}
