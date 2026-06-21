using System.Collections.ObjectModel;
using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeanTracker.MAUI.Features.SSO;

/// <summary>
/// Drives the SSO page: login/logout button state, claims list, and error display.
/// </summary>
public sealed partial class SsoViewModel : ObservableObject
{
    private readonly KeycloakAuthService _authService;

    private string? _identityToken;

    public SsoViewModel(KeycloakAuthService authService)
    {
        _authService = authService;
    }

    // ── Observable properties ────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonText), nameof(StatusText))]
    public partial bool IsLoggedIn { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? UserDisplayName { get; set; }

    public ObservableCollection<ClaimDisplayItem> Claims { get; } = [];

    // ── Computed properties ─────────────────────────────────────────────

    public string ButtonText => IsLoggedIn ? "Sign Out" : "Sign in with SSO";

    public string StatusText => IsLoggedIn
        ? $"Welcome, {UserDisplayName ?? "User"}"
        : "Sign in with your organisation account";

    public bool IsNotBusy => !IsBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // ── Commands ────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoginOrLogoutAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            if (IsLoggedIn)
            {
                await DoLogoutAsync();
            }
            else
            {
                await DoLoginAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] SSO error: {ex}");
            ErrorMessage = $"Authentication error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Private helpers ────────────────────────────────────────────────

    [ObservableProperty]
    public partial bool IsAdmin { get; set; }

    [RelayCommand]
    private async Task GoToAdminPageAsync()
    {
        if (IsAdmin)
        {
            await Shell.Current.GoToAsync("AdminPage");
        }
    }

    private async Task DoLoginAsync()
    {
        var result = await _authService.LoginAsync();

        if (result.IsError)
        {
            ErrorMessage = result.Error == "UserCancel"
                ? "Sign-in was cancelled."
                : $"Sign-in failed: {result.Error}";
            return;
        }

        _identityToken = result.IdentityToken;

        // Populate claims list
        Claims.Clear();
        foreach (var claim in result.User.Claims)
        {
            Claims.Add(new ClaimDisplayItem(claim.Type, claim.Value));
        }

        // Check for admin role (mapped to 'roles' claim in Keycloak)
        IsAdmin = result.User.Claims.Any(c => c.Type == "roles" && c.Value == "admin");

        // Extract a friendly display name
        UserDisplayName = result.User.FindFirst("preferred_username")?.Value
                          ?? result.User.FindFirst("name")?.Value
                          ?? result.User.FindFirst("email")?.Value
                          ?? "User";

        IsLoggedIn = true;
    }

    private async Task DoLogoutAsync()
    {
        var result = await _authService.LogoutAsync(_identityToken);

        // Clear state regardless of logout result (local logout)
        _identityToken = null;
        Claims.Clear();
        UserDisplayName = null;
        IsAdmin = false;
        IsLoggedIn = false;

        if (result.IsError && result.Error != "UserCancel")
        {
            Debug.WriteLine($"[BeanTracker] Logout warning: {result.Error}");
        }
    }

    private bool _hasAttemptedRestore = false;

    public async Task RestoreSessionAsync()
    {
        if (_hasAttemptedRestore || IsLoggedIn) return;
        _hasAttemptedRestore = true;

        var previousToken = await SecureStorage.Default.GetAsync("auth_access_token");
        if (string.IsNullOrEmpty(previousToken))
            return; // Never logged in before.

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _authService.RestoreSessionAsync();
            if (result != null)
            {
                var payload = result.Value;
                _identityToken = payload.IdentityToken;
                Claims.Clear();
                foreach (var claim in payload.User.Claims)
                {
                    Claims.Add(new ClaimDisplayItem(claim.Type, claim.Value));
                }

                IsAdmin = payload.User.Claims.Any(c => c.Type == "roles" && c.Value == "admin");
                UserDisplayName = payload.User.FindFirst("preferred_username")?.Value
                                  ?? payload.User.FindFirst("name")?.Value
                                  ?? payload.User.FindFirst("email")?.Value
                                  ?? "User";
                IsLoggedIn = true;
            }
            else
            {
                // We had a token, but restore failed (likely expired refresh token). Trigger auth!
                await DoLoginAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BeanTracker] Restore session error: {ex}");
            ErrorMessage = $"Failed to restore session: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
