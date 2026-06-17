namespace BeanTracker.MAUI.Features.Favourites;

public sealed partial class FavouritesPage : BeanTracker.MAUI.Features.Host.FeatureView
{
    private readonly FavouritesViewModel _vm;

    public FavouritesPage(FavouritesViewModel vm)
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
