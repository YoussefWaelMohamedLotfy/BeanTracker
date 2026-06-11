using System.ComponentModel.DataAnnotations;

namespace BeanTracker.Core.Bluetooth;

/// <summary>A single BLE characteristic notification persisted for later analysis.</summary>
public sealed class BleDataRecord
{
    [Key]
    public int Id { get; set; }

    // ── Device ──────────────────────────────────────────────────────────────
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;

    // ── Service / Characteristic ────────────────────────────────────────────
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string CharacteristicId { get; set; } = string.Empty;
    public string CharacteristicName { get; set; } = string.Empty;

    // ── Payload ─────────────────────────────────────────────────────────────
    /// <summary>Space-separated hexadecimal bytes, e.g. "3C 00 FF".</summary>
    public string RawHex { get; set; } = string.Empty;

    /// <summary>Printable ASCII representation (non-printable bytes replaced with '·'). Null when no printable bytes.</summary>
    public string? AsciiValue { get; set; }

    // ── Timing ──────────────────────────────────────────────────────────────
    public DateTimeOffset Timestamp { get; set; }

    // ── Session grouping ────────────────────────────────────────────────────
    /// <summary>A caller-supplied label to group a recording session (e.g. "Session 2026-06-06 01:55").</summary>
    public string? SessionLabel { get; set; }
}
