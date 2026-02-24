# DevExpress WPF Reporting & Printing

## Overview
- Cross-platform reporting: design-time + runtime report designer.
- Data sources: SQL, Entity Framework, custom objects, Excel.
- Charts, cross-tab, gauge, barcode components embedded in reports.
- Export to PDF, Excel, HTML, Word.

## Key Classes
| Class | Description |
|-------|-------------|
| `DocumentPreviewControl` | Print preview/export control |
| `XtraReport` | Report definition base class |
| `PrintHelper` | Print utility |
| `PrintableControlLink` | Link control to print |

- **xmlns**: `dxp="http://schemas.devexpress.com/winfx/2008/xaml/printing"`
- **NuGet**: `DevExpress.Wpf.Printing`

## Print Preview
```xml
<dxp:DocumentPreviewControl x:Name="preview"/>
```
```csharp
var report = new SalesReport();
preview.DocumentSource = report;
report.CreateDocument();
```

## Print GridControl
```csharp
var link = new PrintableControlLink(gridControl.View);
link.CreateDocument();
link.ShowPrintPreview(this);
```

## Export
```csharp
report.ExportToPdf("report.pdf");
report.ExportToXlsx("report.xlsx");
report.ExportToHtml("report.html");
```

## Reference
- https://docs.devexpress.com/WPF/6190/controls-and-libraries/printing-exporting
