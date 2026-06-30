using BeanTracker.Core.Breweries;
using BeanTracker.Core.Coffee;
using BeanTracker.Core.Data;
using BeanTracker.Core.Favourites;
using BeanTracker.MAUI.Features.BarcodeScanner;
using BeanTracker.MAUI.Features.Bluetooth;
using BeanTracker.MAUI.Features.Breweries;
using BeanTracker.MAUI.Features.Coffee;
using BeanTracker.MAUI.Features.Favourites;
using BeanTracker.MAUI.Features.Feedback;
using BeanTracker.MAUI.Features.OCR;
using BeanTracker.MAUI.Features.Host;
using BeanTracker.MAUI.Features.SSO;
using BeanTracker.MAUI.Helpers;
using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.Maui.Audio;
using Plugin.Maui.Biometric;
using Plugin.Maui.ScreenSecurity;

namespace BeanTracker.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseScreenSecurity()
            .UseMauiCameraView()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Database — transient so each service gets a fresh unit-of-work
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "beantracker.db");

        // Point to the Aspire API backend port (7268 for HTTPS)
        // Android Emulator uses 10.0.2.2 to reach the host's localhost
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:7268/" : "https://localhost:7268/";
        var serviceEndpoint = new Uri(baseUrl);

        builder.Services.AddTransient(_ =>
        {
            var opts = new DbContextOptionsBuilder<BeanTrackerDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            // Create a custom HttpClient to bypass local SSL certificate validation
            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
            var httpClient = new HttpClient(handler) { BaseAddress = serviceEndpoint };

            return new BeanTrackerDbContext(opts, httpClient);
        });

        // Datasync offline sync service
        builder.Services.AddTransient<DatasyncService>();

        // Services
        builder.Services.AddSingleton<BatteryAwarenessService>();
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<IBiometric>(_ => BiometricAuthenticationService.Default);
        builder.Services.AddSingleton<IScreenSecurity>(_ => ScreenSecurity.Default);
        builder.Services.AddTransient<ICoffeeDrinkService, SyncedCoffeeDrinkService>();
        builder.Services.AddTransient<IFavouritesService, LocalFavouritesService>();
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddTransient<IBreweryService, SyncedBreweryService>();
        builder.Services.AddSingleton<KeycloakAuthService>();
        builder.Services.AddSingleton<ICoffeeImageService, CoffeeImageApiService>();

        // Pages
        builder.Services.AddSingleton<MainHostPage>();
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<CoffeeDrinksPage>();
        builder.Services.AddTransient<CoffeeDrinkDetailPage>();
        builder.Services.AddTransient<FavouritesPage>();
        builder.Services.AddTransient<BreweriesPage>();
        builder.Services.AddTransient<BreweryDetailPage>();
        builder.Services.AddTransient<OcrPage>();
        builder.Services.AddTransient<BarcodeScannerPage>();
        builder.Services.AddTransient<ImageSubmitPage>();
        builder.Services.AddTransient<BluetoothPage>();
        builder.Services.AddTransient<BleDeviceDetailPage>();
        builder.Services.AddTransient<SsoPage>();
        builder.Services.AddTransient<BeanTracker.MAUI.Features.Admin.AdminPage>();

        // ViewModels
        builder.Services.AddSingleton<MainHostViewModel>();
        builder.Services.AddTransient<CoffeeDrinksViewModel>();
        builder.Services.AddTransient<CoffeeDrinkDetailViewModel>();
        builder.Services.AddTransient<FavouritesViewModel>();
        builder.Services.AddTransient<BreweriesViewModel>();
        builder.Services.AddTransient<BreweryDetailViewModel>();
        builder.Services.AddTransient<OcrViewModel>();
        builder.Services.AddTransient<BarcodeScannerViewModel>();
        builder.Services.AddTransient<ImageSubmitViewModel>();
        builder.Services.AddTransient<BluetoothViewModel>();
        builder.Services.AddTransient<BleDeviceDetailViewModel>();
        builder.Services.AddTransient<SsoViewModel>();
        builder.Services.AddTransient<BeanTracker.MAUI.Features.Admin.AdminViewModel>();

        // Popups
        builder.Services.AddTransient<FeedbackPopup>();

        return builder.Build();
    }
}
