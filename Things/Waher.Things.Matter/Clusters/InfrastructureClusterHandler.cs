using Waher.Runtime.Inventory;
using Waher.Things.Matter.DataModel;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Handler for infrastructure clusters (commissioning, security, configuration, etc.), that do not contain
	/// sensor or actuator values of interest. Such clusters are not reported during readout, and are not
	/// subscribed to. Their attributes are available using the Read all attributes query.
	/// </summary>
	public class InfrastructureClusterHandler : ClusterHandler
	{
		/// <summary>
		/// Infrastructure clusters.
		/// </summary>
		public static readonly uint[] InfrastructureClusters = new uint[]
		{
			ClusterIds.Groups,
			0x0005,                                         // Scenes (deprecated)
			ClusterIds.Descriptor,
			ClusterIds.Binding,
			ClusterIds.AccessControl,
			ClusterIds.Actions,
			ClusterIds.OtaSoftwareUpdateProvider,
			ClusterIds.OtaSoftwareUpdateRequestor,
			ClusterIds.LocalizationConfiguration,
			ClusterIds.TimeFormatLocalization,
			ClusterIds.UnitLocalization,
			ClusterIds.PowerSourceConfiguration,
			ClusterIds.GeneralCommissioning,
			ClusterIds.NetworkCommissioning,
			ClusterIds.DiagnosticLogs,
			ClusterIds.SoftwareDiagnostics,
			ClusterIds.EthernetNetworkDiagnostics,
			ClusterIds.TimeSynchronization,
			ClusterIds.AdministratorCommissioning,
			ClusterIds.OperationalCredentials,
			ClusterIds.GroupKeyManagement,
			ClusterIds.FixedLabel,
			ClusterIds.UserLabel,
			0x0042,                                         // Proxy Configuration
			0x0043,                                         // Proxy Discovery
			0x0044,                                         // Proxy Valid
			ClusterIds.IcdManagement,
			ClusterIds.ScenesManagement,
			0x0065,                                         // Groupcast
			0x0750,                                         // Ecosystem Information
			0x0751,                                         // Commissioner Control
			0x0752,                                         // Joint Fabric Datastore
			0x0753,                                         // Joint Fabric Administrator
			0x0801,                                         // TLS Certificate Management
			0x0802,                                         // TLS Client Management
			0x0450,                                         // Network Identity Management
			0x0451,                                         // Wi-Fi Network Management
			0x0452,                                         // Thread Border Router Management
			0x0453,                                         // Thread Network Directory
			0x0455                                          // Commissioning Proxy
		};

		/// <summary>
		/// Handler for infrastructure clusters.
		/// </summary>
		public InfrastructureClusterHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return System.Array.IndexOf(InfrastructureClusters, ClusterId) >= 0 ? Grade.Ok : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override bool HasValues(MatterClusterInfo Cluster) => false;

		/// <inheritdoc/>
		public override InteractionModel.AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return System.Array.Empty<InteractionModel.AttributePath>();
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
		}
	}
}
