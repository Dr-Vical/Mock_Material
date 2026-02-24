namespace RswareDesign.Models;

public class Parameter
{
    public string FtNumber { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Unit { get; set; } = "";
    public string Default { get; set; } = "";
    public string Min { get; set; } = "";
    public string Max { get; set; } = "";
    public string Access { get; set; } = "r/w";
    public string Group { get; set; } = "";
    public bool IsModified { get; set; }
}
