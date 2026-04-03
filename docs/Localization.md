# Localization

PleasantUI ships a reactive localization system built on .NET `ResourceManager`. Switching language at runtime updates every bound string instantly without reloading views.

## Setup

Create `.resx` files for each language:

```
Properties/Localizations/App.resx       ← default (English)
Properties/Localizations/App.ru.resx    ← Russian
Properties/Localizations/App.de.resx    ← German
```

Register the resource manager in your `Application` constructor:

```csharp
public App()
{
    Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.App)));
    Localizer.ChangeLang("en"); // set initial language
}
```

## AXAML binding

Use `{Localize Key}` — updates live when the language changes:

```xml
<TextBlock Text="{Localize WelcomeMessage}" />
<Button Content="{Localize SaveButton}" />
<TextBox PlaceholderText="{Localize SearchPlaceholder}" />
```

### Context prefix

Group related keys with a context prefix to avoid collisions:

```xml
<!-- Looks up "Settings/Theme" -->
<TextBlock Text="{Localize Theme, Context=Settings}" />
```

In the `.resx` file the key is `Settings/Theme`.

### Dynamic binding (e.g. inside a DataTemplate)

```xml
<TextBlock Text="{Localize {CompiledBinding Name}, Context=Theme}" />
```

### Default fallback

```xml
<TextBlock Text="{Localize NoCustomThemes, Default='No custom themes'}" />
```

## Code-behind

```csharp
// Simple lookup — throws-style error string on missing key
string text = Localizer.Instance["WelcomeMessage"];

// Safe lookup with fallback
string title = Localizer.TrDefault("DialogTitle", "Confirm");

// With context
string label = Localizer.TrDefault("Save", "Save", "Settings");

// With format args
string msg = Localizer.Tr("ItemCount", context: null, args: 42);
// resx value: "Found {0} items"  →  "Found 42 items"

// Try-get pattern
if (Localizer.Instance.TryGetString("MyKey", out string value))
    DoSomething(value);
```

## Switching language at runtime

```csharp
Localizer.ChangeLang("ru");
```

All `{Localize}` bindings update immediately. Subscribe to the event if you need to react in code:

```csharp
Localizer.Instance.LocalizationChanged += lang =>
{
    Console.WriteLine($"Language changed to: {lang}");
};
```

## Multiple resource files

You can register multiple resource managers — lookups cascade through all of them in registration order:

```csharp
Localizer.AddRes(new ResourceManager(typeof(App)));          // app strings
Localizer.AddRes(new ResourceManager(typeof(LibraryStrings))); // library strings
```

## Newline in values

Use `\\n` in the `.resx` value — it is automatically converted to `\n` at runtime:

```xml
<data name="MultiLineMessage">
    <value>Line one\\nLine two</value>
</data>
```
