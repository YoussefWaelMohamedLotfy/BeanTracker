using BeanTracker.Core.Breweries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Breweries;

public partial class BreweriesViewModel(IBreweryService breweryService) : ObservableObject
{
    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Brewery> breweries = [];

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
}
