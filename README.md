# BeanTracker

A cross-platform **.NET MAUI** mobile & desktop application for coffee enthusiasts — browse coffee drinks, discover breweries, scan barcodes, and use on-device AI vision (OCR) to identify coffee items from photos. The solution is orchestrated with **.NET Aspire** for local development.

---

## Solution Structure

```
BeanTracker/
├── src/
│   ├── Apps/
│   │   └── BeanTracker.MAUI/          # .NET MAUI cross-platform app
│   ├── Core/
│   │   └── BeanTracker.Core/          # Shared domain models, services & data access
│   └── Aspire/
│       ├── BeanTracker.Aspire.AppHost/ # .NET Aspire orchestrator
│       └── BeanTracker.Aspire.ServiceDefaults/ # Shared Aspire service defaults
```

---

## Projects

### `BeanTracker.MAUI` — Cross-Platform App

The main application targeting **Android**, **iOS**, **macOS (Mac Catalyst)**, and **Windows**.

#### Features

| Feature | Description |
|---|---|
| **Coffee Drinks** | Browse a local catalog of coffee drinks with descriptions, origin, flavor notes, and random images fetched from an external API. Save drinks to Favourites. |
| **Favourites** | View and manage your saved favorite coffee drinks, persisted locally with SQLite via Entity Framework Core. |
| **Breweries** | Search and browse breweries powered by the [Open Brewery DB](https://www.openbrewerydb.org/) public REST API. View brewery details including address, type, and website. |
| **Barcode Scanner** | Real-time barcode/QR-code scanning using the device camera (`Camera.MAUI`). Deduplicates results and plays a beep sound on successful scan. |
| **OCR / AI Vision** | Capture or select an image and run it through a local **Ollama** vision model (`gemma4:e2b`) to identify coffee-related content. Matches AI output against the coffee drink catalog. |
| **Bluetooth (BLE) Data Logger** | Scan and connect to nearby BLE devices, explore services and characteristics, subscribe to notifications/indications, and log data. Upon stopping a recording, offers to share the database file via native sharing sheet. |
| **Feedback** | Star-rating popup with a local notification or toast confirming the submitted rating. |

#### Key Libraries

- **[.NET MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui)** — UI helpers, popups, toasts, alerts
- **[MVVM Community Toolkit](https://github.com/CommunityToolkit/dotnet)** — `ObservableObject`, `RelayCommand`, source generators
- **[Plugin.BLE](https://github.com/dotnet-bluetooth-le/Plugin.BLE)** — Bluetooth Low Energy (BLE) scanning, connection, and data acquisition
- **[CameraMaui](https://github.com/janusw/CameraMaui)** — Camera preview & barcode scanning
- **[OllamaSharp](https://github.com/awaescher/OllamaSharp)** — .NET client for local Ollama LLM inference
- **[Plugin.LocalNotification](https://github.com/thudugala/Plugin.LocalNotification)** — Cross-platform local push notifications
- **[Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio)** — Audio playback (beep on scan)
- **[Plugin.Maui.Biometric](https://github.com/oscoreio/Maui.Biometric)** — Biometric authentication
- **[Plugin.Maui.ScreenSecurity](https://github.com/FabriBertani/Plugin.Maui.ScreenSecurity)** — Prevent screenshots
- **Entity Framework Core + SQLite** — Local database for favourites and BLE recordings

---

### `BeanTracker.Core` — Shared Domain Library

A .NET class library shared across the solution containing:

- **Domain models**: `CoffeeDrink`, `Brewery`, `FavouriteDrink`
- **Service abstractions**: `ICoffeeDrinkService`, `IBreweryService`, `ICoffeeImageService`, `IFavouritesService`
- **Implementations**:
  - `LocalCoffeeDrinkService` — loads drinks from the bundled `drinks.json` asset
  - `BreweryApiService` — HTTP client wrapping the Open Brewery DB API
  - `CoffeeImageApiService` — fetches random coffee images with a 3-minute in-memory cache
  - `LocalFavouritesService` — CRUD operations against SQLite via `BeanTrackerDbContext`
- **Data**: `BeanTrackerDbContext` (EF Core `DbContext` with a `Favourites` DbSet)

---

### `BeanTracker.Aspire.AppHost` — Aspire Orchestrator

Uses **.NET Aspire** to orchestrate the MAUI app for local development with built-in observability (OpenTelemetry). Supports:

- **Windows** — launches the MAUI app as a Windows device target
- **macOS** — launches as Mac Catalyst and an iOS Simulator (with OTLP dev tunnel)
- **Android** — launches an Android device target with an OTLP dev tunnel for telemetry

A public **dev tunnel** (`devtunnel-public`) with anonymous access is configured for connectivity.

---

### `BeanTracker.Aspire.ServiceDefaults` — Aspire Service Defaults

Shared Aspire configuration for OpenTelemetry, health checks, and service discovery defaults — applied to services in the Aspire app host.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [.NET MAUI workload](https://learn.microsoft.com/dotnet/maui/get-started/installation): `dotnet workload install maui`
- [Ollama](https://ollama.com/) (for the OCR/AI Vision feature): pull the `gemma4:e2b` model
  ```bash
  ollama pull gemma4:e2b
  ```
- Android SDK / Xcode (for mobile targets)

---

## Getting Started

### Run with Aspire (recommended)

```bash
cd src/Aspire/BeanTracker.Aspire.AppHost
dotnet run
```

The Aspire dashboard will open, and the MAUI app will launch on the configured device target.

### Run MAUI directly

```bash
cd src/Apps/BeanTracker.MAUI
dotnet build -t:Run -f net10.0-android   # Android
dotnet build -t:Run -f net10.0-windows10.0.19041.0  # Windows
```

---

## Architecture

```
┌─────────────────────────────────────────┐
│           BeanTracker.MAUI              │
│  (MVVM — Pages / ViewModels / Helpers)  │
│                                         │
│  Features:                              │
│   Coffee | Breweries | Barcode | OCR    │
│   Favourites | Feedback | Bluetooth     │
└───────────────────┬─────────────────────┘
                    │ references
┌───────────────────▼─────────────────────┐
│           BeanTracker.Core              │
│  Models | Services | EF Core DbContext  │
└─────────────────────────────────────────┘

External APIs & Devices:
  • Open Brewery DB  →  BreweryApiService
  • coffee.alexflipnote.dev  →  CoffeeImageApiService
  • Ollama (local)  →  OcrViewModel
  • Bluetooth Low Energy Devices  →  BluetoothViewModel / BleDeviceDetailViewModel
```

---

## License

See [LICENSE](LICENSE) for details.
