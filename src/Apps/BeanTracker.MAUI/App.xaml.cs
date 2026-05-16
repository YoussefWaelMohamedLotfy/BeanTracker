using Microsoft.Extensions.DependencyInjection;

namespace BeanTracker.MAUI;

public sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var splash = IPlatformApplication.Current!.Services.GetRequiredService<SplashPage>();
        return new Window(splash);
    }
}