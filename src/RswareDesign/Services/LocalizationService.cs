using System.IO;
using System.Windows;

namespace RswareDesign.Services;

/// <summary>
/// CSV 기반 다국어 리소스 로더.
/// Localization/Strings.csv 파일에서 Key,ko,en 컬럼을 읽어
/// ResourceDictionary에 DynamicResource 키로 등록합니다.
/// </summary>
public static class LocalizationService
{
    private const string CsvRelativePath = "Localization/Strings.csv";
    private static readonly Dictionary<string, (string Ko, string En)> _entries = new();
    private static ResourceDictionary? _currentDict;

    /// <summary>CSV를 한 번 파싱하여 메모리에 보관합니다.</summary>
    public static void Initialize()
    {
        _entries.Clear();

        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CsvRelativePath);
        if (!File.Exists(csvPath)) return;

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2) return; // header + at least 1 row

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // CSV parse: Key,ko,en — values may contain commas inside quotes
            var cols = ParseCsvLine(line);
            if (cols.Length < 3) continue;

            var key = cols[0].Trim();
            var ko = cols[1].Replace("\\n", "\n");
            var en = cols[2].Replace("\\n", "\n");

            _entries[key] = (ko, en);
        }
    }

    /// <summary>지정한 언어로 ResourceDictionary를 생성하여 앱에 적용합니다.</summary>
    public static void ApplyLanguage(bool isKorean)
    {
        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        // Remove previous string dictionaries (both XAML-based and code-generated)
        if (_currentDict != null)
        {
            mergedDicts.Remove(_currentDict);
            _currentDict = null;
        }

        // Also remove XAML-based string dicts if present
        ResourceDictionary? xamlStrings = null;
        foreach (var dict in mergedDicts)
        {
            var src = dict.Source?.OriginalString ?? "";
            if (src.Contains("Strings.ko.xaml") || src.Contains("Strings.en.xaml"))
            {
                xamlStrings = dict;
                break;
            }
        }
        if (xamlStrings != null)
            mergedDicts.Remove(xamlStrings);

        // Build new ResourceDictionary from CSV data
        var newDict = new ResourceDictionary();
        foreach (var (key, (ko, en)) in _entries)
        {
            newDict[key] = isKorean ? ko : en;
        }

        mergedDicts.Add(newDict);
        _currentDict = newDict;
    }

    /// <summary>리소스 키에서 현재 언어의 문자열을 가져옵니다.</summary>
    public static string Get(string key)
    {
        return Application.Current.TryFindResource(key) as string ?? key;
    }

    /// <summary>포맷 문자열 키를 가져와서 인자를 적용합니다.</summary>
    public static string GetFormat(string key, params object[] args)
    {
        var fmt = Get(key);
        return string.Format(fmt, args);
    }

    /// <summary>Simple CSV line parser that handles quoted fields.</summary>
    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // skip escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());

        return result.ToArray();
    }
}
