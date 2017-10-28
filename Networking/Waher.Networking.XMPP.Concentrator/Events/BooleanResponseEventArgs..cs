using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for boolean response callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void BooleanResponseEventHandler(object Sender, BooleanResponseEventArgs e);

	/// <summary>
	/// Event arguments for boolean response responses.
	/// </summary>
	public class BooleanResponseEventArgs : IqResultEventArgs
	{
		private bool result;

		internal BooleanResponseEventArgs(bool Result, IqResultEventArgs Response)
			: base(Response)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public bool Result
		{
			get { return this.result; }
		}
	}
}
