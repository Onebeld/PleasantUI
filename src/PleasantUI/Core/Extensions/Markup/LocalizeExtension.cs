using Avalonia.Data;
using Avalonia.Markup.Xaml;
using PleasantUI.Converters;
using PleasantUI.Core.Localization;

namespace PleasantUI.Core.Extensions.Markup;

/// <summary>
/// A markup extension that provides localized strings.
/// </summary>
public class LocalizeExtension : MarkupExtension
{
    private readonly BindingBase[]? _bindings;

    /// <summary>
    /// Gets or sets the key of the localized string.
    /// </summary>
    public object Key { get; set; }

    /// <summary>
    /// Gets or sets the context of the localized string.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Gets or sets the default value to return if the localized string is not found.
    /// </summary>
    public string? Default { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is a menu item.
    /// </summary>
    /// <remarks>
    /// When this property is set to <see langword="true" />, the localized string will have an underscore character,  
    /// allowing you to navigate the menu using the keyboard.
    /// </remarks>
    public bool MenuBar { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension" /> class with the specified key.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    public LocalizeExtension(object key)
    {
        Key = key;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension" /> class with the specified key and a binding.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    /// <param name="binding">The binding to use for string formatting.</param>
    public LocalizeExtension(object key, BindingBase binding) : this(key)
    {
        _bindings = [binding];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension" /> class with the specified key and two bindings.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    /// <param name="binding1">The first binding to use for string formatting.</param>
    /// <param name="binding2">The second binding to use for string formatting.</param>
    public LocalizeExtension(object key, BindingBase binding1, BindingBase binding2) : this(key)
    {
        _bindings = [binding1, binding2];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension" /> class with the specified key and three bindings.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    /// <param name="binding1">The first binding to use for string formatting.</param>
    /// <param name="binding2">The second binding to use for string formatting.</param>
    /// <param name="binding3">The third binding to use for string formatting.</param>
    public LocalizeExtension(object key, BindingBase binding1, BindingBase binding2, BindingBase binding3) : this(key)
    {
        _bindings = [binding1, binding2, binding3];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension" /> class with the specified key and four bindings.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    /// <param name="binding1">The first binding to use for string formatting.</param>
    /// <param name="binding2">The second binding to use for string formatting.</param>
    /// <param name="binding3">The third binding to use for string formatting.</param>
    /// <param name="binding4">The fourth binding to use for string formatting.</param>
    public LocalizeExtension(object key, BindingBase binding1, BindingBase binding2, BindingBase binding3,
        BindingBase binding4) : this(key)
    {
        _bindings = [binding1, binding2, binding3, binding4];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension" /> class with the specified key and five bindings.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    /// <param name="binding1">The first binding to use for string formatting.</param>
    /// <param name="binding2">The second binding to use for string formatting.</param>
    /// <param name="binding3">The third binding to use for string formatting.</param>
    /// <param name="binding4">The fourth binding to use for string formatting.</param>
    /// <param name="binding5">The fifth binding to use for string formatting.</param>
    public LocalizeExtension(object key, BindingBase binding1, BindingBase binding2, BindingBase binding3,
        BindingBase binding4, BindingBase binding5) : this(key)
    {
        _bindings = [binding1, binding2, binding3, binding4, binding5];
    }

    /// <summary>
    /// Provides the localized string.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The localized string.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Key is string key)
        {
            if (!string.IsNullOrWhiteSpace(Context))
                key = $"{Context}/{Key}";

            string resolvedKey = key;
            bool menuBar = MenuBar;
            string? defaultVal = Default;

            // Build a resolver that applies menu bar prefix and default fallback
            string Resolve()
            {
                if (Localizer.Instance.TryGetString(resolvedKey, out string expression))
                    return menuBar ? "_" + expression : expression;
                if (!string.IsNullOrWhiteSpace(defaultVal))
                    return menuBar ? "_" + defaultVal : defaultVal;
                return expression;
            }

            // LocalizeKeyObservable fires PropertyChanged("Value") + ("") + ("Item") on every
            // language change, so both reflection and compiled bindings pick it up.
            var observable = new LocalizeKeyObservable(Resolve);

            // Use a reflection Binding — reliable for non-AvaloniaObject INPC sources
            var binding = new Binding
            {
                Source = observable,
                Path   = nameof(LocalizeKeyObservable.Value),
                Mode   = BindingMode.OneWay
            };

            if (_bindings is null || _bindings.Length <= 0)
                return binding;

            BindingBase[] bindingBases = GetBindings(binding);
            return new MultiBinding
            {
                Bindings  = bindingBases,
                Converter = new TranslateConverter()
            };
        }
        else if (Key is BindingBase binding)
        {
            BindingBase[] bindingBases = GetBindings(binding);
            return new MultiBinding
            {
                Bindings  = bindingBases,
                Converter = new BindingTranslateConverter(Context)
            };
        }

        throw new NotSupportedException("Key must be a string or BindingBase");
    }

    private BindingBase[] GetBindings(BindingBase binding)
    {
        if (_bindings is null || _bindings.Length <= 0)
            return [binding];

        BindingBase[] bindingBases = new BindingBase[_bindings.Length + 1];

        bindingBases[0] = binding;

        for (int i = 0; i < _bindings.Length; i++)
            bindingBases[i + 1] = _bindings[i];

        return bindingBases;
    }
}