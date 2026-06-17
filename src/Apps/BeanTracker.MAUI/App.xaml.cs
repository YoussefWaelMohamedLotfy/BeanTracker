using BeanTracker.MAUI.Features.Feedback;
using BeanTracker.MAUI.Helpers;

using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.DependencyInjection;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace BeanTracker.MAUI;

public sealed partial class App : Application
{
    // Holds a notification action that arrived before Shell was ready (cold start).
    internal static string? PendingNotificationAction { get; private set; }

    public App()
    {
        InitializeComponent();
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;

        AppDomain.CurrentDomain.UnhandledException += (s, e) => 
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(FileSystem.CacheDirectory, "unhandled_crash.txt"), e.ExceptionObject.ToString());
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(FileSystem.CacheDirectory, "task_crash.txt"), e.Exception.ToString());
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var splash = IPlatformApplication.Current!.Services.GetRequiredService<SplashPage>();
        return new Window(splash);
    }

    private static void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
        if (!e.IsTapped) return;

        switch (e.Request.ReturningData)
        {
            case NotificationConstants.NavigateToFavourites:
                if (Shell.Current is not null)
                    Shell.Current.GoToAsync("//Favourites");
                else
                    PendingNotificationAction = NotificationConstants.NavigateToFavourites;
                break;

            case NotificationConstants.ShowFeedbackPopup:
                if (Shell.Current is not null)
                    MainThread.BeginInvokeOnMainThread(ShowFeedbackPopup);
                else
                    PendingNotificationAction = NotificationConstants.ShowFeedbackPopup;
                break;
        }
    }

    // Called by AppShell once it appears, to replay any action missed during cold start.
    internal static void ConsumePendingAction()
    {
        var action = PendingNotificationAction;
        PendingNotificationAction = null;

        switch (action)
        {
            case NotificationConstants.NavigateToFavourites:
                Shell.Current?.GoToAsync("//Favourites");
                break;

            case NotificationConstants.ShowFeedbackPopup:
                ShowFeedbackPopup();
                break;
        }
    }

    private static void ShowFeedbackPopup()
    {
        var page = Shell.Current?.CurrentPage
                   ?? Application.Current?.Windows?.FirstOrDefault()?.Page;

        if (page is null) return;

        var popup = IPlatformApplication.Current!.Services.GetRequiredService<FeedbackPopup>();
        page.ShowPopup(popup);
    }
}