using Android.App;
using Android.Runtime;
using System.Diagnostics;

namespace BeanTracker.MAUI;

[Application]
public sealed class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        => Debug.WriteLine($"[BeanTracker] Fatal: {e.ExceptionObject}");

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Debug.WriteLine($"[BeanTracker] UnobservedTask: {e.Exception}");
        e.SetObserved();
    }
}
