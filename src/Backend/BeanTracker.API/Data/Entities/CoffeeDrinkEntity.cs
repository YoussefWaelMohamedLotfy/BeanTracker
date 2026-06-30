using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Datasync.Server.EntityFrameworkCore;

namespace BeanTracker.API.Data.Entities;

/// <summary>
/// Server-side entity for coffee drinks. Inherits <see cref="EntityTableData"/> which provides
/// <c>Id</c>, <c>UpdatedAt</c>, <c>Version</c>, and <c>Deleted</c> for Datasync synchronization.
/// </summary>
public class CoffeeDrinkEntity : EntityTableData
{
    [Required, MinLength(1)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Origin { get; set; } = string.Empty;

    /// <summary>Comma-separated flavor notes (e.g. "Chocolate,Nutty,Caramel").</summary>
    public string FlavorNotes { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
}
