namespace BeanTracker.MAUI.Features.Bluetooth;

public sealed partial class BleDeviceDetailPage : ContentPage
{
    private readonly BleDeviceDetailViewModel _vm;

    public BleDeviceDetailPage(BleDeviceDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _ = _vm.CleanupAsync();
    }
}
