using BeanTracker.Core.Bluetooth;
using BeanTracker.Core.Data.Entities;
using BeanTracker.Core.Favourites;
using CommunityToolkit.Datasync.Client.Http;
using CommunityToolkit.Datasync.Client.Offline;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.Core.Data;

/// <summary>
/// Client-side EF Core DbContext with Datasync offline sync support.
/// Inherits <see cref="OfflineDbContext"/> to enable push/pull operations.
/// </summary>
public sealed class BeanTrackerDbContext : OfflineDbContext
{
    private readonly HttpClient _httpClient;

    public BeanTrackerDbContext(DbContextOptions<BeanTrackerDbContext> options, HttpClient httpClient)
        : base(options)
    {
        _httpClient = httpClient;
    }

    // ── Synced entities ───────────────────────────────────────────────────────
    public DbSet<CoffeeDrinkItem> CoffeeDrinks => Set<CoffeeDrinkItem>();
    public DbSet<FavouriteDrinkItem> FavouriteDrinks => Set<FavouriteDrinkItem>();
    public DbSet<BreweryItem> Breweries => Set<BreweryItem>();
    public DbSet<BleDataRecordItem> BleDataRecordsSync => Set<BleDataRecordItem>();

    // ── Local-only entities (not synced) ──────────────────────────────────────
    [DoNotSynchronize]
    public DbSet<FavouriteDrink> Favourites => Set<FavouriteDrink>();

    [DoNotSynchronize]
    public DbSet<BleDataRecord> BleRecordings => Set<BleDataRecord>();

    protected override void OnDatasyncInitialization(DatasyncOfflineOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseHttpClient(_httpClient);

        // Map each entity to its server table endpoint
        optionsBuilder.Entity<CoffeeDrinkItem>(cfg =>
            cfg.Endpoint = new Uri("/tables/coffeedrink", UriKind.Relative));

        optionsBuilder.Entity<FavouriteDrinkItem>(cfg =>
            cfg.Endpoint = new Uri("/tables/favouritedrink", UriKind.Relative));

        optionsBuilder.Entity<BreweryItem>(cfg =>
            cfg.Endpoint = new Uri("/tables/brewery", UriKind.Relative));

        optionsBuilder.Entity<BleDataRecordItem>(cfg =>
            cfg.Endpoint = new Uri("/tables/bledatarecord", UriKind.Relative));
    }
}
