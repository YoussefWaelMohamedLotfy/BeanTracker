namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinkDetailPage : ContentPage
{
    public CoffeeDrinkDetailPage(CoffeeDrinkDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
