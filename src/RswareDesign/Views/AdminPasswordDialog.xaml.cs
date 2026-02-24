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
            MessageBox.Show("Incorrect password.", "Authentication Failed",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            PasswordInput.Clear();
            PasswordInput.Focus();
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
