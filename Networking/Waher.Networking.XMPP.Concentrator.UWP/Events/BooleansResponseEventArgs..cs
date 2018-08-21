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
	public delegate void BooleansResponseEventHandler(object Sender, BooleansResponseEventArgs e);

	/// <summary>
	/// Event arguments for boolean responses responsess.
	/// </summary>
	public class BooleansResponseEventArgs : IqResultEventArgs
	{
		private readonly bool[] result;

		internal BooleansResponseEventArgs(bool[] Result, IqResultEventArgs Responses)
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
