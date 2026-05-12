using BeanTracker.Core.Coffee;
using BeanTracker.Core.Favourites;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeanTracker.MAUI.Features.Coffee;

public partial class CoffeeDrinkDetailViewModel(IFavouritesService favouritesService) : ObservableObject
{
    [ObservableProperty]
    private CoffeeDrink? selectedDrink;

    [ObservableProperty]
    private string favouriteLabel = "Save to Favourites";

    public async Task InitializeAsync(CoffeeDrink drink)
    {
        SelectedDrink = drink;
        var isFav = await favouritesService.IsFavouriteAsync(drink.Id);
        FavouriteLabel = isFav ? "Remove from Favourites" : "Save to Favourites";
    }

    [RelayCommand]
    private async Task ToggleFavouriteAsync()
    {
        if (SelectedDrink is null) return;
        if (await favouritesService.IsFavouriteAsync(SelectedDrink.Id))
        {
            await favouritesService.RemoveAsync(SelectedDrink.Id);
            FavouriteLabel = "Save to Favourites";
        }
        else
        {
            await favouritesService.AddAsync(SelectedDrink.Id);
            FavouriteLabel = "Remove from Favourites";
        }
    }
}
