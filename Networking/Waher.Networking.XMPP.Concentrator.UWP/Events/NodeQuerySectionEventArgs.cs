using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for node query section events.
	/// </summary>
	public class NodeQuerySectionEventArgs : NodeQueryEventArgs
	{
		private readonly QuerySection section;

		internal NodeQuerySectionEventArgs(QuerySection Section, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.section = Section;
		}

		/// <summary>
		/// Section
		/// </summary>
		public QuerySection Section => this.section;
	}
}
