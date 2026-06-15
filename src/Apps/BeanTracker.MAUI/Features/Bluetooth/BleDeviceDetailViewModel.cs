using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Channels;
using BeanTracker.Core.Bluetooth;
using BeanTracker.Core.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;

namespace BeanTracker.MAUI.Features.Bluetooth;

public sealed partial class BleDeviceDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
    private readonly BeanTrackerDbContext _db;
    private IDevice? _device;
    private CancellationTokenSource? _cts;
    private string _sessionLabel = string.Empty;
    private readonly string _dbPath;

    // Maps each subscribed ICharacteristic to its handler delegate so it can be removed cleanly.
    private readonly Dictionary<ICharacteristic, EventHandler<CharacteristicUpdatedEventArgs>>
        _characteristicHandlers = [];

    private Guid _deviceId;

    // Page reference for showing dialogs — set from the code-behind via SetPage().
    private Page? _page;
    
    private Channel<BleDataRecord> _dbWriteChannel = Channel.CreateUnbounded<BleDataRecord>(new UnboundedChannelOptions { SingleReader = true });
    private Task _dbProcessorTask;
    private readonly object _uiLock = new();
    private readonly List<(BleCharacteristicItem item, byte[] bytes, DateTimeOffset now)> _pendingUiUpdates = [];
    private bool _uiUpdateScheduled;

    public BleDeviceDetailViewModel(BeanTrackerDbContext db)
    {
        _db = db;
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "beantracker.db");
        _dbProcessorTask = Task.Run(ProcessDbWritesAsync);
    }

    /// <summary>Called by the code-behind to supply a page reference for dialogs.</summary>
    public void SetPage(Page page) => _page = page;

    private async Task ProcessDbWritesAsync()
    {
        var reader = _dbWriteChannel.Reader;
        try
        {
            await foreach (var record in reader.ReadAllAsync())
            {
                try
                {
                    _db.BleRecordings.Add(record);
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                    
                    MainThread.BeginInvokeOnMainThread(() => RecordingCount++);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[BLE Record] Save failed: {ex.Message}");
                }
            }
        }
        catch (ChannelClosedException) { }
    }

    // ─── Query / navigation data ────────────────────────────────────────────
    [ObservableProperty]
    public partial string DeviceName { get; set; } = "BLE Device";

    // ─── State ──────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy), nameof(IsNotBusy), nameof(BusyMessage), nameof(CanConnect), nameof(CanDisconnect), nameof(ConnectionStatusText), nameof(ConnectionStatusColor))]
    public partial bool IsConnecting { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect), nameof(CanDisconnect), nameof(ConnectionStatusText), nameof(ConnectionStatusColor))]
    public partial bool IsConnected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy), nameof(IsNotBusy), nameof(BusyMessage), nameof(IsNotDiscovering))]
    public partial bool IsDiscovering { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLiveTabVisible), nameof(IsCharacteristicsTabVisible), nameof(LiveTabUnderlineColor), nameof(CharacteristicsTabUnderlineColor), nameof(LiveTabTextColor), nameof(CharacteristicsTabTextColor))]
    public partial int SelectedTab { get; set; } = 0;

    // ─── Recording State ─────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RecordButtonText), nameof(RecordButtonColor), nameof(IsNotRecording))]
    public partial bool IsRecording { get; set; }

    public bool IsNotRecording => !IsRecording;

    [ObservableProperty]
    public partial int RecordingCount { get; set; }

    // ─── Data ───────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLiveData), nameof(HasNoLiveData))]
    public partial ObservableCollection<BleDataLogEntry> LiveDataEntries { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<BleServiceGroup> ServiceGroups { get; set; } = [];

    // ─── Computed ───────────────────────────────────────────────────────────
    public bool IsBusy => IsConnecting || IsDiscovering;
    public bool IsNotBusy => !IsBusy;
    public bool IsNotDiscovering => !IsDiscovering;
    public bool CanConnect => !IsConnected && !IsConnecting;
    public bool CanDisconnect => IsConnected && !IsConnecting;
    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasLiveData => LiveDataEntries.Count > 0;
    public bool HasNoLiveData => !HasLiveData;
    public bool IsLiveTabVisible => SelectedTab == 0;
    public bool IsCharacteristicsTabVisible => SelectedTab == 1;

    public string BusyMessage => IsConnecting
        ? $"Connecting to {DeviceName}…"
        : "Discovering BLE services…";

    public string ConnectionStatusText => IsConnecting ? "Connecting…"
        : IsConnected ? "Connected"
        : "Disconnected";

    public Color ConnectionStatusColor => IsConnected
        ? Color.FromArgb("#2E7D32")
        : Color.FromArgb("#9E9E9E");

    public Color LiveTabUnderlineColor => SelectedTab == 0
        ? Color.FromArgb("#C68642") : Colors.Transparent;

    public Color CharacteristicsTabUnderlineColor => SelectedTab == 1
        ? Color.FromArgb("#C68642") : Colors.Transparent;

    public Color LiveTabTextColor => SelectedTab == 0
        ? Color.FromArgb("#C68642") : Color.FromArgb("#A08060");

    public Color CharacteristicsTabTextColor => SelectedTab == 1
        ? Color.FromArgb("#C68642") : Color.FromArgb("#A08060");

    public string RecordButtonText => IsRecording ? "⏹ Stop Recording" : "⏺ Record";
    public Color RecordButtonColor => IsRecording
        ? Color.FromArgb("#B71C1C")
        : Color.FromArgb("#C68642");

    // ─── Commands ───────────────────────────────────────────────────────────
    [RelayCommand]
    private void SelectLiveTab() => SelectedTab = 0;

    [RelayCommand]
    private void SelectCharacteristicsTab() => SelectedTab = 1;

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        await ConnectAndDiscoverAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async Task DisconnectAsync()
    {
        if (_device is null) return;
        await CleanupSubscriptionsAsync();
        try { await _adapter.DisconnectDeviceAsync(_device); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BLE Detail] Disconnect error: {ex.Message}");
        }
        IsConnected = false;
    }

    [RelayCommand]
    private void ClearLog()
    {
        LiveDataEntries.Clear();
        OnPropertyChanged(nameof(HasLiveData));
        OnPropertyChanged(nameof(HasNoLiveData));
    }

    [RelayCommand]
    private async Task ToggleRecording()
    {
        if (IsRecording)
        {
            IsRecording = false;
            await OfferShareAsync();
        }
        else
        {
            _sessionLabel = $"{DeviceName} — {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}";
            RecordingCount = 0;
            IsRecording = true;
        }
    }

    private async Task OfferShareAsync()
    {
        if (_page is null) return;

        // ── 1. Drain all pending writes before showing the dialog ────────────
        // Completing the writer signals ProcessDbWritesAsync to exit once the
        // queue empties. We await that task so every queued BleDataRecord is
        // committed to disk before we do anything with the file.
        _dbWriteChannel.Writer.TryComplete();
        await _dbProcessorTask;

        bool share = await _page.DisplayAlertAsync(
            title:   "Recording Saved",
            message: $"Session recorded {RecordingCount} entries.\nShare the database file?",
            accept:  "Share",
            cancel:  "Not Now");

        if (share)
        {
            try
            {
                // ── 2. WAL checkpoint ────────────────────────────────────────
                // EF Core's SQLite provider runs in WAL mode by default.
                // Recent writes live in `beantracker.db-wal`, NOT in the main
                // .db file. TRUNCATE merges + clears the WAL so the shared file
                // is fully self-contained and contains all BLE recordings.
                await _db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE)");

                // ── 3. Share ─────────────────────────────────────────────────
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"BeanTracker BLE Session — {DeviceName}",
                    File  = new ShareFile(_dbPath, "application/octet-stream")
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLE Share] Share failed: {ex.Message}");
                await _page.DisplayAlertAsync("Share Failed", ex.Message, "OK");
            }
        }

        // ── 4. Restart the processor so the next recording session works ─────
        _dbWriteChannel = Channel.CreateUnbounded<BleDataRecord>(new UnboundedChannelOptions { SingleReader = true });
        _dbProcessorTask = Task.Run(ProcessDbWritesAsync);
    }


    // ─── IQueryAttributable ─────────────────────────────────────────────────
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("DeviceId", out var id) &&
            Guid.TryParse(id?.ToString(), out var deviceId))
            _deviceId = deviceId;

        if (query.TryGetValue("DeviceName", out var name))
            DeviceName = name?.ToString() ?? "BLE Device";

        _adapter.DeviceConnectionLost += OnConnectionLost;
        _ = ConnectAndDiscoverAsync();
    }

    // ─── Lifecycle ──────────────────────────────────────────────────────────
    public async Task CleanupAsync()
    {
        _adapter.DeviceConnectionLost -= OnConnectionLost;
        await (_cts?.CancelAsync() ?? Task.CompletedTask);
        _cts?.Dispose();
        _cts = null;

        await CleanupSubscriptionsAsync();

        if (_device is not null && IsConnected)
        {
            try { await _adapter.DisconnectDeviceAsync(_device); }
            catch { /* best-effort */ }
        }

        IsConnected = false;
        IsConnecting = false;
        IsDiscovering = false;
    }

    // ─── Private Implementation ─────────────────────────────────────────────
    private async Task ConnectAndDiscoverAsync()
    {
        if (IsConnecting || _deviceId == Guid.Empty) return;

        ErrorMessage = string.Empty;
        ServiceGroups.Clear();
        IsConnecting = true;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            // 20-second connection timeout; some Classic-BT bridged devices are slow to respond.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(20));

            _device = await _adapter.ConnectToKnownDeviceAsync(_deviceId, cancellationToken: timeoutCts.Token);
            IsConnected = true;
            IsConnecting = false;

            await DiscoverServicesAsync(_device);
        }
        catch (DeviceConnectionException ex)
        {
            ErrorMessage = $"Connection failed: {ex.Message}";
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = IsConnecting ? "Connection timed out." : string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private async Task DiscoverServicesAsync(IDevice device)
    {
        IsDiscovering = true;
        var groups = new List<BleServiceGroup>();
        try
        {
            var services = await device.GetServicesAsync();
            foreach (var service in services)
            {
                var serviceName = ResolveName(service.Id, ServiceNames);
                var charItems = new List<BleCharacteristicItem>();
                try
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    foreach (var ch in characteristics)
                    {
                        var charItem = new BleCharacteristicItem
                        {
                            ServiceId = service.Id,
                            ServiceName = serviceName,
                            CharacteristicId = ch.Id,
                            CharacteristicName = ResolveName(ch.Id, CharacteristicNames),
                            Properties = ch.Properties
                        };
                        charItems.Add(charItem);

                        if (charItem.CanNotify || charItem.CanIndicate)
                            await SubscribeAsync(ch, charItem);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[BLE Detail] GetCharacteristics for '{serviceName}' failed: {ex.Message}");
                }

                groups.Add(new BleServiceGroup(serviceName, service.Id, charItems));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Service discovery failed: {ex.Message}";
        }
        finally
        {
            IsDiscovering = false;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ServiceGroups.Clear();
            foreach (var g in groups)
                ServiceGroups.Add(g);
        });
    }

    private async Task SubscribeAsync(ICharacteristic characteristic, BleCharacteristicItem item)
    {
        try
        {
            EventHandler<CharacteristicUpdatedEventArgs> handler =
                (_, e) => OnCharacteristicValueUpdated(e.Characteristic, item);

            characteristic.ValueUpdated += handler;
            _characteristicHandlers[characteristic] = handler;

            await characteristic.StartUpdatesAsync();

            MainThread.BeginInvokeOnMainThread(() => item.IsSubscribed = true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[BLE Detail] Subscribe to '{item.CharacteristicName}' failed: {ex.Message}");
        }
    }

    private async Task CleanupSubscriptionsAsync()
    {
        foreach (var (ch, handler) in _characteristicHandlers)
        {
            ch.ValueUpdated -= handler;
            try { await ch.StopUpdatesAsync(); }
            catch { /* best-effort */ }
        }
        _characteristicHandlers.Clear();
    }

    private void OnCharacteristicValueUpdated(ICharacteristic characteristic, BleCharacteristicItem item)
    {
        var bytes = characteristic.Value ?? [];
        var now = DateTimeOffset.Now;

        if (IsRecording)
        {
            var hex = bytes.Length == 0
                ? "(empty)"
                : string.Join(' ', bytes.Select(b => b.ToString("X2")));
            var hasAscii = bytes.Any(b => b is >= 32 and < 127);
            var ascii = hasAscii ? new string(bytes.Select(b => b is >= 32 and < 127 ? (char)b : '·').ToArray()) : null;

            var record = new BleDataRecord
            {
                DeviceId = _deviceId.ToString(),
                DeviceName = DeviceName,
                ServiceId = item.ServiceId.ToString(),
                ServiceName = item.ServiceName,
                CharacteristicId = item.CharacteristicId.ToString(),
                CharacteristicName = item.CharacteristicName,
                RawHex = hex,
                AsciiValue = ascii,
                Timestamp = now,
                SessionLabel = _sessionLabel
            };
            
            _dbWriteChannel.Writer.TryWrite(record);
        }

        lock (_uiLock)
        {
            _pendingUiUpdates.Add((item, bytes, now));
            if (!_uiUpdateScheduled)
            {
                _uiUpdateScheduled = true;
                Task.Delay(250).ContinueWith(_ => DispatchUiUpdates(), TaskScheduler.Default);
            }
        }
    }

    private void DispatchUiUpdates()
    {
        List<(BleCharacteristicItem item, byte[] bytes, DateTimeOffset now)> batch;
        lock (_uiLock)
        {
            batch = _pendingUiUpdates.ToList();
            _pendingUiUpdates.Clear();
            _uiUpdateScheduled = false;
        }

        if (batch.Count == 0) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            bool liveDataChanged = false;

            foreach (var update in batch)
            {
                var bytes = update.bytes;
                var now = update.now;
                var item = update.item;

                var hex = bytes.Length == 0 ? "(empty)" : string.Join(' ', bytes.Select(b => b.ToString("X2")));
                var ascii = new string(bytes.Select(b => b is >= 32 and < 127 ? (char)b : '·').ToArray());

                item.LastValueHex = hex;
                item.LastValueAscii = ascii;
                item.LastValueTimestamp = now;

                var entry = new BleDataLogEntry
                {
                    Timestamp = now,
                    CharacteristicName = item.CharacteristicName,
                    ServiceName = item.ServiceName,
                    RawBytes = bytes
                };

                LiveDataEntries.Insert(0, entry);
                liveDataChanged = true;
            }

            while (LiveDataEntries.Count > 200)
                LiveDataEntries.RemoveAt(LiveDataEntries.Count - 1);

            if (liveDataChanged)
            {
                OnPropertyChanged(nameof(HasLiveData));
                OnPropertyChanged(nameof(HasNoLiveData));
            }
        });
    }

    private void OnConnectionLost(object? sender, DeviceErrorEventArgs e)
    {
        if (e.Device.Id != _deviceId) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsConnected = false;
            ErrorMessage = "Connection lost.";
        });
    }

    // ─── Standard BLE UUID resolution ───────────────────────────────────────
    private static string ResolveName(Guid uuid, IReadOnlyDictionary<ushort, string> lookup)
    {
        // Standard BLE 16-bit UUIDs follow the pattern 0000XXXX-0000-1000-8000-00805f9b34fb
        var s = uuid.ToString("D").ToLowerInvariant();
        if (s.EndsWith("-0000-1000-8000-00805f9b34fb") &&
            ushort.TryParse(s.AsSpan(4, 4), NumberStyles.HexNumber, null, out var shortId) &&
            lookup.TryGetValue(shortId, out var name))
            return name;

        return uuid.ToString()[..8].ToUpperInvariant();
    }

    private static readonly Dictionary<ushort, string> ServiceNames = new()
    {
        [0x1800] = "Generic Access",
        [0x1801] = "Generic Attribute",
        [0x1802] = "Immediate Alert",
        [0x1803] = "Link Loss",
        [0x1804] = "Tx Power",
        [0x180A] = "Device Information",
        [0x180D] = "Heart Rate",
        [0x180F] = "Battery Service",
        [0x1810] = "Blood Pressure",
        [0x1812] = "HID",
        [0x1813] = "Scan Parameters",
        [0x1814] = "Running Speed & Cadence",
        [0x1816] = "Cycling Speed & Cadence",
        [0x181C] = "User Data",
        [0x181E] = "Bond Management",
    };

    private static readonly Dictionary<ushort, string> CharacteristicNames = new()
    {
        [0x2A00] = "Device Name",
        [0x2A01] = "Appearance",
        [0x2A04] = "Peripheral Preferred Conn. Params",
        [0x2A05] = "Service Changed",
        [0x2A06] = "Alert Level",
        [0x2A07] = "Tx Power Level",
        [0x2A19] = "Battery Level",
        [0x2A24] = "Model Number",
        [0x2A25] = "Serial Number",
        [0x2A26] = "Firmware Revision",
        [0x2A27] = "Hardware Revision",
        [0x2A28] = "Software Revision",
        [0x2A29] = "Manufacturer Name",
        [0x2A37] = "Heart Rate Measurement",
        [0x2A38] = "Body Sensor Location",
        [0x2A6E] = "Temperature",
        [0x2A6F] = "Humidity",
        [0x2A9D] = "Weight Measurement",
        [0x2A9E] = "Weight Scale Feature",
        [0x2ACC] = "Fitness Machine Feature",
        [0x2AD9] = "Fitness Machine Control Point",
    };
}
