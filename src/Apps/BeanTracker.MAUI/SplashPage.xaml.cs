using BeanTracker.Core.Data;
using BeanTracker.MAUI.Helpers;
using Microsoft.EntityFrameworkCore;
using Plugin.Maui.Biometric;

namespace BeanTracker.MAUI;

public sealed partial class SplashPage : ContentPage
{
    private readonly IBiometric _biometric;
    private readonly BatteryAwarenessService _batteryService;

    public SplashPage(IBiometric biometric, BatteryAwarenessService batteryService)
    {
        InitializeComponent();
        _biometric = biometric;
        _batteryService = batteryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var dbTask = Task.Run(InitializeDatabaseAsync);
        await RunSplashAnimationAsync();
        await dbTask; // Ensure DB is ready before proceeding
        await AuthenticateAndNavigateAsync();
    }

    private static async Task InitializeDatabaseAsync()
    {
        using var db = IPlatformApplication.Current!.Services.GetRequiredService<BeanTrackerDbContext>();
        await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
        // Ensures the BleRecordings table exists even on databases created before this feature was added.
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "BleRecordings" (
                "Id"                INTEGER NOT NULL CONSTRAINT "PK_BleRecordings" PRIMARY KEY AUTOINCREMENT,
                "DeviceId"          TEXT    NOT NULL,
                "DeviceName"        TEXT    NOT NULL,
                "ServiceId"         TEXT    NOT NULL,
                "ServiceName"       TEXT    NOT NULL,
                "CharacteristicId"  TEXT    NOT NULL,
                "CharacteristicName" TEXT   NOT NULL,
                "RawHex"            TEXT    NOT NULL,
                "AsciiValue"        TEXT,
                "Timestamp"         TEXT    NOT NULL,
                "SessionLabel"      TEXT
            )
            """).ConfigureAwait(false);
    }

    private async Task RunSplashAnimationAsync()
    {
        // Set starting state
        IconLabel.Scale = 0.5;
        TitleLabel.TranslationY = 28;
        TaglineLabel.TranslationY = 16;

        if (!_batteryService.AnimationsEnabled)
        {
            // Low battery — snap all elements to their final state immediately.
            IconLabel.Scale   = 1; IconLabel.Opacity    = 1;
            TitleLabel.TranslationY = 0; TitleLabel.Opacity = 1;
            RuleView.Opacity  = 1;
            TaglineLabel.TranslationY = 0; TaglineLabel.Opacity = 1;
            return;
        }

        // 1 — Icon blooms in (scale + fade, 650 ms)
        await Task.WhenAll(
            IconLabel.ScaleToAsync(1, 650, Easing.CubicOut),
            IconLabel.FadeToAsync(1, 650, Easing.CubicOut));

        // 2 — Title slides up and fades in (400 ms)
        await Task.WhenAll(
            TitleLabel.TranslateToAsync(0, 0, 400, Easing.CubicOut),
            TitleLabel.FadeToAsync(1, 400));

        // 3 — Accent rule fades in (250 ms)
        await RuleView.FadeToAsync(1, 250);

        // 4 — Tagline slides up and fades in (350 ms)
        await Task.WhenAll(
            TaglineLabel.TranslateToAsync(0, 0, 350, Easing.CubicOut),
            TaglineLabel.FadeToAsync(1, 350));

        // Hold so the user can read the branding
        await Task.Delay(650);
    }

    private async Task AuthenticateAndNavigateAsync()
    {
        var hwStatus = await _biometric.GetAuthenticationStatusAsync(AuthenticatorStrength.Weak);

        // If the device has no biometric hardware, skip auth
        if (hwStatus is BiometricHwStatus.NoHardware
                     or BiometricHwStatus.Unsupported)
        {
            NavigateToShell();
            return;
        }

        // If the device has biometric hardware but nothing is enrolled, require it
        if (hwStatus is BiometricHwStatus.NotEnrolled
                     or BiometricHwStatus.PresentButNotEnrolled)
        {
            await DisplayAlertAsync(
                "Authentication Required",
                "You must register a fingerprint or PIN on your device to use BeanTracker.",
                "Close");

            Application.Current!.Quit();
            return;
        }

        var response = await _biometric.AuthenticateAsync(
            new AuthenticationRequest
            {
                Title = "Unlock BeanTracker",
                Subtitle = "Verify your identity to continue",
                NegativeText = "Cancel",
                Description = "Use biometrics or your device PIN",
                AllowPasswordAuth = true,
                AuthStrength = AuthenticatorStrength.Weak
            },
            CancellationToken.None);

        if (response.Status == BiometricResponseStatus.Success)
        {
            NavigateToShell();
        }
        else
        {
            await DisplayAlertAsync(
                "Authentication Failed",
                "You must authenticate to use BeanTracker.",
                "Close");

            Application.Current!.Quit();
        }
    }

    private static void NavigateToShell()
    {
        Application.Current!.Windows[0].Page = new AppShell();
    }
}
