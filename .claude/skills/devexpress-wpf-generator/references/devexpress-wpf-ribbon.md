# DevExpress WPF Ribbon, Bars and Menu

## Overview
- Office-style Ribbon UI, traditional Toolbar/MainMenu/StatusBar.
- Ribbon: Tab/Group/Item hierarchy, Quick Access Toolbar, Backstage View.
- Bars: BarManager for centralized toolbar management.
- Menus: PopupMenu, RadialMenu, PopupControlContainer.

## Key Classes
| Class | Namespace | Description |
|-------|-----------|-------------|
| `RibbonControl` | `DevExpress.Xpf.Ribbon` | Ribbon main control |
| `RibbonPage` | `DevExpress.Xpf.Ribbon` | Ribbon tab |
| `RibbonPageGroup` | `DevExpress.Xpf.Ribbon` | Group within tab |
| `BackstageViewControl` | `DevExpress.Xpf.Ribbon` | Backstage (File menu) |
| `BarManager` | `DevExpress.Xpf.Bars` | Bar item manager |
| `BarButtonItem` | `DevExpress.Xpf.Bars` | Button item |
| `BarCheckItem` | `DevExpress.Xpf.Bars` | Check toggle item |
| `BarSplitButtonItem` | `DevExpress.Xpf.Bars` | Split dropdown button |

- **xmlns (ribbon)**: `dxr="http://schemas.devexpress.com/winfx/2008/xaml/ribbon"`
- **xmlns (bars)**: `dxb="http://schemas.devexpress.com/winfx/2008/xaml/bars"`
- **NuGet**: `DevExpress.Wpf.Ribbon`

## Basic Ribbon XAML
```xml
<dxr:RibbonControl>
    <dxr:RibbonDefaultPageCategory>
        <dxr:RibbonPage Caption="Home">
            <dxr:RibbonPageGroup Caption="File">
                <dxb:BarButtonItem Content="Open"
                    Glyph="{dx:DXImage SvgImages/Actions/Open.svg}"
                    Command="{Binding OpenCommand}"/>
                <dxb:BarButtonItem Content="Save"
                    Command="{Binding SaveCommand}"/>
            </dxr:RibbonPageGroup>
            <dxr:RibbonPageGroup Caption="Edit">
                <dxb:BarCheckItem Content="Bold"
                    IsChecked="{Binding IsBold}"/>
                <dxb:BarSplitButtonItem Content="Paste">
                    <dxb:BarSplitButtonItem.PopupControl>
                        <dxb:PopupMenu>
                            <dxb:BarButtonItem Content="Paste Special"/>
                        </dxb:PopupMenu>
                    </dxb:BarSplitButtonItem.PopupControl>
                </dxb:BarSplitButtonItem>
            </dxr:RibbonPageGroup>
        </dxr:RibbonPage>
    </dxr:RibbonDefaultPageCategory>
</dxr:RibbonControl>
```

## Backstage View
```xml
<dxr:RibbonControl>
    <dxr:RibbonControl.ApplicationMenu>
        <dxr:BackstageViewControl>
            <dxr:BackstageButtonItem Content="Save" Command="{Binding SaveCommand}"/>
            <dxr:BackstageTabItem Header="Info">
                <TextBlock Text="Application Info"/>
            </dxr:BackstageTabItem>
        </dxr:BackstageViewControl>
    </dxr:RibbonControl.ApplicationMenu>
</dxr:RibbonControl>
```

## StatusBar
```xml
<dxr:RibbonStatusBarControl>
    <dxr:RibbonStatusBarControl.LeftItems>
        <dxb:BarStaticItem Content="{Binding StatusText}"/>
    </dxr:RibbonStatusBarControl.LeftItems>
    <dxr:RibbonStatusBarControl.RightItems>
        <dxb:BarStaticItem Content="{Binding ZoomLevel}"/>
    </dxr:RibbonStatusBarControl.RightItems>
</dxr:RibbonStatusBarControl>
```

## Traditional Bars (BarManager)
```xml
<dxb:BarManager>
    <dxb:BarManager.Items>
        <dxb:BarButtonItem x:Name="btnOpen" Content="Open"
                           Command="{Binding OpenCommand}"/>
    </dxb:BarManager.Items>
    <DockPanel>
        <dxb:MainMenuControl DockPanel.Dock="Top">
            <dxb:BarSubItem Content="File">
                <dxb:BarButtonItemLink BarItemName="btnOpen"/>
            </dxb:BarSubItem>
        </dxb:MainMenuControl>
        <dxb:ToolBarControl DockPanel.Dock="Top">
            <dxb:BarButtonItemLink BarItemName="btnOpen"/>
        </dxb:ToolBarControl>
    </DockPanel>
</dxb:BarManager>
```

## Key Properties
**RibbonControl**: `ToolbarShowMode`, `ShowApplicationButton`, `AllowMinimize`.
**BarButtonItem**: `Content`, `Glyph`, `LargeGlyph`, `Command`, `CommandParameter`, `KeyGesture`.
**BarCheckItem**: `IsChecked`, `GroupIndex` (radio behavior).

## Theme Selector (built-in behavior)
```xml
<dxb:BarSubItem Content="Theme">
    <dxmvvm:Interaction.Behaviors>
        <dxr:BarSubItemThemeSelectorBehavior/>
    </dxmvvm:Interaction.Behaviors>
</dxb:BarSubItem>
```

## Reference
- https://docs.devexpress.com/WPF/6194/controls-and-libraries/ribbon-bars-and-menu
