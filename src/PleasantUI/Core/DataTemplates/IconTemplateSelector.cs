using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using PleasantUI.Controls;
// ReSharper disable CollectionNeverUpdated.Global

namespace PleasantUI.Core.DataTemplates;

/// <summary>
/// Template creation class for <see cref="PleasantIcon"/>
/// </summary>
public class IconTemplateSelector : IDataTemplate
{
    /// <summary>
    /// Gets a dictionary of all available templates for an icon.
    /// </summary>
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new();
    
    /// <inheritdoc />
    public bool Match(object? data)
    {
        if (data == null) return false;

        if (AvailableTemplates.Values.Any(template => template.Match(data)))
            return true;

        return data is Control;
    }

    /// <inheritdoc />
    public Control? Build(object? param)
    {
        if (param == null) return null;
        
        IDataTemplate? element = AvailableTemplates.Values.FirstOrDefault(item => item.Match(param));

        if (element != null)
            return element.Build(param);

        if (param is Control control)
            return control;

        return new TextBlock { Text = param.ToString() };
    }
}