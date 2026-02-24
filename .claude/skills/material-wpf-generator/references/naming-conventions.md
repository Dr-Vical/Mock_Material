# Naming Conventions — Material Design WPF

## C# Naming Rules

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase | `RswareDesign.ViewModels` |
| Class / Struct | PascalCase | `MainWindowViewModel`, `Parameter` |
| Interface | I + PascalCase | `IThemeService`, `IParameterRepository` |
| Enum | PascalCase (no prefix) | `AccessMode`, `DriveState` |
| Enum members | PascalCase | `ReadWrite`, `ReadOnly` |
| Method | Verb-Object PascalCase | `LoadParameters()`, `SaveToFile()` |
| Property | PascalCase | `SelectedParameter`, `IsConnected` |
| Field (private) | _camelCase | `private string _title;` |
| Field (CommunityToolkit) | _camelCase | `[ObservableProperty] private string _title;` |
| Constant | UPPER_SNAKE_CASE | `const int MAX_RETRY_COUNT = 10;` |
| Boolean | is/has/can prefix | `bool IsConnected`, `bool CanSave` |
| Parameter | camelCase | `void Load(int driveAddress)` |
| Command (generated) | PascalCase + Command | `SaveParametersCommand` (from `[RelayCommand] SaveParameters()`) |

## UI Control Naming Prefixes (x:Name in XAML)

### Standard WPF Controls
| Control | Prefix | Example |
|---------|--------|---------|
| Button | btn | `btnSave` |
| TextBox | txt | `txtSearch` |
| TextBlock | tbk | `tbkStatus` |
| ComboBox | cbx | `cbxSerialPort` |
| CheckBox | chk | `chkShowHelps` |
| DataGrid | dtg | `dtgParameters` |
| TreeView | trv | `trvDrives` |
| TabControl | tab | `tabMain` |
| StackPanel | pnl | `pnlActions` |
| GroupBox | grp | `grpSettings` |
| Image | img | `imgLogo` |
| ProgressBar | pgb | `pgbLoading` |
| ListView | lsv | `lsvErrors` |
| StatusBar | stb | `stbMain` |
| ToggleButton | tgl | `tglTheme` |
| Border | bdr | `bdrContainer` |

### MaterialDesign Controls
| Control | Prefix | Example |
|---------|--------|---------|
| Card | crd | `crdPanel` |
| ColorZone | czn | `cznHeader` |
| DialogHost | dlg | `dlgRoot` |
| PackIcon | ico | `icoStatus` |
| Chip | chp | `chpFilter` |
| Snackbar | snk | `snkNotification` |
| PopupBox | ppb | `ppbMore` |
| DrawerHost | drw | `drwNavigation` |
| TransitioningContent | trn | `trnContent` |

### AvalonDock Controls
| Control | Prefix | Example |
|---------|--------|---------|
| DockingManager | dkm | `dkmMain` |
| LayoutAnchorable | anc | `ancDriveTree` |
| LayoutDocument | doc | `docParameters` |
| LayoutAnchorablePane | anp | `anpLeft` |
| LayoutDocumentPane | dcp | `dcpCenter` |

### Fluent.Ribbon Controls
| Control | Prefix | Example |
|---------|--------|---------|
| Ribbon | rib | `ribMain` |
| RibbonTabItem | rti | `rtiFile` |
| RibbonGroupBox | rgb | `rgbConnection` |
| Fluent:Button | rbn | `rbnConnect` |
| Fluent:ComboBox | rcb | `rcbPort` |
| Fluent:ToggleButton | rtg | `rtgDriveTree` |

### ScottPlot Controls
| Control | Prefix | Example |
|---------|--------|---------|
| WpfPlot | plt | `pltMonitor`, `pltOscilloscope` |

## File Naming

| Type | Pattern | Example |
|------|---------|---------|
| View (Window) | `{Name}Window.xaml` | `MainWindow.xaml` |
| View (UserControl) | `{Name}View.xaml` | `ParameterEditorView.xaml` |
| ViewModel | `{Name}ViewModel.cs` | `ParameterEditorViewModel.cs` |
| Model | `{Name}.cs` | `Parameter.cs` |
| Service Interface | `I{Name}Service.cs` | `IThemeService.cs` |
| Service Implementation | `{Name}Service.cs` | `ThemeService.cs` |
| Theme Dictionary | `{Name}Theme.xaml` | `DarkTheme.xaml` |
| Style Dictionary | `{Name}Styles.xaml` | `ButtonStyles.xaml` |
| Converter | `{Name}Converter.cs` | `AccessModeToEditableConverter.cs` |
| Message (Messenger) | `{Name}Message.cs` | `TreeNodeSelectedMessage.cs` |

## i18n Key Naming

Pattern: `loc.{area}.{target}.{detail}`

| Prefix | Area | Example |
|--------|------|---------|
| `app.*` | Application | `loc.app.title` |
| `menu.*` | Ribbon menu | `loc.menu.file.save` |
| `tree.*` | Drive tree | `loc.tree.online`, `loc.tree.offline` |
| `param.*` | Parameters | `loc.param.header.ftnum`, `loc.param.header.value` |
| `monitor.*` | Monitor | `loc.monitor.title`, `loc.monitor.channel.velocity` |
| `oscilloscope.*` | Oscilloscope | `loc.oscilloscope.trigger` |
| `error.*` | Error log | `loc.error.title`, `loc.error.noErrors` |
| `status.*` | Status bar | `loc.status.connected`, `loc.status.disconnected` |
| `action.*` | Action panel | `loc.action.save`, `loc.action.revert` |
| `dialog.*` | Dialogs | `loc.dialog.confirm`, `loc.dialog.serialSettings` |
| `common.*` | Common | `loc.common.ok`, `loc.common.cancel` |
| `theme.*` | Theme | `loc.theme.dark`, `loc.theme.light` |

## Design Token Key Naming

| Category | Pattern | Example |
|----------|---------|---------|
| Color (brush) | PascalCase + Brush | `PrimaryBrush`, `TextPrimary`, `SurfaceBrush` |
| Font family | FontFamily + Role | `FontFamilyUI`, `FontFamilyCode` |
| Font size | FontSize + Scale | `FontSizeXS`, `FontSizeMD`, `FontSizeXL` |
| Spacing | Category.Scale | `Padding.Panel`, `Margin.FormField` |
| Size | Size.Element | `Size.StatusBarHeight`, `Size.GridRowHeight` |
| Radius | Radius.Scale | `Radius.SM`, `Radius.MD`, `Radius.LG` |
| Style | StyleType.Variant | `ButtonStyle.Primary`, `TextStyle.Heading` |

## Comment Style

```csharp
// Place comments on a separate line, not at the end of code.
// End comment text with a period.
// Insert one space between the delimiter (//) and the comment text.
```
