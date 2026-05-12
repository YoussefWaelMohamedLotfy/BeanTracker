namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinksPage : ContentPage
{
    private readonly CoffeeDrinksViewModel _vm;

    public CoffeeDrinksPage(CoffeeDrinksViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }
}
