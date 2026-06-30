using BeanTracker.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.Core.Favourites;

public sealed class LocalFavouritesService(BeanTrackerDbContext context) : IFavouritesService
{
    public async Task<IReadOnlyList<FavouriteDrink>> GetAllAsync()
    {
        var items = await context.FavouriteDrinks.OrderByDescending(f => f.DateSaved).ToListAsync().ConfigureAwait(false);
        return items.Select(f => new FavouriteDrink { DrinkId = f.DrinkId, DateSaved = f.DateSaved }).ToList();
    }

    public async Task AddAsync(string drinkId)
    {
        if (!await context.FavouriteDrinks.AnyAsync(f => f.DrinkId == drinkId).ConfigureAwait(false))
        {
            context.FavouriteDrinks.Add(new Data.Entities.FavouriteDrinkItem 
            { 
                Id = Guid.NewGuid().ToString("N"),
                DrinkId = drinkId,
                DateSaved = DateTime.UtcNow
            });
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task RemoveAsync(string drinkId)
    {
        var entry = await context.FavouriteDrinks.FirstOrDefaultAsync(f => f.DrinkId == drinkId).ConfigureAwait(false);
        if (entry is not null)
        {
            context.FavouriteDrinks.Remove(entry);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> IsFavouriteAsync(string drinkId) =>
        await context.FavouriteDrinks.AnyAsync(f => f.DrinkId == drinkId).ConfigureAwait(false);
}
