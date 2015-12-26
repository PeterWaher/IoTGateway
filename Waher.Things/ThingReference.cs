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
		private static readonly ThingReference empty = new ThingReference(string.Empty, string.Empty, string.Empty);

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

		/// <summary>
		/// Checks if the thing reference is equal to other thing reference.
		/// </summary>
		/// <param name="Ref">Second thing reference.</param>
		/// <returns>If they point to the same thing.</returns>
		public bool SameThing(ThingReference Ref)
		{
			return this.nodeId == Ref.nodeId && this.sourceId == Ref.sourceId && this.cacheType == Ref.cacheType;
		}

		/// <summary>
		/// Empty thing reference. Can be used by sensors that are not part of a concentrator during readout.
		/// </summary>
		public static ThingReference Empty { get { return empty; } }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder Output = new StringBuilder();

			Output.Append(this.nodeId);

			if (!string.IsNullOrEmpty(this.sourceId))
			{
				Output.Append(", ");
				Output.Append(this.sourceId);

				if (!string.IsNullOrEmpty(this.cacheType))
				{
					Output.Append(", ");
					Output.Append(this.cacheType);
				}
			}

			return Output.ToString();
		}
	}
}
