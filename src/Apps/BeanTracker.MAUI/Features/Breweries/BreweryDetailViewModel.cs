using BeanTracker.Core.Breweries;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BeanTracker.MAUI.Features.Breweries;

public partial class BreweryDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Brewery? selectedBrewery;
}
