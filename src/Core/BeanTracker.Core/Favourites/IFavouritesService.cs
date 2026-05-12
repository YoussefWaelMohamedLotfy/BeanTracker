namespace BeanTracker.Core.Favourites;

public interface IFavouritesService
{
    Task<IReadOnlyList<FavouriteDrink>> GetAllAsync();
    Task AddAsync(string drinkId);
    Task RemoveAsync(string drinkId);
    Task<bool> IsFavouriteAsync(string drinkId);
}
