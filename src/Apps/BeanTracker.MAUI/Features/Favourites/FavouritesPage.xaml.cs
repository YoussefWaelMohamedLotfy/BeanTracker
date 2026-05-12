namespace BeanTracker.MAUI.Features.Favourites;

public sealed partial class FavouritesPage : ContentPage
{
    public FavouritesPage(FavouritesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
