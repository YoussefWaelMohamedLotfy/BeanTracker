using BeanTracker.MAUI.Features.Breweries;
using BeanTracker.MAUI.Features.Coffee;

namespace BeanTracker.MAUI;

public sealed partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(CoffeeDrinkDetailPage), typeof(CoffeeDrinkDetailPage));
        Routing.RegisterRoute(nameof(BreweryDetailPage), typeof(BreweryDetailPage));

#if WINDOWS
        this.Loaded += OnWindowsLoaded;
#endif
    }

#if WINDOWS
    // WinUI cannot render SVG images, so we point each tab at the pre-rasterised PNG
    // that MAUI's build task writes next to the executable (e.g. tab_coffee.scale-100.png).
    private void OnWindowsLoaded(object? sender, EventArgs e)
    {
        string baseDir = AppContext.BaseDirectory;
        CoffeeTab.Icon    = ImageSource.FromFile(Path.Combine(baseDir, "tab_coffee.scale-100.png"));
        FavouritesTab.Icon = ImageSource.FromFile(Path.Combine(baseDir, "tab_favourites.scale-100.png"));
        BreweriesTab.Icon  = ImageSource.FromFile(Path.Combine(baseDir, "tab_breweries.scale-100.png"));
    }
#endif
}
