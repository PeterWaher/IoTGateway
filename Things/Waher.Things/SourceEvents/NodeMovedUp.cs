using System;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node moved up event.
	/// </summary>
	public class NodeMovedUp : NodeEvent
    {
		/// <summary>
		/// Node moved up event.
		/// </summary>
		public NodeMovedUp()
			: base()
		{
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public override SourceEventType EventType
		{
			get { return SourceEventType.NodeMovedUp; }
		}
	}
}
