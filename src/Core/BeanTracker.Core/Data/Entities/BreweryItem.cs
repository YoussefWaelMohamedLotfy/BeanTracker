namespace BeanTracker.Core.Data.Entities;

/// <summary>
/// Client-side offline entity for breweries.
/// Must include <c>Id</c>, <c>UpdatedAt</c>, and <c>Version</c> properties
/// for the Datasync client to track synchronization state.
/// </summary>
public class BreweryItem
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? Version { get; set; }

    public string Name { get; set; } = string.Empty;
    public string BreweryType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? WebsiteUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
