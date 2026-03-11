namespace RswareDesign;

public class PrintSettings
{
    public string DocTitle { get; set; } = "";
    public string Company { get; set; } = "";
    public bool IncludeDate { get; set; } = true;

    public bool IncludeDriveModel { get; set; } = true;
    public bool IncludeFirmware { get; set; } = true;
    public bool IncludeConnection { get; set; } = true;

    public bool PrintAllParams { get; set; } = true;
    public bool PrintModifiedOnly { get; set; }
    public bool PrintFavorites { get; set; }
    public bool PrintFaults { get; set; }
    public bool PrintMonitorData { get; set; }

    public string HeaderText { get; set; } = "";
    public string FooterText { get; set; } = "";
    public bool IncludePageNumbers { get; set; } = true;

    public double MarginTop { get; set; } = 20;
    public double MarginBottom { get; set; } = 20;
    public double MarginLeft { get; set; } = 15;
    public double MarginRight { get; set; } = 15;
}
