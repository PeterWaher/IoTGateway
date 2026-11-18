using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.ControlParameters;
using Waher.Things.Matter.InteractionModel;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Interface for classes that map Matter clusters to sensor data fields, control parameters and commands.
	/// Cluster handlers are found using <see cref="Types.FindBest{InterfaceType, ObjectType}(ObjectType)"/>, based
	/// on the cluster ID. Implement this interface (with a default constructor) to add support for new clusters,
	/// or to override how existing clusters are handled.
	/// </summary>
	public interface IClusterHandler : IProcessingSupport<uint>
	{
		/// <summary>
		/// If the cluster contains values that should be reported during readout, or as momentary values when
		/// changes are reported.
		/// </summary>
		/// <param name="Cluster">Cluster information.</param>
		/// <returns>If the cluster has values of interest.</returns>
		bool HasValues(MatterClusterInfo Cluster);

		/// <summary>
		/// Gets the attribute paths to read when performing a readout of the cluster.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="Cluster">Cluster information.</param>
		/// <returns>Attribute paths.</returns>
		AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster);

		/// <summary>
		/// If events of the cluster should be subscribed to.
		/// </summary>
		/// <param name="Cluster">Cluster information.</param>
		/// <returns>If events are of interest.</returns>
		bool HasEvents(MatterClusterInfo Cluster);

		/// <summary>
		/// Adds sensor data fields, based on attribute values.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Values">Attribute values.</param>
		/// <param name="Fields">Field builder.</param>
		void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields);

		/// <summary>
		/// Adds sensor data fields, based on a reported event.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Event">Event report.</param>
		/// <param name="Fields">Field builder.</param>
		void AddEventFields(ClusterContext Context, EventReport Event, FieldBuilder Fields);

		/// <summary>
		/// If the cluster can be controlled using control parameters.
		/// </summary>
		/// <param name="Cluster">Cluster information.</param>
		/// <returns>If the cluster is controllable.</returns>
		bool IsControllable(MatterClusterInfo Cluster);

		/// <summary>
		/// Adds control parameters for the cluster.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Parameters">Control parameters.</param>
		void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters);

		/// <summary>
		/// Adds commands for the cluster.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Commands">Commands.</param>
		void AddCommands(ClusterContext Context, List<ICommand> Commands);
	}
}
