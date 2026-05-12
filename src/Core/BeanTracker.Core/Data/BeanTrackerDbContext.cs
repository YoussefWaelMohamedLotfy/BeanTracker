using BeanTracker.Core.Favourites;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.Core.Data;

public sealed class BeanTrackerDbContext(DbContextOptions<BeanTrackerDbContext> options) : DbContext(options)
{
    public DbSet<FavouriteDrink> Favourites => Set<FavouriteDrink>();
}
