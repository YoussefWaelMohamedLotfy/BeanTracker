namespace BeanTracker.MAUI.Features.SSO;

public sealed partial class SsoPage : BeanTracker.MAUI.Features.Host.FeatureView
{
    public SsoPage(SsoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public override void HandleAppearing()
    {
        base.HandleAppearing();
        if (BindingContext is SsoViewModel vm)
        {
            _ = vm.RestoreSessionAsync();
        }
    }
}
