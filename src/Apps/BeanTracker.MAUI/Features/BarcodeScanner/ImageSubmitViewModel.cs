using System.ComponentModel.DataAnnotations;
using BeanTracker.MAUI.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeanTracker.MAUI.Features.BarcodeScanner;

public sealed partial class ImageSubmitViewModel : ObservableValidator, IQueryAttributable
{
    public ImageSubmitViewModel()
    {
        ValidateAllProperties();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    [NotifyPropertyChangedFor(nameof(HasNoImage))]
    public partial ImageSource? CapturedImage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(TitleCharCount))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters.")]
    public partial string Title { get; set; } = string.Empty;

    public int TitleCharCount => Title?.Length ?? 0;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotSubmitting))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial bool IsSubmitting { get; set; }

    public bool HasImage => CapturedImage is not null;
    public bool HasNoImage => CapturedImage is null;
    public bool IsNotSubmitting => !IsSubmitting;
    public bool CanSubmit => !IsSubmitting && !HasErrors;

    public string TitleError => (GetErrors(nameof(Title)).FirstOrDefault()?.ErrorMessage) ?? string.Empty;
    public bool HasTitleError => GetErrors(nameof(Title)).Any();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ImagePath", out var val) && val is string path && File.Exists(path))
            CapturedImage = ImageSource.FromFile(path);
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
            return;

        IsSubmitting = true;
        try
        {
            // Simulate a network / database submission
            await Task.Delay(900);
            await FeedbackHelper.ShowNotificationAsync("Image submitted successfully!");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
