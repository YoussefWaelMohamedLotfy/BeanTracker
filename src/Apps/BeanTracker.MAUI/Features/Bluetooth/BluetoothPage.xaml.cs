namespace BeanTracker.MAUI.Features.Bluetooth;

public sealed partial class BluetoothPage : BeanTracker.MAUI.Features.Host.FeatureView
{
    private readonly BluetoothViewModel _vm;

    public BluetoothPage(BluetoothViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public override void HandleAppearing()
    {
        base.HandleAppearing();
        _vm.Initialize();
    }

    public override void HandleDisappearing()
    {
        base.HandleDisappearing();
        _vm.Cleanup();
    }
}
