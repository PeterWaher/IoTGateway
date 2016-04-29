using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Things
{
	/// <summary>
	/// Contains a reference to a thing
	/// </summary>
	[CollectionName("ThingReferences")]
	public class ThingReference
	{
		private static readonly ThingReference empty = new ThingReference(string.Empty, string.Empty, string.Empty);

		private string objectId;
		private string nodeId;
		private string sourceId;
		private string cacheType;

		/// <summary>
		/// Contains a reference to a thing
		/// </summary>
		public ThingReference()
			: this(string.Empty, string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Contains a reference to a thing
		/// </summary>
		/// <param name="NodeId">ID of node.</param>
		public ThingReference(string NodeId)
			: this(NodeId, string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Contains a reference to a thing
		/// </summary>
		/// <param name="NodeId">ID of node.</param>
		/// <param name="SourceId">Optional ID of source containing node.</param>
		public ThingReference(string NodeId, string SourceId)
			: this(NodeId, SourceId, string.Empty)
		{
		}

		/// <summary>
		/// Contains a reference to a thing
		/// </summary>
		/// <param name="NodeId">ID of node.</param>
		/// <param name="SourceId">Optional ID of source containing node.</param>
		/// <param name="CacheType">Optional Type of cache in which the Node ID is unique.</param>
		public ThingReference(string NodeId, string SourceId, string CacheType)
		{
			this.objectId = null;
			this.nodeId = NodeId;
			this.sourceId = SourceId;
			this.cacheType = CacheType;
		}

		/// <summary>
		/// Persisted object ID. Is null if object not persisted.
		/// </summary>
		[ObjectId]
		private string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[ShortName("n")]
		public string NodeId
		{
			get { return this.nodeId; }
			set { this.nodeId = value; }
		}

		/// <summary>
		/// Optional ID of source containing node.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("s")]
		public string SourceId
		{
			get { return this.sourceId; }
			set { this.sourceId = value; }
		}

		/// <summary>
		/// Optional Type of cache in which the Node ID is unique.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("c")]
		public string CacheType
		{
			get { return this.cacheType; }
			set { this.cacheType = value; }
		}

		/// <summary>
		/// If the reference is an empty reference.
		/// </summary>
		public bool IsEmpty
		{
			get { return string.IsNullOrEmpty(this.nodeId) && string.IsNullOrEmpty(this.sourceId) && string.IsNullOrEmpty(this.cacheType); }
		}

		/// <summary>
		/// Key for thing reference: [NodeId[, SourceId[, CacheType]]]
		/// </summary>
		public string Key
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				if (!string.IsNullOrEmpty(this.nodeId))
				{
					sb.Append(this.nodeId);

					if (!string.IsNullOrEmpty(this.sourceId))
					{
						sb.Append(", ");
						sb.Append(this.sourceId);

						if (!string.IsNullOrEmpty(this.cacheType))
						{
							sb.Append(", ");
							sb.Append(this.cacheType);
						}
					}
				}

				return sb.ToString();
			}
		}

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
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			ThingReference Ref = obj as ThingReference;
			if (Ref == null)
				return false;
			else
				return this.SameThing(Ref);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.nodeId.GetHashCode() ^
				this.sourceId.GetHashCode() ^
				this.cacheType.GetHashCode();
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
