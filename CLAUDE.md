# RswareDesign

Servo drive parameter configuration/tuning desktop application.
Migration from DevExpress/Prism architecture to Material Design + CommunityToolkit.Mvvm.

## Tech Stack

| Category | Library | Version |
|----------|---------|---------|
| Framework | .NET 8.0 (net8.0-windows) | 8.0 |
| UI Toolkit | MaterialDesignInXamlToolkit | latest |
| MVVM | CommunityToolkit.Mvvm | latest |
| DI | Microsoft.Extensions.DependencyInjection | 8.x |
| Docking | AvalonDock (Xceed.Wpf.AvalonDock) | latest free |
| Ribbon | Fluent.Ribbon | 10.x |
| Chart | ScottPlot.WPF | 5.x |
| Serial | System.IO.Ports | 8.x |
| CSV | CsvHelper | latest |
| Logging | Microsoft.Extensions.Logging + Serilog | latest |

## Architecture

Clean Architecture: `Presentation -> Application -> Domain <- Infrastructure`

```
RswareDesign.sln
├── src/
│   ├── RswareDesign.Domain/            # Entities, interfaces, value objects
│   ├── RswareDesign.Application/       # Services, use cases, DTOs
│   ├── RswareDesign.Infrastructure/    # Serial comm, file I/O, repositories
│   └── Presentation/
│       ├── RswareDesign.Shell/         # Composition root, MainWindow
│       ├── RswareDesign.UI.Themes/     # Theme dictionaries, design tokens
│       ├── RswareDesign.UI.Controls/   # Custom controls
│       └── RswareDesign.Modules/       # Feature module views/viewmodels
└── tests/
```

## Conventions

### MVVM (CommunityToolkit.Mvvm)
- ViewModels inherit `ObservableObject`, use `[ObservableProperty]`, `[RelayCommand]`
- Inter-module messaging via `WeakReferenceMessenger`
- No code-behind logic in Views (data binding only)

### Design Tokens (MUST follow)
```
NEVER: Foreground="#CCCCCC"     -> USE: Foreground="{DynamicResource TextPrimary}"
NEVER: FontSize="12"           -> USE: FontSize="{DynamicResource FontSizeMD}"
NEVER: Background="#1E1E1E"    -> USE: Background="{DynamicResource SurfaceBrush}"
NEVER: Margin="0,0,0,12"      -> USE: Margin="{StaticResource Margin.FormField}"
NEVER: CornerRadius="4"        -> USE: CornerRadius="{StaticResource Radius.MD}"
NEVER: Text="저장"              -> USE: Text="{DynamicResource loc.common.save}"
```

### Color Constraint: MAX 5 roles
1. **Primary** - Accent, active elements, primary buttons
2. **Secondary** - Highlights, selected items, secondary actions
3. **Surface** - Panel/card backgrounds
4. **Background** - App background
5. **Error** - Error/danger states

### i18n (Korean/English)
- All UI text via `{DynamicResource loc.*}` keys
- Parameter names localized from CSV Remark/Name columns
- Key pattern: `loc.{area}.{target}.{detail}`

### Communication
- Serial only (RS-232/485) for all drives (including EtherCAT drives)
- Parameter read/write via SET/STR commands
- Multi-drive support via drive address

## Spec References

| Spec | Path |
|------|------|
| Architecture | `.claude/specs/ARCHITECTURE.md` |
| Design System | `.claude/specs/DESIGN-SYSTEM.md` |
| i18n | `.claude/specs/I18N.md` |
| Parameter System | `.claude/specs/PARAMETER-SYSTEM.md` |
| Communication | `.claude/specs/COMMUNICATION.md` |

## Build

```bash
dotnet restore RswareDesign.sln
dotnet build RswareDesign.sln -c Debug
dotnet run --project src/Presentation/RswareDesign.Shell
```

## Target
- Resolution: 1920x1080 responsive
- OS: Windows 10/11
- Multi-drive: simultaneous connection support
- Offline mode: file-based parameter editing without drive connection
