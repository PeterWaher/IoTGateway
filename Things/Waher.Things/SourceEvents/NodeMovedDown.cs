using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node moved down event.
	/// </summary>
	public class NodeMovedDown : NodeEvent
    {
		/// <summary>
		/// Node moved down event.
		/// </summary>
		public NodeMovedDown()
			: base()
		{
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public override SourceEventType EventType
		{
			get { return SourceEventType.NodeMovedDown; }
		}
	}
}
