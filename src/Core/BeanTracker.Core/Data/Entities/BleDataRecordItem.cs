namespace BeanTracker.Core.Data.Entities;

/// <summary>
/// Client-side offline entity for BLE data records.
/// Must include <c>Id</c>, <c>UpdatedAt</c>, and <c>Version</c> properties
/// for the Datasync client to track synchronization state.
/// </summary>
public class BleDataRecordItem
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? Version { get; set; }

    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;

    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string CharacteristicId { get; set; } = string.Empty;
    public string CharacteristicName { get; set; } = string.Empty;

    public string RawHex { get; set; } = string.Empty;
    public string? AsciiValue { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public string? SessionLabel { get; set; }
}
