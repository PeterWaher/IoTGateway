using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for nodes information callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodesInformationEventHandler(object Sender, NodesInformationEventArgs e);

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
		public NodeInformation[] NodesInformation
		{
			get { return this.nodesInformation; }
		}
	}
}
