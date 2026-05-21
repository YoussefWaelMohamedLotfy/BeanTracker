using BeanTracker.Core.Coffee;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace BeanTracker.MAUI.Features.OCR;

public sealed partial class OcrViewModel : ObservableObject
{
    private readonly ICoffeeDrinkService _coffeeDrinkService;
    private string? _cachedImagePath;

    private static readonly Uri OllamaBaseUri = new(
#if ANDROID
        "http://10.0.2.2:11434"
#else
        "http://localhost:11434"
#endif
    );

    private const string VisionModel = "gemma4:e2b";

    public OcrViewModel(ICoffeeDrinkService coffeeDrinkService)
    {
        _coffeeDrinkService = coffeeDrinkService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    [NotifyPropertyChangedFor(nameof(HasNoImage))]
    [NotifyPropertyChangedFor(nameof(ShowFileSizeInfo))]
    [NotifyPropertyChangedFor(nameof(CanAnalyze))]
    public partial ImageSource? SelectedImageSource { get; set; }

    [ObservableProperty]
    public partial string FileSizeText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAnalyze))]
    public partial CoffeeDrink? SelectedDrink { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<CoffeeDrink> AvailableDrinks { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnalysisResult))]
    public partial string AnalysisResult { get; set; } = string.Empty;

    public bool HasAnalysisResult => !string.IsNullOrEmpty(AnalysisResult);

    [ObservableProperty]
    public partial bool IsAnalyzing { get; set; }

    public bool HasImage => SelectedImageSource is not null;
    public bool HasNoImage => !HasImage;
    public bool CanAnalyze => HasImage && SelectedDrink is not null;

    public bool ShowFileSizeInfo => HasImage && IsDebugMode;

    public static bool IsDebugMode =>
#if DEBUG
        true;
#else
        false;
#endif

    public async Task LoadDrinksAsync()
    {
        AvailableDrinks = await _coffeeDrinkService.GetAllAsync();
    }

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
        _cachedImagePath = null;
        FileSizeText = string.Empty;
        AnalysisResult = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeImageAsync()
    {
        if (_cachedImagePath is null || SelectedDrink is null)
            return;

        IsAnalyzing = true;
        AnalysisResult = string.Empty;

        try
        {
            var imageBytes = await File.ReadAllBytesAsync(_cachedImagePath);
            var base64Image = Convert.ToBase64String(imageBytes);

            using var ollama = new OllamaApiClient(OllamaBaseUri);
            ollama.SelectedModel = VisionModel;

            var prompt = $"""
                You are a coffee expert. Analyze this image in the context of the drink "{SelectedDrink.Name}".
                Describe what you see related to this coffee drink, including:
                - Visual characteristics (color, texture, layers, foam)
                - Any visible ingredients or preparation style
                - Whether the image matches a typical {SelectedDrink.Name} presentation
                Keep your response concise and informative.
                """;

            var message = new Message
            {
                Role = ChatRole.User,
                Content = prompt,
                Images = [base64Image]
            };

            var result = new System.Text.StringBuilder();
            
            await foreach (var chunk in ollama.ChatAsync(new OllamaSharp.Models.Chat.ChatRequest
            {
                Model = VisionModel,
                Messages = [message],
                Stream = true
            }))
            {
                if (chunk?.Message?.Content is { } content)
                {
                    result.Append(content);
                    AnalysisResult = result.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(AnalysisResult))
                AnalysisResult = "No response received. Make sure Ollama is running with a vision model (e.g. gemma4).";
        }
        catch (HttpRequestException ex)
        {
            AnalysisResult = $"Could not connect to Ollama. Make sure it is running on your machine. {ex.Message}";
        }
        catch (Exception ex)
        {
            AnalysisResult = $"Analysis failed: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    partial void OnSelectedImageSourceChanged(ImageSource? value)
    {
        AnalyzeImageCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedDrinkChanged(CoffeeDrink? value)
    {
        AnalyzeImageCommand.NotifyCanExecuteChanged();
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

        _cachedImagePath = destPath;
        SelectedImageSource = ImageSource.FromFile(destPath);
        FileSizeText = FormatFileSize(fileSizeBytes);
        AnalysisResult = string.Empty;
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
