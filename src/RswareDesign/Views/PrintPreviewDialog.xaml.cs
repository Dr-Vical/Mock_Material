using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.IO;

namespace RswareDesign.Views;

public partial class PrintPreviewDialog : Window
{
    private FlowDocument? _flowDoc;

    public PrintPreviewDialog()
    {
        InitializeComponent();
    }

    public void LoadDocument(FlowDocument flowDoc, double pageWidth = 793.7, double pageHeight = 1122.5)
    {
        _flowDoc = flowDoc;

        // Set page dimensions (A4 at 96 DPI: 793.7 x 1122.5)
        flowDoc.PageWidth = pageWidth;
        flowDoc.PageHeight = pageHeight;
        flowDoc.ColumnWidth = pageWidth;

        // Convert FlowDocument to FixedDocument via XPS in-memory
        var tempPath = Path.Combine(Path.GetTempPath(), $"rsware_preview_{Guid.NewGuid()}.xps");

        try
        {
            using (var xpsDoc = new XpsDocument(tempPath, FileAccess.ReadWrite))
            {
                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                writer.Write(((IDocumentPaginatorSource)flowDoc).DocumentPaginator);
            }

            var fixedDoc = new XpsDocument(tempPath, FileAccess.Read);
            var seq = fixedDoc.GetFixedDocumentSequence();
            Viewer.Document = seq;

            var pageCount = seq?.DocumentPaginator?.PageCount ?? 0;
            TxtPageInfo.Text = $"({pageCount} pages)";

            // Clean up XPS file when window closes
            Closed += (_, _) =>
            {
                Viewer.Document = null;
                fixedDoc.Close();
                try { File.Delete(tempPath); } catch { }
            };
        }
        catch
        {
            try { File.Delete(tempPath); } catch { }
        }
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void BtnPrint_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true && _flowDoc != null)
        {
            _flowDoc.PageWidth = dlg.PrintableAreaWidth;
            _flowDoc.PageHeight = dlg.PrintableAreaHeight;
            _flowDoc.ColumnWidth = _flowDoc.PageWidth - _flowDoc.PagePadding.Left - _flowDoc.PagePadding.Right;

            dlg.PrintDocument(((IDocumentPaginatorSource)_flowDoc).DocumentPaginator, "RSware Parameter Report");
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
