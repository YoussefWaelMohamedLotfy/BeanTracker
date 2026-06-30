using BeanTracker.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BeanTracker.Core.Breweries;

/// <summary>
/// A brewery service that reads directly from the local Datasync SQLite table.
/// </summary>
public sealed class SyncedBreweryService(BeanTrackerDbContext context) : IBreweryService
{
    public async Task<IReadOnlyList<Brewery>> GetAllAsync(int page = 1)
    {
        var items = await context.Breweries
            .OrderBy(b => b.Name)
            .Skip((page - 1) * 25)
            .Take(25)
            .ToListAsync()
            .ConfigureAwait(false);
            
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<Brewery>> SearchAsync(string query)
    {
        var items = await context.Breweries
            .Where(b => EF.Functions.Like(b.Name, $"%{query}%"))
            .OrderBy(b => b.Name)
            .Take(25)
            .ToListAsync()
            .ConfigureAwait(false);

        return items.Select(Map).ToList();
    }

    public async Task<Brewery?> GetByIdAsync(string id)
    {
        var item = await context.Breweries.FirstOrDefaultAsync(b => b.Id == id).ConfigureAwait(false);
        return item is null ? null : Map(item);
    }

    private static Brewery Map(Data.Entities.BreweryItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        BreweryType = item.BreweryType,
        City = item.City,
        State = item.State,
        Country = item.Country,
        Address1 = item.Address1,
        Address2 = item.Address2,
        Address3 = item.Address3,
        PostalCode = item.PostalCode,
        Phone = item.Phone,
        WebsiteUrl = item.WebsiteUrl,
        Latitude = item.Latitude,
        Longitude = item.Longitude
    };
}
