using System.ComponentModel.DataAnnotations;

namespace BeanTracker.Core.Favourites;

public class FavouriteDrink
{
    [Key]
    public int Id { get; set; }
    public string DrinkId { get; set; } = string.Empty;
    public DateTime DateSaved { get; set; } = DateTime.UtcNow;
}
