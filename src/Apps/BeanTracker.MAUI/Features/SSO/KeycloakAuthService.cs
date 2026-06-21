using Duende.IdentityModel.OidcClient;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace BeanTracker.MAUI.Features.SSO;

/// <summary>
/// High-level service that wraps the Duende <see cref="OidcClient"/> configured
/// for the BeanTracker Keycloak realm.  Exposes simple <see cref="LoginAsync"/>
/// and <see cref="LogoutAsync"/> methods for the SSO page.
/// </summary>
public sealed class KeycloakAuthService
{
    // TODO: Make configurable via Aspire service-discovery or appsettings.
    private const string Authority = "https://localhost:8081/realms/beantracker";
    private const string ClientId = "beantracker-maui";
    private const string Scope = "openid profile email roles";
#if WINDOWS
    private const string RedirectUri = "http://127.0.0.1:49152/";
    private const string PostLogoutRedirectUri = "http://127.0.0.1:49152/";
#else
    private const string RedirectUri = "beantracker://callback";
    private const string PostLogoutRedirectUri = "beantracker://callback";
#endif

    private readonly OidcClient _oidcClient;

    public KeycloakAuthService()
    {
        var browser = new WebBrowserAuthenticator();

        var options = new OidcClientOptions
        {
            Authority = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:8081/realms/beantracker" : Authority,
            ClientId = ClientId,
            Scope = Scope,
            RedirectUri = RedirectUri,
            PostLogoutRedirectUri = PostLogoutRedirectUri,
            Browser = browser,
        };

#if DEBUG
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        options.HttpClientFactory = _ => new HttpClient(handler);
#endif

        _oidcClient = new OidcClient(options);
    }

    /// <summary>
    /// Initiates the Authorization Code + PKCE flow via the system browser.
    /// </summary>
    public async Task<LoginResult> LoginAsync(CancellationToken cancellationToken = default)
    {
        var result = await _oidcClient.LoginAsync(new LoginRequest(), cancellationToken);
        if (!result.IsError)
        {
            await SecureStorage.Default.SetAsync("auth_access_token", result.AccessToken);
            if (result.IdentityToken != null)
                await SecureStorage.Default.SetAsync("auth_id_token", result.IdentityToken);
            if (result.RefreshToken != null)
                await SecureStorage.Default.SetAsync("auth_refresh_token", result.RefreshToken);
            await SecureStorage.Default.SetAsync("auth_expires_at", result.AccessTokenExpiration.ToString("o"));
        }
        return result;
    }

    /// <summary>
    /// Ends the session with Keycloak and clears the local tokens.
    /// </summary>
    public async Task<LogoutResult> LogoutAsync(string? identityToken = null, CancellationToken cancellationToken = default)
    {
        var result = await _oidcClient.LogoutAsync(new LogoutRequest { IdTokenHint = identityToken }, cancellationToken);
        SecureStorage.Default.Remove("auth_access_token");
        SecureStorage.Default.Remove("auth_id_token");
        SecureStorage.Default.Remove("auth_refresh_token");
        SecureStorage.Default.Remove("auth_expires_at");
        return result;
    }

    /// <summary>
    /// Restores the session from SecureStorage. If the access token is expired, attempts to refresh it.
    /// Returns null if no valid session can be restored.
    /// </summary>
    public async Task<(string? IdentityToken, System.Security.Claims.ClaimsPrincipal User)?> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await SecureStorage.Default.GetAsync("auth_access_token");
        var idToken = await SecureStorage.Default.GetAsync("auth_id_token");
        var refreshToken = await SecureStorage.Default.GetAsync("auth_refresh_token");
        var expiresAtStr = await SecureStorage.Default.GetAsync("auth_expires_at");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(expiresAtStr))
            return null;

        bool isExpired = true;
        if (DateTimeOffset.TryParse(expiresAtStr, out var expiresAt))
        {
            if (expiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                isExpired = false;
            }
        }

        if (isExpired)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var refreshResult = await _oidcClient.RefreshTokenAsync(refreshToken, cancellationToken: cancellationToken);
            if (refreshResult.IsError)
            {
                SecureStorage.Default.Remove("auth_access_token");
                SecureStorage.Default.Remove("auth_id_token");
                SecureStorage.Default.Remove("auth_refresh_token");
                SecureStorage.Default.Remove("auth_expires_at");
                return null;
            }

            accessToken = refreshResult.AccessToken;
            idToken = refreshResult.IdentityToken ?? idToken;
            refreshToken = refreshResult.RefreshToken ?? refreshToken;

            await SecureStorage.Default.SetAsync("auth_access_token", accessToken);
            if (idToken != null) await SecureStorage.Default.SetAsync("auth_id_token", idToken);
            if (refreshToken != null) await SecureStorage.Default.SetAsync("auth_refresh_token", refreshToken);
            await SecureStorage.Default.SetAsync("auth_expires_at", refreshResult.AccessTokenExpiration.ToString("o"));
        }

        var userInfo = await _oidcClient.GetUserInfoAsync(accessToken, cancellationToken);
        if (userInfo.IsError)
            return null;

        var principal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(userInfo.Claims, "Bearer"));
        return (idToken, principal);
    }
}
