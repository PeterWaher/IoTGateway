using System;

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
