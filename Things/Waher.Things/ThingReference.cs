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
	[Index("NodeId", "SourceId", "Partition")]
	public class ThingReference : IThingReference
	{
		private static readonly ThingReference empty = new ThingReference(string.Empty, string.Empty, string.Empty);

		private string objectId = null;
		private string nodeId;
		private string sourceId;
		private string partition;

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
		/// <param name="Partition">Optional partition in which the Node ID is unique.</param>
		public ThingReference(string NodeId, string SourceId, string Partition)
		{
			this.nodeId = NodeId;
			this.sourceId = SourceId;
			this.partition = Partition;
		}

		/// <summary>
		/// Persisted object ID. Is null if object not persisted.
		/// </summary>
		[ObjectId]
		public string ObjectId
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
		/// Optional partition in which the Node ID is unique.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("p")]
		public string Partition
		{
			get { return this.partition; }
			set { this.partition = value; }
		}

		/// <summary>
		/// If the reference is an empty reference.
		/// </summary>
		public bool IsEmpty
		{
			get { return string.IsNullOrEmpty(this.nodeId) && string.IsNullOrEmpty(this.sourceId) && string.IsNullOrEmpty(this.partition); }
		}

		/// <summary>
		/// Key for thing reference: [NodeId[, SourceId[, Partition]]]
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

						if (!string.IsNullOrEmpty(this.partition))
						{
							sb.Append(", ");
							sb.Append(this.partition);
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
			return this.nodeId == Ref.nodeId && this.sourceId == Ref.sourceId && this.partition == Ref.partition;
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
				this.partition.GetHashCode();
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

			if (!string.IsNullOrEmpty(this.objectId))
			{
				Output.Append(this.objectId);
				Output.Append(": ");
			}

			Output.Append(this.nodeId);

			if (!string.IsNullOrEmpty(this.sourceId))
			{
				Output.Append(", ");
				Output.Append(this.sourceId);

				if (!string.IsNullOrEmpty(this.partition))
				{
					Output.Append(", ");
					Output.Append(this.partition);
				}
			}

			return Output.ToString();
		}
	}
}
