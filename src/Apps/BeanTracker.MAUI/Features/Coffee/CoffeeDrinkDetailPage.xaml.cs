namespace BeanTracker.MAUI.Features.Coffee;

public partial class CoffeeDrinkDetailPage : ContentPage
{
    public CoffeeDrinkDetailPage(CoffeeDrinkDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
