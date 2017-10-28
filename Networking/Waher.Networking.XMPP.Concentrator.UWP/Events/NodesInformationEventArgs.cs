using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for nodes information callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NodesInformationEventHandler(object Sender, NodesInformationEventArgs e);

	/// <summary>
	/// Event arguments for nodes information responses.
	/// </summary>
	public class NodesInformationEventArgs : IqResultEventArgs
	{
		private NodeInformation[] nodesInformation;

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
