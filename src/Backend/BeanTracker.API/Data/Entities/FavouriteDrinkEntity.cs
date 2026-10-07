using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Datasync.Server.EntityFrameworkCore;

namespace BeanTracker.API.Data.Entities;

/// <summary>
/// Server-side entity for favourite drinks. Inherits <see cref="EntityTableData"/> which provides
/// <c>Id</c>, <c>UpdatedAt</c>, <c>Version</c>, and <c>Deleted</c> for Datasync synchronization.
/// </summary>
public class FavouriteDrinkEntity : EntityTableData
{
    [Required]
    public string DrinkId { get; set; } = string.Empty;

    public DateTime DateSaved { get; set; } = DateTime.UtcNow;
}
