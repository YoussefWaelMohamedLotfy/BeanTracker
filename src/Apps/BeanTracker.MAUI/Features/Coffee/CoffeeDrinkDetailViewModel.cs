using BeanTracker.Core.Coffee;
using BeanTracker.Core.Favourites;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinkDetailViewModel(
    IFavouritesService favouritesService,
    ICoffeeImageService coffeeImageService)
    : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FlavorNotesText))]
    public partial CoffeeDrink? SelectedDrink { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavouriteLabel))]
    public partial bool IsFavourite { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    public partial bool IsLoading { get; set; }

    public bool IsNotLoading => !IsLoading;

    [ObservableProperty]
    public partial string? ImageUrl { get; set; }

    public string FavouriteLabel => IsFavourite
        ? "❤️  Remove from Favourites"
        : "🤍  Save to Favourites";

    public string FlavorNotesText => SelectedDrink?.FlavorNotes is { Count: > 0 } notes
        ? string.Join(" • ", notes)
        : string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Drink", out var value) && value is CoffeeDrink drink)
            _ = InitializeAsync(drink);
    }

    public async Task InitializeAsync(CoffeeDrink drink)
    {
        IsLoading = true;
        SelectedDrink = drink;

        try
        {
            IsFavourite = await favouritesService.IsFavouriteAsync(drink.Id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] Could not check favourite status: {ex}");
        }

        try
        {
            ImageUrl = await coffeeImageService.GetRandomImageUrlAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] Could not fetch coffee image: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleFavouriteAsync()
    {
        if (SelectedDrink is null) return;
        if (IsFavourite)
        {
            await favouritesService.RemoveAsync(SelectedDrink.Id);
            IsFavourite = false;
        }
        else
        {
            await favouritesService.AddAsync(SelectedDrink.Id);
            IsFavourite = true;
        }
    }
}
