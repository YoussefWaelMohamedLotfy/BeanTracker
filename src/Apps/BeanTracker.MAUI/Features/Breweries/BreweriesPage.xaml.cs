namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweriesPage : ContentPage
{
    public BreweriesPage(BreweriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
