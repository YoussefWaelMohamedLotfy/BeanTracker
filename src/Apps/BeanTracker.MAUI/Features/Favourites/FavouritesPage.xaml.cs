namespace BeanTracker.MAUI.Features.Favourites;

public partial class FavouritesPage : ContentPage
{
    public FavouritesPage(FavouritesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
