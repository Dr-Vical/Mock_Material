using ControlzEx.Theming;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Threading;

namespace RswareDesign;

public partial class App : Application
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        // Clear previous log
        try { File.Delete(LogPath); } catch { }

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;

        base.OnStartup(e);
        ThemeManager.Current.ChangeTheme(this, "Dark.Blue");

        // i18n: CSV 기반 다국어 로드 (기본: 한국어)
        Services.LocalizationService.Initialize();
        Services.LocalizationService.ApplyLanguage(isKorean: true);
    }

    private static void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        // Skip common non-critical exceptions
        var ex = e.Exception;
        var type = ex.GetType().Name;
        if (type is "BindingExpression" or "ResourceReferenceKeyNotFoundException") return;
        if (ex.StackTrace?.Contains("PresentationFramework") == true && type == "InvalidOperationException") return;

        var msg = $"[FirstChance] {type}: {ex.Message}";
        if (ex.StackTrace != null)
            msg += $"\n  at {string.Join("\n  at ", ex.StackTrace.Split('\n').Take(5).Select(s => s.Trim()))}";
        try { File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n\n"); } catch { }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var msg = $"[UNHANDLED-UI] {e.Exception.GetType().Name}: {e.Exception.Message}\n{e.Exception.StackTrace}";
        if (e.Exception.InnerException != null)
            msg += $"\n--- Inner: {e.Exception.InnerException.Message}\n{e.Exception.InnerException.StackTrace}";
        try { File.AppendAllText(LogPath, $"\n=== {DateTime.Now:HH:mm:ss} ===\n{msg}\n"); } catch { }
        Console.Error.WriteLine(msg);
        e.Handled = true;
    }

    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        var msg = $"[UNHANDLED-Domain] {ex?.GetType().Name}: {ex?.Message}\n{ex?.StackTrace}";
        try { File.AppendAllText(LogPath, $"\n=== {DateTime.Now:HH:mm:ss} ===\n{msg}\n"); } catch { }
        Console.Error.WriteLine(msg);
    }
}
