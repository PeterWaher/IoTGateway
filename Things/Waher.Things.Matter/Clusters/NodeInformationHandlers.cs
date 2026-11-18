using Waher.Runtime.Inventory;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Cluster handler for the Basic Information and Bridged Device Basic Information clusters.
	/// </summary>
	public class BasicInformationHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the Basic Information and Bridged Device Basic Information clusters.
		/// </summary>
		public BasicInformationHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			if (ClusterId == ClusterIds.BasicInformation || ClusterId == ClusterIds.BridgedDeviceBasicInformation)
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0001, 0x0002, 0x0003, 0x0004, 0x0005, 0x0006, 0x0008, 0x000a, 0x000b,
				0x000c, 0x000d, 0x000e, 0x000f, 0x0011, 0x0012);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Fields.IsIncluded(FieldType.Identity))
			{
				AddString(Values, Fields, 0x0001, "Vendor Name");

				if (Values.TryGetReported(0x0002, out long VendorId))
					Fields.String("Vendor ID", "0x" + VendorId.ToString("X4"), FieldType.Identity);

				AddString(Values, Fields, 0x0003, "Product Name");

				if (Values.TryGetReported(0x0004, out long ProductId))
					Fields.String("Product ID", "0x" + ProductId.ToString("X4"), FieldType.Identity);

				AddString(Values, Fields, 0x0005, "Node Label");
				AddString(Values, Fields, 0x0006, "Location");
				AddString(Values, Fields, 0x0008, "Hardware Version");
				AddString(Values, Fields, 0x000a, "Software Version");
				AddString(Values, Fields, 0x000b, "Manufacturing Date");
				AddString(Values, Fields, 0x000c, "Part Number");
				AddString(Values, Fields, 0x000d, "Product URL");
				AddString(Values, Fields, 0x000e, "Product Label");
				AddString(Values, Fields, 0x000f, "Serial Number");
				AddString(Values, Fields, 0x0012, "Unique ID");
			}

			if (Values.TryGetReported(0x0011, out bool Reachable))
				Fields.Boolean("Reachable", Reachable, FieldType.Status);
		}

		private static void AddString(ClusterValues Values, FieldBuilder Fields, uint AttributeId, string Name)
		{
			if (Values.TryGetReported(AttributeId, out string s) && !string.IsNullOrEmpty(s))
				Fields.String(Name, s, FieldType.Identity);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => false;
	}

	/// <summary>
	/// Cluster handler for the General Diagnostics cluster.
	/// </summary>
	public class GeneralDiagnosticsHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the General Diagnostics cluster.
		/// </summary>
		public GeneralDiagnosticsHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.GeneralDiagnostics ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0001, 0x0002, 0x0003, 0x0004);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0001, out long RebootCount))
				Fields.Integer("Reboot Count", RebootCount, FieldType.Status);

			if (Values.TryGetReported(0x0002, out double UpTime))
				Fields.Quantity("Up Time", UpTime, 0, "s", FieldType.Status);

			if (Values.TryGetReported(0x0003, out double TotalHours))
				Fields.Quantity("Total Operational Hours", TotalHours, 0, "h", FieldType.Status);

			if (Values.TryGetReported(0x0004, out long BootReason))
				Fields.String("Boot Reason", GetEnumName(ClusterIds.GeneralDiagnostics, "BootReasonEnum", BootReason), FieldType.Status);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => false;
	}

	/// <summary>
	/// Cluster handler for the network diagnostics clusters (Wi-Fi and Thread).
	/// </summary>
	public class NetworkDiagnosticsHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the network diagnostics clusters (Wi-Fi and Thread).
		/// </summary>
		public NetworkDiagnosticsHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			if (ClusterId == ClusterIds.WiFiNetworkDiagnostics || ClusterId == ClusterIds.ThreadNetworkDiagnostics)
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			if (Cluster.ClusterId == ClusterIds.WiFiNetworkDiagnostics)
				return GetPaths(EndpointId, Cluster, 0x0003, 0x0004);
			else
				return GetPaths(EndpointId, Cluster, 0x0000, 0x0001, 0x0002);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Context.ClusterId == ClusterIds.WiFiNetworkDiagnostics)
			{
				if (Values.TryGetReported(0x0003, out long Channel))
					Fields.Integer("Wi-Fi Channel", Channel, FieldType.Status);

				if (Values.TryGetReported(0x0004, out double Rssi))
					Fields.Quantity("RSSI", Rssi, 0, "dBm");
			}
			else
			{
				if (Values.TryGetReported(0x0000, out long Channel))
					Fields.Integer("Thread Channel", Channel, FieldType.Status);

				if (Values.TryGetReported(0x0001, out long Role))
					Fields.String("Thread Role", GetEnumName(ClusterIds.ThreadNetworkDiagnostics, "RoutingRoleEnum", Role), FieldType.Status);

				if (Values.TryGetReported(0x0002, out string NetworkName) && !string.IsNullOrEmpty(NetworkName))
					Fields.String("Thread Network", NetworkName, FieldType.Status);
			}
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => false;
	}
}
