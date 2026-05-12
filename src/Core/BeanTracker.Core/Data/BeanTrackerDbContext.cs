using BeanTracker.Core.Favourites;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.Core.Data;

public class BeanTrackerDbContext(DbContextOptions<BeanTrackerDbContext> options) : DbContext(options)
{
    public DbSet<FavouriteDrink> Favourites => Set<FavouriteDrink>();
}
