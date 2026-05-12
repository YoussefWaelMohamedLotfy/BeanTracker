namespace BeanTracker.MAUI.Features.Favourites;

public sealed partial class FavouritesPage : ContentPage
{
    private readonly FavouritesViewModel _vm;

    public FavouritesPage(FavouritesViewModel vm)
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
