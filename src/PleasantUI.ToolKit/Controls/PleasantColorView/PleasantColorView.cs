using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Core;

namespace PleasantUI.ToolKit.Controls.PleasantColorView;

/// <summary>
/// Represents a view for displaying a pleasant color and providing color manipulation actions.
/// </summary>
[TemplatePart("DeleteColor", typeof(MenuItem))]
[TemplatePart("AddColorButton", typeof(Button))]
public class PleasantColorView : ColorView
{
    /// <inheritdoc cref="StyleKeyOverride" />
    protected override Type StyleKeyOverride => typeof(ColorView);

    /// <inheritdoc cref="OnApplyTemplate" />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        MenuItem? menuItem = e.NameScope.Find<MenuItem>("DeleteColor");
        Button? button = e.NameScope.Find<Button>("AddColorButton");

        if (menuItem is not null)
            menuItem.Click += DeleteColor;

        if (button is not null)
            button.Click += AddColor;
    }

    private void DeleteColor(object? sender, RoutedEventArgs e)
    {
        uint color = HsvColor.ToRgb().ToUInt32();

        if (PleasantSettings.Current is not null)
        {
            PleasantSettings.Current.ColorPalettes.Remove(color);
        }

        Color = Avalonia.Media.Color.FromUInt32(color);
    }

    private void AddColor(object? sender, RoutedEventArgs e)
    {
        uint color = HsvColor.ToRgb().ToUInt32();

        if (PleasantSettings.Current is not null && !PleasantSettings.Current.ColorPalettes.Contains(color))
        {
            PleasantSettings.Current.ColorPalettes.Add(color);
        }
    }
}