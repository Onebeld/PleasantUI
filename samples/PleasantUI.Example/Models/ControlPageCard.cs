using System.ComponentModel;
using Material.Icons;
using PleasantUI.Core.Localization;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Messages;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.Example.Models;

public class ControlPageCard : INotifyPropertyChanged
{
    private readonly IEventAggregator _eventAggregator;
    private string _title;
    private string _description;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string TitleKey { get; }
    public string DescriptionKey { get; }
    public MaterialIconKind Icon { get; set; }
    public Func<IPage> Page { get; set; }

    public string Title
    {
        get => _title;
        private set
        {
            if (_title == value) return;
            _title = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        }
    }

    public string Description
    {
        get => _description;
        private set
        {
            if (_description == value) return;
            _description = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
        }
    }

    public ControlPageCard(string titleKey, MaterialIconKind icon, string descriptionKey, Func<IPage> page, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        TitleKey = titleKey;
        DescriptionKey = descriptionKey;
        Icon = icon;
        Page = page;

        // Read translated values immediately — satellite DLL must already be loaded by now
        _title = Resolve(titleKey);
        _description = Resolve(descriptionKey);

        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string lang)
    {
        void Update()
        {
            Title = Resolve(TitleKey);
            Description = Resolve(DescriptionKey);
        }

        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            Update();
        else
            Avalonia.Threading.Dispatcher.UIThread.Post(Update);
    }

    private static string Resolve(string key) =>
        Localizer.Instance.TryGetString(key, out string value) ? value : key;

    public void OpenPage() =>
        _eventAggregator.PublishAsync(new ChangePageMessage(Page.Invoke()));
}
