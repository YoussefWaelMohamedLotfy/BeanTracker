using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls.Shapes;

namespace BeanTracker.MAUI.Helpers;

internal static class FeedbackHelper
{
    internal static Task ShowNotificationAsync(string message)
    {
#if WINDOWS
        return ShowWindowsToastAsync(message);
#else
        return Toast.Make(message, ToastDuration.Short).Show();
#endif
    }

#if WINDOWS
    private const string OverlayGridId = "BeanTrackerToastOverlay";

    private static async Task ShowWindowsToastAsync(string message)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = Shell.Current?.CurrentPage
                       ?? Application.Current?.Windows?.FirstOrDefault()?.Page;

            if (page is not ContentPage contentPage) return;

            // Reuse an existing overlay grid, or install a new one.
            // We never unwrap — the grid stays as page content for the page's lifetime,
            // avoiding MAUI double-parent errors when re-parenting views.
            Grid overlayGrid;
            if (contentPage.Content is Grid g && g.AutomationId == OverlayGridId)
            {
                overlayGrid = g;
            }
            else
            {
                // Detach current content first so it has no parent before we move it.
                var originalContent = contentPage.Content;
                contentPage.Content = null;

                overlayGrid = new Grid { AutomationId = OverlayGridId };
                if (originalContent is not null)
                    overlayGrid.Add(originalContent);
                contentPage.Content = overlayGrid;
            }

            var toastBorder = new Border
            {
                BackgroundColor = Color.FromArgb("#CC1C1C1C"),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Padding = new Thickness(16, 8),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(16, 0, 16, 60),
                InputTransparent = true,
                Content = new Label
                {
                    Text = message,
                    TextColor = Colors.White,
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center
                },
                Opacity = 0
            };

            overlayGrid.Add(toastBorder);

            await toastBorder.FadeToAsync(1, 250);
            await Task.Delay(2000);
            await toastBorder.FadeToAsync(0, 300);

            overlayGrid.Remove(toastBorder);
        });
    }
#endif
}
