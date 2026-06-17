namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweriesPage : BeanTracker.MAUI.Features.Host.FeatureView
{
    private readonly BreweriesViewModel _vm;

    public BreweriesPage(BreweriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public override void HandleAppearing()
    {
        base.HandleAppearing();
        _vm.LoadCommand.Execute(null);
    }
}
