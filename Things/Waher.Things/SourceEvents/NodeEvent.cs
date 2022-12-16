using System;
using Waher.Persistence.Attributes;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Abstract base class for all node events.
	/// </summary>
	public abstract class NodeEvent : SourceEvent
    {
		private string nodeId = string.Empty;
		private string localId = string.Empty;
		private string logId = string.Empty;
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
			get => this.nodeId;
			set => this.nodeId = value;
		}

		/// <summary>
		/// Optional partition.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("p")]
		public string Partition
		{
			get => this.partition;
			set => this.partition = value;
		}

		/// <summary>
		/// Log identity, if different from <see cref="NodeId"/>.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("lg")]
		public string LogId
		{
			get => this.logId;
			set => this.logId = value;
		}

		/// <summary>
		/// Local identity, if different from <see cref="NodeId"/>.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("lc")]
		public string LocalId
		{
			get => this.localId;
			set => this.localId = value;
		}
	}
}
