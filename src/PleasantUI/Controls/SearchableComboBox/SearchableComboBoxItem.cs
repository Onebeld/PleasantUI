using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// A selectable item in a <see cref="SearchableComboBox"/>.
/// </summary>
public class SearchableComboBoxItem : ListBoxItem
{
    public SearchableComboBoxItem()
    {
        this.GetPropertyChangedObservable(IsFocusedProperty)
            .Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(args =>
            {
                if (args.NewValue is true) 
                    (Parent as SearchableComboBox)?.ItemFocused(this);
            }));
    }

    static SearchableComboBoxItem()
    {
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<SearchableComboBoxItem>(AutomationControlType.ComboBoxItem);
    }

    public override string? ToString()
    {
        return Content?.ToString() ?? base.ToString();
    }
}