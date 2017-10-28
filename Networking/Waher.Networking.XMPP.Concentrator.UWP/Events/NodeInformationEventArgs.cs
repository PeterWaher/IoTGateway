using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node information callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NodeInformationEventHandler(object Sender, NodeInformationEventArgs e);

	/// <summary>
	/// Event arguments for node information responses.
	/// </summary>
	public class NodeInformationEventArgs : IqResultEventArgs
	{
		private NodeInformation nodeInformation;

		internal NodeInformationEventArgs(NodeInformation NodeInformation, IqResultEventArgs Response)
			: base(Response)
		{
			this.nodeInformation = NodeInformation;
		}

		/// <summary>
		/// Node information.
		/// </summary>
		public NodeInformation NodeInformation
		{
			get { return this.nodeInformation; }
		}
	}
}
