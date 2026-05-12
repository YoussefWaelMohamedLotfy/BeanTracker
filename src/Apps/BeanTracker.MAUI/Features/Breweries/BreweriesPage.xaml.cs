namespace BeanTracker.MAUI.Features.Breweries;

public partial class BreweriesPage : ContentPage
{
    public BreweriesPage(BreweriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
