using Avalonia.Controls;

namespace PleasantUI.Example.Interfaces;

public interface IPage
{
    string Title { get; }
    
    bool ShowTitle { get; }
    
    Control Content { get; }
}