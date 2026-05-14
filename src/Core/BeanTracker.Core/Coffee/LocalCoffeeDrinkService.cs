using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeanTracker.Core.Coffee;

/// <summary>
/// Reads coffee drinks from a JSON stream supplied by the MAUI layer.
/// The factory is called once and the result is cached.
/// </summary>
public sealed partial class LocalCoffeeDrinkService(Func<Task<Stream>> openDrinksFile) : ICoffeeDrinkService
{
    private List<CoffeeDrink>? _cache;

    private async Task<List<CoffeeDrink>> LoadAsync()
    {
        if (_cache is not null)
            return _cache;

        await using var stream = await openDrinksFile();
        _cache = await JsonSerializer.DeserializeAsync(stream, CoffeeDrinkSerializerContext.Default.ListCoffeeDrink) ?? [];
        return _cache;
    }

    public async Task<IReadOnlyList<CoffeeDrink>> GetAllAsync() => await LoadAsync();

    public async Task<IReadOnlyList<CoffeeDrink>> SearchAsync(string query)
    {
        var all = await LoadAsync();
        return string.IsNullOrWhiteSpace(query)
            ? all
            : all.Where(d => d.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<CoffeeDrink?> GetByIdAsync(string id)
    {
        var all = await LoadAsync();
        return all.FirstOrDefault(d => d.Id == id);
    }

    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(List<CoffeeDrink>))]
    private sealed partial class CoffeeDrinkSerializerContext : JsonSerializerContext { }
}
