using BeanTracker.Core.Coffee;
using BeanTracker.Core.Favourites;
using BeanTracker.MAUI.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinksViewModel(
    ICoffeeDrinkService coffeeDrinkService,
    ICoffeeImageService coffeeImageService,
    IFavouritesService favouritesService) : ObservableObject
{
    private const int DebounceMs = 400;

    private CancellationTokenSource? _debounceCts;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<CoffeeDrink> Drinks { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CoffeeDrinkCardItem> CardDrinks { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListView))]
    public partial bool IsCardSwipeView { get; set; }

    public bool IsListView => !IsCardSwipeView;

    public string ToggleViewLabel => IsCardSwipeView ? "📋  List" : "🃏  Cards";

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

    partial void OnIsCardSwipeViewChanged(bool value)
        => OnPropertyChanged(nameof(ToggleViewLabel));

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
        BuildCardDrinks(all);
        _ = LoadCardImagesAndFavouritesAsync();
    }

    [RelayCommand]
    private async Task SelectDrinkAsync(CoffeeDrink drink)
    {
        await Shell.Current.GoToAsync(nameof(CoffeeDrinkDetailPage), new Dictionary<string, object>
        {
            { "Drink", drink }
        });
    }

    [RelayCommand]
    private void ToggleView()
    {
        IsCardSwipeView = !IsCardSwipeView;
    }

    [RelayCommand]
    private async Task ToggleCardFavouriteAsync(CoffeeDrinkCardItem item)
    {
        if (item.IsFavourite)
        {
            await favouritesService.RemoveAsync(item.Drink.Id);
            item.IsFavourite = false;
            await FeedbackHelper.ShowNotificationAsync($"'{item.Drink.Name}' removed from Favourites 💔");
        }
        else
        {
            await favouritesService.AddAsync(item.Drink.Id);
            item.IsFavourite = true;
            await FeedbackHelper.ShowNotificationAsync($"'{item.Drink.Name}' added to Favourites ❤️");
        }
    }

    private void BuildCardDrinks(IReadOnlyList<CoffeeDrink> drinks)
    {
        CardDrinks = new ObservableCollection<CoffeeDrinkCardItem>(
            drinks.Select(d => new CoffeeDrinkCardItem(d)));
    }

    private async Task LoadCardImagesAndFavouritesAsync()
    {
        // Load each card's image and favourite state concurrently.
        // Cards update as their data arrives — no blocking wait.
        var tasks = CardDrinks.Select(async item =>
        {
            try { item.IsFavourite = await favouritesService.IsFavouriteAsync(item.Drink.Id); }
            catch { /* non-fatal */ }

            try
            {
                item.ImageUrl = await coffeeImageService.GetImageUrlAsync(item.Drink.Id);
            }
            catch { /* non-fatal */ }
            finally { item.IsImageLoading = false; }
        });
        await Task.WhenAll(tasks);
    }
}

