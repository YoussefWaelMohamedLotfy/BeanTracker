namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweryDetailPage : ContentPage
{
    public BreweryDetailPage(BreweryDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
