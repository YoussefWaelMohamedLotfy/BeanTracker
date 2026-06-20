using BeanTracker.MAUI.Helpers;
using System.ComponentModel;
using System.Diagnostics;

namespace BeanTracker.MAUI.Features.Coffee;

public sealed partial class CoffeeDrinksPage : BeanTracker.MAUI.Features.Host.FeatureView
{
    private readonly CoffeeDrinksViewModel _vm;
    private readonly BatteryAwarenessService _batteryService;
    private CancellationTokenSource? _arrowAnimCts;

    public CoffeeDrinksPage(CoffeeDrinksViewModel vm, BatteryAwarenessService batteryService)
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] CoffeeDrinksPage.InitializeComponent failed: {ex}");
            throw;
        }
        BindingContext = _vm = vm;
        _batteryService = batteryService;
        _vm.PropertyChanged += OnVmPropertyChanged;
        _batteryService.AnimationsEnabledChanged += OnAnimationsEnabledChanged;
    }

    public override void HandleAppearing()
    {
        base.HandleAppearing();
        try
        {
            _vm.LoadCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] CoffeeDrinksPage.OnAppearing failed: {ex}");
        }

        if (_vm.IsCardSwipeView)
            StartArrowAnimation();
    }

    public override void HandleDisappearing()
    {
        base.HandleDisappearing();
        StopArrowAnimation();
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.NewHandler is null)
        {
            _vm.PropertyChanged -= OnVmPropertyChanged;
            _batteryService.AnimationsEnabledChanged -= OnAnimationsEnabledChanged;
        }
    }

    private void OnAnimationsEnabledChanged(object? sender, EventArgs e)
    {
        if (_vm.IsCardSwipeView)
        {
            if (_batteryService.AnimationsEnabled)
                StartArrowAnimation();
            else
                StopArrowAnimation();
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CoffeeDrinksViewModel.IsCardSwipeView))
            return;

        if (_vm.IsCardSwipeView)
        {
            // Some platforms (WinUI in particular) do not re-measure a control that was
            // invisible when first laid out. Force a layout pass after the toggle.
            Dispatcher.Dispatch(() => SwipeCard.InvalidateMeasure());
            StartArrowAnimation();
        }
        else
        {
            StopArrowAnimation();
        }
    }

    private void StartArrowAnimation()
    {
        // Don't start if battery is too low.
        if (!_batteryService.AnimationsEnabled)
            return;

        StopArrowAnimation();
        _arrowAnimCts = new CancellationTokenSource();
        _ = RunArrowAnimationAsync(_arrowAnimCts.Token);
    }

    private void StopArrowAnimation()
    {
        _arrowAnimCts?.Cancel();
        _arrowAnimCts?.Dispose();
        _arrowAnimCts = null;
        LeftArrowHint.TranslationX = 0;
        RightArrowHint.TranslationX = 0;
    }

    private async Task RunArrowAnimationAsync(CancellationToken token)
    {
        const uint stepMs = 520;

        while (!token.IsCancellationRequested)
        {
            try
            {
                // Arrows bounce outward
                await Task.WhenAll(
                    LeftArrowHint.TranslateToAsync(-9, 0, stepMs, Easing.SinInOut),
                    RightArrowHint.TranslateToAsync(9, 0, stepMs, Easing.SinInOut));

                if (token.IsCancellationRequested) break;

                // Arrows return to centre
                await Task.WhenAll(
                    LeftArrowHint.TranslateToAsync(0, 0, stepMs, Easing.SinInOut),
                    RightArrowHint.TranslateToAsync(0, 0, stepMs, Easing.SinInOut));

                // Brief pause before next pulse
                await Task.Delay(300, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
