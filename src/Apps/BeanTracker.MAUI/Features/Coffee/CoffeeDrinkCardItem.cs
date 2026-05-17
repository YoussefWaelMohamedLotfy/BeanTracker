using BeanTracker.Core.Coffee;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinkCardItem(CoffeeDrink drink) : ObservableObject
{
    // Macchiato (#8B4513) and WarmGray200 (#D9C8B4) match Colors.xaml palette entries.
    private static readonly Color FavouriteActiveBackground = Color.FromArgb("#8B4513");
    private static readonly Color FavouriteInactiveBackground = Color.FromArgb("#D9C8B4");
    private static readonly Color FavouriteActiveText = Color.FromArgb("#FAF0E6");   // FlatWhite
    private static readonly Color FavouriteInactiveText = Color.FromArgb("#1A0A00"); // Espresso

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
    [NotifyPropertyChangedFor(nameof(FavouriteButtonBackground))]
    [NotifyPropertyChangedFor(nameof(FavouriteButtonTextColor))]
    public partial bool IsFavourite { get; set; }

    public string FavouriteLabel => IsFavourite
        ? "❤️  Remove from Favourites"
        : "🤍  Save to Favourites";

    /// <summary>Button background color — bound directly to avoid DataTrigger in XAML.</summary>
    public Color FavouriteButtonBackground => IsFavourite ? FavouriteActiveBackground : FavouriteInactiveBackground;

    /// <summary>Button text color — bound directly to avoid DataTrigger in XAML.</summary>
    public Color FavouriteButtonTextColor => IsFavourite ? FavouriteActiveText : FavouriteInactiveText;

    public string FlavorNotesText => Drink.FlavorNotes is { Count: > 0 } notes
        ? string.Join(" • ", notes)
        : string.Empty;
}
