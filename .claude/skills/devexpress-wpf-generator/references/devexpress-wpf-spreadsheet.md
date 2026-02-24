# DevExpress WPF Spreadsheet

## Overview
- Excel-style spreadsheet: 400+ built-in functions, custom functions.
- Cell formatting, conditional formatting, data validation.
- Charts, sparklines, pivot tables, data binding.
- XLSX/XLS/CSV format support, PDF/HTML export, AES encryption.

## Key Classes
| Class | Description |
|-------|-------------|
| `SpreadsheetControl` | Main spreadsheet control |
| `IWorkbook` | Workbook interface |
| `Worksheet` | Worksheet |
| `Cell` / `CellRange` | Cell and range |

- **xmlns**: `dxsps="http://schemas.devexpress.com/winfx/2008/xaml/spreadsheet"`
- **NuGet**: `DevExpress.Wpf.Spreadsheet`

## Basic XAML
```xml
<dxsps:SpreadsheetControl CommandBarStyle="Ribbon"
                            DocumentSource="Data/Template.xlsx"/>
```

## Workbook API
```csharp
var workbook = spreadsheet.Document;
var sheet = workbook.Worksheets[0];

sheet.Cells["A1"].Value = "Product";
sheet.Cells["B1"].Value = "Price";
sheet.Cells["A2"].Value = "Widget";
sheet.Cells["B2"].Value = 29.99;

sheet.Cells["B2"].NumberFormat = "$#,##0.00";
sheet.Range["A1:B1"].Font.Bold = true;
```

## Load/Save
```csharp
spreadsheet.LoadDocument("data.xlsx");
spreadsheet.SaveDocument("output.xlsx", DocumentFormat.Xlsx);
spreadsheet.Document.ExportToPdf("output.pdf");
```

## Key Properties
`CommandBarStyle` (None/Ribbon/Bars), `DocumentSource`, `ReadOnly`, `ShowFormulaBar`.

## Reference
- https://docs.devexpress.com/WPF/15968/controls-and-libraries/spreadsheet
