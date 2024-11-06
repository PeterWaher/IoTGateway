using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for node information responses.
	/// </summary>
	public class NodeInformationEventArgs : IqResultEventArgs
	{
		private readonly NodeInformation nodeInformation;

		internal NodeInformationEventArgs(NodeInformation NodeInformation, IqResultEventArgs Response)
			: base(Response)
		{
			this.nodeInformation = NodeInformation;
		}

		/// <summary>
		/// Node information.
		/// </summary>
		public NodeInformation NodeInformation => this.nodeInformation;
	}
}
