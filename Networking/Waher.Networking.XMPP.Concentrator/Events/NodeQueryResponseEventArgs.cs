using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node query callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NodeQueryResponseEventHandler(object Sender, NodeQueryResponseEventArgs e);

	/// <summary>
	/// Event arguments for node query events.
	/// </summary>
	public class NodeQueryResponseEventArgs : NodeCommandResponseEventArgs
	{
		private readonly NodeQuery query;

		internal NodeQueryResponseEventArgs(NodeQuery Query, DataForm Parameters, IqResultEventArgs e)
			: base(Parameters, e)
		{
			this.query = Query;
		}

		/// <summary>
		/// Node query
		/// </summary>
		public NodeQuery Query
		{
			get { return this.query; }
		}
	}
}
