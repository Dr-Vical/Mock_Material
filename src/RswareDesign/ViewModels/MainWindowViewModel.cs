using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RswareDesign.Models;
using RswareDesign.Services;
using System.Collections.ObjectModel;

namespace RswareDesign.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "RswareDesign - [Drive - ECAT Homing]";

    [ObservableProperty]
    private string _activeDocumentTitle = "ECAT Homing";

    [ObservableProperty]
    private string _selectedNodeType = "ECATHoming";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _selectedPort = "COM3";

    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private string _driveInfo = "CSD7N";

    [ObservableProperty]
    private string _modeInfo = "Offline";

    [ObservableProperty]
    private string _selectedTheme = "Dark";

    public ObservableCollection<string> AvailableThemes { get; } = ["Dark", "Gray", "Light"];

    [ObservableProperty]
    private double _hueValue = 200; // Blue Grey default hue

    [ObservableProperty]
    private double _saturationValue = 25; // Low saturation for gray tone (0-100)

    [ObservableProperty]
    private bool _isKorean = true; // 한영 toggle

    [ObservableProperty]
    private int _selectedFontSize = 12;

    public ObservableCollection<int> AvailableFontSizes { get; } = [8, 9, 10, 11, 12, 13, 14, 16, 18, 20];

    [ObservableProperty]
    private bool _isDriveTreeVisible = true;

    [ObservableProperty]
    private bool _isMainPanelVisible = true;

    [ObservableProperty]
    private bool _isActionPanelVisible = true;

    [ObservableProperty]
    private bool _isErrorLogVisible = true;

    // Compare panel visibility (A/B/C/D) — all panels are equal, at least 1 must remain
    [ObservableProperty]
    private bool _isPanelAVisible = true; // A is default-on at startup

    [ObservableProperty]
    private bool _isPanelBVisible;

    [ObservableProperty]
    private bool _isPanelCVisible;

    [ObservableProperty]
    private bool _isPanelDVisible;

    // Bottom checkboxes
    [ObservableProperty]
    private bool _showHelps;

    [ObservableProperty]
    private bool _showStatus = true;

    [ObservableProperty]
    private bool _showCommands = true;

    public ObservableCollection<string> AvailablePorts { get; } = ["COM1", "COM2", "COM3", "COM4", "COM5"];

    public ObservableCollection<DriveTreeNode> TreeNodes { get; } = [];

    public ObservableCollection<Parameter> Parameters { get; } = [];

    private DriveTreeNode _favoritesNode = new()
    {
        Name = "Favorites", IconKind = "Star", NodeType = "Favorites"
    };

    public ObservableCollection<Parameter> FavoriteParameters { get; } = [];

    public ObservableCollection<StatusEntry> StatusEntries { get; } = [];

    public MainWindowViewModel()
    {
        BuildSampleTree();
        LoadParametersForNode("ECATHoming");
        BuildSampleStatus();

        // Listen for tree node selection
        WeakReferenceMessenger.Default.Register<TreeNodeSelectedMessage>(this, (_, msg) =>
        {
            OnTreeNodeSelected(msg);
        });
    }

    // ═══════════════════════════════════════════════════════════
    //  TREE NODE SELECTION → CENTER CONTENT SWITCH
    // ═══════════════════════════════════════════════════════════

    private void OnTreeNodeSelected(TreeNodeSelectedMessage msg)
    {
        // Update document title
        ActiveDocumentTitle = msg.NodeName;
        Title = $"RswareDesign - [Drive - {msg.NodeName}]";
        SelectedNodeType = msg.NodeType;

        // Load parameters from CSV for selected node
        LoadParametersForNode(msg.NodeType);

        // Update status entries
        StatusEntries.Clear();
        StatusEntries.Add(new StatusEntry { Status = $"{msg.NodeName} Status", Value = "0:IDLE", Units = "" });
        StatusEntries.Add(new StatusEntry { Status = $"{msg.NodeName} Error", Value = "No Error", Units = "" });
    }

    private void LoadParametersForNode(string nodeType)
    {
        foreach (var p in Parameters)
            p.PropertyChanged -= OnParameterPropertyChanged;

        Parameters.Clear();

        if (nodeType == "Favorites")
        {
            // Show favorited parameters (already have IsFavorite=true)
            foreach (var p in FavoriteParameters)
            {
                p.PropertyChanged += OnParameterPropertyChanged;
                Parameters.Add(p);
            }
            return;
        }

        var csvParams = CsvParameterLoader.LoadForNodeType(nodeType);
        foreach (var p in csvParams)
        {
            // Restore favorite state if previously starred
            if (FavoriteParameters.Any(f => f.FtNumber == p.FtNumber))
                p.IsFavorite = true;

            p.PropertyChanged += OnParameterPropertyChanged;
            Parameters.Add(p);
        }
    }

    private void OnParameterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Parameter param) return;

        if (e.PropertyName == nameof(Parameter.IsFavorite))
        {
            UpdateFavorites(param);
            WeakReferenceMessenger.Default.Send(new FavoriteAnimationMessage(param.IsFavorite));
        }
        else if (e.PropertyName == nameof(Parameter.Value))
        {
            ClampValueIfNeeded(param);
        }
    }

    private void ClampValueIfNeeded(Parameter param)
    {
        if (!double.TryParse(param.Value, out double val)) return;

        bool hasMin = double.TryParse(param.Min, out double min);
        bool hasMax = double.TryParse(param.Max, out double max);

        if (hasMin && val < min)
        {
            param.Value = min.ToString();
            StatusEntries.Add(new StatusEntry
            {
                Status = $"[Warning] {param.ShortNumber} {param.Name}",
                Value = $"Value clamped to Min ({min})",
                Units = param.Unit,
            });
        }
        else if (hasMax && val > max)
        {
            param.Value = max.ToString();
            StatusEntries.Add(new StatusEntry
            {
                Status = $"[Warning] {param.ShortNumber} {param.Name}",
                Value = $"Value clamped to Max ({max})",
                Units = param.Unit,
            });
        }
    }

    private void UpdateFavorites(Parameter param)
    {
        if (param.IsFavorite)
        {
            if (FavoriteParameters.All(f => f.FtNumber != param.FtNumber))
            {
                // Store a copy for the favorites list
                FavoriteParameters.Add(new Parameter
                {
                    FtNumber = param.FtNumber,
                    Name = param.Name,
                    Value = param.Value,
                    Unit = param.Unit,
                    Default = param.Default,
                    Min = param.Min,
                    Max = param.Max,
                    Access = param.Access,
                    Group = param.Group,
                    IsFavorite = true,
                });
            }
        }
        else
        {
            var existing = FavoriteParameters.FirstOrDefault(f => f.FtNumber == param.FtNumber);
            if (existing != null)
                FavoriteParameters.Remove(existing);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  COMMANDS
    // ═══════════════════════════════════════════════════════════

    [RelayCommand]
    private void Connect()
    {
        IsConnected = true;
        ConnectionStatus = "Connected";
        ModeInfo = "Online";
    }

    [RelayCommand]
    private void Disconnect()
    {
        IsConnected = false;
        ConnectionStatus = "Disconnected";
        ModeInfo = "Offline";
    }

    [RelayCommand]
    private void Rescan() { }

    [RelayCommand]
    private void SaveParameters() { }

    [RelayCommand]
    private void ExitApp()
    {
        WeakReferenceMessenger.Default.Send(new ShowExitConfirmMessage());
    }

    [RelayCommand]
    private void AdminMotorDb()
    {
        WeakReferenceMessenger.Default.Send(new ShowAdminPasswordMessage("MotorDb"));
    }

    [RelayCommand]
    private void AdminParamSetting()
    {
        WeakReferenceMessenger.Default.Send(new ShowAdminPasswordMessage("ParamSetting"));
    }

    [RelayCommand]
    private void Enable() { }

    [RelayCommand]
    private void DisableAll() { }

    [RelayCommand]
    private void ClearFaultAll() { }

    [RelayCommand]
    private void ClearAllFavorites()
    {
        WeakReferenceMessenger.Default.Send(new ShowClearFavoritesConfirmMessage());
    }

    public void ExecuteClearAllFavorites()
    {
        // Un-star current visible parameters
        foreach (var p in Parameters)
        {
            if (p.IsFavorite)
                p.IsFavorite = false;
        }
        FavoriteParameters.Clear();
    }

    // ═══════════════════════════════════════════════════════════
    //  PROPERTY CHANGE HANDLERS
    // ═══════════════════════════════════════════════════════════

    partial void OnSelectedThemeChanged(string value)
    {
        WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(value));
    }

    partial void OnHueValueChanged(double value)
    {
        WeakReferenceMessenger.Default.Send(new HueChangedMessage(value, SaturationValue));
    }

    partial void OnSaturationValueChanged(double value)
    {
        WeakReferenceMessenger.Default.Send(new HueChangedMessage(HueValue, value));
    }

    partial void OnSelectedFontSizeChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new FontSizeChangedMessage(value));
    }



    partial void OnIsPanelAVisibleChanged(bool value)
    {
        if (!value && !IsPanelBVisible && !IsPanelCVisible && !IsPanelDVisible)
        {
            IsPanelAVisible = true; // Can't remove last panel
            return;
        }
        WeakReferenceMessenger.Default.Send(new ComparePanelChangedMessage("A", value));
    }

    partial void OnIsPanelBVisibleChanged(bool value)
    {
        if (!value && !IsPanelAVisible && !IsPanelCVisible && !IsPanelDVisible)
        {
            IsPanelBVisible = true; // Can't remove last panel
            return;
        }
        WeakReferenceMessenger.Default.Send(new ComparePanelChangedMessage("B", value));
    }

    partial void OnIsPanelCVisibleChanged(bool value)
    {
        if (!value && !IsPanelAVisible && !IsPanelBVisible && !IsPanelDVisible)
        {
            IsPanelCVisible = true; // Can't remove last panel
            return;
        }
        WeakReferenceMessenger.Default.Send(new ComparePanelChangedMessage("C", value));
    }

    partial void OnIsPanelDVisibleChanged(bool value)
    {
        if (!value && !IsPanelAVisible && !IsPanelBVisible && !IsPanelCVisible)
        {
            IsPanelDVisible = true; // Can't remove last panel
            return;
        }
        WeakReferenceMessenger.Default.Send(new ComparePanelChangedMessage("D", value));
    }

    // ═══════════════════════════════════════════════════════════
    //  SAMPLE DATA
    // ═══════════════════════════════════════════════════════════

    private void BuildSampleTree()
    {
        var onlineDrives = new DriveTreeNode
        {
            Name = "On Line Drives", IconKind = "LanConnect", NodeType = "", IsExpanded = true,
            Children =
            [
                new DriveTreeNode
                {
                    Name = "Drive", IconKind = "Cog", NodeType = "", IsExpanded = true,
                    Children =
                    [
                        new DriveTreeNode { Name = "Mode Configuration", IconKind = "TuneVertical", NodeType = "ModeConfig" },
                        _favoritesNode,
                        new DriveTreeNode { Name = "Motor", NodeType = "Motor", CustomIconPath = "M4 14a1 1 0 0 1-.78-1.63l9.9-10.2a.5.5 0 0 1 .86.46l-1.92 6.02A1 1 0 0 0 13 10h7a1 1 0 0 1 .78 1.63l-9.9 10.2a.5.5 0 0 1-.86-.46l1.92-6.02A1 1 0 0 0 11 14z" },
                        new DriveTreeNode
                        {
                            Name = "PID Tuning", IconKind = "ChartLine", NodeType = "PIDTuning", IsExpanded = true,
                            Children =
                            [
                                new DriveTreeNode { Name = "Tuningless", IconKind = "AutoFix", NodeType = "Tuningless" },
                                new DriveTreeNode { Name = "Resonant Suppression", IconKind = "WaveformOutline", NodeType = "ResonantSuppression" },
                                new DriveTreeNode { Name = "Vibration Suppression", IconKind = "Vibrate", NodeType = "VibrationSuppression" },
                                new DriveTreeNode { Name = "Encoders", IconKind = "RotateRight", NodeType = "Encoders" },
                            ]
                        },
                        new DriveTreeNode { Name = "Digital Inputs", IconKind = "ImportExport", NodeType = "DigitalInputs" },
                        new DriveTreeNode { Name = "Digital Outputs", IconKind = "ExportVariant", NodeType = "DigitalOutputs" },
                        new DriveTreeNode { Name = "Analog Outputs", IconKind = "SineWave", NodeType = "AnalogOutputs" },
                        new DriveTreeNode { Name = "ECAT Homing", IconKind = "Home", NodeType = "ECATHoming" },
                        new DriveTreeNode { Name = "Monitor", IconKind = "MonitorDashboard", NodeType = "Monitor" },
                        new DriveTreeNode { Name = "Oscilloscope", IconKind = "ChartBellCurveCumulative", NodeType = "Oscilloscope" },
                        new DriveTreeNode { Name = "Faults", IconKind = "AlertCircle", NodeType = "Faults" },
                        new DriveTreeNode { Name = "Fully Closed System", IconKind = "LinkLock", NodeType = "FullyClosed" },
                        new DriveTreeNode { Name = "ServiceInfo", IconKind = "InformationOutline", NodeType = "ServiceInfo" },
                        new DriveTreeNode { Name = "Control Panel", IconKind = "GamepadVariant", NodeType = "ControlPanel" },
                    ]
                }
            ]
        };

        var offlineDrives = new DriveTreeNode
        {
            Name = "Off Line : Unsaved", IconKind = "LanDisconnect", NodeType = "", IsExpanded = true,
            Children =
            [
                new DriveTreeNode
                {
                    Name = "Group", IconKind = "FormatListBulleted", NodeType = "", IsExpanded = true,
                    Children =
                    [
                        new DriveTreeNode { Name = "Group 0 : Basic", IconKind = "Numeric0BoxOutline", NodeType = "Group0" },
                        new DriveTreeNode { Name = "Group 1 : Gain", IconKind = "Numeric1BoxOutline", NodeType = "Group1" },
                        new DriveTreeNode { Name = "Group 2 : Velocity", IconKind = "Numeric2BoxOutline", NodeType = "Group2" },
                        new DriveTreeNode { Name = "Group 3 : Position", IconKind = "Numeric3BoxOutline", NodeType = "Group3" },
                        new DriveTreeNode { Name = "Group 4 : Current", IconKind = "Numeric4BoxOutline", NodeType = "Group4" },
                        new DriveTreeNode { Name = "Group 5 : Auxiliary", IconKind = "Numeric5BoxOutline", NodeType = "Group5" },
                    ]
                }
            ]
        };

        TreeNodes.Add(onlineDrives);
        TreeNodes.Add(offlineDrives);
    }

    private void BuildSampleStatus()
    {
        StatusEntries.Add(new StatusEntry { Status = "ECAT Homing Status", Value = "0:IDLE", Units = "" });
        StatusEntries.Add(new StatusEntry { Status = "ECAT Homing Error", Value = "No Error", Units = "" });
    }
}

public class StatusEntry
{
    public string Status { get; set; } = "";
    public string Value { get; set; } = "";
    public string Units { get; set; } = "";
}

// ═══════════════════════════════════════════════════════════
//  MESSAGES
// ═══════════════════════════════════════════════════════════

public class OpenOscilloscopeMessage { }

public record ThemeChangedMessage(string ThemeName);

public record HueChangedMessage(double Hue, double Saturation);

public record FontSizeChangedMessage(int FontSize);

public record ShowAdminPasswordMessage(string Target);

public record TreeNodeSelectedMessage(string NodeName, string NodeType);

public record ComparePanelChangedMessage(string PanelId, bool IsVisible);

public record FavoriteToggledMessage(Parameter Parameter);

public class ShowExitConfirmMessage { }

public record ShowMonitorControlMessage();

public record ToggleMonitorSectionMessage(string Section); // "Oscilloscope" or "ControlPanel"

public record FavoriteAnimationMessage(bool IsAdded);

public record ShowClearFavoritesConfirmMessage();
