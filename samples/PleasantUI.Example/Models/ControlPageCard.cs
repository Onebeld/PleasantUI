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

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The localization key for the title.
    /// </summary>
    public string TitleKey { get; }

    /// <summary>
    /// The localization key for the description.
    /// </summary>
    public string DescriptionKey { get; }

    /// <summary>
    /// Resolved title — re-evaluated whenever the language changes.
    /// </summary>
    public string Title =>
        Localizer.Instance.TryGetString(TitleKey, out string value) ? value : TitleKey;

    public Geometry Icon { get; set; }
    public IPage Page { get; set; }

    /// <summary>
    /// Resolved description — re-evaluated whenever the language changes.
    /// </summary>
    public string Description =>
        Localizer.Instance.TryGetString(DescriptionKey, out string value) ? value : DescriptionKey;

    public ControlPageCard(string titleKey, Geometry icon, string descriptionKey, IPage page, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        TitleKey = titleKey;
        DescriptionKey = descriptionKey;

        Icon = icon;
        Page = page;

        Localizer.Instance.LocalizationChanged += _ =>
        {
            // Marshal to UI thread to ensure bindings update properly
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
                });
            }
        };
    }

    public void OpenPage()
    {
        _eventAggregator.PublishAsync(new ChangePageMessage(Page));
    }
}