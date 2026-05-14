using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BeanTracker.Core.Breweries;

public sealed partial class BreweryApiService(HttpClient http) : IBreweryService
{
    private const string BaseUrl = "https://api.openbrewerydb.org/v1/breweries";

    public async Task<IReadOnlyList<Brewery>> GetAllAsync(int page = 1)
    {
        var dtos = await http.GetFromJsonAsync($"{BaseUrl}?page={page}&per_page=25", BrewerySerializerContext.Default.ListBreweryDto);
        return dtos?.Select(Map).ToList() ?? [];
    }

    public async Task<IReadOnlyList<Brewery>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        var dtos = await http.GetFromJsonAsync($"{BaseUrl}?by_name={Uri.EscapeDataString(query)}&per_page=25", BrewerySerializerContext.Default.ListBreweryDto);
        return dtos?.Select(Map).ToList() ?? [];
    }

    public async Task<Brewery?> GetByIdAsync(string id)
    {
        var dto = await http.GetFromJsonAsync($"{BaseUrl}/{Uri.EscapeDataString(id)}", BrewerySerializerContext.Default.BreweryDto);
        return dto is null ? null : Map(dto);
    }

    private static Brewery Map(BreweryDto dto) => new()
    {
        Id = dto.Id ?? string.Empty,
        Name = dto.Name ?? string.Empty,
        BreweryType = dto.BreweryType ?? string.Empty,
        City = dto.City ?? string.Empty,
        State = dto.StateProvince ?? dto.State ?? string.Empty,
        Country = dto.Country ?? string.Empty,
        Address1 = dto.Address1,
        Address2 = dto.Address2,
        Address3 = dto.Address3,
        PostalCode = dto.PostalCode,
        Phone = dto.Phone,
        WebsiteUrl = dto.WebsiteUrl,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude,
    };

    private sealed class BreweryDto
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("brewery_type")] public string? BreweryType { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("state_province")] public string? StateProvince { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
        [JsonPropertyName("address_1")] public string? Address1 { get; set; }
        [JsonPropertyName("address_2")] public string? Address2 { get; set; }
        [JsonPropertyName("address_3")] public string? Address3 { get; set; }
        [JsonPropertyName("postal_code")] public string? PostalCode { get; set; }
        [JsonPropertyName("phone")] public string? Phone { get; set; }
        [JsonPropertyName("website_url")] public string? WebsiteUrl { get; set; }
        [JsonPropertyName("latitude")] public double? Latitude { get; set; }
        [JsonPropertyName("longitude")] public double? Longitude { get; set; }
    }

    [JsonSerializable(typeof(List<BreweryDto>))]
    [JsonSerializable(typeof(BreweryDto))]
    private sealed partial class BrewerySerializerContext : JsonSerializerContext { }
}
