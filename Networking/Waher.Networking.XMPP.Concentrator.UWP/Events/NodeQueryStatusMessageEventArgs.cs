using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Concentrator.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node query section callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NodeQueryStatusMessageEventHandler(object Sender, NodeQueryStatusMessageEventArgs e);

	/// <summary>
	/// Event arguments for node query section events.
	/// </summary>
	public class NodeQueryStatusMessageEventArgs : NodeQueryEventArgs
	{
		private string statusMessage;

		internal NodeQueryStatusMessageEventArgs(string StatusMessage, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.statusMessage = StatusMessage;
		}

		/// <summary>
		/// Status Message
		/// </summary>
		public string StatusMessage
		{
			get { return this.statusMessage; }
		}
	}
}
