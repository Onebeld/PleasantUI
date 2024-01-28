using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Structures;

namespace PleasantUI.Windows;

public partial class MessageBox : ContentDialog
{
    public MessageBox() => InitializeComponent();

    /// <summary>
    /// Displays a message box with the given title and text, and optional buttons and additional text.
    /// </summary>
    /// <param name="parent">The parent window.</param>
    /// <param name="title">The title of the message box.</param>
    /// <param name="text">The text of the message box.</param>
    /// <param name="buttons">The optional buttons to display in the message box.</param>
    /// <param name="additionalText">The optional additional text to display in the message box.</param>
    /// <returns>A Task representing the completion of the message box.</returns>
    public static Task<string> Show(IPleasantWindow parent, string title, string text,
                                    IReadOnlyList<MessageBoxButton>? buttons = null, string? additionalText = null)
    {
        MessageBox messageBox = new();

        string titleValue, textValue;

        if (Application.Current!.TryFindResource(title, out object? objectTitleValue))
            titleValue = objectTitleValue as string ?? string.Empty;
        else titleValue = title;
        
        if (Application.Current!.TryFindResource(text, out object? objectTextValue))
            textValue = objectTextValue as string ?? string.Empty;
        else textValue = text;

        messageBox.Title.Text = titleValue;
        messageBox.Text.Text = textValue;

        string result = "OK";

        void AddButton(MessageBoxButton messageBoxButton)
        {
            string value;

            if (Application.Current!.TryFindResource(messageBoxButton.Text, out object? objectText))
                value = objectText as string ?? string.Empty;
            else value = messageBoxButton.Text;

            Button button = new()
            {
                Content = value, Margin = Thickness.Parse("5"), VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (_, _) =>
            {
                result = messageBoxButton.Result;
                messageBox.Close();
            };
            messageBox.Buttons.Children.Add(button);

            if (messageBoxButton.IsKeyDown)
            {
                messageBox.KeyDown += (_, e) =>
                {
                    if (e.Key != Key.Enter) return;

                    result = messageBoxButton.Result;
                    messageBox.Close();
                };
            }

            if (!messageBoxButton.Default) return;
            
            result = messageBoxButton.Result;
            button.Classes.Add("Accent");

            if (Application.Current != null)
                button.Theme = (ControlTheme)Application.Current.FindResource("AccentButtonTheme")!;
        }

        if (buttons is null || buttons.Count == 0)
        {
            messageBox.Buttons.Columns = 2;
            messageBox.Buttons.Children.Add(new Panel());

            try
            {
                AddButton(new MessageBoxButton
                {
                    Text = (string)Application.Current?.FindResource("Ok") ?? "OK", Result = "OK", Default = true, IsKeyDown = true
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
            messageBox.Buttons.Columns = 2;
            messageBox.Buttons.Children.Add(new Panel());

            AddButton(buttons[0]);
        }
        else
        {
            messageBox.Buttons.Columns = buttons.Count;

            foreach (MessageBoxButton messageBoxButton in buttons)
                AddButton(messageBoxButton);
        }

        if (!string.IsNullOrWhiteSpace(additionalText))
            messageBox.AdditionalText.Text = additionalText;
        else messageBox.AdditionalText.IsVisible = false;

        TaskCompletionSource<string> taskCompletionSource = new();
        
        messageBox.Closed += (_, _) => taskCompletionSource.TrySetResult(result);
        messageBox.Show(parent);

        return taskCompletionSource.Task;
    }
}