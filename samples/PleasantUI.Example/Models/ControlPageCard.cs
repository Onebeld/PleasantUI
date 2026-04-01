using System.ComponentModel;
using Avalonia.Media;
using PleasantUI.Core.Localization;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Messages;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.Example.Models;

public class ControlPageCard : INotifyPropertyChanged
{
    private readonly IEventAggregator _eventAggregator;
    private readonly string _descriptionKey;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; set; }
    public Geometry Icon { get; set; }
    public IPage Page { get; set; }

    /// <summary>
    /// Resolved description — re-evaluated whenever the language changes.
    /// </summary>
    public string Description =>
        Localizer.Instance.TryGetString(_descriptionKey, out string value) ? value : _descriptionKey;

    public ControlPageCard(string title, Geometry icon, string descriptionKey, IPage page, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _descriptionKey = descriptionKey;

        Title = title;
        Icon = icon;
        Page = page;

        Localizer.Instance.LocalizationChanged += _ =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
    }

    public void OpenPage()
    {
        _eventAggregator.PublishAsync(new ChangePageMessage(Page));
    }
}