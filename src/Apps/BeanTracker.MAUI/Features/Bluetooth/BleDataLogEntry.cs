using System.Text;

namespace BeanTracker.MAUI.Features.Bluetooth;

/// <summary>A single incoming value notification captured from a subscribed BLE characteristic.</summary>
public sealed class BleDataLogEntry
{
    public required DateTimeOffset Timestamp { get; init; }
    public required string CharacteristicName { get; init; }
    public required string ServiceName { get; init; }
    public required byte[] RawBytes { get; init; }

    public string TimeDisplay => Timestamp.ToString("HH:mm:ss.fff");

    public string HexDisplay => RawBytes.Length == 0
        ? "(empty)"
        : string.Join(' ', RawBytes.Select(b => b.ToString("X2")));

    public string AsciiDisplay
    {
        get
        {
            if (RawBytes.Length == 0) return string.Empty;
            var sb = new StringBuilder(RawBytes.Length);
            foreach (var b in RawBytes)
                sb.Append(b is >= 32 and < 127 ? (char)b : '·');
            return sb.ToString();
        }
    }

    public string DecimalDisplay => RawBytes.Length switch
    {
        1 => $"{RawBytes[0]}",
        2 => $"{BitConverter.ToInt16(RawBytes, 0)} (LE Int16)",
        4 => $"{BitConverter.ToInt32(RawBytes, 0)} (LE Int32)",
        _ => string.Empty
    };

    public bool HasDecimalDisplay => RawBytes.Length is 1 or 2 or 4;
    public bool HasAsciiDisplay => RawBytes.Any(b => b is >= 32 and < 127);
    public bool HasRawBytes => RawBytes.Length > 0;
}
