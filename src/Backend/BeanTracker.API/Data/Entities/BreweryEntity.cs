using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Datasync.Server.EntityFrameworkCore;

namespace BeanTracker.API.Data.Entities;

/// <summary>
/// Server-side entity for breweries. Inherits <see cref="EntityTableData"/> which provides
/// <c>Id</c>, <c>UpdatedAt</c>, <c>Version</c>, and <c>Deleted</c> for Datasync synchronization.
/// </summary>
public class BreweryEntity : EntityTableData
{
    [Required, MinLength(1)]
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
