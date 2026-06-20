namespace BeanTracker.MAUI.Helpers;

/// <summary>
/// Singleton service that monitors the device battery level and charging state
/// and exposes whether animations should be enabled.
/// Animations are disabled when the battery level is below 20% AND the device is not charging.
/// </summary>
public sealed partial class BatteryAwarenessService : IDisposable
{
    private const double LowBatteryThreshold = 0.20;

    /// <summary>
    /// Returns true when animations should run (battery > 20% or device is charging/fully charged).
    /// </summary>
    public bool AnimationsEnabled { get; private set; }

    /// <summary>
    /// Returns true when the device is currently charging or fully charged.
    /// </summary>
    public bool IsCharging { get; private set; }

    /// <summary>
    /// Fired on the main thread whenever <see cref="AnimationsEnabled"/> changes value.
    /// </summary>
    public event EventHandler? AnimationsEnabledChanged;

    /// <summary>
    /// Fired on the main thread whenever <see cref="IsCharging"/> changes value.
    /// </summary>
    public event EventHandler? IsChargingChanged;

    public BatteryAwarenessService()
    {
        // Evaluate current state immediately so callers get the right answer on first access.
        AnimationsEnabled = ComputeAnimationsEnabled();
        IsCharging = ComputeIsCharging();

        Battery.BatteryInfoChanged += OnBatteryInfoChanged;
    }

    private void OnBatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e)
    {
        var newAnimationsEnabled = ComputeAnimationsEnabled();
        if (newAnimationsEnabled != AnimationsEnabled)
        {
            AnimationsEnabled = newAnimationsEnabled;
            MainThread.BeginInvokeOnMainThread(() => AnimationsEnabledChanged?.Invoke(this, EventArgs.Empty));
        }

        var newIsCharging = ComputeIsCharging();
        if (newIsCharging != IsCharging)
        {
            IsCharging = newIsCharging;
            MainThread.BeginInvokeOnMainThread(() => IsChargingChanged?.Invoke(this, EventArgs.Empty));
        }
    }

    private static bool ComputeAnimationsEnabled()
    {
        // Animations are allowed when the charger is connected (regardless of level)
        // or when the battery level is above the threshold.
        var state = Battery.Default.State;
        return state is BatteryState.Charging or BatteryState.Full || Battery.Default.ChargeLevel > LowBatteryThreshold;
    }

    private static bool ComputeIsCharging()
    {
        var state = Battery.Default.State;
        return state is BatteryState.Charging or BatteryState.Full;
    }

    public void Dispose()
    {
        Battery.BatteryInfoChanged -= OnBatteryInfoChanged;
    }
}
