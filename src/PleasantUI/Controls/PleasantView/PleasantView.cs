﻿using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

[TemplatePart("PART_ModalWindowsPanel", typeof(Panel))]
[TemplatePart("PART_VisualLayerManager", typeof(VisualLayerManager))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
public class PleasantView : ContentControl, IPleasantWindow
{
    private Panel _modalWindowsPanel = null!;

    /// <inheritdoc />
    public SnackbarQueueManager<PleasantSnackbar> SnackbarQueueManager { get; }

    /// <inheritdoc />
    public AvaloniaList<PleasantModalWindow> ModalWindows { get; } = [];

    /// <inheritdoc />
    public AvaloniaList<Control> Controls { get; } = [];

    /// <inheritdoc />
    public VisualLayerManager VisualLayerManager { get; private set; }
    
    static PleasantView()
    {
        TemplateProperty.OverrideDefaultValue<PleasantView>(new FuncControlTemplate((_, ns) => new Panel
        {
            Children =
            {
                new ContentPresenter
                {
                    Name = "PART_ContentPresenter",
                    [~BackgroundProperty] = new TemplateBinding(BackgroundProperty),
                    [~BorderBrushProperty] = new TemplateBinding(BorderBrushProperty),
                    [~BorderThicknessProperty] = new TemplateBinding(BorderThicknessProperty),
                    [~CornerRadiusProperty] = new TemplateBinding(CornerRadiusProperty),
                    [~ContentTemplateProperty] = new TemplateBinding(ContentTemplateProperty),
                    [~ContentProperty] = new TemplateBinding(ContentProperty),
                    [~PaddingProperty] = new TemplateBinding(PaddingProperty),
                    [~VerticalContentAlignmentProperty] = new TemplateBinding(VerticalContentAlignmentProperty),
                    [~HorizontalContentAlignmentProperty] = new TemplateBinding(HorizontalContentAlignmentProperty)
                },
                new Panel
                {
                    Name = "PART_ModalWindowsPanel"
                }
            }
        }.RegisterInNameScope(ns)));
    }
    
    public PleasantView()
    {
        SnackbarQueueManager = new SnackbarQueueManager<PleasantSnackbar>(this);
    }

    /// <inheritdoc />
    public void AddModalWindow(PleasantModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(modalWindow.ModalBackground);
        windowPanel.Children.Add(modalWindow);

        ModalWindows.Add(modalWindow);

        _modalWindowsPanel.Children.Add(windowPanel);
    }

    /// <inheritdoc />
    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        ModalWindows.Remove(modalWindow);
        _modalWindowsPanel.Children.Remove(modalWindow.Parent as Panel ?? throw new NullReferenceException());
    }

    /// <inheritdoc />
    public void AddControl(Control control)
    {
        Controls.Add(control);
        _modalWindowsPanel.Children.Add(control);
    }

    /// <inheritdoc />
    public void RemoveControl(Control control)
    {
        Controls.Remove(control);
        _modalWindowsPanel.Children.Remove(control);
    }


    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindowsPanel = e.NameScope.Get<Panel>("PART_ModalWindowsPanel");
        VisualLayerManager = e.NameScope.Get<VisualLayerManager>("PART_VisualLayerManager");
    }
}