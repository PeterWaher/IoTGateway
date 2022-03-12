using System;

namespace Waher.Script
{
	/// <summary>
	/// Delegate for status events.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e"></param>
	public delegate void StatusEventHandler(object Sender, StatusEventArgs e);

	/// <summary>
	/// Event arguments for status events.
	/// </summary>
	public class StatusEventArgs : EventArgs
	{
		private readonly Expression expression;
		private readonly Variables variables;
		private readonly string status;

		/// <summary>
		/// Event arguments for status events.
		/// </summary>
		/// <param name="Expression">Expression being evaluated.</param>
		/// <param name="Variables">Current variables collection.</param>
		/// <param name="Status">Current status of execution.</param>
		public StatusEventArgs(Expression Expression, Variables Variables, string Status)
		{
			this.expression = Expression;
			this.variables = Variables;
			this.status = Status;
		}

		/// <summary>
		/// Expression being evaluated.
		/// </summary>
		public Expression Expression => this.expression;

		/// <summary>
		/// Current Variables collection.
		/// </summary>
		public Variables Variables => this.variables;

		/// <summary>
		/// Current status of execution.
		/// </summary>
		public object Status => this.status;
	}
}
