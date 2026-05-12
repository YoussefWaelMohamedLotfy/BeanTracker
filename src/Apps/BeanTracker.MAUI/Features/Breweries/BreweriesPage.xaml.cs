namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweriesPage : ContentPage
{
    private readonly BreweriesViewModel _vm;

    public BreweriesPage(BreweriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }
}
