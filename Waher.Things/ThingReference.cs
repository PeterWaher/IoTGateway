using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Things
{
	/// <summary>
	/// Contains a reference to a thing
	/// </summary>
	public class ThingReference
	{
		private string nodeId;
		private string sourceId;
		private string cacheType;

		/// <summary>
		/// Contains a reference to a thing
		/// </summary>
		/// <param name="NodeId">ID of node.</param>
		/// <param name="SourceId">Optional ID of source containing node.</param>
		/// <param name="CacheType">Optional Type of cache in which the Node ID is unique.</param>
		public ThingReference(string NodeId, string SourceId, string CacheType)
		{
			this.nodeId = NodeId;
			this.sourceId = SourceId;
			this.cacheType = CacheType;
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		public string NodeId { get { return this.nodeId; } }

		/// <summary>
		/// Optional ID of source containing node.
		/// </summary>
		public string SourceId { get { return this.sourceId; } }

		/// <summary>
		/// Optional Type of cache in which the Node ID is unique.
		/// </summary>
		public string CacheType { get { return this.cacheType; } }
	}
}
