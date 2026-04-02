# PleasantUI.DataGrid

A separate package that applies the PleasantUI Fluent theme to Avalonia's `DataGrid` control. Install it alongside the core package.

## Installation

```xml
<PackageReference Include="pieckenst.Avalonia12.PleasantUI.DataGrid" Version="5.1.0-alpha1" />
```

## Setup

Add `PleasantDataGridTheme` to your app styles **after** `PleasantTheme`:

```xml
<Application.Styles>
    <PleasantTheme />
    <StyleInclude Source="avares://PleasantUI.DataGrid/PleasantDataGridTheme.axaml" />
</Application.Styles>
```

Or in code:

```csharp
Application.Current.Styles.Add(new PleasantDataGridTheme());
```

## Basic usage

```xml
<DataGrid ItemsSource="{Binding Items}"
          AutoGenerateColumns="False"
          CanUserSortColumns="True"
          CanUserReorderColumns="True"
          SelectionMode="Extended">
    <DataGrid.Columns>
        <DataGridTextColumn     Header="Name"   Binding="{Binding Name}"   Width="*" />
        <DataGridTextColumn     Header="Age"    Binding="{Binding Age}"    Width="80" />
        <DataGridCheckBoxColumn Header="Active" Binding="{Binding IsActive}" Width="70" />
    </DataGrid.Columns>
</DataGrid>
```

## Row details

```xml
<DataGrid RowDetailsVisibilityMode="VisibleWhenSelected">
    <DataGrid.RowDetailsTemplate>
        <DataTemplate>
            <Border Padding="16 8" Background="{DynamicResource ControlFillColor1}">
                <TextBlock Text="{Binding Details}" />
            </Border>
        </DataTemplate>
    </DataGrid.RowDetailsTemplate>
    <!-- columns -->
</DataGrid>
```

## Styled elements

The theme provides complete styling for:

- `DataGrid` — main control with scrollbars, disabled overlay, drop indicator
- `DataGridColumnHeader` — with sort icons (ascending/descending), hover/pressed states
- `DataGridRow` — with selection highlight using accent colors, invalid state
- `DataGridCell` — with focus visual, currency indicator, invalid border
- `DataGridRowHeader` — separator brush
- `DataGridRowGroupHeader` — expander button, property name, item count

## Brush resources

These resources can be overridden in your app's `ThemeDictionaries`:

| Key | Default | Description |
|---|---|---|
| `DataGridRowBackgroundBrush` | `Transparent` | Row background |
| `DataGridRowInvalidBrush` | `#33FF0000` | Invalid row overlay |
| `DataGridDetailsPresenterBackgroundBrush` | `Transparent` | Row details background |
| `DataGridDropLocationIndicatorBackground` | `AccentColor` | Column drag indicator |
| `DataGridScrollBarsSeparatorBackground` | `BackgroundColor2` | Corner between scrollbars |
| `DataGridDisabledVisualElementBackground` | `#66FFFFFF` | Disabled overlay |
