using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
		private readonly string status;

		/// <summary>
		/// Event arguments for status events.
		/// </summary>
		/// <param name="Expression">Expression being evaluated.</param>
		/// <param name="Status">Current status of execution.</param>
		public StatusEventArgs(Expression Expression, string Status)
		{
			this.expression = Expression;
			this.status = Status;
		}

		/// <summary>
		/// Expression being evaluated.
		/// </summary>
		public Expression Expression
		{
			get { return this.expression; }
		}

		/// <summary>
		/// Current status of execution.
		/// </summary>
		public object Status
		{
			get { return this.status; }
		}
	}
}
