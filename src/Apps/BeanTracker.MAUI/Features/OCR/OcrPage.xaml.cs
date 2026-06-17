namespace BeanTracker.MAUI.Features.OCR;

public sealed partial class OcrPage : BeanTracker.MAUI.Features.Host.FeatureView
{
    private readonly OcrViewModel _vm;

    public OcrPage(OcrViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    public override async void HandleAppearing()
    {
        base.HandleAppearing();
        await _vm.LoadDrinksAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OcrViewModel.AnalysisResult))
            MainThread.BeginInvokeOnMainThread(() =>
                PageScrollView.ScrollToAsync(AnalysisResultBorder, ScrollToPosition.End, animated: false));
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.NewHandler is null)
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
    }
}
