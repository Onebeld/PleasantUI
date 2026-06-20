using System.Runtime.Serialization;
using PleasantUI.Core;

namespace PleasantUI.Example;

public class AppSettings : ViewModelBase
{
    /// <summary>
    /// Gets the singleton instance of the AppSettings class.
    /// </summary>
    public static AppSettings? Current { get; set; }
    
    /// <summary>
    /// Gets or sets the current language code (e.g., "en", "ru").
    /// </summary>
    [DataMember]
    public string Language
    {
        get;
        set => SetProperty(ref field, value);
    } = "en";
}