using BeanTracker.Core.Breweries;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BeanTracker.MAUI.Features.Breweries;

public sealed partial class BreweryDetailViewModel : ObservableObject
{
    [ObservableProperty]
    public partial Brewery? SelectedBrewery { get; set; }
}
