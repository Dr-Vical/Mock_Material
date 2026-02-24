# DevExpress WPF Navigation Controls

## Overview
- AccordionControl (recommended), NavBarControl (legacy), TileBar, TreeViewControl, HamburgerMenu, WizardControl.

## Key Classes
| Class | Description |
|-------|-------------|
| `AccordionControl` | Accordion navigation |
| `AccordionItem` | Accordion item |
| `NavBarControl` | Outlook-style nav bar |
| `HamburgerMenu` | Adaptive side navigation |
| `WizardControl` | Step-by-step wizard |
| `TreeViewControl` | Hierarchical tree view |

- **xmlns (accordion)**: `dxnav="http://schemas.devexpress.com/winfx/2008/xaml/accordion"`
- **NuGet**: `DevExpress.Wpf.Accordion`

## AccordionControl
```xml
<dxnav:AccordionControl ItemsSource="{Binding MenuItems}"
                          SelectedItem="{Binding SelectedMenuItem}">
    <dxnav:AccordionControl.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding Name}"/>
        </HierarchicalDataTemplate>
    </dxnav:AccordionControl.ItemTemplate>
</dxnav:AccordionControl>
```

## HamburgerMenu
```xml
<dx:HamburgerMenu ItemsSource="{Binding MenuItems}"
                    SelectedItem="{Binding SelectedItem}"
                    Content="{Binding SelectedContent}">
    <dx:HamburgerMenu.ItemTemplate>
        <DataTemplate>
            <dx:HamburgerMenuIconBarItem Content="{Binding Name}"
                Glyph="{Binding Icon}"/>
        </DataTemplate>
    </dx:HamburgerMenu.ItemTemplate>
</dx:HamburgerMenu>
```

## WizardControl
```xml
<dx:WizardControl FinishCommand="{Binding CompleteCommand}">
    <dx:WelcomeWizardPage Header="Welcome"
        Description="Setup wizard introduction"/>
    <dx:WizardPage Header="Settings"
        AllowNext="{Binding IsSettingsValid}">
        <dxlc:LayoutControl>
            <!-- settings form -->
        </dxlc:LayoutControl>
    </dx:WizardPage>
    <dx:CompletionWizardPage Header="Complete"
        Description="Setup is complete"/>
</dx:WizardControl>
```

## Reference
- https://docs.devexpress.com/WPF/116553/controls-and-libraries/navigation
