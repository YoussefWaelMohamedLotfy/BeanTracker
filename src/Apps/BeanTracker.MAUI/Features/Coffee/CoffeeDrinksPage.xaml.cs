namespace BeanTracker.MAUI.Features.Coffee;

public partial class CoffeeDrinksPage : ContentPage
{
    public CoffeeDrinksPage(CoffeeDrinksViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
