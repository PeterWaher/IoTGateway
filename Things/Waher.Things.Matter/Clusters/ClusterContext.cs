using System;
using System.Threading.Tasks;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.Metering;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Context in which a cluster is processed.
	/// </summary>
	public class ClusterContext
	{
		/// <summary>
		/// Context in which a cluster is processed.
		/// </summary>
		/// <param name="Device">Device node.</param>
		/// <param name="Endpoint">Endpoint node, or null if the cluster is processed on the device node (endpoint 0).</param>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="Cluster">Cluster information.</param>
		public ClusterContext(MatterDeviceNode Device, MatterEndpointNode Endpoint, ushort EndpointId, MatterClusterInfo Cluster)
		{
			this.Device = Device;
			this.Endpoint = Endpoint;
			this.EndpointId = EndpointId;
			this.Cluster = Cluster;
			this.Definition = MatterDataModel.GetCluster(Cluster.ClusterId);
		}

		/// <summary>
		/// Device node.
		/// </summary>
		public MatterDeviceNode Device { get; }

		/// <summary>
		/// Endpoint node, or null if the cluster is processed on the device node (endpoint 0).
		/// </summary>
		public MatterEndpointNode Endpoint { get; }

		/// <summary>
		/// Node representing the endpoint in the topology.
		/// </summary>
		public MeteringNode Node => (MeteringNode)this.Endpoint ?? this.Device;

		/// <summary>
		/// Endpoint ID.
		/// </summary>
		public ushort EndpointId { get; }

		/// <summary>
		/// Cluster information.
		/// </summary>
		public MatterClusterInfo Cluster { get; }

		/// <summary>
		/// Cluster ID.
		/// </summary>
		public uint ClusterId => this.Cluster.ClusterId;

		/// <summary>
		/// Cluster definition, if known.
		/// </summary>
		public ClusterDefinition Definition { get; }

		/// <summary>
		/// Name of cluster.
		/// </summary>
		public string ClusterName => this.Definition?.Name ?? MatterDataModel.GetClusterName(this.Cluster.ClusterId);

		/// <summary>
		/// Registry of field names used on the endpoint.
		/// </summary>
		public FieldNameRegistry Names => this.Device.GetFieldNames(this.EndpointId);

		/// <summary>
		/// Device types of the endpoint.
		/// </summary>
		public uint[] DeviceTypes => this.Endpoint?.DeviceTypes ?? Array.Empty<uint>();

		/// <summary>
		/// Checks if the endpoint has a given device type.
		/// </summary>
		/// <param name="DeviceType">Device type ID.</param>
		/// <returns>If the endpoint has the device type.</returns>
		public bool HasDeviceType(uint DeviceType)
		{
			return Array.IndexOf(this.DeviceTypes, DeviceType) >= 0;
		}

		/// <summary>
		/// Gets the last known value of an attribute of the cluster, from the attribute cache.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value, or null if not known.</returns>
		public TlvElement GetCachedValue(uint AttributeId)
		{
			return this.Device.GetCachedValue(this.EndpointId, this.Cluster.ClusterId, AttributeId);
		}

		/// <summary>
		/// Gets the last known value of an attribute of another cluster on the same endpoint, from the attribute cache.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value, or null if not known.</returns>
		public TlvElement GetCachedValue(uint ClusterId, uint AttributeId)
		{
			return this.Device.GetCachedValue(this.EndpointId, ClusterId, AttributeId);
		}

		/// <summary>
		/// Reads an attribute of the cluster from the device.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value.</returns>
		public Task<TlvElement> ReadAttributeAsync(uint AttributeId)
		{
			return this.Device.ReadAttributeAsync(this.EndpointId, this.Cluster.ClusterId, AttributeId);
		}

		/// <summary>
		/// Reads attributes of the cluster from the device.
		/// </summary>
		/// <param name="AttributeIds">Attribute IDs.</param>
		/// <returns>Read result.</returns>
		public Task<ReadResult> ReadAttributesAsync(params uint[] AttributeIds)
		{
			AttributePath[] Paths = new AttributePath[AttributeIds.Length];
			int i, c = AttributeIds.Length;

			for (i = 0; i < c; i++)
				Paths[i] = new AttributePath(this.EndpointId, this.Cluster.ClusterId, AttributeIds[i]);

			return this.Device.ReadAttributesAsync(Paths);
		}

		/// <summary>
		/// Writes an attribute of the cluster.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value to write.</param>
		/// <param name="Timed">If a timed interaction is required.</param>
		public Task WriteAttributeAsync(uint AttributeId, object Value, bool Timed = false)
		{
			if (!Timed && (this.Definition?.GetAttribute(AttributeId)?.TimedWrite ?? false))
				Timed = true;

			return this.Device.WriteAttributeAsync(this.EndpointId, this.Cluster.ClusterId, AttributeId, Value, Timed);
		}

		/// <summary>
		/// Invokes a command on the cluster.
		/// </summary>
		/// <param name="CommandId">Command ID.</param>
		/// <param name="Fields">Command fields, or null if no fields.</param>
		/// <param name="Timed">If a timed interaction is required.</param>
		/// <returns>Invoke result.</returns>
		public async Task<InvokeResult> InvokeAsync(uint CommandId, TlvElement Fields = null, bool Timed = false)
		{
			if (!Timed && (this.Definition?.GetCommand(CommandId)?.Timed ?? false))
				Timed = true;

			InvokeResult Result = await this.Device.InvokeAsync(this.EndpointId, this.Cluster.ClusterId, CommandId, Fields, Timed);
			return Result.AssertSuccess();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Device?.NodeId + "/" + this.EndpointId.ToString() + "/" + this.ClusterName;
		}
	}
}
