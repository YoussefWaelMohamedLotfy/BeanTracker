using Microsoft.UI.Xaml;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BeanTracker.MAUI.WinUI;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public sealed partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += OnUnhandledException;
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"[BeanTracker] Unhandled exception: {e.Exception}");
        // Mark as handled to prevent an immediate crash; MAUI's own handler will take over.
        e.Handled = true;
    }
}

