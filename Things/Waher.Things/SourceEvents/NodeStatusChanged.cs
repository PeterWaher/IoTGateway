using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node status changed event.
	/// </summary>
	public class NodeStatusChanged : NodeStatusEvent
    {
		private Message[] messages = null;

		/// <summary>
		/// Node status changed event.
		/// </summary>
		public NodeStatusChanged()
			: base()
		{
		}

		/// <summary>
		/// Messages on node, at time of event.
		/// </summary>
		public Message[] Messages
		{
			get { return this.messages; }
			set { this.messages = value; }
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public override SourceEventType EventType
		{
			get { return SourceEventType.NodeStatusChanged; }
		}
	}
}
