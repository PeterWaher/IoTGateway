using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things.Matter.DataModel;

namespace Waher.Things.Matter
{
	/// <summary>
	/// Information about a server cluster available on an endpoint of a Matter node. The information is persisted
	/// with the endpoint node, and updated when endpoints are scanned.
	/// </summary>
	public class MatterClusterInfo
	{
		/// <summary>
		/// Information about a server cluster available on an endpoint of a Matter node.
		/// </summary>
		public MatterClusterInfo()
		{
		}

		/// <summary>
		/// Information about a server cluster available on an endpoint of a Matter node.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		public MatterClusterInfo(uint ClusterId)
		{
			this.ClusterId = ClusterId;
		}

		/// <summary>
		/// Cluster ID.
		/// </summary>
		public uint ClusterId { get; set; }

		/// <summary>
		/// Feature map of the cluster.
		/// </summary>
		public uint FeatureMap { get; set; }

		/// <summary>
		/// Cluster revision.
		/// </summary>
		public uint Revision { get; set; }

		/// <summary>
		/// Attributes supported by the cluster (the AttributeList global attribute). If null, the attribute list is not known.
		/// </summary>
		public uint[] Attributes { get; set; }

		/// <summary>
		/// Commands accepted by the cluster (the AcceptedCommandList global attribute). If null, the command list is not known.
		/// </summary>
		public uint[] Commands { get; set; }

		/// <summary>
		/// Cluster definition, if known.
		/// </summary>
		public ClusterDefinition GetDefinition() => MatterDataModel.GetCluster(this.ClusterId);

		/// <summary>
		/// Name of cluster.
		/// </summary>
		public string GetName() => MatterDataModel.GetClusterName(this.ClusterId);

		/// <summary>
		/// Checks if the cluster supports an attribute.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>If the attribute is supported. If the attribute list is not known, true is returned.</returns>
		public bool HasAttribute(uint AttributeId)
		{
			return this.Attributes is null || Array.IndexOf(this.Attributes, AttributeId) >= 0;
		}

		/// <summary>
		/// Checks if the cluster accepts a command.
		/// </summary>
		/// <param name="CommandId">Command ID.</param>
		/// <returns>If the command is accepted. If the command list is not known, true is returned.</returns>
		public bool HasCommand(uint CommandId)
		{
			return this.Commands is null || Array.IndexOf(this.Commands, CommandId) >= 0;
		}

		/// <summary>
		/// Checks if a feature bit is set in the feature map.
		/// </summary>
		/// <param name="Bit">Bit number.</param>
		/// <returns>If the feature is supported.</returns>
		public bool HasFeature(int Bit)
		{
			return (this.FeatureMap & (1U << Bit)) != 0;
		}

		/// <summary>
		/// Gets the IDs of supported attributes, if known, or the attributes defined in the data model, if not known.
		/// </summary>
		/// <returns>Attribute IDs.</returns>
		public uint[] GetAttributeIds()
		{
			if (!(this.Attributes is null))
				return this.Attributes;

			List<uint> Result = new List<uint>();
			ClusterDefinition Definition = this.GetDefinition();

			if (!(Definition is null))
			{
				foreach (AttributeDefinition Attribute in Definition.Attributes)
				{
					if (!Attribute.Optional)
						Result.Add(Attribute.Id);
				}
			}

			Result.Sort();

			return Result.ToArray();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.GetName());

			if (this.FeatureMap != 0)
			{
				string[] Features = this.GetDefinition()?.GetFeatureNames(this.FeatureMap);

				if (!(Features is null) && Features.Length > 0)
				{
					sb.Append(" [");
					sb.Append(string.Join(", ", Features));
					sb.Append(']');
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Finds a cluster in an array of clusters.
		/// </summary>
		/// <param name="Clusters">Clusters.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <returns>Cluster information, or null if not found.</returns>
		public static MatterClusterInfo Find(MatterClusterInfo[] Clusters, uint ClusterId)
		{
			if (Clusters is null)
				return null;

			foreach (MatterClusterInfo Cluster in Clusters)
			{
				if (Cluster.ClusterId == ClusterId)
					return Cluster;
			}

			return null;
		}
	}
}
