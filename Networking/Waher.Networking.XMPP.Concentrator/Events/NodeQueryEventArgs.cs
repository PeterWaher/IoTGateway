using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node query callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeQueryEventHandler(object Sender, NodeQueryEventArgs e);

	/// <summary>
	/// Event arguments for node query events.
	/// </summary>
	public class NodeQueryEventArgs : MessageEventArgs
	{
		private readonly NodeQuery query;

		internal NodeQueryEventArgs(NodeQuery Query, MessageEventArgs Message)
			: base(Message)
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
