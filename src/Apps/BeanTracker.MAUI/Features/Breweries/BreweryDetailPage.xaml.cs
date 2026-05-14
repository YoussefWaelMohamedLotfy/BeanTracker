using System.ComponentModel;
using System.Globalization;

namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweryDetailPage : ContentPage
{
    public BreweryDetailPage(BreweryDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.PropertyChanged += OnViewModelPropertyChanged;
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
