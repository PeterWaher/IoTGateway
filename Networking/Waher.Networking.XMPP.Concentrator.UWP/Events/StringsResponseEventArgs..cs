using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
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
		public string[] Result => this.result;
	}
}
