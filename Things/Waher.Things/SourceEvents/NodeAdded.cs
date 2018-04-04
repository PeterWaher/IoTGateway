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
	}
}
