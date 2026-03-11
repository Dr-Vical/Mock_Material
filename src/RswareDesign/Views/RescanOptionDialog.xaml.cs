using System.Windows;
using System.Windows.Controls;

namespace RswareDesign.Views;

public partial class RescanOptionDialog : Window
{
    private static readonly string[] DriveModels =
    {
        "CSD7N", "CSD7S", "CSD5N", "CSD5S",
        "CSD3N", "CSD3S", "CAD7N", "CAD5N",
        "CLD7N", "CLD5N", "CSD7NP", "CSD5NP",
    };

    private readonly List<CheckBox> _modelChecks = new();
    private readonly List<(TextBox from, TextBox to)> _channelRanges = new();

    public RescanOptionDialog()
    {
        InitializeComponent();
        BuildModelList();
        BuildChannelList();
    }

    private void BuildModelList()
    {
        foreach (var model in DriveModels)
        {
            var chk = new CheckBox
            {
                Content = model,
                IsChecked = true,
                Margin = new Thickness(0, 2, 0, 2),
                FontSize = 12,
            };
            _modelChecks.Add(chk);
            ModelCheckList.Children.Add(chk);
        }
    }

    private void BuildChannelList()
    {
        foreach (var model in DriveModels)
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            var label = new TextBlock
            {
                Text = model,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Foreground = (System.Windows.Media.Brush)FindResource("TextSecondary"),
            };
            Grid.SetColumn(label, 0);

            var fromBox = new TextBox
            {
                Text = "0",
                HorizontalContentAlignment = HorizontalAlignment.Center,
                FontSize = 11,
                Padding = new Thickness(4, 2, 4, 2),
                Style = (Style)FindResource("MaterialDesignOutlinedTextBox"),
            };
            Grid.SetColumn(fromBox, 1);

            var tilde = new TextBlock
            {
                Text = "~",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(6, 0, 6, 0),
                FontSize = 12,
                Foreground = (System.Windows.Media.Brush)FindResource("TextSecondary"),
            };
            Grid.SetColumn(tilde, 2);

            var toBox = new TextBox
            {
                Text = "10",
                HorizontalContentAlignment = HorizontalAlignment.Center,
                FontSize = 11,
                Padding = new Thickness(4, 2, 4, 2),
                Style = (Style)FindResource("MaterialDesignOutlinedTextBox"),
            };
            Grid.SetColumn(toBox, 3);

            row.Children.Add(label);
            row.Children.Add(fromBox);
            row.Children.Add(tilde);
            row.Children.Add(toBox);

            _channelRanges.Add((fromBox, toBox));
            ChannelList.Children.Add(row);
        }
    }

    private void ChkSelectAll_Click(object sender, RoutedEventArgs e)
    {
        var isChecked = ChkSelectAll.IsChecked == true;
        foreach (var chk in _modelChecks)
            chk.IsChecked = isChecked;
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
