namespace BeanTracker.MAUI.Features.Host;

public abstract class FeatureView : ContentView
{
    public virtual void HandleAppearing() { }
    public virtual void HandleDisappearing() { }
}
