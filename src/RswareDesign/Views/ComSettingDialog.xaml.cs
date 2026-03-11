using System.Windows;
using System.Windows.Media;

namespace RswareDesign.Views;

public partial class ComSettingDialog : Window
{
    public ComSettingDialog()
    {
        InitializeComponent();
        LoadDefaults();
    }

    // ═══ Result properties ═══

    public string SelectedPort => CmbPort.SelectedItem?.ToString() ?? "";
    public int BaudRate => int.TryParse(CmbBaudRate.SelectedItem?.ToString(), out var v) ? v : 115200;
    public int DataBits => int.TryParse(CmbDataBits.SelectedItem?.ToString(), out var v) ? v : 8;
    public string SelectedParity => CmbParity.SelectedItem?.ToString() ?? "None";
    public string SelectedStopBits => CmbStopBits.SelectedItem?.ToString() ?? "1";
    public string CommType => CmbCommType.SelectedItem?.ToString() ?? "RS-232";
    public int DriveAddress => int.TryParse(TxtAddress.Text, out var v) ? v : 1;
    public int Timeout => int.TryParse(TxtTimeout.Text, out var v) ? v : 1000;
    public int RetryCount => int.TryParse(TxtRetry.Text, out var v) ? v : 3;

    private void LoadDefaults()
    {
        // Ports (mockup — real app uses SerialPort.GetPortNames())
        RefreshPorts();

        // Baud rates
        var baudRates = new[] { "9600", "19200", "38400", "57600", "115200", "230400", "460800", "921600" };
        CmbBaudRate.ItemsSource = baudRates;
        CmbBaudRate.SelectedItem = "115200";

        // Data bits
        CmbDataBits.ItemsSource = new[] { "7", "8" };
        CmbDataBits.SelectedItem = "8";

        // Parity
        CmbParity.ItemsSource = new[] { "None", "Even", "Odd", "Mark", "Space" };
        CmbParity.SelectedItem = "None";

        // Stop bits
        CmbStopBits.ItemsSource = new[] { "1", "1.5", "2" };
        CmbStopBits.SelectedItem = "1";

        // Communication type
        CmbCommType.ItemsSource = new[] { "RS-232", "RS-485" };
        CmbCommType.SelectedItem = "RS-232";
    }

    private void RefreshPorts()
    {
        var current = CmbPort.SelectedItem?.ToString();
        // Mockup port list — replace with SerialPort.GetPortNames() in production
        var ports = new[] { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8" };
        CmbPort.ItemsSource = ports;
        CmbPort.SelectedItem = current != null && ports.Contains(current) ? current : "COM3";
    }

    public void SetCurrentValues(string port, int baudRate)
    {
        CmbPort.SelectedItem = port;
        CmbBaudRate.SelectedItem = baudRate.ToString();
    }

    private void BtnRefreshPorts_Click(object sender, RoutedEventArgs e)
    {
        RefreshPorts();
    }

    private void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        // Simulate connection test
        try
        {
            StatusIndicator.Fill = (Brush)FindResource("WarningBrush");
            TxtStatus.Text = "Testing...";

            // In real app, this would actually try to open the port
            StatusIndicator.Fill = (Brush)FindResource("SuccessBrush");
            TxtStatus.Text = $"{SelectedPort} @ {BaudRate} — OK";
        }
        catch
        {
            StatusIndicator.Fill = (Brush)FindResource("ErrorBrush");
            TxtStatus.Text = "Connection failed";
        }
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
