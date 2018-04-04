using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Abstract base class for all node events.
	/// </summary>
	public abstract class NodeEvent : SourceEvent
    {
		private string nodeId = string.Empty;
		private string partition = string.Empty;

		/// <summary>
		/// Abstract base class for all node events.
		/// </summary>
		public NodeEvent()
			: base()
		{
		}

		/// <summary>
		/// Node identity.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("n")]
		public string NodeId
		{
			get { return this.nodeId; }
			set { this.nodeId = value; }
		}

		/// <summary>
		/// Optional partition.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("p")]
		public string Partition
		{
			get { return this.partition; }
			set { this.partition = value; }
		}
	}
}
