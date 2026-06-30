using BeanTracker.Core.Data;
using BeanTracker.Core.Favourites;
using BeanTracker.MAUI.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Favourites;

public sealed partial class FavouritesViewModel(
    IFavouritesService favouritesService,
    DatasyncService datasyncService) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FavouriteDrink> Favourites { get; set; } = [];

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            await datasyncService.SynchronizeAsync();
            await LoadAsync();
            await FeedbackHelper.ShowNotificationAsync("Sync completed! 🚀");
        }
        catch (Exception ex)
        {
            await FeedbackHelper.ShowNotificationAsync($"Sync failed: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

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
        await FeedbackHelper.ShowNotificationAsync("Removed from Favourites 💔");
    }
}
