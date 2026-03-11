using CommunityToolkit.Mvvm.ComponentModel;

namespace RswareDesign.Models;

public partial class Parameter : ObservableObject
{
    public string FtNumber { get; set; } = "";
    public string ShortNumber => FtNumber.Replace("Ft-", "");
    public string Name { get; set; } = "";

    [ObservableProperty]
    private string _value = "";
    public string Unit { get; set; } = "";
    public string Default { get; set; } = "";
    public string Min { get; set; } = "";
    public string Max { get; set; } = "";
    public string Access { get; set; } = "r/w";
    public string Group { get; set; } = "";
    public bool IsModified { get; set; }

    /// <summary>
    /// Enum options parsed from CSV Remark column (e.g., "0: Thermal Model Method").
    /// Key = numeric value, Value = display label.
    /// </summary>
    public List<ParameterOption>? Options { get; set; }

    public bool HasOptions => Options != null && Options.Count > 0;

    [ObservableProperty]
    private bool _isFavorite;
}

public class ParameterOption
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
    public string Display => $"{Value}: {Label}";

    public override string ToString() => Display;
}
