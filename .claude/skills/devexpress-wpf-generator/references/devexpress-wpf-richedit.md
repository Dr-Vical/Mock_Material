# DevExpress WPF Rich Text Editor

## Overview
- Word-inspired rich text editor supporting DOCX, DOC, RTF, HTML, PDF, TXT.
- Built-in Ribbon UI, formatting tools, spell check, track changes.
- Mail merge, TOC, headers/footers, footnotes.

## Key Classes
| Class | Description |
|-------|-------------|
| `RichEditControl` | Main rich text control |
| `Document` | Document model (API) |

- **xmlns**: `dxre="http://schemas.devexpress.com/winfx/2008/xaml/richedit"`
- **NuGet**: `DevExpress.Wpf.RichEdit`

## Basic XAML
```xml
<dxre:RichEditControl CommandBarStyle="Ribbon"
                       DocumentSource="{Binding DocumentPath}"
                       Modified="{Binding IsModified, Mode=TwoWay}"/>
```

## Load/Save Document
```csharp
richEdit.LoadDocument("document.docx", DocumentFormat.OpenXml);
richEdit.SaveDocument("output.pdf", DocumentFormat.Pdf);
```

## Document API
```csharp
var doc = richEdit.Document;
doc.BeginUpdate();
doc.AppendText("Hello World");
var range = doc.InsertText(doc.Range.Start, "Header\n");
doc.CharacterProperties(range).Bold = true;
doc.EndUpdate();
```

## Key Properties
`CommandBarStyle` (None/Ribbon/Bars), `DocumentSource`, `ActiveViewType` (Simple/Draft/PrintLayout), `ReadOnly`.

## Reference
- https://docs.devexpress.com/WPF/7249/controls-and-libraries/rich-text-editor
