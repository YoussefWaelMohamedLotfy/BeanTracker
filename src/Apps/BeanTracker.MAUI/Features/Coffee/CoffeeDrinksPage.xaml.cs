namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinksPage : ContentPage
{
    public CoffeeDrinksPage(CoffeeDrinksViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
