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
	public delegate void NodeQuerySectionEventHandler(object Sender, NodeQuerySectionEventArgs e);

	/// <summary>
	/// Event arguments for node query section events.
	/// </summary>
	public class NodeQuerySectionEventArgs : NodeQueryEventArgs
	{
		private QuerySection section;

		internal NodeQuerySectionEventArgs(QuerySection Section, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.section = Section;
		}

		/// <summary>
		/// Section
		/// </summary>
		public QuerySection Section
		{
			get { return this.section; }
		}
	}
}
