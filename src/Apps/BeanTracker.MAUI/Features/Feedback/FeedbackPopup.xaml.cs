using BeanTracker.MAUI.Helpers;
using CommunityToolkit.Maui.Views;

namespace BeanTracker.MAUI.Features.Feedback;

public sealed partial class FeedbackPopup : Popup
{
    public FeedbackPopup()
    {
        InitializeComponent();
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var rating = (int)StarRating.Rating;
        await CloseAsync();
        await FeedbackHelper.ShowNotificationAsync($"Thanks for your {rating}⭐ rating! ☕");
    }

    private async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}
