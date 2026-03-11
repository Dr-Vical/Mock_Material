using System.Windows;

namespace RswareDesign.Views;

public partial class AdminPasswordDialog : Window
{
    private const string AdminPassword = "admin"; // mockup password

    public AdminPasswordDialog()
    {
        InitializeComponent();
        PasswordInput.Focus();
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordInput.Password == AdminPassword)
        {
            DialogResult = true;
            Close();
        }
        else
        {
            ConfirmActionDialog.Info(this,
                "Authentication Failed", "Incorrect password.",
                MaterialDesignThemes.Wpf.PackIconKind.ShieldAlertOutline,
                "ErrorBrush");
            PasswordInput.Clear();
            PasswordInput.Focus();
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
