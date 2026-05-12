using BeanTracker.Core.Coffee;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Coffee;

public partial class CoffeeDrinksViewModel(ICoffeeDrinkService coffeeDrinkService) : ObservableObject
{
    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CoffeeDrink> drinks = [];

    [RelayCommand]
    private async Task SearchAsync()
    {
        var results = await coffeeDrinkService.SearchAsync(SearchQuery);
        Drinks = new ObservableCollection<CoffeeDrink>(results);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var all = await coffeeDrinkService.GetAllAsync();
        Drinks = new ObservableCollection<CoffeeDrink>(all);
    }
}
