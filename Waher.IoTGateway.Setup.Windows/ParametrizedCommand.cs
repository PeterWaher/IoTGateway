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
	/// <param name="Parameter">Parameter</param>
	/// <returns>If the command can be executed.</returns>
	public delegate bool CanExecuteParametrizedHandler(object? Parameter);

	/// <summary>
	/// Delegate for callback methods called when a parametrized command is executed.
	/// </summary>
	/// <param name="Parameter">Parameter</param>
	public delegate void ExecuteParametrizedHandler(object? Parameter);

	/// <summary>
	/// Defines a custom parametrized command.
	/// </summary>
	public class ParametrizedCommand : ICommand
	{
		private readonly CanExecuteParametrizedHandler? canExecuteCallback;
		private readonly ExecuteParametrizedHandler executeCallback;

		/// <summary>
		/// Defines a custom parametrized command.
		/// </summary>
		/// <param name="ExecuteCallback">Method called when the command is executed.</param>
		public ParametrizedCommand(ExecuteParametrizedHandler ExecuteCallback)
			: this(null, ExecuteCallback)
		{
		}

		/// <summary>
		/// Defines a custom parametrized command.
		/// </summary>
		/// <param name="CanExecuteCallback">Method called to determine if the command can be executed.</param>
		/// <param name="ExecuteCallback">Method called when the command is executed.</param>
		public ParametrizedCommand(CanExecuteParametrizedHandler? CanExecuteCallback, ExecuteParametrizedHandler ExecuteCallback)
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
					return this.canExecuteCallback(parameter);
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
		public void Execute(object? parameter)
		{
			if (this.executeCallback is not null)
			{
				try
				{
					this.executeCallback(parameter);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}
	}
}
