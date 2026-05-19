using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
using Plugin.Maui.Audio;

namespace BeanTracker.MAUI.Features.BarcodeScanner;

public sealed partial class BarcodeScannerPage : ContentPage
{
    private readonly BarcodeScannerViewModel _vm;
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _beepPlayer;
    private bool _cameraConfigured = false;

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

        // Prefer back camera for barcode scanning
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

        // CamerasLoaded may fire on a background thread — marshal to UI thread
        MainThread.BeginInvokeOnMainThread(async () =>
            await CameraView.StartCameraAsync());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Init beep player in the background — never block camera startup
        _ = EnsureBeepPlayerAsync();

        // Cameras already configured (e.g. returning from ImageSubmitPage): restart preview
        if (_cameraConfigured)
            _ = CameraView.StartCameraAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        // Await the stop so it fully completes before StartCameraAsync can be called again
        await CameraView.StopCameraAsync();
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
}
