using BeanTracker.Core.Coffee;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinkCardItem(CoffeeDrink drink) : ObservableObject
{
    public CoffeeDrink Drink { get; } = drink;

    /// <summary>
    /// Set by the ViewModel so XAML can bind directly without RelativeSource.
    /// </summary>
    public ICommand? ToggleFavouriteCommand { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsImageVisible))]
    public partial string? ImageUrl { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsImageVisible))]
    public partial bool IsImageLoading { get; set; } = true;

    public bool IsImageVisible => !IsImageLoading && ImageUrl is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavouriteLabel))]
    public partial bool IsFavourite { get; set; }

    public string FavouriteLabel => IsFavourite
        ? "❤️  Remove from Favourites"
        : "🤍  Save to Favourites";

    public string FlavorNotesText => Drink.FlavorNotes is { Count: > 0 } notes
        ? string.Join(" • ", notes)
        : string.Empty;
}
