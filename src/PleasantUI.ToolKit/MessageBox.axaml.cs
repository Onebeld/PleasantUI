using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
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
    /// Displays a message box with the given title and text, and optional buttons, additional text, and visual style.
    /// </summary>
    /// <param name="parent">The parent window.</param>
    /// <param name="title">The title of the message box.</param>
    /// <param name="text">The text of the message box.</param>
    /// <param name="buttons">The optional buttons to display in the message box.</param>
    /// <param name="additionalText">The optional additional text to display in the message box.</param>
    /// <param name="style">Controls the visual appearance — use <see cref="MessageBoxStyle.Danger"/> for destructive actions.</param>
    /// <returns>A Task that resolves to the Result string of the button that was clicked.</returns>
    public static Task<string> Show(
        IPleasantWindow parent,
        string title,
        string text,
        IReadOnlyList<MessageBoxButton>? buttons = null,
        string? additionalText = null,
        MessageBoxStyle style = MessageBoxStyle.Default)
    {
        MessageBox messageBox = new();

        string titleValue = Localizer.Instance.TryGetString(title, out string resolvedTitle) ? resolvedTitle : title;
        string textValue  = Localizer.Instance.TryGetString(text,  out string resolvedText)  ? resolvedText  : text;

        // Apply danger style — red header with icon, hide the default plain title
        if (style == MessageBoxStyle.Danger)
        {
            messageBox.FindControl<Border>("DangerHeader")!.IsVisible     = true;
            messageBox.FindControl<TextBlock>("DefaultTitle")!.IsVisible  = false;
            messageBox.FindControl<TextBlock>("Title")!.Text              = titleValue;
        }
        else
        {
            messageBox.FindControl<TextBlock>("DefaultTitle")!.Text = titleValue;
        }

        messageBox.FindControl<TextBlock>("Text")!.Text = textValue;

        var buttonsPanel = messageBox.FindControl<UniformGrid>("Buttons")!;
        string result = "OK";

        void AddButton(MessageBoxButton messageBoxButton)
        {
            // Original: try resource dictionary first, then fall back to raw text
            string value;
            if (Application.Current!.TryFindResource(messageBoxButton.Text, out object? objectText))
                value = objectText as string ?? string.Empty;
            else
                value = messageBoxButton.Text;

            Button button = new()
            {
                Content = value,
                Margin = Thickness.Parse("5"),
                VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (_, _) =>
            {
                result = messageBoxButton.Result;
                _ = messageBox.CloseAsync();
            };
            buttonsPanel.Children.Add(button);

            if (messageBoxButton.IsKeyDown)
                messageBox.KeyDown += (_, e) =>
                {
                    if (e.Key != Key.Enter) return;
                    result = messageBoxButton.Result;
                    _ = messageBox.CloseAsync();
                };

            if (!messageBoxButton.Default) return;

            result = messageBoxButton.Result;

            // Danger style: destructive default button gets red theme
            // Default style: accent (original behaviour preserved)
            if (style == MessageBoxStyle.Danger)
            {
                button.Theme = ResourceExtensions.GetResource<ControlTheme>("DangerButtonTheme");
            }
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

            // Original: try/catch fallback for missing localization key
            try
            {
                AddButton(new MessageBoxButton
                {
                    Text = Localizer.Instance["Ok"], Result = "OK", Default = true, IsKeyDown = true
                });
            }
            catch
            {
                AddButton(new MessageBoxButton
                {
                    Text = "OK", Result = "OK", Default = true, IsKeyDown = true
                });
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

        var additionalTextBox = messageBox.FindControl<TextBox>("AdditionalText")!;
        if (!string.IsNullOrWhiteSpace(additionalText))
            additionalTextBox.Text = additionalText;
        else
            additionalTextBox.IsVisible = false;

        TaskCompletionSource<string> taskCompletionSource = new();
        messageBox.Closed += (_, _) => taskCompletionSource.TrySetResult(result);
        messageBox.ShowAsync(parent);

        return taskCompletionSource.Task;
    }
}
