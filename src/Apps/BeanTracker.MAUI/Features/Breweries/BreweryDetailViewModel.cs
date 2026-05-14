using BeanTracker.Core.Breweries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweryDetailViewModel(IBreweryService breweryService)
    : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAddress), nameof(HasPhone), nameof(HasWebsite), nameof(AddressDisplay), nameof(HasLocation))]
    public partial Brewery? SelectedBrewery { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;
    public bool HasAddress => !string.IsNullOrEmpty(SelectedBrewery?.Address1);
    public bool HasPhone => !string.IsNullOrEmpty(SelectedBrewery?.Phone);
    public bool HasWebsite => !string.IsNullOrEmpty(SelectedBrewery?.WebsiteUrl);
    public bool HasLocation => SelectedBrewery?.Latitude is not null && SelectedBrewery?.Longitude is not null;

    public string AddressDisplay
    {
        get
        {
            if (SelectedBrewery is null) return string.Empty;
            var parts = new[] { SelectedBrewery.Address1, SelectedBrewery.Address2, SelectedBrewery.Address3, SelectedBrewery.PostalCode }
                .Where(p => !string.IsNullOrEmpty(p));
            return string.Join("\n", parts);
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("BreweryId", out var value) && value is string id)
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(string id)
    {
        IsBusy = true;
        try
        {
            SelectedBrewery = await breweryService.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] Could not load brewery details: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenPhoneAsync()
    {
        if (!string.IsNullOrEmpty(SelectedBrewery?.Phone))
            await Launcher.OpenAsync($"tel:{SelectedBrewery.Phone}");
    }

    [RelayCommand]
    private async Task OpenWebsiteAsync()
    {
        if (!string.IsNullOrEmpty(SelectedBrewery?.WebsiteUrl))
            await Launcher.OpenAsync(SelectedBrewery.WebsiteUrl);
    }
}
