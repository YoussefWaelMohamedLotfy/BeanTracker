using BeanTracker.MAUI.Features.BarcodeScanner;
using BeanTracker.MAUI.Features.Bluetooth;
using BeanTracker.MAUI.Features.Breweries;
using BeanTracker.MAUI.Features.Coffee;
using BeanTracker.MAUI.Features.Favourites;
using BeanTracker.MAUI.Features.OCR;
using BeanTracker.MAUI.Features.SSO;
using BeanTracker.MAUI.Helpers;
using System.ComponentModel;

namespace BeanTracker.MAUI.Features.Host;

public sealed partial class MainHostPage : ContentPage
{
    private readonly MainHostViewModel _vm;
    private readonly BatteryAwarenessService _batteryService;

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
        BluetoothPage      bluetoothPage,
        SsoPage            ssoPage,
        BatteryAwarenessService batteryService)
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
            bluetoothPage,
            ssoPage
        ];

        // Add all views to the grid but hide them initially
        foreach (var view in _views)
        {
            view.IsVisible = false;
            MainContent.Children.Add(view);
        }

        ShowTab(_vm.SelectedTabIndex);
        _views[_vm.SelectedTabIndex].HandleAppearing();

        _batteryService = batteryService;
        _batteryService.AnimationsEnabledChanged += OnAnimationsEnabledChanged;
        _batteryService.IsChargingChanged += OnIsChargingChanged;

        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _views[_vm.SelectedTabIndex].HandleAppearing();

        if (!_batteryService.AnimationsEnabled)
            _ = FeedbackHelper.ShowLowBatterySnackbarAsync();
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

    private void OnAnimationsEnabledChanged(object? sender, EventArgs e)
    {
        if (!_batteryService.AnimationsEnabled && this.Window is not null)
            _ = FeedbackHelper.ShowLowBatterySnackbarAsync();
    }

    private void OnIsChargingChanged(object? sender, EventArgs e)
    {
        if (this.Window is not null)
        {
            if (_batteryService.IsCharging)
            {
                _ = FeedbackHelper.ShowChargingSnackbarAsync();
            }
            else if (_batteryService.AnimationsEnabled)
            {
                // Only show "no longer charging" if we aren't immediately entering low-battery mode.
                // If we are, OnAnimationsEnabledChanged will show the low-battery snackbar instead.
                _ = FeedbackHelper.ShowNotChargingSnackbarAsync();
            }
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
