using System.ComponentModel;
using System.Diagnostics;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinksPage : ContentPage
{
    private readonly CoffeeDrinksViewModel _vm;

    public CoffeeDrinksPage(CoffeeDrinksViewModel vm)
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] CoffeeDrinksPage.InitializeComponent failed: {ex}");
            throw;
        }
        BindingContext = _vm = vm;
        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            _vm.LoadCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] CoffeeDrinksPage.OnAppearing failed: {ex}");
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CoffeeDrinksViewModel.IsCardSwipeView) && _vm.IsCardSwipeView)
        {
            // Some platforms (WinUI in particular) do not re-measure a control that was
            // invisible when first laid out. Force a layout pass after the toggle.
            Dispatcher.Dispatch(() => SwipeCard.InvalidateMeasure());
        }
    }
}
