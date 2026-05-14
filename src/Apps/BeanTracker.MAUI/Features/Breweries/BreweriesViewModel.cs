using BeanTracker.Core.Breweries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweriesViewModel(IBreweryService breweryService) : ObservableObject
{
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<Brewery> Breweries { get; set; } = [];

    [RelayCommand]
    private async Task SearchAsync()
    {
        var results = await breweryService.SearchAsync(SearchQuery);
        Breweries = new ObservableCollection<Brewery>(results);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var all = await breweryService.GetAllAsync();
        Breweries = new ObservableCollection<Brewery>(all);
    }

    [RelayCommand]
    private static async Task SelectBreweryAsync(Brewery brewery)
    {
        await Shell.Current.GoToAsync(nameof(BreweryDetailPage), new Dictionary<string, object>
        {
            ["BreweryId"] = brewery.Id
        });
    }
}
