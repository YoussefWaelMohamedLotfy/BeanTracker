using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
using Plugin.Maui.Audio;

namespace BeanTracker.MAUI.Features.BarcodeScanner;

public sealed partial class BarcodeScannerPage : ContentPage, IDisposable
{
    private readonly BarcodeScannerViewModel _vm;
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _beepPlayer;

    // Serialise all camera start/stop operations so they never race
    private readonly SemaphoreSlim _cameraLock = new(1, 1);
    private bool _isVisible = false;
    private bool _cameraConfigured = false;
    private bool _disposed;

    public BarcodeScannerPage(BarcodeScannerViewModel vm, IAudioManager audioManager)
    {
        InitializeComponent();
        _vm = vm;
        _audioManager = audioManager;
        BindingContext = vm;

        CameraView.BarcodeDetected += OnBarcodeDetected;
        CameraView.CamerasLoaded += OnCamerasLoaded;
    }

    private void OnCamerasLoaded(object? sender, EventArgs e)
    {
        if (CameraView.NumCamerasDetected == 0)
            return;

        CameraView.Camera = CameraView.Cameras
            .FirstOrDefault(c => c.Position == CameraPosition.Back)
            ?? CameraView.Cameras[0];

        CameraView.BarCodeOptions = new BarcodeDecodeOptions
        {
            AutoRotate = true,
            TryHarder = true,
            TryInverted = true,
            ReadMultipleCodes = false
        };

        _cameraConfigured = true;

        // Start only if the page is already visible
        if (_isVisible)
            _ = ApplyCameraStateAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = EnsureBeepPlayerAsync();

        _isVisible = true;
        if (_cameraConfigured)
            _ = ApplyCameraStateAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isVisible = false;
        _ = ApplyCameraStateAsync();
    }

    /// <summary>
    /// Single entry point for all camera start/stop transitions.
    /// The semaphore guarantees operations are sequential — never concurrent.
    /// </summary>
    private async Task ApplyCameraStateAsync()
    {
        await _cameraLock.WaitAsync();
        try
        {
            if (_isVisible)
                await CameraView.StartCameraAsync();
            else
                await CameraView.StopCameraAsync();
        }
        finally
        {
            _cameraLock.Release();
        }
    }

    private async Task EnsureBeepPlayerAsync()
    {
        if (_beepPlayer is not null)
            return;

        var stream = await FileSystem.OpenAppPackageFileAsync("beep.mp3");
        _beepPlayer = _audioManager.CreatePlayer(stream);
        _beepPlayer.Volume = 1.0;
    }

    private async void OnCaptureImageClicked(object? sender, EventArgs e)
    {
        try
        {
            var stream = await CameraView.TakePhotoAsync();
            if (stream is null)
                return;

            var fileName = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var destPath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (var fileStream = File.Create(destPath))
                await stream.CopyToAsync(fileStream);

            await Shell.Current.GoToAsync(nameof(ImageSubmitPage), new Dictionary<string, object>
            {
                ["ImagePath"] = destPath
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private void OnBarcodeDetected(object? sender, BarcodeEventArgs args)
    {
        var newCount = args.Result
            .Where(r => !string.IsNullOrWhiteSpace(r.Text))
            .Count(r => _vm.AddBarcode(r.Text, r.BarcodeFormat.ToString()));

        if (newCount > 0)
            PlayBeep();
    }

    private void PlayBeep()
    {
        if (_beepPlayer is null)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _beepPlayer.Stop();
            _beepPlayer.Play();
        });
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.NewHandler is null)
            Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        CameraView.BarcodeDetected -= OnBarcodeDetected;
        CameraView.CamerasLoaded -= OnCamerasLoaded;

        _beepPlayer?.Dispose();
        _beepPlayer = null;

        _cameraLock.Dispose();
    }
}
