namespace BeanTracker.MAUI;

public sealed partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RunSplashAnimationAsync();
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

        // Hand off to the main shell
        Application.Current!.Windows[0].Page = new AppShell();
    }
}
