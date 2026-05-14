namespace BeanTracker.Core.Coffee;

public interface ICoffeeImageService
{
    Task<string?> GetRandomImageUrlAsync();
}
