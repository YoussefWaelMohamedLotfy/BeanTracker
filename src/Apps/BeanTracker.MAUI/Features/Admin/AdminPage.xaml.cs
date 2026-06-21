namespace BeanTracker.MAUI.Features.Admin;

public partial class AdminPage : ContentPage
{
    public AdminPage(AdminViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
