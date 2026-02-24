# SDK Interfaces Specification

> Last updated: 2026-02-19 | Source: `repos/Platform/src/SDK/Platform.Sdk.Abstractions/Services/`

## Summary

Platform SDK provides service interfaces for plugins to interact with the host application. All services are registered as Singleton in DI and available via `GetService<T>()` in `PluginModuleBase`. **SDK is Prism-free** — plugins reference only `Platform.Sdk.Abstractions` and `Platform.Sdk.Common` (zero external NuGet dependencies).

---

## 1. IShellService — Shell Layout Control

**Interface:** `Sdk.Abstractions/Services/IShellService.cs`
**Implementation:** `Shell/Services/ShellService.cs`

```csharp
void RegisterNavigator(string title, Type viewType, string? iconName = null);
void UnregisterNavigator(string title);
void OpenDocument(string contentId, string title, Type viewType, object? parameter = null);
void OpenDocument(string contentId, string title, object viewInstance, object? parameter = null);
void CloseDocument(string contentId);
void ActivateDocument(string contentId);
void ShowToolPanel(string panelId, bool show = true);
void CleanupByAssembly(string assemblyFullName);

IReadOnlyList<string> OpenDocumentIds { get; }
string? ActiveDocumentId { get; }
```

> **OpenDocument overloads:** The `Type viewType` overload resolves the view from the parent DI container. The `object viewInstance` overload accepts a pre-created view instance — plugins with child containers should use this overload to pass views resolved from their own child container. Internally, the Type-based overload delegates to the instance-based version.

---

## 2. IDialogService — Common Dialogs

**Interface:** `Sdk.Abstractions/Services/IDialogService.cs`
**Implementation:** `Shell/Services/DialogService.cs`

```csharp
Task<bool> ShowConfirmAsync(string title, string message);
Task ShowAlertAsync(string title, string message);
Task ShowErrorAsync(string title, string message, Exception? exception = null);
Task<string?> ShowOpenFileDialogAsync(string filter, string? initialDirectory = null);
Task<string?> ShowSaveFileDialogAsync(string filter, string? defaultFileName = null, string? initialDirectory = null);
Task<string?> ShowFolderBrowserDialogAsync(string? description = null, string? initialDirectory = null);
Task<T?> ShowSelectionDialogAsync<T>(string title, string instruction, IEnumerable<T> items) where T : class;
```

---

## 3. IStatusService — Status Bar Control

**Interface:** `Sdk.Abstractions/Services/IStatusService.cs`
**Implementation:** `Shell/Services/StatusService.cs`

```csharp
void SetStatus(string message, bool isBusy = false);
void SetProgress(int? percentage);   // 0-100 or null to hide
void ClearStatus();

string CurrentStatus { get; }
bool IsBusy { get; }
int? CurrentProgress { get; }

event EventHandler<StatusChangedEventArgs>? StatusChanged;
```

**EventArgs:** `StatusChangedEventArgs` — Message (string), IsBusy (bool), Progress (int?)

---

## 4. ILoggerService — Output Window Logging

**Interface:** `Sdk.Abstractions/Services/ILoggerService.cs`
**Implementation:** `Shell/Services/LoggerService.cs`

```csharp
void LogInfo(string message, string? source = null);
void LogWarning(string message, string? source = null);
void LogError(string message, string? source = null);
void LogError(string message, Exception exception, string? source = null);
void LogDebug(string message, string? source = null);
void Clear();

event EventHandler<LogEventArgs>? LogReceived;
```

**Related types:**
- `LogLevel` enum: Debug, Info, Warning, Error
- `LogEventArgs` — Timestamp (DateTime), Level (LogLevel), Message (string), Source (string?), Exception (Exception?)

---

## 5. IMenuService — Dynamic Menu/Toolbar Management

**Interface:** `Sdk.Abstractions/Services/IMenuService.cs`
**Implementation:** `Shell/Services/MenuService.cs`

```csharp
void AddMenuItem(string parentPath, string header, ICommand command, string? iconName = null, string? gestureText = null, string? moduleId = null);
void RemoveMenuItem(string parentPath, string header);
void AddMenuSeparator(string parentPath);
void AddToolbarItem(string groupName, string header, ICommand command, string? iconName = null, string? moduleId = null);
void RemoveToolbarItem(string groupName, string header);
void AddToolbarSeparator(string groupName);
void RemoveMenuItemsByModule(string moduleId);      // DIM — bulk cleanup
void RemoveToolbarItemsByModule(string moduleId);    // DIM — bulk cleanup

event EventHandler<MenuChangedEventArgs>? MenuChanged;
event EventHandler<ToolbarChangedEventArgs>? ToolbarChanged;
```

**Related types:**
- `MenuChangeType` enum: Added, Removed
- `MenuChangedEventArgs` — ChangeType, ParentPath, Header
- `ToolbarChangedEventArgs` — ChangeType, GroupName, Header

> `RemoveMenuItemsByModule` / `RemoveToolbarItemsByModule` — Auto-called by PluginModuleBase.CleanupSdkServices() on Deactivate. Only removes items registered with a matching moduleId.

---

## 6. INavigatorService — Navigator Tree Management

**Interface:** `Sdk.Abstractions/Services/INavigatorService.cs`
**Implementation:** Part of `UI.Modules.ProjectExplorer`

```csharp
void RegisterNode(NavigatorNodeDefinition definition);
void UpdateNode(string nodeId, Action<NavigatorNodeDefinition> update);
void RemoveNode(string nodeId);
void RemoveNodesByModule(string moduleId);
void SelectNode(string nodeId);

event EventHandler<NodeSelectedEventArgs>? NodeSelected;
```

**Related types:**
- `NavigatorNodeDefinition`:
  - `string Id { get; init; }` — Unique node ID
  - `string DisplayName { get; set; }`
  - `string ModuleId { get; init; }`
  - `string? ParentId { get; init; }`
  - `string? Icon { get; set; }`
  - `string? Status { get; set; }` — "Running", "Error", "Idle"
  - `string? Badge { get; set; }` — Alert count
  - `NavigatorNodeType NodeType { get; init; }`
  - `object? Tag { get; set; }` — User data
  - `int SortOrder { get; init; }`
- `NavigatorNodeType` enum: Category, Item, Device
- `NodeSelectedEventArgs` — NodeId, ModuleId, NodeType, Tag

---

## 7. IPropertyPanelService — Properties Panel Control

**Interface:** `Sdk.Abstractions/Services/IPropertyPanelService.cs`
**Implementation:** `Shell/Services/PropertyPanelService.cs`

```csharp
void ShowProperties(PropertyPanelDefinition definition);
void Clear();

PropertyPanelDefinition? CurrentDefinition { get; }

event EventHandler<PropertyValueChangedEventArgs>? PropertyValueChanged;
```

**Related types:**
- `PropertyPanelDefinition`:
  - `string Title { get; init; }`
  - `string ModuleId { get; init; }`
  - `IReadOnlyList<PropertyItemDefinition> Items { get; init; }`
- `PropertyItemDefinition`:
  - `string Key { get; init; }`
  - `string DisplayName { get; init; }`
  - `string? Category { get; init; }`
  - `PropertyType PropertyType { get; init; }`
  - `object? Value { get; set; }`
  - `bool IsReadOnly { get; init; }`
  - `string? Description { get; init; }`
  - `IReadOnlyList<string>? EnumValues { get; init; }`
  - `double? MinValue { get; init; }`
  - `double? MaxValue { get; init; }`
  - `string? Unit { get; init; }` — "mm/s", "%", "pulse"
- `PropertyType` enum: Text, Integer, Number, Boolean, Enum, Color, FilePath, MultilineText
- `PropertyValueChangedEventArgs` — ModuleId, PropertyKey, OldValue, NewValue

---

## 8. IDeviceBroker — Device Access Control

**Interface:** `Sdk.Abstractions/Services/IDeviceBroker.cs`
**Implementation:** `Shell/Services/DeviceBrokerService.cs`

```csharp
Task<IDeviceLease> AcquireAsync(string deviceId, string requesterId, AccessLevel level);
void Release(string deviceId, string requesterId);
DeviceAccessInfo GetAccessInfo(string deviceId);
void ReleaseAll(string requesterId);

event EventHandler<AccessChangedEventArgs>? AccessChanged;
```

**Related types:**
- `AccessLevel` enum: Read, Write
- `IDeviceLease` (IDisposable):
  - `string DeviceId { get; }`
  - `string OwnerId { get; }`
  - `AccessLevel GrantedLevel { get; }`
  - `bool IsValid { get; }`
  - `event EventHandler<AccessDemotedEventArgs>? AccessDemoted`
  - `event EventHandler<AccessPromotedEventArgs>? AccessPromoted`
- `AccessDemotedEventArgs` — PreviousLevel, CurrentLevel, Reason
- `AccessPromotedEventArgs` — PreviousLevel, AvailableLevel, Reason
- `AccessChangedEventArgs` — DeviceId, ChangeType, ModuleId, Level
- `AccessChangeType` enum: Acquired, Released, Demoted, Promoted
- `DeviceAccessInfo` — DeviceId, WriteOwner?, Readers (IReadOnlyList<string>), IsInUse

---

## 9. ILocalizationService — Runtime Localization

**Interface:** `Sdk.Abstractions/Services/ILocalizationService.cs`
**Implementation:** `UI.DesignSystem/Services/LocalizationService.cs`

```csharp
string Get(string key, string? defaultValue = null);
string GetFormatted(string key, params object[] args);
void SetLanguage(string languageCode);
void RegisterTranslations(string sourceId, IDictionary<string, string> translations);
void UnregisterTranslations(string sourceId);

IReadOnlyList<string> AvailableLanguages { get; }
string CurrentLanguage { get; }

event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
```

**EventArgs:** `LanguageChangedEventArgs` — PreviousLanguage (string), NewLanguage (string)

---

## 10. IDesignTokenService — Theme/Font Token Management

**Interface:** `UI.DesignSystem/Services/IDesignTokenService.cs`
**Implementation:** `UI.DesignSystem/Services/DesignTokenService.cs`

> Note: This interface is in UI.DesignSystem, not SDK.Abstractions (accessible to shell modules only).

```csharp
void ApplyTheme(string themeName);
string CurrentTheme { get; }
bool IsDarkTheme { get; }

void SetFontFamily(string uiFont, string? codeFont = null);
string CurrentFontFamilyUI { get; }
string CurrentFontFamilyCode { get; }

void SetFontScale(double scale);
double CurrentFontScale { get; }

event EventHandler<ThemeTokensUpdatedEventArgs>? TokensUpdated;
```

---

## 11. IProjectContext — Current Project State

**Interface:** `Sdk.Abstractions/Services/IProjectContext.cs`
**Implementation:** `Shell/Services/ProjectContext.cs`

```csharp
string? ProjectName { get; }
string? ProjectPath { get; }
bool HasProject { get; }
```

---

## 12. IPluginLifecycleManager — Plugin Lifecycle Control

**Interface:** `Infrastructure.Plugins/Lifecycle/IPluginLifecycleManager.cs`
**Implementation:** `Infrastructure.Plugins/Lifecycle/PluginLifecycleManager.cs`

```csharp
Task DiscoverPluginsAsync();
Task LoadPluginAsync(string pluginId);
Task ActivatePluginAsync(string pluginId);
Task DeactivatePluginAsync(string pluginId);
Task UnloadPluginAsync(string pluginId);

// Error recovery (Phase A: Global Exception Handler)
string? IdentifyFaultyPlugin(Exception exception);       // StackTrace → Assembly → PluginId matching
bool TryRecoverFromPluginError(string pluginId);          // Auto-deactivate faulting plugin

IReadOnlyDictionary<string, PluginInfo> Plugins { get; }

event EventHandler<PluginStateChangedEventArgs>? StateChanged;
```

> `IdentifyFaultyPlugin` walks the exception StackTrace, matches assemblies against loaded plugin ALCs, and returns the matching pluginId (or null). `TryRecoverFromPluginError` deactivates the identified plugin and logs the error.

---

## 13. IServiceRegistry — DI Service Registration

**Interface:** `Sdk.Abstractions/DependencyInjection/IServiceRegistry.cs`
**Adapter:** `Infrastructure.Plugins/Adapters/DryIocServiceRegistry.cs` (wraps IContainer directly)

```csharp
void Register<TService, TImplementation>();
void RegisterSingleton<TService, TImplementation>();
void RegisterInstance<TService>(TService instance);
void RegisterView<TView, TViewModel>(string? name = null);
void Register<TService>();
void RegisterSingleton<TService>();
```

> Used in plugin's `RegisterServices(IServiceRegistry)`. Replaces Prism IContainerRegistry.

---

## 14. IEventBus — Event Publish/Subscribe

**Interface:** `Sdk.Abstractions/Events/IEventBus.cs`
**Adapter:** `Infrastructure.Plugins/Adapters/PrismEventBusAdapter.cs` (wraps IEventAggregator, UIThread)

```csharp
void Publish<TEvent>(TEvent payload) where TEvent : class;
IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
IDisposable Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter) where TEvent : class;
```

> Wrapped by PluginModuleBase's `Subscribe<T>()` / `Publish<T>()` convenience methods. Subscriptions are auto-tracked and bulk-disposed on Deactivate (prevents event subscription leaks).

---

## 15. IPluginModule — Plugin Module Contract

**Interface:** `Sdk.Abstractions/Module/IPluginModule.cs`
**Base class:** `Sdk.Common/Module/PluginModuleBase.cs`

```csharp
// Extends: IDisposable (Prism IModule removed)
string ModuleId { get; }
string DisplayName { get; }
string Description { get; }
string Version { get; }
bool IsActive { get; }

void RegisterServices(IServiceRegistry registry);   // DI type registration
void Initialize(IServiceProvider services);          // Initialization (calls Activate)
void Activate();
void Deactivate();
void BringToFront();
```

### PluginModuleBase 제공 메서드

```csharp
// Service access
protected T GetService<T>();        // Throws if not registered
protected T? TryGetService<T>();     // Returns null if not registered

// Events (auto-tracked, auto-disposed on Deactivate)
protected void Subscribe<TEvent>(Action<TEvent> handler);
protected void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter);
protected void Publish<TEvent>(TEvent payload);

// Lifecycle
public CancellationToken ModuleCancellationToken { get; }
protected void RunOnUI(Action action);
protected Task RunOnUIAsync(Action action);
```

---

## 16. Inter-Plugin Event Contract (ModuleEvents)

**File:** `Sdk.Abstractions/Events/ModuleEvents.cs`

All payloads use string serialization (ALC type identity safe). Events include SourceModuleId and TargetModuleId for directed messaging.

### DataRequestEventArgs — Request-Response Pattern (Request)

```csharp
string SourceModuleId { get; }
string TargetModuleId { get; }
string RequestId { get; }          // Correlation ID for matching response
string DataType { get; }           // Requested data type identifier
string? Parameters { get; }        // JSON-serialized request parameters
```

### DataResponseEventArgs — Request-Response Pattern (Response)

```csharp
string SourceModuleId { get; }
string TargetModuleId { get; }
string RequestId { get; }          // Matches DataRequestEventArgs.RequestId
bool Success { get; }
string? Data { get; }              // JSON-serialized response data
string? ErrorMessage { get; }
```

### ConfigurationChangedEventArgs — Configuration Broadcast

```csharp
string SourceModuleId { get; }
string TargetModuleId { get; }     // "*" for broadcast to all
string ConfigKey { get; }
string? OldValue { get; }          // JSON-serialized
string? NewValue { get; }          // JSON-serialized
```

> All payloads are `string`-based to avoid ALC type identity issues — plugins in different ALCs cannot share concrete types, but can exchange JSON strings safely.

---

## Source File Paths

```
Core Abstractions:
  src/SDK/Platform.Sdk.Abstractions/DependencyInjection/IServiceRegistry.cs
  src/SDK/Platform.Sdk.Abstractions/Events/IEventBus.cs
  src/SDK/Platform.Sdk.Abstractions/Module/IPluginModule.cs

Events:
  src/SDK/Platform.Sdk.Abstractions/Events/ModuleEvents.cs

Interfaces:
  src/SDK/Platform.Sdk.Abstractions/Services/IShellService.cs
  src/SDK/Platform.Sdk.Abstractions/Services/IDialogService.cs
  src/SDK/Platform.Sdk.Abstractions/Services/IStatusService.cs
  src/SDK/Platform.Sdk.Abstractions/Services/ILoggerService.cs
  src/SDK/Platform.Sdk.Abstractions/Services/IMenuService.cs
  src/SDK/Platform.Sdk.Abstractions/Services/INavigatorService.cs
  src/SDK/Platform.Sdk.Abstractions/Services/IPropertyPanelService.cs
  src/SDK/Platform.Sdk.Abstractions/Services/IDeviceBroker.cs
  src/SDK/Platform.Sdk.Abstractions/Services/ILocalizationService.cs
  src/SDK/Platform.Sdk.Abstractions/Module/IPluginModule.cs

Definitions:
  src/SDK/Platform.Sdk.Abstractions/Definitions/NavigatorNodeDefinition.cs
  src/SDK/Platform.Sdk.Abstractions/Definitions/PropertyDefinition.cs
  src/SDK/Platform.Sdk.Abstractions/Manifest/PluginManifest.cs

Plugin Lifecycle:
  src/Infrastructure/Platform.Infrastructure.Plugins/Lifecycle/IPluginLifecycleManager.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Lifecycle/PluginLifecycleManager.cs

Adapters (Infrastructure):
  src/Infrastructure/Platform.Infrastructure.Plugins/Adapters/DryIocServiceRegistry.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Adapters/DryIocServiceProvider.cs
  src/Infrastructure/Platform.Infrastructure.Plugins/Adapters/PrismEventBusAdapter.cs

Implementations:
  src/Presentation/Platform.Shell/Services/ShellService.cs
  src/Presentation/Platform.Shell/Services/DialogService.cs
  src/Presentation/Platform.Shell/Services/StatusService.cs
  src/Presentation/Platform.Shell/Services/LoggerService.cs
  src/Presentation/Platform.Shell/Services/MenuService.cs
  src/Presentation/Platform.Shell/Services/PropertyPanelService.cs
  src/Presentation/Platform.Shell/Services/DeviceBrokerService.cs
  src/Presentation/Platform.Shell/Services/ProjectContext.cs
  src/Presentation/Platform.UI.DesignSystem/Services/LocalizationService.cs
  src/Presentation/Platform.UI.DesignSystem/Services/DesignTokenService.cs
```

---

## Change History

| Date | Change |
|------|--------|
| 2026-02-13 | Initial spec created from source code extraction |
| 2026-02-19 | SDK Prism decoupling: Added IServiceRegistry, IEventBus. Removed IModule from IPluginModule. Added moduleId + RemoveByModule to IMenuService. ViewModelBase: BindableBase replaced with INPC. PluginModuleBase: parameterless ctor, GetService, auto-tracked Subscribe |
| 2026-02-19 | Phase A/B/F: Added IShellService.OpenDocument(viewInstance) overload, IPluginLifecycleManager.IdentifyFaultyPlugin/TryRecoverFromPluginError, inter-plugin event contract (DataRequest/DataResponse/ConfigurationChanged EventArgs) |
