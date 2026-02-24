# DevExpress WPF PDF Viewer

## Overview
- Display PDF files in WPF without external viewer.
- View, navigate, print, search, form editing, annotations.

## Key Classes
| Class | Description |
|-------|-------------|
| `PdfViewerControl` | Main PDF viewer control |

- **xmlns**: `dxpdf="http://schemas.devexpress.com/winfx/2008/xaml/pdf"`
- **NuGet**: `DevExpress.Wpf.PdfViewer`

## Basic XAML
```xml
<dxpdf:PdfViewerControl DocumentSource="Data/Sample.pdf"
                         CommandBarStyle="Bars"
                         ZoomMode="PageLevel"/>
```

## Load from ViewModel
```xml
<dxpdf:PdfViewerControl DocumentSource="{Binding PdfFilePath}"/>
```
```csharp
// Or load from stream.
pdfViewer.DocumentSource = new MemoryStream(pdfBytes);
```

## Key Properties
`DocumentSource`, `CommandBarStyle` (None/Bars/Ribbon), `ZoomMode`, `ZoomFactor`, `CurrentPageNumber`, `PageCount`.

## Printing & Export
```csharp
pdfViewer.Print();
pdfViewer.ExportToImage("output.png");
```

## Reference
- https://docs.devexpress.com/WPF/15082/controls-and-libraries/pdf-viewer
