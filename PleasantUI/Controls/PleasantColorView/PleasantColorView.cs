﻿using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Core;

namespace PleasantUI.Controls;

public class PleasantColorView : ColorView
{
    protected override Type StyleKeyOverride => typeof(ColorView);

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
        PleasantSettings.Instance.SavedColorPalette.Remove(color);
        
        Color = Avalonia.Media.Color.FromUInt32(color);
    }
    
    private void AddColor(object? sender, RoutedEventArgs e)
    {
        uint color = HsvColor.ToRgb().ToUInt32();
        if (PleasantSettings.Instance.SavedColorPalette.Contains(color)) return;

        PleasantSettings.Instance.SavedColorPalette.Add(color);
    }
}