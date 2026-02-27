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

    [ObservableProperty]
    private bool _isFavorite;
}
