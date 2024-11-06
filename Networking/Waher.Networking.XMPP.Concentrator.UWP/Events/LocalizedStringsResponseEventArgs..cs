using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for localized string array responses responsess.
	/// </summary>
	public class LocalizedStringsResponseEventArgs : IqResultEventArgs
	{
		private readonly LocalizedString[] result;

		internal LocalizedStringsResponseEventArgs(LocalizedString[] Result, IqResultEventArgs Responses)
			: base(Responses)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public LocalizedString[] Result => this.result;
	}
}
