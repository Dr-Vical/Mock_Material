using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using RswareDesign.Models;
using RswareDesign.ViewModels;
using RswareDesign.Views;

namespace RswareDesign.Services;

public static class PrintDocumentBuilder
{
    public static FlowDocument Build(
        PrintSettings settings,
        MainWindowViewModel? vm,
        Dictionary<string, CompareParameterPanel> panels)
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 10,
            Foreground = Brushes.Black,
        };

        // ═══ Document Title ═══
        if (!string.IsNullOrEmpty(settings.DocTitle))
        {
            doc.Blocks.Add(new Paragraph(new Run(settings.DocTitle))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4),
            });
        }

        // ═══ Company + Date ═══
        if (!string.IsNullOrEmpty(settings.Company) || settings.IncludeDate)
        {
            var subHeader = "";
            if (!string.IsNullOrEmpty(settings.Company))
                subHeader += settings.Company;
            if (settings.IncludeDate)
            {
                if (subHeader.Length > 0) subHeader += "  |  ";
                subHeader += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            doc.Blocks.Add(new Paragraph(new Run(subHeader))
            {
                FontSize = 10,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 12),
            });
        }

        // ═══ Header Text ═══
        if (!string.IsNullOrEmpty(settings.HeaderText))
        {
            doc.Blocks.Add(new Paragraph(new Run(settings.HeaderText))
            {
                FontSize = 9,
                Foreground = Brushes.DarkGray,
                Margin = new Thickness(0, 0, 0, 8),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(0, 0, 0, 4),
            });
        }

        // ═══ Drive Info ═══
        if (vm != null && (settings.IncludeDriveModel || settings.IncludeFirmware || settings.IncludeConnection))
        {
            AddSectionHeader(doc, "Drive Information");

            var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 12) };
            table.Columns.Add(new TableColumn { Width = new GridLength(140) });
            table.Columns.Add(new TableColumn { Width = GridLength.Auto });
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            if (settings.IncludeDriveModel)
                AddInfoRow(rowGroup, "Drive Model", vm.DriveInfo);
            if (settings.IncludeFirmware)
                AddInfoRow(rowGroup, "Firmware", vm.FirmwareVersion);
            if (settings.IncludeConnection)
                AddInfoRow(rowGroup, "Port / Status", $"{vm.SelectedPort} / {vm.ConnectionStatus}");

            doc.Blocks.Add(table);
        }

        // ═══ Parameters ═══
        if (settings.PrintAllParams || settings.PrintModifiedOnly)
        {
            foreach (var kvp in panels)
            {
                var panelId = kvp.Key;
                var panel = kvp.Value;
                var parameters = panel.PanelParameters;
                if (parameters == null || parameters.Count == 0) continue;

                var items = settings.PrintModifiedOnly
                    ? parameters.Where(p => p.IsModified || p.Value != p.Default).ToList()
                    : parameters.ToList();

                if (items.Count == 0) continue;

                var label = settings.PrintModifiedOnly
                    ? $"Parameters [{panelId}] — Modified Only ({items.Count})"
                    : $"Parameters [{panelId}] ({items.Count})";

                AddSectionHeader(doc, label);
                AddParameterTable(doc, items);
            }
        }

        // ═══ Favorites ═══
        if (settings.PrintFavorites && vm?.FavoriteParameters != null && vm.FavoriteParameters.Count > 0)
        {
            AddSectionHeader(doc, $"Favorite Parameters ({vm.FavoriteParameters.Count})");
            AddParameterTable(doc, vm.FavoriteParameters.ToList());
        }

        // ═══ Footer Text ═══
        if (!string.IsNullOrEmpty(settings.FooterText))
        {
            doc.Blocks.Add(new Paragraph(new Run(settings.FooterText))
            {
                FontSize = 9,
                Foreground = Brushes.DarkGray,
                Margin = new Thickness(0, 12, 0, 0),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0.5, 0, 0),
                Padding = new Thickness(0, 4, 0, 0),
            });
        }

        return doc;
    }

    private static void AddSectionHeader(FlowDocument doc, string text)
    {
        doc.Blocks.Add(new Paragraph(new Run(text))
        {
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 8, 0, 4),
            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
            Padding = new Thickness(6, 2, 6, 2),
        });
    }

    private static void AddInfoRow(TableRowGroup group, string label, string value)
    {
        var row = new TableRow();
        row.Cells.Add(new TableCell(new Paragraph(new Run(label))
        {
            FontWeight = FontWeights.SemiBold,
            FontSize = 10,
            Margin = new Thickness(0, 1, 0, 1),
        }));
        row.Cells.Add(new TableCell(new Paragraph(new Run(value))
        {
            FontSize = 10,
            Margin = new Thickness(0, 1, 0, 1),
        }));
        group.Rows.Add(row);
    }

    private static void AddParameterTable(FlowDocument doc, List<Parameter> parameters)
    {
        var table = new Table
        {
            CellSpacing = 0,
            Margin = new Thickness(0, 0, 0, 12),
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
        };

        // Columns: FtNumber, Name, Value, Unit, Default, Min, Max
        table.Columns.Add(new TableColumn { Width = new GridLength(70) });   // FtNumber
        table.Columns.Add(new TableColumn { Width = new GridLength(180) });  // Name
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });   // Value
        table.Columns.Add(new TableColumn { Width = new GridLength(50) });   // Unit
        table.Columns.Add(new TableColumn { Width = new GridLength(70) });   // Default
        table.Columns.Add(new TableColumn { Width = new GridLength(60) });   // Min
        table.Columns.Add(new TableColumn { Width = new GridLength(60) });   // Max

        var rowGroup = new TableRowGroup();
        table.RowGroups.Add(rowGroup);

        // Header row
        var headerRow = new TableRow { Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)) };
        foreach (var h in new[] { "FT No.", "Parameter", "Value", "Unit", "Default", "Min", "Max" })
        {
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(h))
            {
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0),
                Padding = new Thickness(4, 2, 4, 2),
            }));
        }
        rowGroup.Rows.Add(headerRow);

        // Data rows
        bool alt = false;
        foreach (var p in parameters)
        {
            var row = new TableRow();
            if (alt)
                row.Background = new SolidColorBrush(Color.FromRgb(248, 248, 248));

            var isModified = p.Value != p.Default;

            AddCell(row, p.ShortNumber, 8);
            AddCell(row, p.Name, 8);
            AddCell(row, p.Value, 8, isModified ? FontWeights.Bold : FontWeights.Normal,
                isModified ? Brushes.DarkRed : Brushes.Black);
            AddCell(row, p.Unit, 8);
            AddCell(row, p.Default, 8);
            AddCell(row, p.Min, 8);
            AddCell(row, p.Max, 8);

            rowGroup.Rows.Add(row);
            alt = !alt;
        }

        doc.Blocks.Add(table);
    }

    private static void AddCell(TableRow row, string text, double fontSize,
        FontWeight? weight = null, Brush? foreground = null)
    {
        row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? ""))
        {
            FontSize = fontSize,
            FontWeight = weight ?? FontWeights.Normal,
            Foreground = foreground ?? Brushes.Black,
            Margin = new Thickness(0),
            Padding = new Thickness(4, 1, 4, 1),
        })
        {
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0.5, 0.5),
        });
    }
}
