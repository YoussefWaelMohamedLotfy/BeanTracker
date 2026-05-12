namespace BeanTracker.MAUI.Features.Breweries;

public partial class BreweryDetailPage : ContentPage
{
    public BreweryDetailPage(BreweryDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
