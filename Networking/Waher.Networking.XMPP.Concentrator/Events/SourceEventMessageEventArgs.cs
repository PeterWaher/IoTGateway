using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Things.Queries;
using Waher.Things.SourceEvents;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for data source section callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SourceEventMessageEventHandler(object Sender, SourceEventMessageEventArgs e);

	/// <summary>
	/// Event arguments for data source section events.
	/// </summary>
	public class SourceEventMessageEventArgs : MessageEventArgs
	{
		private SourceEvent sourceEvent;

		internal SourceEventMessageEventArgs(SourceEvent SourceEvent, MessageEventArgs Message)
			: base(Message)
		{
			this.sourceEvent = SourceEvent;
		}

		/// <summary>
		/// Source Event
		/// </summary>
		public SourceEvent Event
		{
			get { return this.sourceEvent; }
		}
	}
}
