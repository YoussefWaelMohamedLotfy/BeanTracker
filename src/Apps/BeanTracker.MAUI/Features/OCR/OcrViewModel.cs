using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeanTracker.MAUI.Features.OCR;

public sealed partial class OcrViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    [NotifyPropertyChangedFor(nameof(HasNoImage))]
    [NotifyPropertyChangedFor(nameof(ShowFileSizeInfo))]
    public partial ImageSource? SelectedImageSource { get; set; }

    [ObservableProperty]
    public partial string FileSizeText { get; set; } = string.Empty;

    public bool HasImage => SelectedImageSource is not null;
    public bool HasNoImage => !HasImage;

    public bool ShowFileSizeInfo => HasImage && IsDebugMode;

    public static bool IsDebugMode =>
#if DEBUG
        true;
#else
        false;
#endif

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlertAsync("Not Supported", "Camera capture is not available on this device.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            await LoadFileResultAsync(photo);
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlertAsync("Permission Denied", "Camera permission is required to take photos.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task UploadFileAsync()
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            };

            var result = await FilePicker.Default.PickAsync(options);
            await LoadFileResultAsync(result);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void ClearImage()
    {
        SelectedImageSource = null;
        FileSizeText = string.Empty;
    }

    private async Task LoadFileResultAsync(FileResult? file)
    {
        if (file is null)
            return;

        // Copy to cache so the ImageSource remains valid after the picker closes
        var destPath = Path.Combine(FileSystem.CacheDirectory, file.FileName);

        long fileSizeBytes;
        using (var sourceStream = await file.OpenReadAsync())
        using (var destStream = File.Create(destPath))
        {
            await sourceStream.CopyToAsync(destStream);
            fileSizeBytes = destStream.Length;
        }

        SelectedImageSource = ImageSource.FromFile(destPath);
        FileSizeText = FormatFileSize(fileSizeBytes);
    }

    private static string FormatFileSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024.0):F2} MB"
        };
    }
}
