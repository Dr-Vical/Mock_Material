namespace RswareDesign.Models;

public class ActionButton
{
    public string Label { get; set; } = "";
    public string IconKind { get; set; } = "Help";
    public bool IsSeparator { get; set; }
    public string Style { get; set; } = "Primary"; // "Primary" | "Secondary" | "Outlined"
}
