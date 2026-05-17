using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BeanTracker.Core.Coffee;

public sealed partial class CoffeeImageApiService(HttpClient http) : ICoffeeImageService
{
    private const string ApiUrl = "https://coffee.alexflipnote.dev/random.json";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(3);

    private readonly ConcurrentDictionary<string, (string Url, DateTime FetchedAt)> _cache = new();

    public async Task<string?> GetImageUrlAsync(string drinkId)
    {
        if (_cache.TryGetValue(drinkId, out var cached) &&
            DateTime.UtcNow - cached.FetchedAt < CacheDuration)
        {
            return cached.Url;
        }

        var url = await FetchRandomUrlAsync();
        if (url is not null)
            _cache[drinkId] = (url, DateTime.UtcNow);
        return url;
    }

    private async Task<string?> FetchRandomUrlAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var response = await http.GetFromJsonAsync(ApiUrl, CoffeeSerializerContext.Default.CoffeeImageResponse, cts.Token);
        return response?.File;
    }

    private sealed class CoffeeImageResponse
    {
        [JsonPropertyName("file")]
        public string? File { get; set; }
    }

    [JsonSerializable(typeof(CoffeeImageResponse))]
    private sealed partial class CoffeeSerializerContext : JsonSerializerContext { }
}
