using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace RswareDesign.Views;

public partial class ConfirmActionDialog : Window
{
    public ConfirmActionDialog(string title, string message,
        PackIconKind icon = PackIconKind.HelpCircleOutline,
        string confirmText = "OK",
        string? confirmBrushKey = null)
    {
        InitializeComponent();

        TxtTitle.Text = title;
        TxtMessage.Text = message;
        HeaderIcon.Kind = icon;
        BtnConfirm.Content = confirmText;

        if (confirmBrushKey != null &&
            Application.Current.TryFindResource(confirmBrushKey) is Brush brush)
        {
            BtnConfirm.Background = brush;
        }

        MouseLeftButtonDown += (_, _) => DragMove();
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        };
    }

    /// <summary>
    /// Show a styled confirmation dialog. Returns true if confirmed.
    /// </summary>
    public static bool Ask(Window? owner, string title, string message,
        PackIconKind icon = PackIconKind.HelpCircleOutline,
        string confirmText = "OK",
        string? confirmBrushKey = null)
    {
        var dialog = new ConfirmActionDialog(title, message, icon, confirmText, confirmBrushKey);
        if (owner != null) dialog.Owner = owner;
        return dialog.ShowDialog() == true;
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
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
