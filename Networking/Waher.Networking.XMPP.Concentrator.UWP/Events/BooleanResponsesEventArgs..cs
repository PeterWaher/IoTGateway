using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for boolean responses callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void BooleanResponsesEventHandler(object Sender, BooleanResponsesEventArgs e);

	/// <summary>
	/// Event arguments for boolean responses responsess.
	/// </summary>
	public class BooleanResponsesEventArgs : IqResultEventArgs
	{
		private bool[] result;

		internal BooleanResponsesEventArgs(bool[] Result, IqResultEventArgs Responses)
			: base(Responses)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public bool[] Result
		{
			get { return this.result; }
		}
	}
}
