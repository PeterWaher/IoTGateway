using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node query table callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NodeQueryTableUpdatedEventHandler(object Sender, NodeQueryTableUpdatedEventArgs e);

	/// <summary>
	/// Event arguments for node query table events.
	/// </summary>
	public class NodeQueryTableUpdatedEventArgs : NodeQueryTableEventArgs
	{
		private readonly Record[] newRecords;

		internal NodeQueryTableUpdatedEventArgs(QueryTable Table, NodeQuery Query, Record[] NewRecords, MessageEventArgs Message)
			: base(Table, Query, Message)
		{
			this.newRecords = NewRecords;
		}

		/// <summary>
		/// New Records
		/// </summary>
		public Record[] NewRecords
		{
			get { return this.newRecords; }
		}
	}
}
