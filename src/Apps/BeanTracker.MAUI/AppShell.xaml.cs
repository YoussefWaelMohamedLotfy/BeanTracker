using BeanTracker.MAUI.Features.BarcodeScanner;
using BeanTracker.MAUI.Features.Bluetooth;
using BeanTracker.MAUI.Features.Breweries;
using BeanTracker.MAUI.Features.Coffee;
using BeanTracker.MAUI.Helpers;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using System.Diagnostics;

namespace BeanTracker.MAUI;

public sealed partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(CoffeeDrinkDetailPage), typeof(CoffeeDrinkDetailPage));
        Routing.RegisterRoute(nameof(BreweryDetailPage), typeof(BreweryDetailPage));
        Routing.RegisterRoute(nameof(ImageSubmitPage), typeof(ImageSubmitPage));
        Routing.RegisterRoute(nameof(BleDeviceDetailPage), typeof(BleDeviceDetailPage));

#if WINDOWS
        this.Loaded += OnWindowsLoaded;
#endif

        _ = ScheduleFeedbackNotificationAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Replay any notification action that arrived before the Shell was ready (cold start).
        App.ConsumePendingAction();
    }

    private static async Task ScheduleFeedbackNotificationAsync()
    {
        try
        {
            // Only schedule once — skip if it's already pending.
            var pending = await LocalNotificationCenter.Current.GetPendingNotificationList();
            if (pending.Any(n => n.NotificationId == NotificationConstants.FeedbackNotificationId))
                return;

            if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
                await LocalNotificationCenter.Current.RequestNotificationPermission();

            var notification = new NotificationRequest
            {
                NotificationId = NotificationConstants.FeedbackNotificationId,
                Title = "How are you enjoying BeanTracker? ☕",
                Description = "Tap to rate your experience!",
                ReturningData = NotificationConstants.ShowFeedbackPopup,
                Schedule =
                {
                    NotifyTime = DateTimeOffset.Now.AddSeconds(10),
                    RepeatType = NotificationRepeat.TimeInterval,
                    NotifyRepeatInterval = TimeSpan.FromSeconds(10)
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] Could not schedule feedback notification: {ex}");
        }
    }

#if WINDOWS
    // WinUI cannot render SVG images, so we point each tab at the pre-rasterised PNG
    // that MAUI's build task writes next to the executable (e.g. tab_coffee.scale-100.png).
    private void OnWindowsLoaded(object? sender, EventArgs e)
    {
        this.Loaded -= OnWindowsLoaded;
        string baseDir = AppContext.BaseDirectory;
        CoffeeTab.Icon    = ImageSource.FromFile(Path.Combine(baseDir, "tab_coffee.scale-100.png"));
        FavouritesTab.Icon = ImageSource.FromFile(Path.Combine(baseDir, "tab_favourites.scale-100.png"));
        BreweriesTab.Icon  = ImageSource.FromFile(Path.Combine(baseDir, "tab_breweries.scale-100.png"));
    }
#endif
}
