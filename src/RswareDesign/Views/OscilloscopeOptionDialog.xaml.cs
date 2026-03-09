using System.Windows;

namespace RswareDesign.Views;

public partial class OscilloscopeOptionDialog : Window
{
    public OscilloscopeOptionDialog()
    {
        InitializeComponent();
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
