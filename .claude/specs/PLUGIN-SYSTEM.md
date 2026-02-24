# Plugin System Specification

> Last updated: 2026-02-19 | Source: `repos/Platform/src/SDK/Platform.Sdk.Common/Module/`, `repos/Platform/src/Infrastructure/Platform.Infrastructure.Plugins/`

## Summary

Platform uses an ALC-isolated plugin system. Plugins are discovered via `plugin.json` manifests, loaded into separate `AssemblyLoadContext` instances via `LoadFromStream` (no file locks during build), and managed through a state machine lifecycle. Each plugin gets its own DryIoc child container for DI isolation. Plugins are loaded asynchronously after `Shell.Loaded` (no sync-over-async). Plugins must extend `PluginModuleBase` and can only reference SDK assemblies. **SDK is Prism-free** — plugins have zero external NuGet dependencies (no Prism, no DryIoc). **Hot reload (runtime replacement of a loaded plugin) is not feasible** due to WPF constraints — activate/deactivate toggle is supported, but full unload+reload of a new DLL version requires app restart. Global exception handlers auto-identify and deactivate faulting plugins.

---

## 1. PluginModuleBase Pattern

**File:** `Sdk.Common/Module/PluginModuleBase.cs`

### Required Overrides (abstract)

```csharp
string ModuleId { get; }        // Unique plugin identifier
string DisplayName { get; }     // Display name in Navigator
string Description { get; }     // Plugin description
string Version { get; }         // SemVer version

void OnActivate();              // Add views to regions, subscribe events
void OnDeactivate();            // Remove views from regions
```

### Optional Overrides (virtual)

```csharp
void RegisterServices(IServiceRegistry registry);  // DI type registration
void BringToFront();                               // Tab switching
void OnBecameActive();                             // Post-activation hook
void OnBecameInactive();                           // Post-deactivation hook
```

### Built-in Helpers

```csharp
// Service access
T GetService<T>();                  // Throws if not registered
T? TryGetService<T>();              // Returns null if not registered

// Events (auto-tracked, auto-disposed on Deactivate)
void Subscribe<TEvent>(Action<TEvent> handler);
void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter);
void Publish<TEvent>(TEvent payload);

// Lifecycle
CancellationToken ModuleCancellationToken { get; }  // Cancelled on Deactivate
void RunOnUI(Action action);                         // UI thread dispatcher
Task RunOnUIAsync(Action action);                    // Async UI thread dispatcher
```

### Lifecycle Flow

```
CreateChild container (parentDryIoc.CreateChild())
  → PluginInfo.ChildContainer = child IContainer
  → DryIocServiceRegistry wraps child IContainer

RegisterServices(IServiceRegistry)
  → Plugin registers DI types into child container (optional)

Create module instance (Activator.CreateInstance(moduleType))
  → NOT container.Resolve — avoids requiring DI registration of the module type itself

Initialize(IServiceProvider)
  → Store service provider (child container's DryIocServiceProvider)
  → Call Activate()

Activate()
  → new CancellationTokenSource()
  → LoadTranslations()           // i18n: load Lang/{lang}.json
  → SubscribeLanguageChanged()   // i18n: subscribe to language switch
  → OnActivate()                 // Plugin adds views, subscribes events
  → IsActive = true
  → OnBecameActive()

Deactivate()
  → OnDeactivate()               // Plugin removes views from regions
  → DisposeSubscriptions()       // Auto-dispose all tracked event subscriptions
  → CleanupSdkServices()         // Remove navigator nodes, menu items, release devices
  → Cancel CancellationToken
  → IsActive = false
  → OnBecameInactive()

Dispose()
  → Deactivate() if active
  → Cancel/Dispose CTS
  → Services = null               // Break reference for ALC GC
  → ChildContainer.Dispose()      // Disposes all child-scoped registrations
```

> **Event subscription safety:** All `Subscribe<T>()` calls are auto-tracked. On `Deactivate()`, all subscriptions are bulk-disposed via `DisposeSubscriptions()`. No manual unsubscribe needed — prevents event subscription leaks.

> **Menu/toolbar cleanup:** `CleanupSdkServices()` calls `IMenuService.RemoveMenuItemsByModule(ModuleId)` and `RemoveToolbarItemsByModule(ModuleId)` to remove all menu/toolbar items registered by the plugin.

### Implementation Example

```csharp
public class CalculatorModule : PluginModuleBase
{
    private CalculatorView? _view;
    private CalculatorViewModel? _viewModel;

    public override string ModuleId => "Calculator";
    public override string DisplayName => "Motion Calculator";
    public override string Description => "FA Motion Control Calculator";
    public override string Version => "1.0.0";

    public override void RegisterServices(IServiceRegistry registry)
    {
        registry.RegisterSingleton<ICalculatorService, CalculatorService>();
        registry.Register<CalculatorViewModel>();
        registry.RegisterView<CalculatorView, CalculatorViewModel>();
    }

    protected override void OnActivate()
    {
        var shell = GetService<IShellService>();
        _viewModel = GetService<CalculatorViewModel>();
        _view = GetService<CalculatorView>();
        _view.DataContext = _viewModel;

        shell.OpenDocument("calc-main", DisplayName, _view);  // Instance overload for child container

        // Auto-tracked: no manual unsubscribe needed
        Subscribe<DeviceSelectedEventArgs>(OnDeviceSelected);
    }

    protected override void OnDeactivate()
    {
        var shell = GetService<IShellService>();
        shell.CloseDocument("calc-main");
        _view = null;
        _viewModel = null;
        // Event subscriptions auto-disposed by base class
    }

    private void OnDeviceSelected(DeviceSelectedEventArgs e)
    {
        // Handle device selection
    }
}
```

---

## 2. plugin.json Schema

### Required Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | Unique plugin identifier (e.g., "Calculator") |
| `name` | string | Plugin name |
| `version` | string | SemVer version |
| `displayName` | string | Display name |
| `description` | string | Description |
| `entryPoint` | string | Entry DLL filename (e.g., "MotionCalculator.dll") |
| `moduleType` | string | Full type name (e.g., "MotionCalculator.CalculatorModule") |

### Optional Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `author` | string? | null | Author name |
| `loadMode` | string | "OnDemand" | "OnDemand" or "WhenAvailable" |
| `minHostVersion` | string? | null | Min host version |
| `maxHostVersion` | string? | null | Max host version |
| `dependencies` | PluginDependency[] | [] | Plugin dependencies |
| `permissions` | string[] | [] | Requested permissions |
| `isolation` | PluginIsolation? | null | ALC isolation settings |

### Example (MotorMonitor)

```json
{
    "id": "Monitoring",
    "name": "MotorMonitor",
    "version": "1.0.0",
    "displayName": "Motor Monitor",
    "description": "Real-time motor system monitoring",
    "author": "MotorMonitor",
    "entryPoint": "MotorMonitor.dll",
    "moduleType": "MotorMonitor.MonitoringModule",
    "loadMode": "OnDemand"
}
```

---

## 3. ALC Isolation

### Assembly Loading

- **PluginLoadContext** inherits `AssemblyLoadContext(isCollectible: true)`
- **LoadFromStream** reads DLL+PDB into `MemoryStream` — no file locks (enables rebuild while app runs), but **hot reload is not feasible** (WPF type registration cannot be cleanly unloaded)
- **Shared assembly prefixes** (loaded from host ALC):
  ```
  Prism.*, DryIoc*, Platform.Sdk.*, DevExpress.*,
  Microsoft.Xaml.Behaviors, System.*, Microsoft.*,
  netstandard, WindowsBase, PresentationCore, PresentationFramework
  ```
  > Note: Prism/DryIoc remain in shared prefixes because the host Shell uses them internally. However, **plugins do not reference Prism directly** — they interact via SDK abstractions only.
- **Plugin-specific assemblies** — loaded via `AssemblyDependencyResolver` into plugin ALC

### Unload Sequence

```
1. Deactivate module (remove views, release devices, dispose subscriptions)
2. Dispose module (clear Services reference)
3. Dispose child container (ChildContainer.Dispose() — replaces old UnregisterPluginTypes())
4. Unload AssemblyLoadContext
5. GC.Collect() + GC.WaitForPendingFinalizers()
6. Remove from plugin dictionary
```

---

## 4. Plugin State Machine

```
Discovered → Validated → Loaded → Activated
                              ↓           ↓
                          Deactivated → Unloaded
                              ↓
                            Error
```

> **Auto-transition:** `Activated → Error` occurs when `TryRecoverFromPluginError()` auto-deactivates a faulting plugin identified by the global exception handler.

| State | Meaning |
|-------|---------|
| Discovered | Manifest loaded from plugin.json |
| Validated | Manifest validation passed |
| Loaded | Assembly loaded into ALC |
| Activated | Module initialized, views added |
| Deactivated | Module deactivated, assembly still in memory |
| Unloaded | Assembly unloaded from ALC |
| Error | Error occurred during any transition |

**Events:** `IPluginLifecycleManager.StateChanged` fires `PluginStateChangedEventArgs(PluginId, OldState, NewState, ErrorMessage?)`

---

## 5. Build Configuration

### Plugin .csproj Requirements

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>

  <!-- SDK References (DLL, not ProjectReference) — zero external NuGet deps -->
  <ItemGroup>
    <Reference Include="Platform.Sdk.Abstractions">
      <HintPath>..\..\SDK\Platform.Sdk.Abstractions.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Platform.Sdk.Common">
      <HintPath>..\..\SDK\Platform.Sdk.Common.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <!-- DevExpress (same version as host, for UI controls) -->
  <ItemGroup>
    <PackageReference Include="DevExpressXpf.Core" Version="25.2.4" />
  </ItemGroup>
</Project>
```

> **No Prism dependency:** Plugins no longer need `Prism.DryIoc` NuGet package. All DI and event interactions go through SDK abstractions (`IServiceRegistry`, `IEventBus`). The host Infrastructure layer bridges SDK abstractions to Prism internals via adapters.

### Output Path

Plugin DLLs are copied to `Plugins/{PluginId}/` under the host's output directory.

### SDK DLL Copy

After building the host app, copy SDK DLLs to plugin project:
```
Platform.Sdk.Abstractions.dll
Platform.Sdk.Common.dll
Platform.Sdk.DevExpress.dll (if needed)
```

---

## 6. Existing Plugins

| Plugin | ID | ModuleType | Repository |
|--------|-----|-----------|-----------|
| MotionCalculator | Calculator | `MotionCalculator.CalculatorModule` | `repos/MotionCalculator/` |
| MotorMonitor | Monitoring | `MotorMonitor.MonitoringModule` | `repos/MotorMonitor/` |

---

## 7. Async Plugin Loading

External plugins are no longer loaded during Prism's `ConfigureModuleCatalog()` or `InitializeModules()` — eliminating all sync-over-async (`.Wait()`) anti-patterns.

### Loading Sequence

```
Shell.Loaded event
  → LoadExternalPluginsAsync()         // Fully async
    → DiscoverPluginsAsync()           // Scan plugin directories
    → For each enabled plugin:
      → LoadPluginAsync(pluginId)      // ALC load + child container setup
      → ActivatePluginAsync(pluginId)  // Initialize + activate
```

External plugins are NOT added to the Prism module catalog. They are managed entirely by `IPluginLifecycleManager` outside of Prism's module system.

---

## 8. Error Recovery

### Global Exception Handler (Phase A)

`App.xaml.cs OnStartup` registers 3 exception handlers:
- `DispatcherUnhandledException` — UI thread
- `AppDomain.UnhandledException` — Non-UI unhandled
- `TaskScheduler.UnobservedTaskException` — Unobserved Task

### Plugin Fault Identification

When an unhandled exception occurs:
1. `IPluginLifecycleManager.IdentifyFaultyPlugin(exception)` walks the StackTrace
2. Matches exception assemblies against loaded plugin ALCs
3. Returns the faulting `pluginId` (or `null` if not plugin-related)

### Auto-Recovery

If a faulting plugin is identified:
1. `TryRecoverFromPluginError(pluginId)` deactivates the plugin
2. Logs the error via `ILoggerService`
3. Plugin state transitions to `Error`
4. Application continues running without the faulted plugin

---

## 9. Inter-Plugin Event Contract

**File:** `Sdk.Abstractions/Events/ModuleEvents.cs`

Three event types for cross-plugin communication. All payloads use string serialization (JSON) to avoid ALC type identity issues.

| Event | Purpose |
|-------|---------|
| `DataRequestEventArgs` | Request data from a specific plugin (SourceModuleId, TargetModuleId, RequestId, DataType, Parameters) |
| `DataResponseEventArgs` | Response to a data request (SourceModuleId, TargetModuleId, RequestId, Success, Data, ErrorMessage) |
| `ConfigurationChangedEventArgs` | Broadcast configuration changes (SourceModuleId, TargetModuleId, ConfigKey, OldValue, NewValue) |

> TargetModuleId = `"*"` for broadcast. RequestId correlates request-response pairs. See SDK-INTERFACES.md section 16 for full property signatures.

---

## Source File Paths

```
SDK:
  src/SDK/Platform.Sdk.Abstractions/Module/IPluginModule.cs
  src/SDK/Platform.Sdk.Abstractions/DependencyInjection/IServiceRegistry.cs
  src/SDK/Platform.Sdk.Abstractions/Events/IEventBus.cs
  src/SDK/Platform.Sdk.Abstractions/Events/ModuleEvents.cs
  src/SDK/Platform.Sdk.Abstractions/Manifest/PluginManifest.cs
  src/SDK/Platform.Sdk.Common/Module/PluginModuleBase.cs
  src/SDK/Platform.Sdk.Common/Base/ViewModelBase.cs

Infrastructure (Adapters):
  src/Infrastructure/Platform.Infrastructure.Plugins/Adapters/DryIocServiceRegistry.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Adapters/DryIocServiceProvider.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Adapters/PrismEventBusAdapter.cs

Infrastructure (Plugin System):
  src/Infrastructure/Platform.Infrastructure.Plugins/Discovery/ManifestDiscoveryService.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Lifecycle/PluginLifecycleManager.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Lifecycle/PluginState.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Lifecycle/PluginInfo.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Loading/PluginAssemblyLoader.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Loading/PluginLoadContext.cs
```

---

## Change History

| Date | Change |
|------|--------|
| 2026-02-13 | Initial spec created from source code extraction |
| 2026-02-19 | SDK Prism decoupling: PluginModuleBase now parameterless ctor, uses IServiceRegistry/IEventBus instead of Prism types. Removed Prism.DryIoc from plugin .csproj requirements. Added auto-tracked subscriptions and menu cleanup to lifecycle. Updated implementation example. |
| 2026-02-19 | Phase A/C/D/F: Per-plugin child containers (DryIoc CreateChild, Activator.CreateInstance). Async loading via Shell.Loaded. Error recovery with plugin fault identification and auto-deactivation. Inter-plugin event contract (DataRequest/DataResponse/ConfigurationChanged). Updated lifecycle flow, unload sequence, and example. |
