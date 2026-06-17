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
        builder.Services.AddTransient(_ =>
        {
            var opts = new DbContextOptionsBuilder<BeanTrackerDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
            return new BeanTrackerDbContext(opts);
        });

        // Services
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<IBiometric>(_ => BiometricAuthenticationService.Default);
        builder.Services.AddSingleton<IScreenSecurity>(_ => ScreenSecurity.Default);
        builder.Services.AddSingleton<ICoffeeDrinkService>(_ =>
            new LocalCoffeeDrinkService(() => FileSystem.OpenAppPackageFileAsync("drinks.json")));
        builder.Services.AddTransient<IFavouritesService, LocalFavouritesService>();
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddTransient<IBreweryService, BreweryApiService>();
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

        // Popups
        builder.Services.AddTransient<FeedbackPopup>();

        var app = builder.Build();

        using var db = app.Services.GetRequiredService<BeanTrackerDbContext>();
        db.Database.EnsureCreated();
        // Ensures the BleRecordings table exists even on databases created before this feature was added.
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "BleRecordings" (
                "Id"                INTEGER NOT NULL CONSTRAINT "PK_BleRecordings" PRIMARY KEY AUTOINCREMENT,
                "DeviceId"          TEXT    NOT NULL,
                "DeviceName"        TEXT    NOT NULL,
                "ServiceId"         TEXT    NOT NULL,
                "ServiceName"       TEXT    NOT NULL,
                "CharacteristicId"  TEXT    NOT NULL,
                "CharacteristicName" TEXT   NOT NULL,
                "RawHex"            TEXT    NOT NULL,
                "AsciiValue"        TEXT,
                "Timestamp"         TEXT    NOT NULL,
                "SessionLabel"      TEXT
            )
            """);

        return app;
    }
}
