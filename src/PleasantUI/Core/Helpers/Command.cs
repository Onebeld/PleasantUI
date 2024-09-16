using System.Windows.Input;

namespace PleasantUI.Core.Helpers;

/// <summary>
/// Represents a generic command that can execute an action or asynchronous function.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public class Command<T> : Command, ICommand
{
    private readonly Func<T?, Task>? _acb;
    private readonly Action<T?>? _action;

    private bool _busy;

    /// <summary>
    /// Initializes a new instance of the <see cref="Command{T}" /> class with an action.
    /// </summary>
    /// <param name="action">The action to execute when the command is invoked.</param>
    public Command(Action<T?> action)
    {
        _action = action;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command{T}" /> class with an asynchronous function.
    /// </summary>
    /// <param name="acb">The asynchronous function to execute when the command is invoked.</param>
    public Command(Func<T?, Task> acb)
    {
        _acb = acb;
    }

    /// <inheritdoc />
    public override event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public override bool CanExecute(object? parameter)
    {
        return !_busy;
    }

    /// <inheritdoc />
    public override async void Execute(object? parameter)
    {
        if (_busy) return;

        try
        {
            _busy = true;
            if (_action != null) _action((T?)parameter);
            else await _acb?.Invoke((T?)parameter)!;
        }
        finally
        {
            _busy = false;
        }
    }
}

/// <summary>
/// Represents an abstract base class for commands.
/// </summary>
public abstract class Command : ICommand
{
    /// <inheritdoc />
    public abstract bool CanExecute(object? parameter);

    /// <inheritdoc />
    public abstract void Execute(object? parameter);

    /// <inheritdoc />
    public abstract event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Creates a new command from an action.
    /// </summary>
    /// <param name="action">The action to execute when the command is invoked.</param>
    /// <returns>A new <see cref="Command" /> instance.</returns>
    public static Command Create(Action? action)
    {
        return new Command<object>(_ => action?.Invoke());
    }

    /// <summary>
    /// Creates a new command from an action with a typed argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the command argument.</typeparam>
    /// <param name="cb">The action to execute when the command is invoked.</param>
    /// <returns>A new <see cref="Command" /> instance.</returns>
    public static Command Create<TArg>(Action<TArg?> cb)
    {
        return new Command<TArg>(cb);
    }

    /// <summary>
    /// Creates a new command from an asynchronous function.
    /// </summary>
    /// <param name="cb">The asynchronous function to execute when the command is invoked.</param>
    /// <returns>A new <see cref="Command" /> instance.</returns>
    public static Command CreateFromTask(Func<Task> cb)
    {
        return new Command<object>(_ => cb());
    }
}