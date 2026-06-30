using BeanTracker.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.Core.Coffee;

/// <summary>
/// A coffee drink service that reads directly from the local Datasync SQLite table.
/// </summary>
public sealed class SyncedCoffeeDrinkService(BeanTrackerDbContext context) : ICoffeeDrinkService
{
    public async Task<IReadOnlyList<CoffeeDrink>> GetAllAsync()
    {
        var items = await context.CoffeeDrinks.ToListAsync().ConfigureAwait(false);
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<CoffeeDrink>> SearchAsync(string query)
    {
        var items = await context.CoffeeDrinks
            .Where(d => EF.Functions.Like(d.Name, $"%{query}%"))
            .ToListAsync().ConfigureAwait(false);
        
        return items.Select(Map).ToList();
    }

    public async Task<CoffeeDrink?> GetByIdAsync(string id)
    {
        var item = await context.CoffeeDrinks.FirstOrDefaultAsync(d => d.Id == id).ConfigureAwait(false);
        return item is null ? null : Map(item);
    }

    private static CoffeeDrink Map(Data.Entities.CoffeeDrinkItem item)
    {
        var flavorNotes = string.IsNullOrWhiteSpace(item.FlavorNotes) 
            ? [] 
            : item.FlavorNotes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        return new CoffeeDrink
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Origin = item.Origin,
            FlavorNotes = flavorNotes,
            ImageUrl = item.ImageUrl
        };
    }
}
