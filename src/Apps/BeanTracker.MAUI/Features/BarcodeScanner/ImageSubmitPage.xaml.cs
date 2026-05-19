namespace BeanTracker.MAUI.Features.BarcodeScanner;

public sealed partial class ImageSubmitPage : ContentPage
{
    public ImageSubmitPage(ImageSubmitViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
