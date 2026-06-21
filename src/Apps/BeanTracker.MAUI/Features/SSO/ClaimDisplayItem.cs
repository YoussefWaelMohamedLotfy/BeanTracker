namespace BeanTracker.MAUI.Features.SSO;

/// <summary>
/// Simple display model for a single claim returned from the identity token.
/// </summary>
public sealed record ClaimDisplayItem(string Type, string Value);
