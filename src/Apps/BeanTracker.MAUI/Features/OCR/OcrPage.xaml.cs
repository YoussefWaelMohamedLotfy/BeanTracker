namespace BeanTracker.MAUI.Features.OCR;

public sealed partial class OcrPage : ContentPage
{
    public OcrPage(OcrViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
