var builder = DistributedApplication.CreateBuilder(args);

var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
    .WithAnonymousAccess();

var mauiApp = builder.AddMauiProject("mauiapp", "../../Apps/BeanTracker.MAUI/BeanTracker.MAUI.csproj");

if (OperatingSystem.IsWindows())
{
    mauiApp.AddWindowsDevice();
}

if (OperatingSystem.IsMacOS())
{
    mauiApp.AddMacCatalystDevice();

    mauiApp.AddiOSSimulator()
        .WithOtlpDevTunnel();
}

mauiApp.AddAndroidDevice()
    .WithOtlpDevTunnel();

await builder.Build().RunAsync();
