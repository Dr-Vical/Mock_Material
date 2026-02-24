using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows.Data;

namespace RswareDesign.Converters;

public class StringToPackIconKindConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && Enum.TryParse<PackIconKind>(s, out var kind))
            return kind;
        return PackIconKind.Folder;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
