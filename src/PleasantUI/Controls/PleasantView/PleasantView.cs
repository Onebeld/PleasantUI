using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// A content control that serves as the main view container for PleasantUI applications, 
/// supporting modal windows and a snackbar queue manager.
/// </summary>
[TemplatePart("PART_VisualLayerManager", typeof(VisualLayerManager))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
public class PleasantView : ContentControl, IPleasantWindow
{
    /// <inheritdoc />
    public SnackbarQueueManager<PleasantSnackbar> SnackbarQueueManager { get; } = new();

    /// <inheritdoc />
    public AvaloniaList<PleasantPopupElement> ModalWindows { get; } = [];

    static PleasantView()
    {
        TemplateProperty.OverrideDefaultValue<PleasantView>(new FuncControlTemplate((_, ns) => new ContentPresenter
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
        }.RegisterInNameScope(ns)));
    }
}