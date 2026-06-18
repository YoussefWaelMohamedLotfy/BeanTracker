using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeanTracker.MAUI.Features.Host;

/// <summary>
/// Drives the custom scrollable bottom tab bar in <see cref="MainHostPage"/>.
/// </summary>
public sealed partial class MainHostViewModel : ObservableObject
{
    // ── Tab indices (must match the order in MainHostPage.xaml) ──────────
    public const int TabCoffee        = 0;
    public const int TabFavourites    = 1;
    public const int TabBreweries     = 2;
    public const int TabOcr           = 3;
    public const int TabBarcode       = 4;
    public const int TabBluetooth     = 5;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(CoffeeTabTextColor),
        nameof(FavouritesTabTextColor),
        nameof(BreweriesTabTextColor),
        nameof(OcrTabTextColor),
        nameof(BarcodeTabTextColor),
        nameof(BluetoothTabTextColor),
        nameof(CoffeeUnderlineOpacity),
        nameof(FavouritesUnderlineOpacity),
        nameof(BreweriesUnderlineOpacity),
        nameof(OcrUnderlineOpacity),
        nameof(BarcodeUnderlineOpacity),
        nameof(BluetoothUnderlineOpacity))]
    public partial int SelectedTabIndex { get; set; } = TabCoffee;


    // ── Tab text colours (resolved in XAML via AppThemeBinding fallback) ─
    //    We return string keys so the XAML can use DynamicResource-equivalent
    //    colours via converter; simpler: just expose Color directly.
    private static readonly Color SelectedColor   = Color.FromArgb("#C68642"); // Cappuccino / Primary
    private Color UnselectedColor => Application.Current?.RequestedTheme == AppTheme.Dark 
        ? Colors.LightGray 
        : Colors.DarkGray;

    public Color CoffeeTabTextColor     => SelectedTabIndex == TabCoffee     ? SelectedColor : UnselectedColor;
    public Color FavouritesTabTextColor => SelectedTabIndex == TabFavourites ? SelectedColor : UnselectedColor;
    public Color BreweriesTabTextColor  => SelectedTabIndex == TabBreweries  ? SelectedColor : UnselectedColor;
    public Color OcrTabTextColor        => SelectedTabIndex == TabOcr        ? SelectedColor : UnselectedColor;
    public Color BarcodeTabTextColor    => SelectedTabIndex == TabBarcode    ? SelectedColor : UnselectedColor;
    public Color BluetoothTabTextColor  => SelectedTabIndex == TabBluetooth  ? SelectedColor : UnselectedColor;

    // ── Underline indicator opacity (1 = visible, 0 = hidden) ───────────
    public double CoffeeUnderlineOpacity     => SelectedTabIndex == TabCoffee     ? 1.0 : 0.0;
    public double FavouritesUnderlineOpacity => SelectedTabIndex == TabFavourites ? 1.0 : 0.0;
    public double BreweriesUnderlineOpacity  => SelectedTabIndex == TabBreweries  ? 1.0 : 0.0;
    public double OcrUnderlineOpacity        => SelectedTabIndex == TabOcr        ? 1.0 : 0.0;
    public double BarcodeUnderlineOpacity    => SelectedTabIndex == TabBarcode    ? 1.0 : 0.0;
    public double BluetoothUnderlineOpacity  => SelectedTabIndex == TabBluetooth  ? 1.0 : 0.0;

    // ── Command ──────────────────────────────────────────────────────────
    [RelayCommand]
    private void SelectTab(string indexStr)
    {
        if (int.TryParse(indexStr, out int index))
        {
            SelectedTabIndex = index;
        }
    }
}
