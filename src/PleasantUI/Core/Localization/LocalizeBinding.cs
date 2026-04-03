using Avalonia.Data;

namespace PleasantUI.Core.Localization;

/// <summary>
/// Creates one-way Avalonia bindings to localized strings for use in code-behind.
/// This is the programmatic equivalent of <c>{Localize ...}</c>.
/// </summary>
public static class LocalizeBinding
{
    /// <summary>
    /// Creates a one-way binding that updates whenever the app language changes.
    /// </summary>
    public static Binding Create(string key, string? context = null, string? @default = null, bool menuBar = false)
    {
        if (!string.IsNullOrWhiteSpace(context))
            key = $"{context}/{key}";

        string resolvedKey = key;
        string? defaultVal = @default;
        bool menu = menuBar;

        string Resolve()
        {
            if (Localizer.Instance.TryGetString(resolvedKey, out string expression))
                return menu ? "_" + expression : expression;

            if (!string.IsNullOrWhiteSpace(defaultVal))
                return menu ? "_" + defaultVal : defaultVal;

            return expression;
        }

        var observable = new LocalizeKeyObservable(Resolve);
        return new Binding
        {
            Source = observable,
            Path = nameof(LocalizeKeyObservable.Value),
            Mode = BindingMode.OneWay,
            FallbackValue = resolvedKey,
            TargetNullValue = resolvedKey
        };
    }
}

