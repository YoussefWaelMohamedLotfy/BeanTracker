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
}
