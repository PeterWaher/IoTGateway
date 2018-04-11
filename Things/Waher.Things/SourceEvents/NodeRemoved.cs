using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node removed event.
	/// </summary>
	public class NodeRemoved : NodeEvent
    {
		/// <summary>
		/// Node removed event.
		/// </summary>
		public NodeRemoved()
			: base()
		{
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public override SourceEventType EventType
		{
			get { return SourceEventType.NodeRemoved; }
		}
	}
}
