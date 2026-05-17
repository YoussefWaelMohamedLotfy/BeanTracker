namespace BeanTracker.MAUI.Features.OCR;

public sealed partial class OcrPage : ContentPage
{
    public OcrPage(OcrViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is OcrViewModel vm)
            await vm.LoadDrinksAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OcrViewModel.AnalysisResult))
            MainThread.BeginInvokeOnMainThread(() =>
                PageScrollView.ScrollToAsync(AnalysisResultBorder, ScrollToPosition.End, animated: false));
    }
}
