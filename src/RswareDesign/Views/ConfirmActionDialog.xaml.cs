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

            // Color the dialog border based on severity
            DialogBorder.BorderBrush = brush;
            DialogBorder.BorderThickness = new Thickness(2);
        }

        Loaded += (_, _) =>
        {
            if (_hideCancel)
                BtnCancel.Visibility = Visibility.Collapsed;
        };

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

    /// <summary>
    /// Show a styled info/warning dialog with OK button only (no cancel).
    /// </summary>
    public static void Info(Window? owner, string title, string message,
        PackIconKind icon = PackIconKind.InformationOutline,
        string? borderBrushKey = null)
    {
        var dialog = new ConfirmActionDialog(title, message, icon,
            Services.LocalizationService.Get("loc.btn.ok"), borderBrushKey)
        {
            _hideCancel = true
        };
        if (owner != null) dialog.Owner = owner;
        dialog.ShowDialog();
    }

    private bool _hideCancel;

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
