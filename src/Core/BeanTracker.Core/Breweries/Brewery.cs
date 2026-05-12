namespace BeanTracker.Core.Breweries;

public class Brewery
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BreweryType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? WebsiteUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
