using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for nodes information responses.
	/// </summary>
	public class NodesInformationEventArgs : IqResultEventArgs
	{
		private readonly NodeInformation[] nodesInformation;

		internal NodesInformationEventArgs(NodeInformation[] NodesInformation, IqResultEventArgs Response)
			: base(Response)
		{
			this.nodesInformation = NodesInformation;
		}

		/// <summary>
		/// Nodes information.
		/// </summary>
		public NodeInformation[] NodesInformation => this.nodesInformation;
	}
}
