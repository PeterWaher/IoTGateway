using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node query object callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeQueryObjectEventHandler(object Sender, NodeQueryObjectEventArgs e);

	/// <summary>
	/// Event arguments for node query object events.
	/// </summary>
	public class NodeQueryObjectEventArgs : NodeQueryEventArgs
	{
		private readonly QueryObject obj;

		internal NodeQueryObjectEventArgs(QueryObject Object, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.obj = Object;
		}

		/// <summary>
		/// Object
		/// </summary>
		public QueryObject Object
		{
			get { return this.obj; }
		}
	}
}
