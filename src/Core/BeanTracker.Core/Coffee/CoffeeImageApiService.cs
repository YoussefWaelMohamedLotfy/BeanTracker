using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BeanTracker.Core.Coffee;

public sealed partial class CoffeeImageApiService(HttpClient http) : ICoffeeImageService
{
    private const string Url = "https://coffee.alexflipnote.dev/random.json";

    public async Task<string?> GetRandomImageUrlAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var response = await http.GetFromJsonAsync(Url, CoffeeSerializerContext.Default.CoffeeImageResponse, cts.Token);
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
