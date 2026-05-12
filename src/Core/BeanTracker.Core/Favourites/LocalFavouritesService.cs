using BeanTracker.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.Core.Favourites;

public class LocalFavouritesService(BeanTrackerDbContext context) : IFavouritesService
{
    public async Task<IReadOnlyList<FavouriteDrink>> GetAllAsync() =>
        await context.Favourites.OrderByDescending(f => f.DateSaved).ToListAsync();

    public async Task AddAsync(string drinkId)
    {
        if (!await context.Favourites.AnyAsync(f => f.DrinkId == drinkId))
        {
            context.Favourites.Add(new FavouriteDrink { DrinkId = drinkId });
            await context.SaveChangesAsync();
        }
    }

    public async Task RemoveAsync(string drinkId)
    {
        var entry = await context.Favourites.FirstOrDefaultAsync(f => f.DrinkId == drinkId);
        if (entry is not null)
        {
            context.Favourites.Remove(entry);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsFavouriteAsync(string drinkId) =>
        await context.Favourites.AnyAsync(f => f.DrinkId == drinkId);
}
