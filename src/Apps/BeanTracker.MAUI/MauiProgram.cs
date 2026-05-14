using BeanTracker.Core.Breweries;
using BeanTracker.Core.Coffee;
using BeanTracker.Core.Data;
using BeanTracker.Core.Favourites;
using BeanTracker.MAUI.Features.Breweries;
using BeanTracker.MAUI.Features.Coffee;
using BeanTracker.MAUI.Features.Favourites;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeanTracker.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
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
        builder.Services.AddSingleton<ICoffeeDrinkService>(_ =>
            new LocalCoffeeDrinkService(() => FileSystem.OpenAppPackageFileAsync("drinks.json")));
        builder.Services.AddTransient<IFavouritesService, LocalFavouritesService>();
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddTransient<IBreweryService, BreweryApiService>();
        builder.Services.AddTransient<ICoffeeImageService, CoffeeImageApiService>();

        // Pages
        builder.Services.AddTransient<CoffeeDrinksPage>();
        builder.Services.AddTransient<CoffeeDrinkDetailPage>();
        builder.Services.AddTransient<FavouritesPage>();
        builder.Services.AddTransient<BreweriesPage>();
        builder.Services.AddTransient<BreweryDetailPage>();

        // ViewModels
        builder.Services.AddTransient<CoffeeDrinksViewModel>();
        builder.Services.AddTransient<CoffeeDrinkDetailViewModel>();
        builder.Services.AddTransient<FavouritesViewModel>();
        builder.Services.AddTransient<BreweriesViewModel>();
        builder.Services.AddTransient<BreweryDetailViewModel>();

        var app = builder.Build();

        // Ensure SQLite schema is created on first launch
        using var db = app.Services.GetRequiredService<BeanTrackerDbContext>();
        db.Database.EnsureCreated();

        return app;
    }
}
