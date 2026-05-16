using System.ComponentModel;
using System.Globalization;
using Plugin.Maui.ScreenSecurity;

namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweryDetailPage : ContentPage
{
    private readonly BreweryDetailViewModel _vm;
    private readonly IScreenSecurity _screenSecurity;

    public BreweryDetailPage(BreweryDetailViewModel vm, IScreenSecurity screenSecurity)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _screenSecurity = screenSecurity;
        _screenSecurity.ThrowErrors = true;
        vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetScreenSecurity(true);
    }

    protected override void OnDisappearing()
    {
#if !ANDROID
        // For iOS/Windows the plugin correctly handles backgrounding vs navigation
        SetScreenSecurity(false);
#endif
        base.OnDisappearing();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
#if ANDROID
        // Remove FLAG_SECURE only when navigating away inside the app.
        // OnDisappearing fires too early (before the Recents thumbnail is captured),
        // so we use OnNavigatedFrom which fires only on in-app navigation.
        SetScreenSecurity(false);
#endif
        base.OnNavigatedFrom(args);
    }

    private void SetScreenSecurity(bool enable)
    {
#if ANDROID
        // Use FLAG_SECURE directly — most reliable cross-version approach
        var activity = Platform.CurrentActivity;
        if (activity == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (enable)
                activity.Window?.AddFlags(Android.Views.WindowManagerFlags.Secure);
            else
                activity.Window?.ClearFlags(Android.Views.WindowManagerFlags.Secure);
        });
#else
        // Use the plugin for iOS / Windows
        Dispatcher.Dispatch(() =>
        {
            try
            {
                if (enable)
                {
                    if (!_screenSecurity.IsProtectionEnabled)
                        _screenSecurity.ActivateScreenSecurityProtection();
                }
                else
                {
                    _screenSecurity.DeactivateScreenSecurityProtection();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScreenSecurity] {(enable ? "Activate" : "Deactivate")} failed: {ex.Message}");
            }
        });
#endif
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(BreweryDetailViewModel.HasLocation)) return;
        if (BindingContext is not BreweryDetailViewModel vm || !vm.HasLocation) return;

        var lat = vm.SelectedBrewery!.Latitude!.Value.ToString(CultureInfo.InvariantCulture);
        var lon = vm.SelectedBrewery.Longitude!.Value.ToString(CultureInfo.InvariantCulture);
        var name = (vm.SelectedBrewery.Name ?? string.Empty)
            .Replace("\\", "\\\\").Replace("'", "\\'");

        MapWebView.Source = new HtmlWebViewSource
        {
            Html = $$"""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
                    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                    <style>html,body,#map{margin:0;padding:0;width:100%;height:100%}</style>
                </head>
                <body>
                    <div id="map"></div>
                    <script>
                        var map = L.map('map').setView([{{lat}},{{lon}}], 15);
                        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                            attribution: '&copy; OpenStreetMap contributors',
                            maxZoom: 19
                        }).addTo(map);
                        L.marker([{{lat}},{{lon}}]).addTo(map).bindPopup('{{name}}').openPopup();
                    </script>
                </body>
                </html>
                """
        };
    }
}
