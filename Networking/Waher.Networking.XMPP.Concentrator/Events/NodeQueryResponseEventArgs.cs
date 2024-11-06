using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
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
		public NodeQuery Query => this.query;
	}
}
