using System.Windows;
using RswareDesign.Services;
using RswareDesign.ViewModels;

namespace RswareDesign.Views;

public partial class PageSetupDialog : Window
{
    public PageSetupDialog()
    {
        InitializeComponent();
    }

    // ═══ Result properties ═══

    public string DocTitle => TxtDocTitle.Text;
    public string Company => TxtCompany.Text;
    public bool IncludeDate => ChkIncludeDate.IsChecked == true;

    public bool IncludeDriveModel => ChkDriveModel.IsChecked == true;
    public bool IncludeFirmware => ChkFirmware.IsChecked == true;
    public bool IncludeConnection => ChkConnection.IsChecked == true;

    public bool PrintAllParams => ChkAllParams.IsChecked == true;
    public bool PrintModifiedOnly => ChkModifiedOnly.IsChecked == true;
    public bool PrintFavorites => ChkFavorites.IsChecked == true;
    public bool PrintFaults => ChkFaults.IsChecked == true;
    public bool PrintMonitorData => ChkMonitorData.IsChecked == true;

    public string HeaderText => TxtHeader.Text;
    public string FooterText => TxtFooter.Text;
    public bool IncludePageNumbers => ChkPageNumbers.IsChecked == true;

    public double MarginTop => double.TryParse(TxtMarginTop.Text, out var v) ? v : 20;
    public double MarginBottom => double.TryParse(TxtMarginBottom.Text, out var v) ? v : 20;
    public double MarginLeft => double.TryParse(TxtMarginLeft.Text, out var v) ? v : 15;
    public double MarginRight => double.TryParse(TxtMarginRight.Text, out var v) ? v : 15;

    /// <summary>
    /// Set externally by MainWindow so preview can access panels.
    /// </summary>
    public Dictionary<string, CompareParameterPanel>? Panels { get; set; }

    private void BtnPreview_Click(object sender, RoutedEventArgs e)
    {
        var settings = BuildSettings();
        var vm = Owner?.DataContext as MainWindowViewModel;
        var panels = Panels ?? new Dictionary<string, CompareParameterPanel>();

        var doc = PrintDocumentBuilder.Build(settings, vm, panels);

        var preview = new PrintPreviewDialog { Owner = this };
        preview.LoadDocument(doc);
        preview.ShowDialog();
    }

    private PrintSettings BuildSettings() => new()
    {
        DocTitle = DocTitle,
        Company = Company,
        IncludeDate = IncludeDate,
        IncludeDriveModel = IncludeDriveModel,
        IncludeFirmware = IncludeFirmware,
        IncludeConnection = IncludeConnection,
        PrintAllParams = PrintAllParams,
        PrintModifiedOnly = PrintModifiedOnly,
        PrintFavorites = PrintFavorites,
        PrintFaults = PrintFaults,
        PrintMonitorData = PrintMonitorData,
        HeaderText = HeaderText,
        FooterText = FooterText,
        IncludePageNumbers = IncludePageNumbers,
        MarginTop = MarginTop,
        MarginBottom = MarginBottom,
        MarginLeft = MarginLeft,
        MarginRight = MarginRight,
    };

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
