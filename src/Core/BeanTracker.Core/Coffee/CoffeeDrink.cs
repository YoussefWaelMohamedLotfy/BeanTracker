namespace BeanTracker.Core.Coffee;

public sealed class CoffeeDrink
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public List<string> FlavorNotes { get; set; } = [];
    public string ImageUrl { get; set; } = string.Empty;
}
