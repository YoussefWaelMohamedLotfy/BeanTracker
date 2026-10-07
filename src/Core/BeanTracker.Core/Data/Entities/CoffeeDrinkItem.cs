namespace BeanTracker.Core.Data.Entities;

/// <summary>
/// Client-side offline entity for coffee drinks.
/// Must include <c>Id</c>, <c>UpdatedAt</c>, and <c>Version</c> properties
/// for the Datasync client to track synchronization state.
/// </summary>
public class CoffeeDrinkItem
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? Version { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;

    /// <summary>Comma-separated flavor notes (e.g. "Chocolate,Nutty,Caramel").</summary>
    public string FlavorNotes { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
}
