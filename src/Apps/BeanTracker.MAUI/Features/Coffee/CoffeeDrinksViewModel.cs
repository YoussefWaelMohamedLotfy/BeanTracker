using BeanTracker.Core.Coffee;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinksViewModel(ICoffeeDrinkService coffeeDrinkService) : ObservableObject
{
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<CoffeeDrink> Drinks { get; set; } = [];

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
