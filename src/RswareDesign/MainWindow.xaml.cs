using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ControlzEx.Theming;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using AvalonDock.Layout;
using RswareDesign.Services;
using RswareDesign.ViewModels;
using RswareDesign.Views;

namespace RswareDesign;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<OpenOscilloscopeMessage>(this, (_, _) =>
        {
            var dialog = new OscilloscopeDialog { Owner = this };
            dialog.ShowDialog();
        });

        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (_, msg) =>
        {
            SwitchTheme(msg.ThemeName);
        });

        WeakReferenceMessenger.Default.Register<HueChangedMessage>(this, (_, msg) =>
        {
            UpdateThemeColors(msg.Hue, msg.Saturation);
        });

        WeakReferenceMessenger.Default.Register<FontSizeChangedMessage>(this, (_, msg) =>
        {
            UpdateFontSizes(msg.FontSize);
        });

        WeakReferenceMessenger.Default.Register<ShowAdminPasswordMessage>(this, (_, msg) =>
        {
            var dialog = new AdminPasswordDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                // Password verified — open admin feature
            }
        });

        WeakReferenceMessenger.Default.Register<ComparePanelChangedMessage>(this, (_, msg) =>
        {
            UpdateComparePanels();
        });

        WeakReferenceMessenger.Default.Register<TreeNodeSelectedMessage>(this, (_, msg) =>
        {
            // If a specific drive is active but its panel doesn't exist, show warning
            if (msg.ActiveDrive != null && !_comparePanels.ContainsKey(msg.ActiveDrive))
            {
                ConfirmActionDialog.Info(this,
                    LocalizationService.Get("loc.drive.nopanel.title"),
                    LocalizationService.Get("loc.drive.nopanel.msg").Replace("\\n", "\n"),
                    MaterialDesignThemes.Wpf.PackIconKind.InformationOutline,
                    "WarningBrush");
                return;
            }

            // Refresh per-panel data + action buttons when tree selection changes
            var newButtons = ActionButtonRegistry.GetForNodeType(msg.NodeType);
            var vm = DataContext as MainWindowViewModel;
            foreach (var (id, panel) in _comparePanels)
            {
                // If a specific drive is active, only update that drive's panel
                if (msg.ActiveDrive != null && id != msg.ActiveDrive)
                    continue;

                if (msg.NodeType == "Favorites" && vm != null)
                {
                    // Favorites: bind directly to VM's FavoriteParameters (shared collection)
                    panel.PanelParameters = vm.FavoriteParameters;
                }
                else
                {
                    panel.PanelParameters = CsvParameterLoader.LoadForPanel(msg.NodeType, id);
                    SyncFavoriteState(panel.PanelParameters, vm);
                    WatchPanelFavorites(panel.PanelParameters, vm);
                }
                panel.TotalCount = panel.PanelParameters?.Count ?? 0;
                panel.LoadedCount = 0;
                panel.StartLoadingAnimation();
                panel.ActionButtons = newButtons;
                panel.RebuildActionButtons();
            }
        });

        WeakReferenceMessenger.Default.Register<ToggleMonitorSectionMessage>(this, (_, msg) =>
        {
            ToggleMonitorSection(msg.Section, msg.DriveId);
        });

        WeakReferenceMessenger.Default.Register<ShowClearFavoritesConfirmMessage>(this, (_, _) =>
        {
            if (ConfirmActionDialog.Ask(this,
                    LocalizationService.Get("loc.monitor.confirm.clearfav.title"),
                    LocalizationService.Get("loc.monitor.confirm.clearfav.msg"),
                    MaterialDesignThemes.Wpf.PackIconKind.StarRemoveOutline,
                    LocalizationService.Get("loc.monitor.confirm.clearfav.btn"), "WarningBrush")
                && DataContext is MainWindowViewModel vm)
            {
                vm.ExecuteClearAllFavorites();
            }
        });

        WeakReferenceMessenger.Default.Register<ActionButtonClickedMessage>(this, (_, msg) =>
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm == null) return;

            switch (msg.Label)
            {
                case "Save Favorite":
                    SaveFavorites(vm);
                    break;
                case "Load Favorite":
                    LoadFavorites(vm);
                    break;
                case "Export":
                case "Export Log":
                    ExportParameters(msg.PanelId);
                    break;
                case "Import":
                    ImportParameters(msg.PanelId);
                    break;
            }
        });

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (_, msg) =>
        {
            LocalizationService.ApplyLanguage(msg.IsKorean);
        });

        WeakReferenceMessenger.Default.Register<DisableAllActivatedMessage>(this, (_, _) =>
        {
            PlayDisableAllBorderAnimation();
        });

        WeakReferenceMessenger.Default.Register<ShowExitConfirmMessage>(this, (_, _) =>
        {
            var dialog = new ConfirmExitDialog { Owner = this };
            if (dialog.ShowDialog() == true)
                Application.Current.Shutdown();
        });

        errorLogPane.Closing += (s, args) =>
        {
            args.Cancel = true;
            errorLogPane.Hide();
            if (DataContext is MainWindowViewModel vmErr)
                vmErr.IsErrorLogVisible = false;
        };

        // Sync VM toggle properties → AvalonDock panes & layout
        if (DataContext is MainWindowViewModel vmInit)
        {
            vmInit.PropertyChanged += (s, e) =>
            {
                if (s is not MainWindowViewModel vm2) return;

                switch (e.PropertyName)
                {
                    case nameof(MainWindowViewModel.IsErrorLogVisible):
                        if (vm2.IsErrorLogVisible) errorLogPane.Show();
                        else errorLogPane.Hide();
                        break;

                    case nameof(MainWindowViewModel.IsDriveTreeVisible):
                        if (vm2.IsDriveTreeVisible) driveTreePane.Show();
                        else driveTreePane.Hide();
                        break;

                    case nameof(MainWindowViewModel.IsMainPanelVisible):
                        // Only act if state doesn't match
                        if (vm2.IsMainPanelVisible && _isParamCollapsed)
                            BtnCollapseParams_Click(this, new RoutedEventArgs()); // expand
                        else if (!vm2.IsMainPanelVisible && !_isParamCollapsed)
                            BtnCollapseParams_Click(this, new RoutedEventArgs()); // collapse
                        break;
                }
            };

            // Sync DriveTree pane hide → VM toggle off
            driveTreePane.Hiding += (_, args) =>
            {
                vmInit.IsDriveTreeVisible = false;
            };
        }

        // Initialize Panel A on startup + apply window chrome
        Loaded += (_, _) =>
        {
            // Auto-load favorites from file if exists
            if (DataContext is MainWindowViewModel vmLoad)
            {
                var defaultFile = FavoriteFileService.GetDefaultFile();
                if (defaultFile != null)
                {
                    var loaded = FavoriteFileService.Load(defaultFile);
                    foreach (var p in loaded)
                        vmLoad.FavoriteParameters.Add(p);
                }
            }

            UpdateComparePanels();
            ApplyWindowChrome(isDark: true); // default is dark theme
            ApplyAvalonDockHeaderColor();

            // Monitor area starts hidden (show via Graph/Control button click)
            monitorPanelGrid.Visibility = Visibility.Collapsed;
            MonitorColumn.Width = new GridLength(0);
            CenterSplitter.Visibility = Visibility.Collapsed;
            CenterSplitterColumn.Width = new GridLength(0);

            // (tiled watermark removed — single fixed logo only)
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  SAVE / LOAD FAVORITES
    // ═══════════════════════════════════════════════════════════

    private void SaveFavorites(MainWindowViewModel vm)
    {
        if (vm.FavoriteParameters.Count == 0) return;

        var folder = FavoriteFileService.GetFavoriteFolder();
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            InitialDirectory = folder,
            Filter = "Favorite files (*.ini)|*.ini",
            DefaultExt = ".ini",
            FileName = "favorites"
        };
        if (dlg.ShowDialog(this) == true)
        {
            FavoriteFileService.Save(dlg.FileName, vm.FavoriteParameters);
        }
    }

    private void LoadFavorites(MainWindowViewModel vm)
    {
        var folder = FavoriteFileService.GetFavoriteFolder();
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            InitialDirectory = folder,
            Filter = "Favorite files (*.ini)|*.ini",
            DefaultExt = ".ini"
        };
        if (dlg.ShowDialog(this) == true)
        {
            var loaded = FavoriteFileService.Load(dlg.FileName);
            vm.FavoriteParameters.Clear();
            foreach (var p in loaded)
                vm.FavoriteParameters.Add(p);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  RESCAN OPTION
    // ═══════════════════════════════════════════════════════════

    private void BtnRescanOption_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new RescanOptionDialog { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            var vm = DataContext as MainWindowViewModel;
            vm?.AddStatus("Rescan option updated");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  COMMUNICATION SETTINGS
    // ═══════════════════════════════════════════════════════════

    private void BtnComSetting_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        var dlg = new ComSettingDialog { Owner = this };
        dlg.SetCurrentValues(vm?.SelectedPort ?? "COM3", 115200);

        if (dlg.ShowDialog() == true && vm != null)
        {
            vm.SelectedPort = dlg.SelectedPort;
            vm.AddStatus($"COM: {dlg.SelectedPort} @ {dlg.BaudRate}, {dlg.CommType}, Addr={dlg.DriveAddress}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  PRINT / PAGE SETUP
    // ═══════════════════════════════════════════════════════════

    private PrintSettings? _pageSettings;
    private System.Windows.Controls.PrintDialog? _printDialog;

    private void BtnPageSetup_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new PageSetupDialog { Owner = this, Panels = _comparePanels };

        // Restore previous settings
        if (_pageSettings != null)
        {
            dlg.TxtDocTitle.Text = _pageSettings.DocTitle;
            dlg.TxtCompany.Text = _pageSettings.Company;
            dlg.ChkIncludeDate.IsChecked = _pageSettings.IncludeDate;
            dlg.ChkDriveModel.IsChecked = _pageSettings.IncludeDriveModel;
            dlg.ChkFirmware.IsChecked = _pageSettings.IncludeFirmware;
            dlg.ChkConnection.IsChecked = _pageSettings.IncludeConnection;
            dlg.ChkAllParams.IsChecked = _pageSettings.PrintAllParams;
            dlg.ChkModifiedOnly.IsChecked = _pageSettings.PrintModifiedOnly;
            dlg.ChkFavorites.IsChecked = _pageSettings.PrintFavorites;
            dlg.ChkFaults.IsChecked = _pageSettings.PrintFaults;
            dlg.ChkMonitorData.IsChecked = _pageSettings.PrintMonitorData;
            dlg.TxtHeader.Text = _pageSettings.HeaderText;
            dlg.TxtFooter.Text = _pageSettings.FooterText;
            dlg.ChkPageNumbers.IsChecked = _pageSettings.IncludePageNumbers;
            dlg.TxtMarginTop.Text = _pageSettings.MarginTop.ToString();
            dlg.TxtMarginBottom.Text = _pageSettings.MarginBottom.ToString();
            dlg.TxtMarginLeft.Text = _pageSettings.MarginLeft.ToString();
            dlg.TxtMarginRight.Text = _pageSettings.MarginRight.ToString();
        }

        if (dlg.ShowDialog() == true)
        {
            _pageSettings = new PrintSettings
            {
                DocTitle = dlg.DocTitle,
                Company = dlg.Company,
                IncludeDate = dlg.IncludeDate,
                IncludeDriveModel = dlg.IncludeDriveModel,
                IncludeFirmware = dlg.IncludeFirmware,
                IncludeConnection = dlg.IncludeConnection,
                PrintAllParams = dlg.PrintAllParams,
                PrintModifiedOnly = dlg.PrintModifiedOnly,
                PrintFavorites = dlg.PrintFavorites,
                PrintFaults = dlg.PrintFaults,
                PrintMonitorData = dlg.PrintMonitorData,
                HeaderText = dlg.HeaderText,
                FooterText = dlg.FooterText,
                IncludePageNumbers = dlg.IncludePageNumbers,
                MarginTop = dlg.MarginTop,
                MarginBottom = dlg.MarginBottom,
                MarginLeft = dlg.MarginLeft,
                MarginRight = dlg.MarginRight,
            };
        }
    }

    private void BtnPrintSetup_Click(object sender, RoutedEventArgs e)
    {
        _printDialog ??= new System.Windows.Controls.PrintDialog();
        _printDialog.ShowDialog();
    }

    private void BtnPrint_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        var settings = _pageSettings ?? new PrintSettings();
        var doc = PrintDocumentBuilder.Build(settings, vm, _comparePanels);

        doc.PagePadding = new Thickness(
            settings.MarginLeft * 96 / 25.4,
            settings.MarginTop * 96 / 25.4,
            settings.MarginRight * 96 / 25.4,
            settings.MarginBottom * 96 / 25.4);

        var preview = new PrintPreviewDialog { Owner = this };
        preview.LoadDocument(doc);
        preview.ShowDialog();
    }

    // ═══════════════════════════════════════════════════════════
    //  EXPORT / IMPORT PARAMETERS
    // ═══════════════════════════════════════════════════════════

    private void ExportParameters(string panelId)
    {
        if (!_comparePanels.TryGetValue(panelId, out var panel)) return;
        var parameters = panel.PanelParameters;
        if (parameters == null || parameters.Count == 0) return;

        var vm = DataContext as MainWindowViewModel;
        var nodeType = vm?.SelectedNodeType ?? "Parameters";

        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv",
            FileName = $"{vm?.DriveInfo}_{nodeType}_{panelId}_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (dlg.ShowDialog(this) == true)
        {
            int count = ParameterFileService.Export(dlg.FileName, parameters,
                vm?.DriveInfo ?? "", vm?.FirmwareVersion ?? "");
            vm?.AddStatus($"Export: {count} parameters → {System.IO.Path.GetFileName(dlg.FileName)}");
        }
    }

    private void ImportParameters(string panelId)
    {
        if (!_comparePanels.TryGetValue(panelId, out var panel)) return;
        var parameters = panel.PanelParameters;
        if (parameters == null) return;

        var vm = DataContext as MainWindowViewModel;

        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv"
        };

        if (dlg.ShowDialog(this) == true)
        {
            var (overrides, fileDrive, fileFirmware) = ParameterFileService.Import(dlg.FileName);

            // Validate drive type match
            var currentDrive = vm?.DriveInfo ?? "";
            if (!string.IsNullOrEmpty(fileDrive) && !string.IsNullOrEmpty(currentDrive)
                && fileDrive != currentDrive)
            {
                if (!ConfirmActionDialog.Ask(this,
                    "Import Warning",
                    $"Drive mismatch!\n\nFile: {fileDrive} (FW: {fileFirmware})\nCurrent: {currentDrive} (FW: {vm?.FirmwareVersion})\n\nContinue import?",
                    MaterialDesignThemes.Wpf.PackIconKind.AlertCircleOutline,
                    "Import",
                    "WarningBrush")) return;
            }

            var (restored, overridden) = ParameterFileService.ApplyImport(parameters, overrides);
            vm?.AddStatus($"Import: {overridden} applied, {restored} restored ({System.IO.Path.GetFileName(dlg.FileName)})");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  COMPARE PANEL MANAGEMENT (A/B/C/D)
    // ═══════════════════════════════════════════════════════════

    private readonly Dictionary<string, CompareParameterPanel> _comparePanels = new();
    private readonly HashSet<string> _newlyAddedPanels = new();

    private void UpdateComparePanels()
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        var panelStates = new Dictionary<string, bool>
        {
            ["A"] = vm.IsPanelAVisible,
            ["B"] = vm.IsPanelBVisible,
            ["C"] = vm.IsPanelCVisible,
            ["D"] = vm.IsPanelDVisible,
        };

        // Remove panels that are no longer visible
        foreach (var id in _comparePanels.Keys.ToList())
        {
            if (!panelStates.GetValueOrDefault(id))
            {
                _comparePanels.Remove(id);
            }
        }

        // Add panels that should be visible
        foreach (var (id, visible) in panelStates)
        {
            if (visible && !_comparePanels.ContainsKey(id))
            {
                string nodeType = vm.SelectedNodeType;
                System.Collections.ObjectModel.ObservableCollection<Models.Parameter> panelParams;
                if (nodeType == "Favorites")
                {
                    panelParams = vm.FavoriteParameters;
                }
                else
                {
                    panelParams = CsvParameterLoader.LoadForPanel(nodeType, id);
                    SyncFavoriteState(panelParams, vm);
                    WatchPanelFavorites(panelParams, vm);
                }
                var panel = new CompareParameterPanel
                {
                    PanelLabel = id,
                    DataContext = vm,
                    HeaderBrush = Application.Current.TryFindResource($"Panel{id}Brush") as Brush,
                    LabelBrush = Application.Current.TryFindResource($"Panel{id}Accent") as Brush,
                    PanelParameters = panelParams,
                    TotalCount = panelParams.Count,
                    LoadedCount = 0,
                    ActionButtons = ActionButtonRegistry.GetForNodeType(nodeType),
                };
                panel.CanCloseCheck = () => _comparePanels.Count > 1;
                panel.CloseRequested += (s, _) =>
                {
                    // Last panel: collapse param area instead of closing
                    if (_comparePanels.Count <= 1)
                    {
                        if (!_isParamCollapsed)
                            BtnCollapseParams_Click(this, new RoutedEventArgs());
                        return;
                    }

                    switch (id)
                    {
                        case "A": vm.IsPanelAVisible = false; break;
                        case "B": vm.IsPanelBVisible = false; break;
                        case "C": vm.IsPanelCVisible = false; break;
                        case "D": vm.IsPanelDVisible = false; break;
                    }
                };
                _comparePanels[id] = panel;
                _newlyAddedPanels.Add(id);
            }
        }

        RebuildCenterLayout();

        // Auto-expand param area when panels are added while collapsed
        if (_comparePanels.Count > 0 && _isParamCollapsed)
        {
            _isParamCollapsed = false;
            ParamExpandedArea.Visibility = Visibility.Visible;
            ParamCollapsedBar.Visibility = Visibility.Collapsed;
            ParamContentColumn.Width = new GridLength(1, GridUnitType.Star);
            ParamPanelColumn.Width = new GridLength(1, GridUnitType.Star);

            if (_isMonitorVisible)
            {
                CenterSplitterColumn.Width = GridLength.Auto;
                CenterSplitter.Visibility = Visibility.Visible;
                MonitorColumn.Width = new GridLength(2, GridUnitType.Star);
            }
        }
    }

    private void PlayDisableAllBorderAnimation()
    {
        // Rotating red border on the DisableAll button using DashOffset animation
        var errorBrush = Application.Current.TryFindResource("ErrorBrush") as Brush ?? Brushes.Red;
        DisableAllBorderAnim.BorderBrush = errorBrush;

        var sb = new System.Windows.Media.Animation.Storyboard();

        // Fade in border
        var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
        System.Windows.Media.Animation.Storyboard.SetTarget(fadeIn, DisableAllBorderAnim);
        System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeIn, new PropertyPath(OpacityProperty));

        // Hold visible
        var hold = new System.Windows.Media.Animation.DoubleAnimation(1, 1, TimeSpan.FromMilliseconds(1200))
        { BeginTime = TimeSpan.FromMilliseconds(150) };
        System.Windows.Media.Animation.Storyboard.SetTarget(hold, DisableAllBorderAnim);
        System.Windows.Media.Animation.Storyboard.SetTargetProperty(hold, new PropertyPath(OpacityProperty));

        // Fade out
        var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400))
        { BeginTime = TimeSpan.FromMilliseconds(1350) };
        System.Windows.Media.Animation.Storyboard.SetTarget(fadeOut, DisableAllBorderAnim);
        System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));

        sb.Children.Add(fadeIn);
        sb.Children.Add(hold);
        sb.Children.Add(fadeOut);
        sb.Begin();
    }


    private void RebuildCenterLayout()
    {
        var orderedPanels = _comparePanels.OrderBy(p => p.Key).ToList();
        int count = orderedPanels.Count;
        var activeSet = new HashSet<CompareParameterPanel>(orderedPanels.Select(p => p.Value));

        UpdateWatermarkVisibility();

        // Remove panels that are no longer active + old splitters
        foreach (var child in centerPanelGrid.Children.OfType<CompareParameterPanel>().ToList())
        {
            if (!activeSet.Contains(child))
                centerPanelGrid.Children.Remove(child);
        }
        foreach (var child in centerPanelGrid.Children.OfType<GridSplitter>().ToList())
            centerPanelGrid.Children.Remove(child);

        // Rebuild grid definitions
        centerPanelGrid.RowDefinitions.Clear();
        centerPanelGrid.ColumnDefinitions.Clear();

        if (count == 0)
        {
            // Auto-collapse param area when no panels remain
            if (!_isParamCollapsed)
            {
                _isParamCollapsed = true;
                ParamExpandedArea.Visibility = Visibility.Collapsed;
                ParamCollapsedBar.Visibility = Visibility.Visible;
                ParamContentColumn.Width = new GridLength(0);
            }
            SyncColumnLayout();
            return;
        }

        var splitterBrush = Application.Current.TryFindResource("BorderDefault") as Brush
                            ?? Brushes.Gray;

        if (count == 1)
        {
            // Single panel: no splitters
        }
        else if (count == 2)
        {
            // 2 panels: col0 | splitter | col1
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var vSplitter = new GridSplitter
            {
                Width = 4,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = splitterBrush,
                Cursor = System.Windows.Input.Cursors.SizeWE,
            };
            Grid.SetColumn(vSplitter, 1);
            centerPanelGrid.Children.Add(vSplitter);
        }
        else // 3 or 4 panels: 2x2 grid with splitters
        {
            // col0 | vSplitter | col1
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            centerPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // row0 | hSplitter | row1
            centerPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            centerPanelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            centerPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Vertical splitter (spans all rows)
            var vSplitter = new GridSplitter
            {
                Width = 4,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = splitterBrush,
                Cursor = System.Windows.Input.Cursors.SizeWE,
            };
            Grid.SetColumn(vSplitter, 1);
            Grid.SetRowSpan(vSplitter, 3);
            centerPanelGrid.Children.Add(vSplitter);

            // Horizontal splitter (spans all columns)
            var hSplitter = new GridSplitter
            {
                Height = 4,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = splitterBrush,
                Cursor = System.Windows.Input.Cursors.SizeNS,
            };
            Grid.SetRow(hSplitter, 1);
            Grid.SetColumnSpan(hSplitter, 3);
            centerPanelGrid.Children.Add(hSplitter);
        }

        // Panel placement: account for splitter columns/rows
        // 1 panel: col=0, row=0
        // 2 panels: col=0/2, row=0
        // 3-4 panels: col=0/2, row=0/2
        int[] cols = count <= 2 ? [0, 2, 0, 2] : [0, 2, 0, 2];
        int[] rows = count <= 2 ? [0, 0, 0, 0] : [0, 0, 2, 2];

        for (int i = 0; i < count && i < 4; i++)
        {
            var panel = orderedPanels[i].Value;
            Grid.SetColumn(panel, cols[i]);
            Grid.SetRow(panel, rows[i]);

            // Only add to grid if not already there
            if (!centerPanelGrid.Children.Contains(panel))
                centerPanelGrid.Children.Add(panel);
        }

        // Animate only newly added panels (fade-in + slight scale + loading bar)
        foreach (var (id, panel) in orderedPanels)
        {
            if (_newlyAddedPanels.Contains(id))
            {
                panel.Opacity = 0;
                panel.RenderTransform = new ScaleTransform(0.95, 0.95);
                panel.RenderTransformOrigin = new Point(0.5, 0.5);

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                var scaleX = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                var scaleY = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                panel.BeginAnimation(OpacityProperty, fadeIn);
                ((ScaleTransform)panel.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
                ((ScaleTransform)panel.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);

                panel.StartLoadingAnimation();
            }
        }
        _newlyAddedPanels.Clear();

        // Update monitor layout when panel count changes
        if (_isMonitorVisible)
            ShowMonitorArea();
    }

    // ═══════════════════════════════════════════════════════════
    //  FAVORITE STATE SYNC
    // ═══════════════════════════════════════════════════════════

    private static void SyncFavoriteState(
        System.Collections.ObjectModel.ObservableCollection<Models.Parameter> panelParams,
        MainWindowViewModel? vm)
    {
        if (vm == null) return;
        var favSet = new HashSet<string>(vm.FavoriteParameters.Select(f => f.FtNumber));
        foreach (var p in panelParams)
            p.IsFavorite = favSet.Contains(p.FtNumber);
    }

    /// <summary>
    /// Subscribe to panel parameter IsFavorite changes → update VM FavoriteParameters.
    /// </summary>
    private static void WatchPanelFavorites(
        System.Collections.ObjectModel.ObservableCollection<Models.Parameter> panelParams,
        MainWindowViewModel vm)
    {
        foreach (var p in panelParams)
        {
            p.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Models.Parameter.IsFavorite) && s is Models.Parameter param)
                {
                    vm.UpdateFavorites(param);
                    WeakReferenceMessenger.Default.Send(new FavoriteAnimationMessage(param.IsFavorite));
                }
            };
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  MONITOR & CONTROL PANEL TOGGLE
    // ═══════════════════════════════════════════════════════════

    private bool _isMonitorVisible;
    private readonly Dictionary<string, MonitorControlDialog> _oscilloInstances = new();
    private readonly Dictionary<string, LayoutAnchorable> _controlPanes = new();

    private void ToggleMonitorSection(string section, string driveId)
    {
        switch (section)
        {
            case "Oscilloscope":
                if (_oscilloInstances.ContainsKey(driveId))
                {
                    _oscilloInstances.Remove(driveId);
                }
                else
                {
                    var view = new MonitorControlDialog { DataContext = DataContext };
                    view.SetDriveIdentity(driveId);
                    _oscilloInstances[driveId] = view;
                }
                RebuildMonitorLayout();
                SyncDriveVisibility();
                break;

            case "ControlPanel":
                if (_controlPanes.TryGetValue(driveId, out var existingPane))
                {
                    if (existingPane.Content is ControlPanelView oldCpv)
                        oldCpv.StopUpdating();
                    existingPane.Close();
                    _controlPanes.Remove(driveId);
                }
                else
                {
                    var cpv = new ControlPanelView();
                    cpv.SetDriveIdentity(driveId);
                    cpv.StartUpdating();

                    var pane = new LayoutAnchorable
                    {
                        Title = LocalizationService.GetFormat("loc.control.panel.title", driveId),
                        ContentId = $"controlPanel_{driveId}",
                        Content = cpv,
                        CanClose = true,
                        CanFloat = true,
                        CanAutoHide = true,
                        FloatingWidth = 390,
                        FloatingHeight = 403,
                    };
                    pane.Closed += (_, _) =>
                    {
                        if (pane.Content is ControlPanelView closedCpv)
                            closedCpv.StopUpdating();
                        _controlPanes.Remove(driveId);
                        SyncDriveVisibility();
                    };

                    // Add to layout and float
                    var layoutRoot = dockManager.Layout;
                    var mainPanel = layoutRoot.Descendents().OfType<LayoutPanel>().First();
                    var anchorGroup = mainPanel.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
                    if (anchorGroup != null)
                        anchorGroup.Children.Add(pane);
                    else
                    {
                        var newAnchorPane = new LayoutAnchorablePane(pane);
                        mainPanel.Children.Add(newAnchorPane);
                    }

                    int offset = _controlPanes.Count * 30;
                    pane.Float();
                    pane.FloatingLeft = Left + Width / 2 - 195 + offset;
                    pane.FloatingTop = Top + Height / 2 - 210 + offset;

                    _controlPanes[driveId] = pane;
                }
                SyncDriveVisibility();
                break;
        }
    }

    private void SyncDriveVisibility()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.ActiveGraphDrives.Clear();
        foreach (var key in _oscilloInstances.Keys) vm.ActiveGraphDrives.Add(key);
        vm.ActiveControlDrives.Clear();
        foreach (var key in _controlPanes.Keys) vm.ActiveControlDrives.Add(key);
        // Notify DriveTreeView to update indicators
        WeakReferenceMessenger.Default.Send(new ComparePanelChangedMessage("", false));
    }

    private void RebuildMonitorLayout()
    {
        monitorPanelGrid.Children.Clear();
        monitorPanelGrid.RowDefinitions.Clear();
        monitorPanelGrid.ColumnDefinitions.Clear();

        var panels = _oscilloInstances.OrderBy(x => x.Key).ToList();
        int count = panels.Count;

        if (count == 0)
        {
            HideMonitorArea();
            return;
        }

        var splitterBrush = Application.Current.TryFindResource("BorderDefault") as Brush
                            ?? Brushes.Gray;

        if (count == 1)
        {
            // Single: full area
            monitorPanelGrid.Children.Add(panels[0].Value);
        }
        else if (count == 2)
        {
            // 2 panels: top/bottom (2 rows)
            monitorPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            monitorPanelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            monitorPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid.SetRow(panels[0].Value, 0);
            Grid.SetRow(panels[1].Value, 2);
            monitorPanelGrid.Children.Add(panels[0].Value);
            monitorPanelGrid.Children.Add(panels[1].Value);

            var hSplitter = new GridSplitter
            {
                Height = 4, HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = splitterBrush, Cursor = System.Windows.Input.Cursors.SizeNS,
            };
            Grid.SetRow(hSplitter, 1);
            monitorPanelGrid.Children.Add(hSplitter);
        }
        else // 3 or 4: 2×2 grid
        {
            monitorPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            monitorPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            monitorPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            monitorPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            monitorPanelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            monitorPanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            int[] cols = [0, 2, 0, 2];
            int[] rows = [0, 0, 2, 2];

            for (int i = 0; i < count && i < 4; i++)
            {
                Grid.SetColumn(panels[i].Value, cols[i]);
                Grid.SetRow(panels[i].Value, rows[i]);
                monitorPanelGrid.Children.Add(panels[i].Value);
            }

            // Vertical splitter (spans all rows)
            var vSplitter = new GridSplitter
            {
                Width = 4, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = splitterBrush, Cursor = System.Windows.Input.Cursors.SizeWE,
            };
            Grid.SetColumn(vSplitter, 1);
            Grid.SetRowSpan(vSplitter, 3);
            monitorPanelGrid.Children.Add(vSplitter);

            // Horizontal splitter (spans all columns)
            var hSplitter = new GridSplitter
            {
                Height = 4, HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = splitterBrush, Cursor = System.Windows.Input.Cursors.SizeNS,
            };
            Grid.SetRow(hSplitter, 1);
            Grid.SetColumnSpan(hSplitter, 3);
            monitorPanelGrid.Children.Add(hSplitter);
        }

        ShowMonitorArea();
    }

    private void ShowMonitorArea()
    {
        _isMonitorVisible = true;
        monitorPanelGrid.Visibility = Visibility.Visible;

        // Auto-collapse param area if no panels
        bool hasParams = _comparePanels.Count > 0;
        if (!hasParams && !_isParamCollapsed)
        {
            _isParamCollapsed = true;
            ParamExpandedArea.Visibility = Visibility.Collapsed;
            ParamCollapsedBar.Visibility = Visibility.Visible;
            ParamContentColumn.Width = new GridLength(0);
        }

        SyncColumnLayout();
    }

    private void HideMonitorArea()
    {
        _isMonitorVisible = false;
        monitorPanelGrid.Visibility = Visibility.Collapsed;
        SyncColumnLayout();
    }

    private void UpdateWatermarkVisibility()
    {
        // Logo visible only when: panels collapsed (>> bar) AND no monitor/graph open
        CenterWatermark.Visibility = (_isParamCollapsed && !_isMonitorVisible)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Centralized column layout based on (_isParamCollapsed, _isMonitorVisible)
    /// </summary>
    private void SyncColumnLayout()
    {
        if (!_isParamCollapsed && !_isMonitorVisible)
        {
            // Params only — full width
            ParamPanelColumn.Width = new GridLength(1, GridUnitType.Star);
            CenterSplitterColumn.Width = new GridLength(0);
            CenterSplitter.Visibility = Visibility.Collapsed;
            MonitorColumn.Width = new GridLength(0);
        }
        else if (!_isParamCollapsed && _isMonitorVisible)
        {
            // Params + Monitor — split
            ParamPanelColumn.Width = new GridLength(1, GridUnitType.Star);
            CenterSplitterColumn.Width = GridLength.Auto;
            CenterSplitter.Visibility = Visibility.Visible;
            MonitorColumn.Width = new GridLength(2, GridUnitType.Star);
        }
        else if (_isParamCollapsed && !_isMonitorVisible)
        {
            // Collapsed, no monitor — Star so watermark fills space
            ParamPanelColumn.Width = new GridLength(1, GridUnitType.Star);
            CenterSplitterColumn.Width = new GridLength(0);
            CenterSplitter.Visibility = Visibility.Collapsed;
            MonitorColumn.Width = new GridLength(0);
        }
        else // _isParamCollapsed && _isMonitorVisible
        {
            // Collapsed + Monitor — monitor fills
            ParamPanelColumn.Width = GridLength.Auto;
            ParamPanelColumn.MinWidth = 0;
            CenterSplitterColumn.Width = new GridLength(0);
            CenterSplitter.Visibility = Visibility.Collapsed;
            MonitorColumn.Width = new GridLength(1, GridUnitType.Star);
        }

        UpdateWatermarkVisibility();
    }

    private bool _isParamCollapsed;

    private void ParamCollapsedBar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        BtnCollapseParams_Click(sender, e);
    }

    private void BtnCollapseParams_Click(object sender, RoutedEventArgs e)
    {
        if (!_isParamCollapsed)
        {
            // Collapse
            _isParamCollapsed = true;
            if (DataContext is MainWindowViewModel vmSync) vmSync.IsMainPanelVisible = false;
            ParamExpandedArea.Visibility = Visibility.Collapsed;
            ParamCollapsedBar.Visibility = Visibility.Visible;
            ParamContentColumn.Width = new GridLength(0);
        }
        else
        {
            // Expand
            _isParamCollapsed = false;
            if (DataContext is MainWindowViewModel vmSync2) vmSync2.IsMainPanelVisible = true;
            ParamExpandedArea.Visibility = Visibility.Visible;
            ParamCollapsedBar.Visibility = Visibility.Collapsed;
            ParamContentColumn.Width = new GridLength(1, GridUnitType.Star);

            // Auto-open panel A if no panels exist
            if (_comparePanels.Count == 0 && DataContext is MainWindowViewModel vmPanel)
            {
                vmPanel.IsPanelAVisible = true;
                UpdateComparePanels();
                RebuildCenterLayout();
                return; // RebuildCenterLayout calls SyncColumnLayout
            }
        }
        SyncColumnLayout();
    }

    // ═══════════════════════════════════════════════════════════
    //  THEME SWITCHING (Dark / Gray / Light)
    // ═══════════════════════════════════════════════════════════

    private void SwitchTheme(string themeName)
    {
        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        bool isDark = themeName != "Light";

        // 1. Switch MaterialDesign base theme (FIRST — adds its own resource dicts)
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }
        catch { /* Ignore */ }

        // 2. Switch Fluent.Ribbon theme (Gray uses Dark.Red to match red accent)
        try
        {
            var fluentTheme = themeName switch
            {
                "Gray"  => "Dark.Red",
                "Light" => "Light.Steel",
                _       => "Dark.Steel",
            };
            ThemeManager.Current.ChangeTheme(app, fluentTheme);
        }
        catch { /* Ignore */ }

        // 3. Switch AvalonDock theme via DictionaryTheme (overlay color support)
        try
        {
            _patchedFloatingWindows.Clear();
            ApplyAvalonDockHeaderColor(isDark);
        }
        catch { /* Ignore */ }

        // 4. Swap our color dictionary LAST — overrides Fluent.Ribbon defaults
        //    (Colors.xaml includes Fluent.Ribbon.Brushes.* override keys)
        ResourceDictionary? currentColors = null;
        foreach (var dict in mergedDicts)
        {
            var src = dict.Source?.OriginalString ?? "";
            if (src.Contains("DarkColors.xaml") || src.Contains("GrayColors.xaml") || src.Contains("LightColors.xaml"))
            {
                currentColors = dict;
                break;
            }
        }

        if (currentColors != null)
            mergedDicts.Remove(currentColors);

        var newSource = themeName switch
        {
            "Light" => "Themes/LightColors.xaml",
            "Gray"  => "Themes/GrayColors.xaml",
            _       => "Themes/DarkColors.xaml",
        };
        mergedDicts.Add(new ResourceDictionary
        {
            Source = new Uri(newSource, UriKind.Relative)
        });

        // 5. Re-apply accent colors with current hue/saturation
        var vm = DataContext as MainWindowViewModel;
        if (vm != null)
            UpdateThemeColors(vm.HueValue, vm.SaturationValue);

        // 6. Update title bar color to match new theme
        ApplyWindowChrome(isDark);

        // 7. Refresh chart colors (ScottPlot doesn't use DynamicResource)
        foreach (var view in _oscilloInstances.Values)
            view.RefreshChartTheme();
    }

    // ═══════════════════════════════════════════════════════════
    //  AVALONDOCK THEME via DictionaryTheme (overlay color override)
    // ═══════════════════════════════════════════════════════════

    // DictionaryTheme is abstract — concrete subclass
    private sealed class RsDictionaryTheme : AvalonDock.Themes.DictionaryTheme
    {
        public RsDictionaryTheme(ResourceDictionary rd) : base(rd) { }
    }

    // Keep reference to the theme dict so we can modify it on theme switch
    private ResourceDictionary? _avalonThemeDict;

    private void ApplyAvalonDockHeaderColor(bool isDark = true)
    {
        try
        {
            // Load the VS2013 theme ResourceDictionary
            var themeUri = isDark
                ? new Uri("/AvalonDock.Themes.VS2013;component/DarkTheme.xaml", UriKind.Relative)
                : new Uri("/AvalonDock.Themes.VS2013;component/LightTheme.xaml", UriKind.Relative);

            _avalonThemeDict = new ResourceDictionary { Source = themeUri };

            // Override accent color in the loaded dictionary — this propagates to OverlayWindow
            // because DictionaryTheme shares the SAME ResourceDictionary instance
            var rk = typeof(AvalonDock.Themes.VS2013.Themes.ResourceKeys);
            var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            var keys = new Dictionary<string, object>();
            foreach (var prop in rk.GetProperties(bf))
            {
                var key = prop.GetValue(null);
                if (key != null) keys[prop.Name] = key;
            }
            foreach (var field in rk.GetFields(bf))
            {
                var key = field.GetValue(null);
                if (key != null && !keys.ContainsKey(field.Name)) keys[field.Name] = key;
            }

            var deepRed = new SolidColorBrush(Color.FromRgb(139, 26, 26));
            deepRed.Freeze();
            var lightText = new SolidColorBrush(Colors.White);
            lightText.Freeze();
            var grip = new SolidColorBrush(Color.FromRgb(180, 80, 80));
            grip.Freeze();

            void Set(string name, object value)
            {
                if (keys.TryGetValue(name, out var key))
                    _avalonThemeDict[key] = value;
            }

            // Master accent color — OverlayWindow DynamicResource references this
            Set("ControlAccentColorKey", Color.FromRgb(139, 26, 26));
            Set("ControlAccentBrushKey", deepRed);

            // Docked state
            Set("ToolWindowCaptionActiveBackground", deepRed);
            Set("ToolWindowCaptionActiveText", lightText);
            Set("ToolWindowCaptionActiveGrip", grip);
            Set("DocumentWellTabSelectedActiveBackground", deepRed);

            // Floating state
            Set("FloatingWindowToolWindowBorder", deepRed);
            Set("FloatingWindowDocumentBorder", deepRed);
            Set("FloatingWindowTitleBarBackground", deepRed);
            Set("FloatingWindowTitleBarText", lightText);

            // Docking indicators — via same dictionary (DictionaryTheme shares instance)
            Set("DockingButtonForegroundBrushKey", deepRed);
            Set("PreviewBoxBorderBrushKey", deepRed);
            var previewBg = new SolidColorBrush(Color.FromArgb(128, 139, 26, 26));
            previewBg.Freeze();
            Set("PreviewBoxBackgroundBrushKey", previewBg);

            // Apply as DictionaryTheme — OverlayWindow will use same dict instance
            dockManager.Theme = new RsDictionaryTheme(_avalonThemeDict);

            // Hook for floating window visual tree patching (backup)
            dockManager.LayoutChanged -= DockManager_InjectFloatingResources;
            dockManager.LayoutUpdated -= DockManager_InjectFloatingResources;
            dockManager.LayoutChanged += DockManager_InjectFloatingResources;
            dockManager.LayoutUpdated += DockManager_InjectFloatingResources;
        }
        catch { }
    }

    private readonly HashSet<int> _patchedFloatingWindows = new();
    private bool _isPatchingFloating;

    private void DockManager_InjectFloatingResources(object? sender, EventArgs e)
    {
        if (_isPatchingFloating) return; // prevent re-entrancy
        _isPatchingFloating = true;
        try
        {
            var windows = Application.Current.Windows.OfType<Window>().ToList();
            foreach (var w in windows)
            {
                if (w is not AvalonDock.Controls.LayoutAnchorableFloatingWindowControl &&
                    w is not AvalonDock.Controls.LayoutDocumentFloatingWindowControl)
                    continue;

                if (!w.IsLoaded || !_patchedFloatingWindows.Add(w.GetHashCode())) continue;

                // Walk visual tree after render to fix blue elements
                w.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, () =>
                {
                    try { PatchFloatingWindowVisualTree(w); } catch { }
                });
            }
        }
        catch { }
        finally { _isPatchingFloating = false; }
    }

    private static bool IsBlueAccent(Color c)
    {
        // VS2013 blue accent: #007ACC or similar blue tones (broad detection)
        return c.B > 140 && c.R < 100 && c.G < 180 && c.A > 100;
    }

    private static readonly SolidColorBrush _patchRed = CreateFrozenBrush(139, 26, 26);
    private static readonly SolidColorBrush _patchWhite = CreateFrozenBrush(255, 255, 255);
    private static readonly SolidColorBrush _patchGrip = CreateFrozenBrush(180, 80, 80);
    private static readonly SolidColorBrush _patchLightGray = CreateFrozenBrush(190, 190, 190);

    private static SolidColorBrush CreateFrozenBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    private void PatchFloatingWindowVisualTree(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // Border: Background + BorderBrush
            if (child is System.Windows.Controls.Border border)
            {
                if (border.Background is SolidColorBrush bg && IsBlueAccent(bg.Color))
                    border.Background = _patchRed;
                if (border.BorderBrush is SolidColorBrush bb && IsBlueAccent(bb.Color))
                    border.BorderBrush = _patchRed;
            }

            // Panel: Background
            if (child is System.Windows.Controls.Panel panel &&
                panel.Background is SolidColorBrush pbg && IsBlueAccent(pbg.Color))
                panel.Background = _patchRed;

            // Shape (Path, Line, Rectangle, Ellipse): Fill + Stroke → light gray for grip dots
            if (child is System.Windows.Shapes.Shape shape)
            {
                if (shape.Fill is SolidColorBrush sf && IsBlueAccent(sf.Color))
                    shape.Fill = _patchLightGray;
                if (shape.Stroke is SolidColorBrush ss && IsBlueAccent(ss.Color))
                    shape.Stroke = _patchLightGray;
            }

            // TextBlock/Control: Foreground (blue dots could be text-based grip)
            if (child is System.Windows.Controls.TextBlock tb &&
                tb.Foreground is SolidColorBrush tf && IsBlueAccent(tf.Color))
                tb.Foreground = _patchWhite;

            if (child is System.Windows.Controls.Control ctrl &&
                ctrl.Foreground is SolidColorBrush cf && IsBlueAccent(cf.Color))
                ctrl.Foreground = _patchWhite;

            PatchFloatingWindowVisualTree(child);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ACCENT COLOR (hue + saturation based icon/accent colors)
    // ═══════════════════════════════════════════════════════════

    private void UpdateThemeColors(double hue, double satPercent)
    {
        var res = Application.Current.Resources;
        var vm = DataContext as MainWindowViewModel;
        bool isDark = vm?.SelectedTheme != "Light";

        // Convert 0-100 slider → 0.0-1.0
        double sat = satPercent / 100.0;

        // Primary colors
        var primary = HslToColor(hue, sat, isDark ? 0.60 : 0.42);
        res["PrimaryBrush"] = new SolidColorBrush(primary);
        res["PrimaryHoverBrush"] = new SolidColorBrush(HslToColor(hue, sat, isDark ? 0.52 : 0.35));

        // Secondary = analogous hue (+20)
        double secHue = (hue + 20) % 360;
        double secSat = Math.Max(0, sat * 0.7);
        var secondary = HslToColor(secHue, secSat, isDark ? 0.75 : 0.55);
        res["SecondaryBrush"] = new SolidColorBrush(secondary);
        res["SecondaryHoverBrush"] = new SolidColorBrush(HslToColor(secHue, secSat, isDark ? 0.68 : 0.48));

        // RibbonItemBrush follows Secondary
        res["RibbonItemBrush"] = new SolidColorBrush(secondary);

        // Theme-aware derived colors
        res["StatusBarBrush"] = new SolidColorBrush(HslToColor(hue, sat * 0.8, isDark ? 0.38 : 0.42));
        res["BorderFocused"] = new SolidColorBrush(HslToColor(hue, secSat, isDark ? 0.65 : 0.48));
        res["SelectedRowBrush"] = new SolidColorBrush(HslToColor(hue, sat * 0.6, isDark ? 0.28 : 0.82));

        // Update MaterialDesign theme palette
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetPrimaryColor(primary);
            theme.SetSecondaryColor(secondary);
            paletteHelper.SetTheme(theme);
        }
        catch { /* Ignore */ }
    }

    // ═══════════════════════════════════════════════════════════
    //  FONT SIZE
    // ═══════════════════════════════════════════════════════════

    private static readonly Dictionary<string, double> BaseFontSizes = new()
    {
        ["FontSizeXS"]  = 10,
        ["FontSizeSM"]  = 11,
        ["FontSizeMD"]  = 12,
        ["FontSizeLG"]  = 13,
        ["FontSizeXL"]  = 14,
        ["FontSizeXXL"] = 16,
        ["FontSize3XL"] = 20,
    };

    private void UpdateFontSizes(int baseMd)
    {
        double scale = baseMd / 12.0;
        var res = Application.Current.Resources;
        foreach (var (key, baseSize) in BaseFontSizes)
            res[key] = Math.Round(baseSize * scale, 1);
    }

    // ═══════════════════════════════════════════════════════════
    //  TITLE BAR & WINDOW CHROME (Windows 11 DWM)
    // ═══════════════════════════════════════════════════════════

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_CAPTION_COLOR = 35;
    private const int DWMWA_TEXT_COLOR = 36;
    private const int DWMWCP_ROUND = 2;

    private void ApplyWindowChrome(bool isDark)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;

        // Rounded corners
        int cornerPref = DWMWCP_ROUND;
        DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));

        // Immersive dark mode (affects title bar button icons)
        int darkMode = isDark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

        // Caption (title bar) color — COLORREF = 0x00BBGGRR
        var bgBrush = Application.Current.TryFindResource("BackgroundBrush") as SolidColorBrush;
        if (bgBrush != null)
        {
            var c = bgBrush.Color;
            int colorRef = c.R | (c.G << 8) | (c.B << 16);
            DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, sizeof(int));
        }

        // Title text color
        var textBrush = Application.Current.TryFindResource("TextPrimary") as SolidColorBrush;
        if (textBrush != null)
        {
            var c = textBrush.Color;
            int colorRef = c.R | (c.G << 8) | (c.B << 16);
            DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref colorRef, sizeof(int));
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  HSL → Color Conversion
    // ═══════════════════════════════════════════════════════════

    private static Color HslToColor(double h, double s, double l)
    {
        h %= 360;
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        double m = l - c / 2;

        double r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255));
    }
}
