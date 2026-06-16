using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace PleasantUI.Core.DataTemplates;


public class IconTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();
    
    // Метод Match вызывается Avalonia автоматически для каждого шаблона в коллекции.
    // Мы просто используем базовую проверку типов, которая уже заложена в DataTemplates.
    public bool Match(object? data)
    {
        if (data == null) return false;

        // Проверяем, есть ли внутри нашей коллекции шаблон, 
        // у которого DataType совпадает с типом пришедшего Enum (или объекта)
        foreach (var template in AvailableTemplates.Values)
        {
            if (template.Match(data))
            {
                return true;
            }
        }

        return false;
    }

    public Control? Build(object? param)
    {
        if (param == null) return null;
        
        var element = AvailableTemplates.Values.FirstOrDefault(item => item.Match(param));

        // Извлекаем объект и приводим его к IDataTemplate во время выполнения
        if (element != null)
        {
            return element.Build(param);
        }

        return new TextBlock { Text = param.ToString() };
    }
}