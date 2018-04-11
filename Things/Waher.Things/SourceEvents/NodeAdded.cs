using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node added event.
	/// </summary>
	public class NodeAdded : NodeParametersEvent
    {
		private string afterNodeId = string.Empty;
		private string afterPartition = string.Empty;
		private string displayName = string.Empty;
		private string nodeType = string.Empty;
		private bool sniffable = false;

		/// <summary>
		/// Node added event.
		/// </summary>
		public NodeAdded()
			: base()
		{
		}

		/// <summary>
		/// In an ordered set of nodes, the new node is added after this node, if provided.
		/// </summary>
		[DefaultValueStringEmpty]
		public string AfterNodeId
		{
			get { return this.afterNodeId; }
			set { this.afterNodeId = value; }
		}

		/// <summary>
		/// In an ordered set of nodes, the new node is added after the node defined by <see cref="AfterNodeId"/>, in this partition, if provided.
		/// </summary>
		[DefaultValueStringEmpty]
		public string AfterPartition
		{
			get { return this.afterPartition; }
			set { this.afterPartition = value; }
		}

		/// <summary>
		/// Display name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string DisplayName
		{
			get { return this.displayName; }
			set { this.displayName = value; }
		}

		/// <summary>
		/// Type of node, at time of event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string NodeType
		{
			get { return this.nodeType; }
			set { this.nodeType = value; }
		}

		/// <summary>
		/// If node was sniffable, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool Sniffable
		{
			get { return this.sniffable; }
			set { this.sniffable = value; }
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public override SourceEventType EventType
		{
			get { return SourceEventType.NodeAdded; }
		}
	}
}
