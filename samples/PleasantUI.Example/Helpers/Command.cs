using System.Windows.Input;

namespace PleasantUI.Example.Helpers;

public class Command<T> : Command, ICommand
{
	private readonly Action<T?>? _action;
	private readonly Func<T?, Task>? _acb;

	public Command(Action<T?> action)
	{
		_action = action;
	}

	public Command(Func<T?, Task> acb)
	{
		_acb = acb;
	}

	private bool Busy { get; set; }

	public override event EventHandler? CanExecuteChanged;

	public override bool CanExecute(object? parameter)
	{
		return !Busy;
	}

	public override async void Execute(object? parameter)
	{
		if (Busy) return;

		try
		{
			Busy = true;
			if (_action != null) _action((T?)parameter);
			else await _acb?.Invoke((T?)parameter)!;
		}
		finally
		{
			Busy = false;
		}
	}
}

public abstract class Command : ICommand
{
	public abstract bool CanExecute(object? parameter);
	public abstract void Execute(object? parameter);
	public abstract event EventHandler? CanExecuteChanged;

	public static Command Create(Action action)
	{
		return new Command<object>(_ => action());
	}

	public static Command Create<TArg>(Action<TArg?> cb)
	{
		return new Command<TArg>(cb);
	}

	public static Command CreateFromTask(Func<Task> cb)
	{
		return new Command<object>(_ => cb());
	}
}