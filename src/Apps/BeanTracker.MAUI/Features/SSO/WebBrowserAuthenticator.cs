using Duende.IdentityModel.OidcClient.Browser;
using Duende.IdentityModel.Client;

namespace BeanTracker.MAUI.Features.SSO;

/// <summary>
/// Adapts MAUI's <see cref="WebAuthenticator"/> to the Duende
/// <see cref="IBrowser"/> interface so the system browser is used for
/// the Keycloak login page (as recommended by RFC 8252).
/// </summary>
public sealed class WebBrowserAuthenticator : Duende.IdentityModel.OidcClient.Browser.IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            IDictionary<string, string> properties;
#if WINDOWS
            properties = await WindowsAuthenticateAsync(options, cancellationToken);
#else
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(new(options.StartUrl), new(options.EndUrl), cancellationToken);
            properties = authResult.Properties;
#endif

            // Build the callback URL from the returned properties so
            // OidcClient can extract the authorization code.
            var callbackUrl = new RequestUrl(options.EndUrl)
                .Create([.. properties]);

            return new BrowserResult
            {
                Response = callbackUrl,
                ResultType = BrowserResultType.Success
            };
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel
            };
        }
        catch (Exception ex)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = ex.Message
            };
        }
    }

#if WINDOWS
    private static async Task<IDictionary<string, string>> WindowsAuthenticateAsync(BrowserOptions options, CancellationToken cancellationToken)
    {
        using var listener = new System.Net.HttpListener();
        listener.Prefixes.Add(options.EndUrl);
        listener.Start();

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = options.StartUrl,
            UseShellExecute = true
        });

        // Wait for the browser to redirect
        var contextTask = listener.GetContextAsync();
        var delayTask = Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        var completedTask = await Task.WhenAny(contextTask, delayTask);
        
        if (completedTask == delayTask)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new TaskCanceledException();
        }

        var context = await contextTask;
        var request = context.Request;
        var response = context.Response;

        string responseString = "<html><head><style>body { font-family: sans-serif; text-align: center; padding-top: 50px; background-color: #1A0A00; color: white; }</style></head><body><h2>Login complete!</h2><p>You can close this tab and return to BeanTracker.</p><script>setTimeout(function() { window.close(); }, 3000);</script></body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        var dict = new Dictionary<string, string>();
        foreach (string key in request.QueryString.AllKeys)
        {
            if (key != null)
                dict[key] = request.QueryString[key]!;
        }

        return dict;
    }
#endif
}
