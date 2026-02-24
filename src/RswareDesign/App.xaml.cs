using ControlzEx.Theming;
using System.Windows;

namespace RswareDesign;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.Current.ChangeTheme(this, "Dark.Blue");
    }
}
