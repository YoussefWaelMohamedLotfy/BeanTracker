using BeanTracker.Core.Breweries;
using BeanTracker.Core.Data;
using BeanTracker.MAUI.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweriesViewModel(
    IBreweryService breweryService,
    DatasyncService datasyncService) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

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
        var all = await breweryService.GetAllAsync();
        Breweries = new ObservableCollection<Brewery>(all);
    }

    [ObservableProperty]
    public partial Brewery? SelectedItem { get; set; }

    [RelayCommand]
    private async Task SelectBreweryAsync(Brewery brewery)
    {
        if (brewery is null) return;

        await Shell.Current.GoToAsync(nameof(BreweryDetailPage), new Dictionary<string, object>
        {
            ["BreweryId"] = brewery.Id
        });

        SelectedItem = null;
    }
}
