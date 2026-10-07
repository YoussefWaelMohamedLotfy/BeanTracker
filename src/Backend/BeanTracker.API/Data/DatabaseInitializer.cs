using Microsoft.EntityFrameworkCore;

namespace BeanTracker.API.Data;

/// <summary>
/// Ensures the database schema exists and installs PostgreSQL triggers required by the
/// Datasync Community Toolkit. The triggers automatically set <c>UpdatedAt</c> and <c>Version</c>
/// on every INSERT or UPDATE, which is required for the datasync protocol.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Creates the database, applies any pending migrations or ensures the schema is created,
    /// then installs datasync triggers on all entity tables.
    /// </summary>
    public static async Task InitializeAsync(ApiDbContext context)
    {
        await context.Database.EnsureCreatedAsync().ConfigureAwait(false);

        // Install the shared trigger function
        await context.Database.ExecuteSqlRawAsync("""
            CREATE OR REPLACE FUNCTION entity_datasync() RETURNS trigger AS $$
            BEGIN
                NEW."UpdatedAt" = NOW() AT TIME ZONE 'UTC';
                NEW."Version" = convert_to(gen_random_uuid()::text, 'UTF8');
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
            """).ConfigureAwait(false);

        // Install per-table triggers
        string[] tableNames = ["CoffeeDrinks", "FavouriteDrinks", "Breweries", "BleDataRecords"];
        foreach (var table in tableNames)
        {
            await context.Database.ExecuteSqlRawAsync($"""
                CREATE OR REPLACE TRIGGER
                    {table}_datasync
                BEFORE INSERT OR UPDATE ON
                    "{table}"
                FOR EACH ROW EXECUTE PROCEDURE
                    entity_datasync();
                """).ConfigureAwait(false);
        }

        // Seed data if empty
        if (!await context.CoffeeDrinks.AnyAsync().ConfigureAwait(false))
        {
            await SeedCoffeeDrinksAsync(context).ConfigureAwait(false);
        }

        if (!await context.Breweries.AnyAsync().ConfigureAwait(false))
        {
            await SeedBreweriesAsync(context).ConfigureAwait(false);
        }
    }

    private static async Task SeedCoffeeDrinksAsync(ApiDbContext context)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "drinks.json");
        if (!File.Exists(filePath)) return;

        using var stream = File.OpenRead(filePath);
        using var document = await System.Text.Json.JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        foreach (var element in document.RootElement.EnumerateArray())
        {
            var flavors = new List<string>();
            if (element.TryGetProperty("flavorNotes", out var fnElement))
            {
                foreach (var f in fnElement.EnumerateArray())
                {
                    flavors.Add(f.GetString() ?? "");
                }
            }

            var entity = new Entities.CoffeeDrinkEntity
            {
                Id = element.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N"),
                Name = element.GetProperty("name").GetString() ?? "",
                Description = element.GetProperty("description").GetString() ?? "",
                Origin = element.GetProperty("origin").GetString() ?? "",
                FlavorNotes = string.Join(",", flavors),
                ImageUrl = element.GetProperty("imageUrl").GetString() ?? ""
            };

            context.CoffeeDrinks.Add(entity);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    private static async Task SeedBreweriesAsync(ApiDbContext context)
    {
        context.Breweries.AddRange(
            new Entities.BreweryEntity
            {
                Id = "b1",
                Name = "MadTree Brewing",
                BreweryType = "regional",
                City = "Cincinnati",
                State = "Ohio",
                Country = "United States",
                Address1 = "3301 Madison Rd",
                WebsiteUrl = "http://www.madtreebrewing.com"
            },
            new Entities.BreweryEntity
            {
                Id = "b2",
                Name = "Modern Times Beer",
                BreweryType = "regional",
                City = "San Diego",
                State = "California",
                Country = "United States",
                Address1 = "3725 Greenwood St",
                WebsiteUrl = "http://www.moderntimesbeer.com"
            },
            new Entities.BreweryEntity
            {
                Id = "b3",
                Name = "Brooklyn Brewery",
                BreweryType = "regional",
                City = "Brooklyn",
                State = "New York",
                Country = "United States",
                Address1 = "79 N 11th St",
                WebsiteUrl = "http://www.brooklynbrewery.com"
            }
        );

        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}
