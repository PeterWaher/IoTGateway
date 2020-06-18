using System;

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

		/// <summary>
		/// Creates an event object from a node object.
		/// </summary>
		/// <param name="Node">Node removed.</param>
		public static NodeRemoved FromNode(INode Node)
		{
			return new NodeRemoved()
			{
				NodeId = Node.NodeId,
				Partition = Node.Partition,
				LogId = NodeAdded.EmptyIfSame(Node.LogId, Node.NodeId),
				LocalId = NodeAdded.EmptyIfSame(Node.LocalId, Node.NodeId),
				SourceId = Node.SourceId,
				Timestamp = DateTime.Now
			};
		}

	}
}
