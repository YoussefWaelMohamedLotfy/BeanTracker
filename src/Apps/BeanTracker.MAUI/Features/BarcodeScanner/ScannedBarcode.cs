namespace BeanTracker.MAUI.Features.BarcodeScanner;

public sealed class ScannedBarcode
{
    public string Text { get; }
    public string Format { get; }
    public DateTime ScannedAt { get; }
    public string DisplayTime => ScannedAt.ToString("HH:mm:ss");

    public ScannedBarcode(string text, string format, DateTime scannedAt)
    {
        Text = text;
        Format = format;
        ScannedAt = scannedAt;
    }
}
