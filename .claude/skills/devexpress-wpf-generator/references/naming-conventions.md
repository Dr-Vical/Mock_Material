# Naming Conventions

## C# Naming Rules

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase | `ProjectName.ViewModels` |
| Class / Struct | PascalCase | `MainViewModel`, `AnimationInfo` |
| Interface | I + PascalCase | `IDataService` |
| Enum | e + PascalCase | `eDirection` |
| Enum members | PascalCase | `PositiveDirection` |
| Method | Verb-Object PascalCase | `LoadData()`, `SaveChanges()` |
| Property | PascalCase | `CustomerName` |
| Field / Variable | camelCase | `elapsedTimeInDays` |
| Constant | UPPER_SNAKE_CASE | `const int MAX_RETRY_COUNT = 10;` |
| Boolean variable | is/has/can/should prefix | `bool isConnected` |
| Loop counter | i, j, k (simple loops) | `for (int i = 0; ...)` |
| Parameter | camelCase | `void DoSomething(int itemCount)` |

## UI Control Naming Prefixes

Use these prefixes for `x:Name` in XAML or variable names in code-behind.

### Standard WPF Controls

| Control | Prefix | Example |
|---------|--------|---------|
| Button | btn | `btnSave` |
| TextBox | txt | `txtCustomerName` |
| Label | lbl | `lblStatus` |
| ComboBox | cbx | `cbxCategory` |
| CheckBox | chk | `chkIsActive` |
| DataGrid | dtg | `dtgOrders` |
| TreeView | trv | `trvDepartments` |
| TabControl | tab | `tabMain` |
| Panel | pnl | `pnlContent` |
| GroupBox | grp | `grpSettings` |
| Image | img | `imgLogo` |
| ProgressBar | pgb | `pgbLoading` |
| DateTimePicker | dtp | `dtpStartDate` |
| RichTextBox | rtb | `rtbDescription` |
| ListView | lsv | `lsvItems` |
| Timer | tmr | `tmrRefresh` |
| Form | frm | `frmMain` |
| UserControl | usc | `uscDashboard` |

### DevExpress Controls

| Control | Prefix | Example |
|---------|--------|---------|
| GridControl | grd | `grdProducts` |
| TreeListControl | trl | `trlDepartments` |
| RibbonControl | rib | `ribMain` |
| DockLayoutManager | dlm | `dlmWorkspace` |
| LayoutControl | lyc | `lycForm` |
| ChartControl | cht | `chtSales` |
| SchedulerControl | sch | `schCalendar` |
| GanttControl | gnt | `gntProject` |
| SpreadsheetControl | sps | `spsEditor` |
| RichEditControl | rte | `rteDocument` |
| DiagramControl | dgm | `dgmFlow` |
| PivotGridControl | pvt | `pvtAnalysis` |
| MapControl | map | `mapRegion` |
| CircularGaugeControl | gau | `gauTemperature` |
| HeatmapControl | hmp | `hmpActivity` |
| TreeMapControl | tmp | `tmpHierarchy` |
| PdfViewerControl | pdf | `pdfViewer` |
| PropertyGridControl | prg | `prgSettings` |
| AccordionControl | acd | `acdNavigation` |
| NavBarControl | nav | `navMenu` |
| DXTabControl | dtb | `dtbPages` |

## Comment Style

```csharp
// Place comments on a separate line, not at the end of code.
// End comment text with a period.
// Insert one space between the delimiter (//) and the comment text.
```

## String Handling

```csharp
// Use string interpolation for short concatenation.
string displayName = $"{lastName} {firstName}";

// Use StringBuilder for large text assembly.
StringBuilder builder = new StringBuilder();
for (int i = 0; i < 10000; i++)
{
    builder.Append(phrase);
}

// Use raw string literals on .NET 8 (C# 12).
string rawMessage = """
    This is a long message.
    No need to escape \n or \t.
    """;
```

## Layout Rules

```csharp
// One declaration per line.
string text1 = "one";
string text2 = "two";

// Break long expressions with indentation.
double result = data1 + data2 + data3 + data4 +
                data5;
```
