using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeanTracker.MAUI.Features.BarcodeScanner;

public sealed partial class BarcodeScannerViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    public partial ObservableCollection<ScannedBarcode> ScannedBarcodes { get; set; } = [];

    public bool HasResults => ScannedBarcodes.Count > 0;
    public bool HasNoResults => ScannedBarcodes.Count == 0;

    public bool AddBarcode(string text, string format)
    {
        // Ignore duplicates already in the list
        if (ScannedBarcodes.Any(b => b.Text == text))
            return false;

        var barcode = new ScannedBarcode(text, format, DateTime.Now);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ScannedBarcodes.Insert(0, barcode);
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasNoResults));
        });
        return true;
    }

    [RelayCommand]
    private void ClearResults()
    {
        ScannedBarcodes.Clear();
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(HasNoResults));
    }
}
