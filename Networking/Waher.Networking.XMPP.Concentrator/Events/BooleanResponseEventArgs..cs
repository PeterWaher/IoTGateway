using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for boolean response responses.
	/// </summary>
	public class BooleanResponseEventArgs : IqResultEventArgs
	{
		private readonly bool result;

		internal BooleanResponseEventArgs(bool Result, IqResultEventArgs Response)
			: base(Response)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public bool Result => this.result;
	}
}
