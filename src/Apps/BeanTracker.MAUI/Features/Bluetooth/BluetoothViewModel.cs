using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BeanTracker.MAUI.Features.Bluetooth;

public sealed partial class BluetoothViewModel : ObservableObject
{
    private readonly IBluetoothLE _ble = CrossBluetoothLE.Current;
    private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
    private CancellationTokenSource? _scanCts;
    private bool _isInitialized;

    [ObservableProperty]
    public partial ObservableCollection<BleDeviceItem> Devices { get; set; } = [];

    [ObservableProperty]
    public partial string BluetoothStatusText { get; set; } = "Checking…";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBluetoothOff))]
    public partial bool IsBluetoothOn { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotScanning))]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    public bool HasDevices => Devices.Count > 0;
    public bool HasNoDevices => !HasDevices;
    public bool IsBluetoothOff => !IsBluetoothOn;
    public bool IsNotScanning => !IsScanning;

    public void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        _adapter.ScanTimeout = 12_000;

        _ble.StateChanged += OnBleStateChanged;
        _adapter.DeviceDiscovered += OnDeviceDiscovered;
        _adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;

        UpdateBluetoothStatus();
    }

    public void Cleanup()
    {
        if (!_isInitialized) return;
        _isInitialized = false;

        _ble.StateChanged -= OnBleStateChanged;
        _adapter.DeviceDiscovered -= OnDeviceDiscovered;
        _adapter.ScanTimeoutElapsed -= OnScanTimeoutElapsed;

        _scanCts?.Cancel();
        _scanCts?.Dispose();
        _scanCts = null;
    }

    private void OnBleStateChanged(object? sender, BluetoothStateChangedArgs e) =>
        MainThread.BeginInvokeOnMainThread(UpdateBluetoothStatus);

    private void OnDeviceDiscovered(object? sender, DeviceEventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() => AddOrUpdateDevice(e.Device));

    private void OnScanTimeoutElapsed(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() => IsScanning = false);

    private void UpdateBluetoothStatus()
    {
        IsBluetoothOn = _ble.State == BluetoothState.On;
        BluetoothStatusText = _ble.State switch
        {
            BluetoothState.On => "Bluetooth is On",
            BluetoothState.Off => "Bluetooth is Off",
            BluetoothState.TurningOn => "Turning on…",
            BluetoothState.TurningOff => "Turning off…",
            BluetoothState.Unavailable => "Bluetooth unavailable on this device",
            BluetoothState.Unauthorized => "Bluetooth access not authorized",
            _ => "Bluetooth state unknown"
        };
    }

    [ObservableProperty]
    public partial BleDeviceItem? SelectedItem { get; set; }

    [RelayCommand]
    private async Task SelectDeviceAsync(BleDeviceItem device)
    {
        if (device is null) return;

        await Shell.Current.GoToAsync(nameof(BleDeviceDetailPage), new Dictionary<string, object>
        {
            ["DeviceId"] = device.Id.ToString(),
            ["DeviceName"] = device.Name
        });

        SelectedItem = null;
    }

    /// <summary>Returns system-connected or bonded devices (primarily useful on Android).</summary>
    [RelayCommand]
    private async Task LoadSystemDevicesAsync()
    {
        await LoadAndEnrichDevicesAsync();
    }

    /// <summary>Pull-to-refresh: re-queries system devices to update connection states.</summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadAndEnrichDevicesAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task LoadAndEnrichDevicesAsync()
    {
        var systemDevices = await Task.Run(() => _adapter.GetSystemConnectedOrPairedDevices());

#if ANDROID
        // BLUETOOTH_CONNECT is required on API 31+ to read BluetoothDevice.Address from
        // getConnectedDevices(). Ensure it is granted before invoking the native check.
        Dictionary<string, string> nativeProfiles = [];
        var hasConnectPermission = !OperatingSystem.IsAndroidVersionAtLeast(31) ||
            await Permissions.CheckStatusAsync<BluetoothConnectPermission>() == PermissionStatus.Granted;

        if (!hasConnectPermission)
            hasConnectPermission = await Permissions.RequestAsync<BluetoothConnectPermission>() == PermissionStatus.Granted;

        if (hasConnectPermission)
            nativeProfiles = await Task.Run(GetNativeConnectionProfiles);
#endif

        foreach (var device in systemDevices)
        {
#if ANDROID
            var address = (device.NativeDevice as Android.Bluetooth.BluetoothDevice)?.Address;
            nativeProfiles.TryGetValue(address ?? string.Empty, out var profile);
            AddOrUpdateDevice(device, overrideState: profile is not null ? "Connected" : null, connectionProfile: profile);
#else
            AddOrUpdateDevice(device);
#endif
        }
        NotifyDevicesChanged();
    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (!IsBluetoothOn)
        {
            await Shell.Current.DisplayAlertAsync("Bluetooth Off", "Please enable Bluetooth to scan for devices.", "OK");
            return;
        }

        var hasPermissions = await EnsureBluetoothPermissionsAsync();
        if (!hasPermissions)
        {
            await Shell.Current.DisplayAlertAsync("Permission Required",
                "Bluetooth permissions are required to scan for devices.", "OK");
            return;
        }

        IsScanning = true;
        _scanCts = new CancellationTokenSource();
        try
        {
            await _adapter.StartScanningForDevicesAsync(cancellationToken: _scanCts.Token);
        }
        catch (OperationCanceledException) { /* stopped by user or cleanup */ }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Scan Error", $"Could not scan: {ex.Message}", "OK");
        }
        finally
        {
            IsScanning = false;
            _scanCts?.Dispose();
            _scanCts = null;
        }
    }

    [RelayCommand]
    private void StopScan()
    {
        _scanCts?.Cancel();
    }

    [RelayCommand]
    private void ClearDevices()
    {
        Devices.Clear();
        NotifyDevicesChanged();
    }

    private void AddOrUpdateDevice(IDevice device, string? overrideState = null, string? connectionProfile = null)
    {
        var state = overrideState ?? device.State.ToString();
        var existing = Devices.FirstOrDefault(d => d.Id == device.Id);
        if (existing is not null)
        {
            existing.Rssi = device.Rssi;
            existing.State = state;
            existing.BondState = device.BondState.ToString();
            existing.ConnectionProfile = connectionProfile ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(device.Name) && existing.Name == "Unknown Device")
                existing.Name = device.Name;
        }
        else
        {
            Devices.Insert(0, new BleDeviceItem
            {
                Name = string.IsNullOrWhiteSpace(device.Name) ? "Unknown Device" : device.Name,
                Id = device.Id,
                Rssi = device.Rssi,
                State = state,
                BondState = device.BondState.ToString(),
                ConnectionProfile = connectionProfile ?? string.Empty
            });
        }
        NotifyDevicesChanged();
    }

    private void NotifyDevicesChanged()
    {
        OnPropertyChanged(nameof(HasDevices));
        OnPropertyChanged(nameof(HasNoDevices));
    }

    private static async Task<bool> EnsureBluetoothPermissionsAsync()
    {
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            var scan = await Permissions.RequestAsync<BluetoothScanPermission>();
            var connect = await Permissions.RequestAsync<BluetoothConnectPermission>();
            return scan == PermissionStatus.Granted && connect == PermissionStatus.Granted;
        }
        else
        {
            var location = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return location == PermissionStatus.Granted;
        }
#else
        return await Task.FromResult(true);
#endif
    }

#if ANDROID
    /// <summary>
    /// Queries Android's BluetoothManager across all standard Bluetooth profiles
    /// (GATT, GATT Server, A2DP, Headset) and returns a map of MAC address → comma-separated
    /// list of profiles on which each device is currently connected.
    /// This catches Classic BT connections (e.g. Galaxy Watch via A2DP) that Plugin.BLE misses.
    /// </summary>
    private static Dictionary<string, string> GetNativeConnectionProfiles()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var context = Android.App.Application.Context;
        var btManager = context.GetSystemService(Android.Content.Context.BluetoothService)
                        as Android.Bluetooth.BluetoothManager;
        if (btManager is null) return result;

        var profiles = new (Android.Bluetooth.ProfileType Type, string Label)[]
        {
            (Android.Bluetooth.ProfileType.Gatt,       "BLE GATT"),
            (Android.Bluetooth.ProfileType.GattServer, "BLE GATT Server"),
            (Android.Bluetooth.ProfileType.A2dp,       "A2DP"),
            (Android.Bluetooth.ProfileType.Headset,    "Headset"),
        };

        foreach (var (type, label) in profiles)
        {
            try
            {
                foreach (var nativeDevice in btManager.GetConnectedDevices(type))
                {
                    var address = nativeDevice.Address;
                    if (address is null) continue;
                    result[address] = result.TryGetValue(address, out var existing)
                        ? $"{existing}, {label}"
                        : label;
                }
            }
            catch { /* profile not supported on this device/OS version */ }
        }
        return result;
    }

    private sealed class BluetoothScanPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            [("android.permission.BLUETOOTH_SCAN", true)];
    }

    private sealed class BluetoothConnectPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            [("android.permission.BLUETOOTH_CONNECT", true)];
    }
#endif
}
