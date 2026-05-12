namespace BeanTracker.Core.Coffee;

public interface ICoffeeDrinkService
{
    Task<IReadOnlyList<CoffeeDrink>> GetAllAsync();
    Task<IReadOnlyList<CoffeeDrink>> SearchAsync(string query);
    Task<CoffeeDrink?> GetByIdAsync(string id);
}
