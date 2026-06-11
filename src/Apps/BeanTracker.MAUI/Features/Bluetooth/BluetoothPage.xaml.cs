namespace BeanTracker.MAUI.Features.Bluetooth;

public sealed partial class BluetoothPage : ContentPage
{
    private readonly BluetoothViewModel _vm;

    public BluetoothPage(BluetoothViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Initialize();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Cleanup();
    }
}
