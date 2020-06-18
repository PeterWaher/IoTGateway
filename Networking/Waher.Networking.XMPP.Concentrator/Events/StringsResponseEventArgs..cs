using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for string array responses callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task StringsResponseEventHandler(object Sender, StringsResponseEventArgs e);

	/// <summary>
	/// Event arguments for string array responses responsess.
	/// </summary>
	public class StringsResponseEventArgs : IqResultEventArgs
	{
		private readonly string[] result;

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
