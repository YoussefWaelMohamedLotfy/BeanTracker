using BeanTracker.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.API.Data;

/// <summary>
/// Server-side EF Core DbContext for the Datasync-enabled API.
/// Uses PostgreSQL with triggers to automatically maintain <c>UpdatedAt</c> and <c>Version</c>.
/// </summary>
public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<CoffeeDrinkEntity> CoffeeDrinks => Set<CoffeeDrinkEntity>();
    public DbSet<FavouriteDrinkEntity> FavouriteDrinks => Set<FavouriteDrinkEntity>();
    public DbSet<BreweryEntity> Breweries => Set<BreweryEntity>();
    public DbSet<BleDataRecordEntity> BleDataRecords => Set<BleDataRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PostgreSQL: Set default value for UpdatedAt so the datasync trigger can manage it
        modelBuilder.Entity<CoffeeDrinkEntity>()
            .Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        modelBuilder.Entity<FavouriteDrinkEntity>()
            .Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        modelBuilder.Entity<BreweryEntity>()
            .Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        modelBuilder.Entity<BleDataRecordEntity>()
            .Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        base.OnModelCreating(modelBuilder);
    }
}
