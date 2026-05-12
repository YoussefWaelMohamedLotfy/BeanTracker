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
    }
}
