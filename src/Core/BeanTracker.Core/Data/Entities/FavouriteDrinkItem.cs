namespace BeanTracker.Core.Data.Entities;

/// <summary>
/// Client-side offline entity for favourite drinks.
/// Must include <c>Id</c>, <c>UpdatedAt</c>, and <c>Version</c> properties
/// for the Datasync client to track synchronization state.
/// </summary>
public class FavouriteDrinkItem
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? Version { get; set; }

    public string DrinkId { get; set; } = string.Empty;
    public DateTime DateSaved { get; set; } = DateTime.UtcNow;
}
