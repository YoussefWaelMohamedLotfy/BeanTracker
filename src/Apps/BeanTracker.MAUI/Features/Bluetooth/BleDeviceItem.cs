using CommunityToolkit.Mvvm.ComponentModel;

namespace BeanTracker.MAUI.Features.Bluetooth;

public sealed partial class BleDeviceItem : ObservableObject
{
    public Guid Id { get; init; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RssiText))]
    [NotifyPropertyChangedFor(nameof(SignalStrengthLabel))]
    public partial int Rssi { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(StateBadgeColor))]
    public partial string State { get; set; } = "Unknown";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BondStateBadgeColor))]
    [NotifyPropertyChangedFor(nameof(BondStateLabel))]
    [NotifyPropertyChangedFor(nameof(IsBondStateVisible))]
    public partial string BondState { get; set; } = "NotSupported";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionProfileVisible))]
    public partial string ConnectionProfile { get; set; } = string.Empty;

    public bool IsConnected => State == "Connected";

    public bool IsBondStateVisible => BondState != "NotSupported";

    public bool IsConnectionProfileVisible => !string.IsNullOrEmpty(ConnectionProfile);

    public string BondStateLabel => BondState switch
    {
        "Bonded" => "🔗 Bonded",
        "NotBonded" => "🔓 Not Bonded",
        "Bonding" => "⏳ Bonding…",
        _ => string.Empty
    };

    public Color BondStateBadgeColor => BondState switch
    {
        "Bonded" => Color.FromArgb("#1565C0"),
        "Bonding" => Color.FromArgb("#C68642"),
        _ => Color.FromArgb("#9E9E9E")
    };

    public string ShortId => Id.ToString()[..8].ToUpperInvariant();

    public string RssiText => Rssi == 0 ? "N/A" : $"{Rssi} dBm";

    public string SignalStrengthLabel => Rssi switch
    {
        0 => "—",
        >= -50 => "Excellent",
        >= -65 => "Good",
        >= -80 => "Fair",
        _ => "Weak"
    };

    public Color StateBadgeColor => State switch
    {
        "Connected" => Color.FromArgb("#2E7D32"),
        "Disconnected" => Color.FromArgb("#9E9E9E"),
        _ => Color.FromArgb("#C68642")
    };
}
