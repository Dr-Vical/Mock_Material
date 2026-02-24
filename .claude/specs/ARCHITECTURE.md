# Architecture Specification — RswareDesign

> Last updated: 2026-02-24 | Migrated from Platform (DevExpress/Prism/.NET 10)

## Summary

RswareDesign is a WPF desktop application for servo drive parameter configuration/tuning. Built on .NET 8, MaterialDesignInXamlToolkit, CommunityToolkit.Mvvm, AvalonDock, and Fluent.Ribbon. Follows Clean Architecture with dependency inversion: Presentation → Application → Domain ← Infrastructure. Serial-only communication for all drive types (including EtherCAT drives).

---

## 1. Layer Diagram

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│  Shell │ UI.Themes │ UI.Controls │ Modules.*             │
├─────────────────────────────────────────────────────────┤
│                   Application Layer                      │
│  Services │ UseCases │ DTOs │ Messaging                  │
├─────────────────────────────────────────────────────────┤
│                     Domain Layer                         │
│  Entities │ Interfaces │ ValueObjects │ Enums            │
├─────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                     │
│  Serial Communication │ File I/O │ Repositories          │
└─────────────────────────────────────────────────────────┘
```

## 2. Solution Project Map (8 projects)

### Domain (1)
| Project | Role |
|---------|------|
| `RswareDesign.Domain` | Entities (Parameter, Drive, DriveGroup), interfaces (ISerialPort, IParameterRepository, IDriveRepository), value objects (FtNumber, ParameterRange), enums (AccessMode, DriveState, ParameterValueType) |

### Application (1)
| Project | Role |
|---------|------|
| `RswareDesign.Application` | Business logic services (ParameterService, DriveConnectionService, OfflineService), use cases, DTOs, messaging contracts |

### Infrastructure (1)
| Project | Role |
|---------|------|
| `RswareDesign.Infrastructure` | Serial communication (SerialCommunicationService), CSV parsing (CsvParameterLoader), file I/O (FileParameterRepository, JsonSettingsRepository), repository implementations |

### Presentation (5)
| Project | Role |
|---------|------|
| `RswareDesign.Shell` | Composition root, App.xaml, MainWindow, DI registration, startup |
| `RswareDesign.UI.Themes` | Design token system, theme manager, ResourceDictionaries (Dark/Light), style dictionaries |
| `RswareDesign.UI.Controls` | Custom WPF controls (ParameterDataGrid, DriveTreePanel, ErrorLogPanel) |
| `RswareDesign.Modules` | Feature modules: ParameterEditor, Monitor, Oscilloscope, ControlPanel, Faults, DriveTree, ErrorLog |
| *(Shell includes Ribbon/StatusBar directly)* | |

---

## 3. Dependency Rules

### Allowed References
```
Presentation → Application → Domain
Infrastructure → Domain
Presentation → Infrastructure (DI registration only, in Shell)
```

### Prohibited References
```
Domain → Application, Infrastructure, Presentation
Application → Infrastructure, Presentation
Infrastructure → Application, Presentation
```

---

## 4. DI Registration Map (Shell/App.xaml.cs)

### Infrastructure Services
| Interface | Implementation | Lifetime |
|-----------|---------------|----------|
| `ISerialPortService` | `SerialPortService` | Singleton |
| `IParameterRepository` | `CsvParameterRepository` | Singleton |
| `IDriveRepository` | `DriveRepository` | Singleton |
| `IFileService` | `FileService` | Singleton |
| `ISettingsRepository` | `JsonSettingsRepository` | Singleton |

### Application Services
| Interface | Implementation | Lifetime |
|-----------|---------------|----------|
| `IParameterService` | `ParameterService` | Singleton |
| `IDriveConnectionService` | `DriveConnectionService` | Singleton |
| `IOfflineService` | `OfflineService` | Singleton |
| `IParameterEditService` | `ParameterEditService` | Singleton |

### Presentation Services
| Interface | Implementation | Lifetime |
|-----------|---------------|----------|
| `IThemeService` | `ThemeService` | Singleton |
| `ILocalizationService` | `LocalizationService` | Singleton |
| `INavigationService` | `NavigationService` | Singleton |
| `IDockingService` | `DockingService` | Singleton |
| `IDialogService` | `DialogService` | Singleton |
| `IStatusBarService` | `StatusBarService` | Singleton |

### ViewModels (Transient)
| Type | Notes |
|------|-------|
| `MainWindowViewModel` | Shell ViewModel |
| `DriveTreeViewModel` | Tree panel |
| `ParameterEditorViewModel` | Center panel |
| `MonitorViewModel` | ScottPlot monitor |
| `OscilloscopeViewModel` | ScottPlot oscilloscope |
| `ErrorLogViewModel` | Bottom error panel |
| `ControlPanelViewModel` | Drive control panel |
| `FaultsViewModel` | Fault history |
| `ConnectionSettingsViewModel` | Serial port dialog |

---

## 5. MainWindow Layout (AvalonDock)

```
┌────────────────────────────────────────────────────────────────┐
│ [Fluent.Ribbon]                                                │
│  File │ Tools │ Options │ Connection │ Views                   │
├──────────┬─────────────────────────────────┬───────────────────┤
│ Left     │ Center (AvalonDock Document)    │ Right             │
│ Anchorable│                                │ Anchorable        │
│          │ ┌─────────────────────────────┐ │                   │
│ DriveTree│ │ Parameter DataGrid          │ │ [Save Parameters] │
│          │ │ FtNo│Name│Value│Unit│Default│ │ [Revert]          │
│ On-Line  │ │     │Min │Max │Access      │ │ [Setup...]        │
│ └ Drive  │ │                             │ │ [Simple/Detail]   │
│   ├ Mode │ └─────────────────────────────┘ │ [Close]           │
│   ├ Motor│ ┌─────────────────────────────┐ │ [Help]            │
│   └ ...  │ │ Bottom Checkboxes           │ │                   │
│ Off-Line │ │ ☐Show Helps ☑Show Status    │ │                   │
│ └ Group  │ │ ☑Show Commands              │ │                   │
│   ├ Grp0 │ └─────────────────────────────┘ │                   │
│   └ Grp5 │                                 │                   │
├──────────┴─────────────────────────────────┴───────────────────┤
│ Bottom Anchorable: Error/Status Panel                          │
│ STATUS        │ VALUE    │ UNITS                               │
│ Drive Status  │ 0:IDLE   │                                     │
│ Drive Error   │ No Error │                                     │
├────────────────────────────────────────────────────────────────┤
│ [StatusBar] Connected: COM3 │ Drive: CSD7N │ Mode: Online      │
└────────────────────────────────────────────────────────────────┘
```

### AvalonDock Panel Registry
| Panel ID | Type | Default Position | Content |
|----------|------|-----------------|---------|
| `DriveTree` | LayoutAnchorable | Left | Drive/parameter tree navigator |
| `ParameterEditor` | LayoutDocument | Center | Parameter DataGrid (default) |
| `Monitor` | LayoutDocument | Center (tab) | ScottPlot real-time monitor |
| `Oscilloscope` | LayoutDocument | Center (tab) | ScottPlot oscilloscope |
| `ControlPanel` | LayoutDocument | Center (tab) | Drive control |
| `Faults` | LayoutDocument | Center (tab) | Fault history |
| `ActionPanel` | LayoutAnchorable | Right | Save/Revert/Setup buttons |
| `ErrorLog` | LayoutAnchorable | Bottom | Status & error log |
| `ServiceInfo` | LayoutDocument | Center (tab) | Drive service info |

### Ribbon Menu Structure

**File Tab**
| Item | Command | Shortcut |
|------|---------|----------|
| Open | OpenProjectCommand | Ctrl+O |
| Save | SaveParametersCommand | Ctrl+S |
| Load | LoadFromFileCommand | Ctrl+L |
| Close | CloseProjectCommand | |

**Tools Tab**
| Item | Command |
|------|---------|
| Drive | ShowDriveConfigCommand |
| Motor | ShowMotorConfigCommand |
| Encoder | ShowEncoderConfigCommand |

**Options Tab**
| Item | Command |
|------|---------|
| Font | ShowFontSettingsCommand |
| Theme | ToggleThemeCommand (Dark/Light) |
| User Mode | SetUserModeCommand (Basic/Advanced/Expert) |

**Connection Tab**
| Item | Command |
|------|---------|
| Rescan | RescanPortsCommand |
| Serial Port Setting | ShowSerialSettingsCommand |

**Views Tab**
| Item | Type | Command |
|------|------|---------|
| Drive Tree | ToggleButton | ToggleDriveTreeCommand |
| Monitor | ToggleButton | ToggleMonitorCommand |
| Oscilloscope | ToggleButton | ToggleOscilloscopeCommand |
| Error Log | ToggleButton | ToggleErrorLogCommand |
| Control Panel | ToggleButton | ToggleControlPanelCommand |
| Faults | ToggleButton | ToggleFaultsCommand |
| Service Info | ToggleButton | ToggleServiceInfoCommand |

---

## 6. Module Navigation Flow

```
User clicks tree node
  → DriveTreeViewModel.SelectedNodeChanged
    → WeakReferenceMessenger.Send<TreeNodeSelectedMessage>
      → ParameterEditorViewModel receives message
        → Load parameters for selected node from IParameterService
          → Online: read from drive via Serial
          → Offline: load from file/CSV
        → Display in DataGrid
```

### Messaging Contracts (WeakReferenceMessenger)
| Message | Payload | Sender → Receiver |
|---------|---------|-------------------|
| `TreeNodeSelectedMessage` | NodeId, NodeType, DriveAddress | DriveTree → ParameterEditor |
| `DriveConnectedMessage` | DriveInfo, PortName | ConnectionService → All |
| `DriveDisconnectedMessage` | DriveAddress | ConnectionService → All |
| `ParameterChangedMessage` | ParamNumber, OldValue, NewValue | ParameterEditor → StatusBar |
| `ThemeChangedMessage` | ThemeName, IsDark | ThemeService → All |
| `LanguageChangedMessage` | LangCode | LocalizationService → All |
| `ErrorOccurredMessage` | Severity, Source, Message | Any → ErrorLog |

---

## 7. Startup Sequence

```
1. App.xaml.cs → ConfigureServices(IServiceCollection)
   ↓
2. Register Infrastructure services
   ↓
3. Register Application services
   ↓
4. Register Presentation services + ViewModels
   ↓
5. Build IServiceProvider
   ↓
6. Create MainWindow + MainWindowViewModel
   ↓
7. Load settings (theme, language, last layout)
   ↓
8. Apply theme (Dark/Light)
   ↓
9. Apply language (ko/en)
   ↓
10. Restore AvalonDock layout
   ↓
11. Show MainWindow
   ↓
12. Auto-scan serial ports (background)
```

---

## 8. Migration Map (Platform → RswareDesign)

| Platform (Old) | RswareDesign (New) |
|---------------|-------------------|
| .NET 10 | .NET 8 |
| Prism 9 (DryIoc) | CommunityToolkit.Mvvm + MS DI |
| DevExpress 25.2 | MaterialDesignInXamlToolkit |
| DevExpress DockLayoutManager | AvalonDock |
| DevExpress RibbonControl | Fluent.Ribbon |
| DevExpress GridControl | WPF DataGrid + Material styling |
| Custom charts | ScottPlot.WPF |
| IEventAggregator (Prism) | WeakReferenceMessenger |
| BindableBase | ObservableObject |
| DelegateCommand | RelayCommand / AsyncRelayCommand |
| IContainerRegistry | IServiceCollection |
| ALC Plugin system | Built-in modules (no ALC) |
| 5 layers (20 projects) | 4 layers (8 projects) |
| EtherCAT + Serial | Serial only |

---

## Change History

| Date | Change |
|------|--------|
| 2026-02-24 | Initial spec — migrated from Platform architecture for Material Design stack |
