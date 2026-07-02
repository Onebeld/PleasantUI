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
    public static BindingBase Create(object? key, string? context = null, string? @default = null, bool menuBar = false)
    {
        if (key is Enum)
            key = Localizer.UnsanitizeIdentifier(key.ToString());
        
        if (!string.IsNullOrWhiteSpace(context))
            key = $"{context}/{key}";

        string resolvedKey = key?.ToString() ??  string.Empty;
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

        LocalizeKeyObservable observable = new(Resolve);
        
        return CompiledBinding.Create(
            (LocalizeKeyObservable value) => value.Value,
            observable,
            mode: BindingMode.OneWay,
            fallbackValue: resolvedKey,
            targetNullValue: resolvedKey);
    }
}

