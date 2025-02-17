using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Events;
using Waher.Networking;

namespace Waher.IoTGateway.Setup.Windows
{
	/// <summary>
	/// Delegate for callback methods called when evaluating if a command can be executed or not.
	/// </summary>
	/// <returns>If the command can be executed.</returns>
	public delegate bool CanExecuteHandler();

	/// <summary>
	/// Delegate for callback methods called when a command is executed.
	/// </summary>
	public delegate Task ExecuteHandler();

	/// <summary>
	/// Defines a custom command.
	/// </summary>
	public class Command : ICommand
	{
		private readonly CanExecuteHandler? canExecuteCallback;
		private readonly ExecuteHandler executeCallback;

		/// <summary>
		/// Defines a custom command.
		/// </summary>
		/// <param name="ExecuteCallback">Method called when the command is executed.</param>
		public Command(ExecuteHandler ExecuteCallback)
			: this(null, ExecuteCallback)
		{
		}

		/// <summary>
		/// Defines a custom command.
		/// </summary>
		/// <param name="CanExecuteCallback">Method called to determine if the command can be executed.</param>
		/// <param name="ExecuteCallback">Method called when the command is executed.</param>
		public Command(CanExecuteHandler? CanExecuteCallback, ExecuteHandler ExecuteCallback)
		{
			this.canExecuteCallback = CanExecuteCallback;
			this.executeCallback = ExecuteCallback;
		}

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		/// <summary>
		/// Raises the <see cref="CanExecuteChanged"/> event.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			MainWindow.Instance?.Dispatcher.BeginInvoke(() =>
			{
				return this.CanExecuteChanged.Raise(this, EventArgs.Empty);
			});
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>true if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object? parameter)
		{
			if (this.canExecuteCallback is null)
				return true;
			else
			{
				try
				{
					return this.canExecuteCallback();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					return false;
				}
			}
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		public async void Execute(object? parameter)
		{
			if (this.executeCallback is not null)
			{
				try
				{
					await this.executeCallback();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}
	}
}
