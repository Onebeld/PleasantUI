using System.ComponentModel;
using Avalonia.Controls;

namespace PleasantUI.Example.Interfaces;

public interface IPage : INotifyPropertyChanged
{
    /// <summary>The localization key used to resolve <see cref="Title"/>.</summary>
    string TitleKey { get; }

    /// <summary>Resolved, localized title — updates when the language changes.</summary>
    string Title { get; }

    bool ShowTitle { get; }

    Control Content { get; }
}
