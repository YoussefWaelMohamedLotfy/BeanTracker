using BeanTracker.MAUI.Features.BarcodeScanner;
using BeanTracker.MAUI.Features.Bluetooth;
using BeanTracker.MAUI.Features.Breweries;
using BeanTracker.MAUI.Features.Coffee;
using BeanTracker.MAUI.Features.Favourites;
using BeanTracker.MAUI.Features.OCR;
using System.ComponentModel;

namespace BeanTracker.MAUI.Features.Host;

public sealed partial class MainHostPage : ContentPage
{
    private readonly MainHostViewModel _vm;

    // The 6 feature views resolved from DI.
    private readonly FeatureView[] _views;

    private int _previousTabIndex = MainHostViewModel.TabCoffee;

    public MainHostPage(
        MainHostViewModel  vm,
        CoffeeDrinksPage   coffeePage,
        FavouritesPage     favouritesPage,
        BreweriesPage      breweriesPage,
        OcrPage            ocrPage,
        BarcodeScannerPage barcodePage,
        BluetoothPage      bluetoothPage)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        _views =
        [
            coffeePage,
            favouritesPage,
            breweriesPage,
            ocrPage,
            barcodePage,
            bluetoothPage
        ];

        // Add all views to the grid but hide them initially
        foreach (var view in _views)
        {
            view.IsVisible = false;
            MainContent.Children.Add(view);
        }

        ShowTab(_vm.SelectedTabIndex);
        _views[_vm.SelectedTabIndex].HandleAppearing();

        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _views[_vm.SelectedTabIndex].HandleAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _views[_vm.SelectedTabIndex].HandleDisappearing();
    }

    /// <summary>
    /// Swaps <see cref="MainContent"/> to show the view for <paramref name="tabIndex"/>.
    /// </summary>
    private void ShowTab(int tabIndex)
    {
        for (int i = 0; i < _views.Length; i++)
        {
            _views[i].IsVisible = (i == tabIndex);
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainHostViewModel.SelectedTabIndex))
            return;

        // Tell the outgoing view it is leaving.
        _views[_previousTabIndex].HandleDisappearing();

        // Swap content, then notify incoming view it is appearing.
        ShowTab(_vm.SelectedTabIndex);
        _views[_vm.SelectedTabIndex].HandleAppearing();

        _previousTabIndex = _vm.SelectedTabIndex;
    }
}
