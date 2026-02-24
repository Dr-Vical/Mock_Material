# DevExpress WPF Property Grid

## Overview
- Display/edit object properties (like Visual Studio Properties window).
- Built-in search, category mode, attribute support, validation.
- Custom inline editors, sub-property display, collection editing.

## Key Classes
| Class | Description |
|-------|-------------|
| `PropertyGridControl` | Main property grid |
| `PropertyDefinition` | Property definition |
| `CategoryDefinition` | Category definition |

- **xmlns**: `dxprg="http://schemas.devexpress.com/winfx/2008/xaml/propertygrid"`
- **NuGet**: `DevExpress.Wpf.PropertyGrid`

## Basic XAML
```xml
<dxprg:PropertyGridControl SelectedObject="{Binding CurrentItem}"
                            ShowCategories="True"
                            ShowSearchPanel="True"/>
```

## Custom Property Definitions
```xml
<dxprg:PropertyGridControl SelectedObject="{Binding Settings}">
    <dxprg:PropertyGridControl.PropertyDefinitions>
        <dxprg:PropertyDefinition Path="Name" Category="General"/>
        <dxprg:PropertyDefinition Path="Color" Category="Appearance">
            <dxprg:PropertyDefinition.EditSettings>
                <dxe:ColorEditSettings/>
            </dxprg:PropertyDefinition.EditSettings>
        </dxprg:PropertyDefinition>
    </dxprg:PropertyGridControl.PropertyDefinitions>
</dxprg:PropertyGridControl>
```

## Key Properties
`SelectedObject`, `ShowCategories`, `ShowSearchPanel`, `SortMode` (Ascending/NoSort), `ExpandCategoriesWhenSelectedObjectChanged`.

## Reference
- https://docs.devexpress.com/WPF/15711/controls-and-libraries/property-grid
