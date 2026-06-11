using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.BLE.Abstractions;

namespace BeanTracker.MAUI.Features.Bluetooth;

/// <summary>Observable model representing a single BLE characteristic and its latest notification value.</summary>
public sealed partial class BleCharacteristicItem : ObservableObject
{
    public required Guid ServiceId { get; init; }
    public required string ServiceName { get; init; }
    public required Guid CharacteristicId { get; init; }
    public required string CharacteristicName { get; init; }
    public required CharacteristicPropertyType Properties { get; init; }

    [ObservableProperty]
    public partial bool IsSubscribed { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasValue))]
    public partial string LastValueHex { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastValueAscii { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TimestampDisplay))]
    public partial DateTimeOffset? LastValueTimestamp { get; set; }

    public bool CanRead => (Properties & CharacteristicPropertyType.Read) != 0;
    public bool CanWrite => (Properties & (CharacteristicPropertyType.Write | CharacteristicPropertyType.WriteWithoutResponse)) != 0;
    public bool CanNotify => (Properties & CharacteristicPropertyType.Notify) != 0;
    public bool CanIndicate => (Properties & CharacteristicPropertyType.Indicate) != 0;
    public bool HasValue => !string.IsNullOrEmpty(LastValueHex);

    public string ShortUuid => CharacteristicId.ToString()[..8].ToUpperInvariant();

    public string PropertiesDisplay
    {
        get
        {
            var flags = new List<string>(4);
            if (CanRead) flags.Add("Read");
            if (CanWrite) flags.Add("Write");
            if (CanNotify) flags.Add("Notify");
            if (CanIndicate) flags.Add("Indicate");
            if ((Properties & CharacteristicPropertyType.Broadcast) != 0) flags.Add("Broadcast");
            return flags.Count > 0 ? string.Join(" · ", flags) : "None";
        }
    }

    public string TimestampDisplay => LastValueTimestamp?.ToString("HH:mm:ss.fff") ?? "—";
}
