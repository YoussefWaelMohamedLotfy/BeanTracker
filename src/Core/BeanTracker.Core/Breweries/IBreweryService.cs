namespace BeanTracker.Core.Breweries;

public interface IBreweryService
{
    Task<IReadOnlyList<Brewery>> GetAllAsync(int page = 1);
    Task<IReadOnlyList<Brewery>> SearchAsync(string query);
    Task<Brewery?> GetByIdAsync(string id);
}
