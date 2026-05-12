using BeanTracker.Core.Favourites;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Favourites;

public sealed partial class FavouritesViewModel(IFavouritesService favouritesService) : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<FavouriteDrink> Favourites { get; set; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        var all = await favouritesService.GetAllAsync();
        Favourites = new ObservableCollection<FavouriteDrink>(all);
    }

    [RelayCommand]
    private async Task RemoveFavouriteAsync(string drinkId)
    {
        await favouritesService.RemoveAsync(drinkId);
        var item = Favourites.FirstOrDefault(f => f.DrinkId == drinkId);
        if (item is not null) Favourites.Remove(item);
    }
}
