using Plugin.Maui.Biometric;

namespace BeanTracker.MAUI;

public sealed partial class SplashPage : ContentPage
{
    private readonly IBiometric _biometric;

    public SplashPage(IBiometric biometric)
    {
        InitializeComponent();
        _biometric = biometric;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RunSplashAnimationAsync();
        await AuthenticateAndNavigateAsync();
    }

    private async Task RunSplashAnimationAsync()
    {
        // Set starting state
        IconLabel.Scale = 0.5;
        TitleLabel.TranslationY = 28;
        TaglineLabel.TranslationY = 16;

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

        // If the device has no biometric hardware or nothing enrolled, skip auth
        if (hwStatus is BiometricHwStatus.NoHardware
                     or BiometricHwStatus.Unsupported
                     or BiometricHwStatus.NotEnrolled
                     or BiometricHwStatus.PresentButNotEnrolled)
        {
            NavigateToShell();
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
