using PleasantUI.Core.Structures;

namespace PleasantUI.Example;

public static class MessageBoxButtons
{
    public static readonly IReadOnlyList<MessageBoxButton> YesNo = new []
    {
        new MessageBoxButton
        {
            Text = App.GetString("Yes"), Default = true, Result = "Yes", IsKeyDown = true
        },
        new MessageBoxButton
        {
            Text = App.GetString("No"), Result = "No"
        }
    };

    public static readonly IReadOnlyList<MessageBoxButton> YesNoCancel = new []
    {
        new MessageBoxButton
        {
            Text = "Yes", Default = true, Result = "Yes", IsKeyDown = true
        },
        new MessageBoxButton
        {
            Text = "No", Result = "No"
        },
        new MessageBoxButton
        {
            Text = "Cancel", Result = "Cancel"
        }
    };

    public static readonly IReadOnlyList<MessageBoxButton> Ok = new[]
    {
        new MessageBoxButton
        {
            Text = "Ok", Default = true, Result = "Ok", IsKeyDown = true
        },
    };
}