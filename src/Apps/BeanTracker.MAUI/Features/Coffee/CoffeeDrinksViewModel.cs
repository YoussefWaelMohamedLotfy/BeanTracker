using BeanTracker.Core.Coffee;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinksViewModel(ICoffeeDrinkService coffeeDrinkService) : ObservableObject
{
    private const int DebounceMs = 400;

    private CancellationTokenSource? _debounceCts;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<CoffeeDrink> Drinks { get; set; } = [];

    // Called by CommunityToolkit.Mvvm whenever SearchQuery changes (every keystroke).
    partial void OnSearchQueryChanged(string value)
    {
        if (_debounceCts is not null)
        {
            _ = _debounceCts.CancelAsync();
            _debounceCts.Dispose();
        }
        _debounceCts = new CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(value))
        {
            // Cleared — show everything immediately, no delay.
            _ = LoadAsync();
            return;
        }

        _ = DebounceSearchAsync(_debounceCts.Token);
    }

    private async Task DebounceSearchAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(DebounceMs, token);
            await ApplySearchAsync();
        }
        catch (OperationCanceledException)
        {
            // Debounce cancelled — a newer keystroke superseded this one; nothing to do.
        }
    }

    // Bound to SearchBar.SearchCommand — fires immediately on keyboard search press.
    [RelayCommand]
    private async Task SearchAsync()
    {
        if (_debounceCts is not null)
            _ = _debounceCts.CancelAsync(); // discard any in-flight debounce
        await ApplySearchAsync();
    }

    private async Task ApplySearchAsync()
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

    [RelayCommand]
    private async Task SelectDrinkAsync(CoffeeDrink drink)
    {
        await Shell.Current.GoToAsync(nameof(CoffeeDrinkDetailPage), new Dictionary<string, object>
        {
            { "Drink", drink }
        });
    }
}
