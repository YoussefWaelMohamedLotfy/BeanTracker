using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Datasync.Server.EntityFrameworkCore;

namespace BeanTracker.API.Data.Entities;

/// <summary>
/// Server-side entity for BLE data records. Inherits <see cref="EntityTableData"/> which provides
/// <c>Id</c>, <c>UpdatedAt</c>, <c>Version</c>, and <c>Deleted</c> for Datasync synchronization.
/// </summary>
public class BleDataRecordEntity : EntityTableData
{
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
