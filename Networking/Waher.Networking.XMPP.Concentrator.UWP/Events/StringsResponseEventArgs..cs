using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for boolean rersponses callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void StringsResponseEventHandler(object Sender, StringsResponseEventArgs e);

	/// <summary>
	/// Event arguments for boolean rersponses rersponsess.
	/// </summary>
	public class StringsResponseEventArgs : IqResultEventArgs
	{
		private string[] result;

		internal StringsResponseEventArgs(string[] Result, IqResultEventArgs Responses)
			: base(Responses)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public string[] Result
		{
			get { return this.result; }
		}
	}
}
