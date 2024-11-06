using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for command array responses responsess.
	/// </summary>
	public class CommandsEventArgs : IqResultEventArgs
	{
		private readonly NodeCommand[] result;

		internal CommandsEventArgs(NodeCommand[] Result, IqResultEventArgs s)
			: base(s)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public NodeCommand[] Result => this.result;
	}
}
