using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using PleasantUI.Controls;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Extensions;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Localization;
using PleasantUI.Core.Structures;

namespace PleasantUI.ToolKit;

/// <summary>
/// A control that displays a message box.
/// </summary>
public sealed partial class MessageBox : ContentDialog
{
    private MessageBox() => InitializeComponent();

    /// <summary>
    /// Displays a message box and returns the clicked button's result string.
    /// </summary>
    public static Task<string> Show(
        IPleasantWindow parent,
        string title,
        string text,
        IReadOnlyList<MessageBoxButton>? buttons = null,
        string? additionalText = null,
        MessageBoxStyle style = MessageBoxStyle.Default,
        Control? extraContent = null,
        Action<Control>? onContentReady = null)
    {
        return ShowCore<string>(
            parent, title, text, buttons, additionalText, style, extraContent, onContentReady,
            valueSelector: null);
    }

    /// <summary>
    /// Displays a message box with extra content and returns both the button result and a
    /// value extracted from <paramref name="extraContent"/> via <paramref name="valueSelector"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value to capture from the extra content.</typeparam>
    /// <param name="valueSelector">
    /// Called just before the dialog closes to read state from the extra content control.
    /// </param>
    /// <param name="onContentReady">
    /// Called after the extra content is placed in the dialog but before it is shown.
    /// Use this to wire up event handlers or set initial state on the control.
    /// </param>
    public static Task<MessageBoxResult<T>> Show<T>(
        IPleasantWindow parent,
        string title,
        string text,
        Control extraContent,
        Func<Control, T> valueSelector,
        IReadOnlyList<MessageBoxButton>? buttons = null,
        string? additionalText = null,
        MessageBoxStyle style = MessageBoxStyle.Default,
        Action<Control>? onContentReady = null)
    {
        return ShowCore<MessageBoxResult<T>>(
            parent, title, text, buttons, additionalText, style, extraContent, onContentReady,
            valueSelector: (btn, ctrl) => new MessageBoxResult<T>
            {
                Button = btn,
                Value  = valueSelector(ctrl)
            });
    }

    // ── Core implementation ───────────────────────────────────────────────────

    private static Task<TResult> ShowCore<TResult>(
        IPleasantWindow parent,
        string title,
        string text,
        IReadOnlyList<MessageBoxButton>? buttons,
        string? additionalText,
        MessageBoxStyle style,
        Control? extraContent,
        Action<Control>? onContentReady,
        Func<string, Control?, TResult>? valueSelector)
    {
        MessageBox messageBox = new();

        string titleValue = Localizer.Instance.TryGetString(title, out string rt) ? rt : title;
        string textValue  = Localizer.Instance.TryGetString(text,  out string rx) ? rx : text;

        // ── Header ────────────────────────────────────────────────────────────
        if (style == MessageBoxStyle.Danger)
        {
            messageBox.FindControl<Border>("DangerHeader")!.IsVisible    = true;
            messageBox.FindControl<TextBlock>("DefaultTitle")!.IsVisible = false;
            messageBox.FindControl<TextBlock>("Title")!.Text             = titleValue;
        }
        else
        {
            messageBox.FindControl<TextBlock>("DefaultTitle")!.Text = titleValue;
        }

        messageBox.FindControl<TextBlock>("Text")!.Text = textValue;

        // ── Extra content slot ────────────────────────────────────────────────
        var extraSlot = messageBox.FindControl<ContentPresenter>("ExtraContent")!;
        Control? slottedControl = null;

        if (extraContent is not null)
        {
            slottedControl      = extraContent;
            extraSlot.Content   = extraContent;
            extraSlot.IsVisible = true;
            onContentReady?.Invoke(extraContent);
        }
        else if (!string.IsNullOrWhiteSpace(additionalText))
        {
            var textBox = new TextBox
            {
                Classes       = { "MultilineTextBoxFlat" },
                Text          = additionalText,
                IsReadOnly    = true,
                AcceptsReturn = true,
                TextWrapping  = TextWrapping.NoWrap,
                FontFamily    = new FontFamily("Cascadia Code,Consolas,Menlo,monospace"),
                FontSize      = 12,
                MinHeight     = 100,
                MaxHeight     = 300,
                MaxWidth      = 600,
                [ScrollViewer.HorizontalScrollBarVisibilityProperty] = ScrollBarVisibility.Auto,
                [ScrollViewer.VerticalScrollBarVisibilityProperty]   = ScrollBarVisibility.Auto,
            };
            slottedControl      = textBox;
            extraSlot.Content   = textBox;
            extraSlot.IsVisible = true;
        }

        // ── Buttons ───────────────────────────────────────────────────────────
        var buttonsPanel = messageBox.FindControl<UniformGrid>("Buttons")!;
        string result = "OK";

        void AddButton(MessageBoxButton mb)
        {
            string value;
            if (Application.Current!.TryFindResource(mb.Text, out object? res))
                value = res as string ?? string.Empty;
            else
                value = mb.Text;

            Button button = new()
            {
                Content           = value,
                Margin            = Thickness.Parse("5"),
                VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (_, _) =>
            {
                result = mb.Result;
                _ = messageBox.CloseAsync();
            };
            buttonsPanel.Children.Add(button);

            if (mb.IsKeyDown)
                messageBox.KeyDown += (_, e) =>
                {
                    if (e.Key != Key.Enter) return;
                    result = mb.Result;
                    _ = messageBox.CloseAsync();
                };

            if (!mb.Default) return;

            result = mb.Result;

            // Theme priority: explicit ThemeKey > style-based default > accent
            if (mb.ThemeKey is not null)
                button.Theme = ResourceExtensions.GetResource<ControlTheme>(mb.ThemeKey);
            else if (style == MessageBoxStyle.Danger)
                button.Theme = ResourceExtensions.GetResource<ControlTheme>("DangerButtonTheme");
            else
            {
                button.Classes.Add("Accent");
                button.Theme = ResourceExtensions.GetResource<ControlTheme>("AccentButtonTheme");
            }
        }

        if (buttons is null || buttons.Count == 0)
        {
            buttonsPanel.Columns = 2;
            buttonsPanel.Children.Add(new Panel());
            try
            {
                AddButton(new MessageBoxButton { Text = Localizer.Instance["Ok"], Result = "OK", Default = true, IsKeyDown = true });
            }
            catch
            {
                AddButton(new MessageBoxButton { Text = "OK", Result = "OK", Default = true, IsKeyDown = true });
            }
        }
        else if (buttons.Count == 1)
        {
            buttonsPanel.Columns = 2;
            buttonsPanel.Children.Add(new Panel());
            AddButton(buttons[0]);
        }
        else
        {
            buttonsPanel.Columns = buttons.Count;
            foreach (MessageBoxButton btn in buttons)
                AddButton(btn);
        }

        // ── Result ────────────────────────────────────────────────────────────
        var tcs = new TaskCompletionSource<TResult>();

        messageBox.Closed += (_, _) =>
        {
            TResult finalResult = valueSelector is not null
                ? valueSelector(result, slottedControl!)
                : (TResult)(object)result;   // plain string overload

            tcs.TrySetResult(finalResult);
        };

        messageBox.ShowAsync(parent);
        return tcs.Task;
    }
}
