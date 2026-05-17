namespace BeanTracker.Core.Coffee;

public interface ICoffeeImageService
{
    /// <summary>
    /// Returns a coffee image URL for the given drink, cached per drink ID for 3 minutes.
    /// </summary>
    Task<string?> GetImageUrlAsync(string drinkId);
}
