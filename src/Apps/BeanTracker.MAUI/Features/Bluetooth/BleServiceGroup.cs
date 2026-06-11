namespace BeanTracker.MAUI.Features.Bluetooth;

/// <summary>
/// Grouped list of characteristics belonging to a single BLE service.
/// Constructed once after full discovery and treated as immutable after binding.
/// </summary>
public sealed class BleServiceGroup(string serviceName, Guid serviceId, IEnumerable<BleCharacteristicItem> items)
    : List<BleCharacteristicItem>(items)
{
    public string ServiceName { get; } = serviceName;
    public Guid ServiceId { get; } = serviceId;
    public string ShortUuid => ServiceId.ToString()[..8].ToUpperInvariant();
    public string DisplayName => $"{ServiceName}";
    public string SubtitleDisplay => $"{ShortUuid}  ·  {Count} characteristic{(Count == 1 ? "" : "s")}";
}
