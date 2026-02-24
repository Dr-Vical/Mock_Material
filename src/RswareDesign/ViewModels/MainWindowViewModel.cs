using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RswareDesign.Models;
using System.Collections.ObjectModel;

namespace RswareDesign.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "RswareDesign - [Drive - ECAT Homing]";

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
    private bool _isDarkTheme = true;

    [ObservableProperty]
    private bool _showHelps;

    [ObservableProperty]
    private bool _showStatus = true;

    [ObservableProperty]
    private bool _showCommands = true;

    [ObservableProperty]
    private bool _isDriveTreeVisible = true;

    [ObservableProperty]
    private bool _isErrorLogVisible = true;

    [ObservableProperty]
    private bool _isActionPanelVisible = true;

    public ObservableCollection<string> AvailablePorts { get; } = ["COM1", "COM2", "COM3", "COM4", "COM5"];

    public ObservableCollection<DriveTreeNode> TreeNodes { get; } = [];

    public ObservableCollection<Parameter> Parameters { get; } = [];

    public ObservableCollection<StatusEntry> StatusEntries { get; } = [];

    public MainWindowViewModel()
    {
        BuildSampleTree();
        BuildSampleParameters();
        BuildSampleStatus();
    }

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
    private void Rescan()
    {
        // Rescan serial ports
    }

    [RelayCommand]
    private void SaveParameters()
    {
        // Save parameters to drive
    }

    [RelayCommand]
    private void RevertParameters()
    {
        // Revert to original values
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(value));
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
    }

    [RelayCommand]
    private void OpenOscilloscope()
    {
        WeakReferenceMessenger.Default.Send(new OpenOscilloscopeMessage());
    }

    private void BuildSampleTree()
    {
        var onlineDrives = new DriveTreeNode
        {
            Name = "On Line Drives", IconKind = "LanConnect", IsExpanded = true,
            Children =
            [
                new DriveTreeNode
                {
                    Name = "Drive", IconKind = "Cog", IsExpanded = true,
                    Children =
                    [
                        new DriveTreeNode { Name = "Mode Configuration", IconKind = "TuneVertical" },
                        new DriveTreeNode { Name = "Motor", IconKind = "Engine" },
                        new DriveTreeNode
                        {
                            Name = "PID Tuning", IconKind = "ChartLine", IsExpanded = true,
                            Children =
                            [
                                new DriveTreeNode { Name = "Tuningless", IconKind = "AutoFix" },
                                new DriveTreeNode { Name = "Resonant Suppression", IconKind = "WaveformOutline" },
                                new DriveTreeNode { Name = "Vibration Suppression", IconKind = "Vibrate" },
                                new DriveTreeNode { Name = "Encoders", IconKind = "RotateRight" },
                            ]
                        },
                        new DriveTreeNode { Name = "Digital Inputs", IconKind = "ImportExport" },
                        new DriveTreeNode { Name = "Digital Outputs", IconKind = "ExportVariant" },
                        new DriveTreeNode { Name = "Analog Outputs", IconKind = "SineWave" },
                        new DriveTreeNode { Name = "Monitor", IconKind = "MonitorDashboard" },
                        new DriveTreeNode { Name = "Oscilloscope", IconKind = "ChartBellCurveCumulative" },
                        new DriveTreeNode { Name = "Faults", IconKind = "AlertCircle" },
                        new DriveTreeNode { Name = "Fully Closed System", IconKind = "LinkLock" },
                        new DriveTreeNode { Name = "ServiceInfo", IconKind = "InformationOutline" },
                        new DriveTreeNode { Name = "Control Panel", IconKind = "GamepadVariant" },
                    ]
                }
            ]
        };

        var offlineDrives = new DriveTreeNode
        {
            Name = "Off Line : Unsaved", IconKind = "LanDisconnect", IsExpanded = true,
            Children =
            [
                new DriveTreeNode
                {
                    Name = "Group", IconKind = "FormatListBulleted", IsExpanded = true,
                    Children =
                    [
                        new DriveTreeNode { Name = "Group 0 : Basic", IconKind = "Numeric0BoxOutline" },
                        new DriveTreeNode { Name = "Group 1 : Gain", IconKind = "Numeric1BoxOutline" },
                        new DriveTreeNode { Name = "Group 2 : Velocity", IconKind = "Numeric2BoxOutline" },
                        new DriveTreeNode { Name = "Group 3 : Position", IconKind = "Numeric3BoxOutline" },
                        new DriveTreeNode { Name = "Group 4 : Current", IconKind = "Numeric4BoxOutline" },
                        new DriveTreeNode { Name = "Group 5 : Auxiliary", IconKind = "Numeric5BoxOutline" },
                    ]
                }
            ]
        };

        TreeNodes.Add(onlineDrives);
        TreeNodes.Add(offlineDrives);
    }

    private void BuildSampleParameters()
    {
        Parameters.Add(new Parameter { FtNumber = "5.14", Name = "ECAT Abs Origin Offset", Value = "0", Unit = "counts", Default = "0", Min = "-2147483647", Max = "2147483647", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "5.15", Name = "ECAT Homing Method", Value = "35", Unit = "", Default = "35", Min = "-128", Max = "127", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "5.16", Name = "ECAT Homing TimeOut", Value = "0", Unit = "sec", Default = "0", Min = "0", Max = "500", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "5.17", Name = "ECAT Homing Offset", Value = "0", Unit = "counts", Default = "0", Min = "-2147483647", Max = "2147483647", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "5.18", Name = "ECAT Homing Velocity 1", Value = "200000", Unit = "counts/sec", Default = "200000", Min = "0", Max = "2147483647", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "5.19", Name = "ECAT Homing Velocity 2", Value = "200000", Unit = "counts/sec", Default = "200000", Min = "0", Max = "2147483647", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "5.20", Name = "ECAT Homing Acceleration", Value = "2000000", Unit = "counts/sec^2", Default = "2000000", Min = "0", Max = "2147483647", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "0.06 D1", Name = "Absolute Homing Completed", Value = "Not Completed", Unit = "", Default = "Not Completed", Min = "", Max = "", Access = "r" });
        Parameters.Add(new Parameter { FtNumber = "01.009", Name = "Home Current", Value = "100", Unit = "%", Default = "100", Min = "1", Max = "250", Access = "r/w" });
        Parameters.Add(new Parameter { FtNumber = "01.010", Name = "Home Current Time", Value = "0", Unit = "msec", Default = "0", Min = "0", Max = "1000", Access = "r/w" });
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

public class OpenOscilloscopeMessage { }

public record ThemeChangedMessage(bool IsDark);
