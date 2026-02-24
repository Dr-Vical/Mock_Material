# DevExpress WPF Data Editors

## Overview
- Editor controls for text, numbers, dates, selection, images, colors.
- Standalone use and inline editors within GridControl/TreeListControl.
- Mask input, validation, value formatting support.

## Key Classes
| Class | Description |
|-------|-------------|
| `TextEdit` | Text input |
| `SpinEdit` | Numeric input (spin buttons) |
| `DateEdit` | Date/time picker |
| `ComboBoxEdit` | Dropdown selection |
| `CheckEdit` | Checkbox (boolean) |
| `LookUpEdit` | Grid-style dropdown lookup |
| `ListBoxEdit` | Listbox |
| `ButtonEdit` | Text with button |
| `MemoEdit` | Multiline text |
| `PasswordBoxEdit` | Password input |
| `ColorEdit` | Color picker |
| `TrackBarEdit` | Slider |
| `ProgressBarEdit` | Progress bar |

- **xmlns**: `dxe="http://schemas.devexpress.com/winfx/2008/xaml/editors"`
- **NuGet**: `DevExpress.Wpf.Core`

## Basic Usage
```xml
<dxe:TextEdit EditValue="{Binding Name, Mode=TwoWay}"
              NullText="Enter name..." NullValueButtonPlacement="EditBox"/>

<dxe:SpinEdit EditValue="{Binding Quantity, Mode=TwoWay}"
              MinValue="0" MaxValue="9999" Increment="1" IsFloatValue="False"/>

<dxe:DateEdit EditValue="{Binding BirthDate, Mode=TwoWay}"
              DisplayFormatString="yyyy-MM-dd"/>

<dxe:ComboBoxEdit ItemsSource="{Binding Categories}"
                   EditValue="{Binding SelectedCategory, Mode=TwoWay}"
                   DisplayMember="Name" ValueMember="Id"
                   IsTextEditable="False"/>

<dxe:CheckEdit IsChecked="{Binding IsActive, Mode=TwoWay}" Content="Active"/>
```

## Mask Input
```xml
<dxe:TextEdit MaskType="RegEx" Mask="\d{3}-\d{4}-\d{4}"
              MaskUseAsDisplayFormat="True"/>

<dxe:TextEdit MaskType="Numeric" Mask="n2"
              MaskUseAsDisplayFormat="True"/>
```

## LookUpEdit (grid dropdown)
```xml
<dxe:LookUpEdit ItemsSource="{Binding Products}"
                 EditValue="{Binding SelectedProductId, Mode=TwoWay}"
                 DisplayMember="ProductName" ValueMember="Id"
                 AutoPopulateColumns="False">
    <dxe:LookUpEdit.PopupContentTemplate>
        <ControlTemplate>
            <dxg:GridControl>
                <dxg:GridControl.Columns>
                    <dxg:GridColumn FieldName="ProductName"/>
                    <dxg:GridColumn FieldName="Price"/>
                </dxg:GridControl.Columns>
            </dxg:GridControl>
        </ControlTemplate>
    </dxe:LookUpEdit.PopupContentTemplate>
</dxe:LookUpEdit>
```

## Validation
```xml
<dxe:TextEdit EditValue="{Binding Email, Mode=TwoWay, ValidatesOnDataErrors=True}"
              InvalidValueBehavior="WaitForValidValue"/>
```

## Key Properties (common)
`EditValue`, `NullText`, `NullValueButtonPlacement`, `DisplayFormatString`, `MaskType`, `Mask`, `IsReadOnly`, `ShowBorder`.

## Reference
- https://docs.devexpress.com/WPF/6187/controls-and-libraries/data-editors
