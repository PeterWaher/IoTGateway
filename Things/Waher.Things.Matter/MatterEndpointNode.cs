using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Matter.Clusters;
using Waher.Things.Matter.Commands;
using Waher.Things.Matter.DataModel;
using Waher.Things.Metering;

namespace Waher.Things.Matter
{
	/// <summary>
	/// Node representing an endpoint on a Matter device. Endpoints are created automatically when the device is
	/// commissioned or scanned. Clusters on the endpoint are mapped to sensor data fields, control parameters and commands.
	/// </summary>
	public class MatterEndpointNode : ProvisionedMeteringNode, ISensor, IActuator, IEncryptedProperties
	{
		private MatterClusterInfo[] clusters = null;
		private uint[] deviceTypes = null;
		private string label = string.Empty;
		private string pinCode = string.Empty;
		private int endpointId = 1;

		/// <summary>
		/// Node representing an endpoint on a Matter device.
		/// </summary>
		public MatterEndpointNode()
			: base()
		{
		}

		#region Properties

		/// <summary>
		/// Endpoint ID.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(200, "Endpoint ID:")]
		[ToolTip(201, "ID of the endpoint on the device.")]
		[Range(0, 65534)]
		[Required]
		public int EndpointId
		{
			get => this.endpointId;
			set => this.endpointId = value;
		}

		/// <summary>
		/// Label of the endpoint, as reported by the device (for bridged devices).
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(202, "Label:")]
		[ToolTip(203, "Label of the endpoint, as reported by the device.")]
		[ReadOnly]
		[DefaultValueStringEmpty]
		public string Label
		{
			get => this.label;
			set => this.label = value;
		}

		/// <summary>
		/// Device types of the endpoint.
		/// </summary>
		[DefaultValueNull]
		public uint[] DeviceTypes
		{
			get => this.deviceTypes;
			set => this.deviceTypes = value;
		}

		/// <summary>
		/// Server clusters available on the endpoint.
		/// </summary>
		[DefaultValueNull]
		public MatterClusterInfo[] Clusters
		{
			get => this.clusters;
			set => this.clusters = value;
		}

		/// <summary>
		/// Device types, as a human-readable string.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(204, "Device types:")]
		[ToolTip(205, "Device types implemented by the endpoint.")]
		public string DeviceTypesDescription
		{
			get
			{
				if (this.deviceTypes is null || this.deviceTypes.Length == 0)
					return string.Empty;

				StringBuilder sb = new StringBuilder();

				foreach (uint DeviceType in this.deviceTypes)
				{
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append(MatterDataModel.GetDeviceTypeName(DeviceType));
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// Clusters, as a human-readable string.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(206, "Clusters:")]
		[ToolTip(207, "Server clusters available on the endpoint.")]
		public string ClustersDescription
		{
			get
			{
				if (this.clusters is null || this.clusters.Length == 0)
					return string.Empty;

				StringBuilder sb = new StringBuilder();

				foreach (MatterClusterInfo Cluster in this.clusters)
				{
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append(Cluster.ToString());
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// PIN code used for remote operation of door locks.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(208, "PIN code:")]
		[ToolTip(209, "PIN code used when locking or unlocking door locks remotely, if required by the lock.")]
		[Masked]
		[Encrypted(32)]
		[DefaultValueStringEmpty]
		public string PinCode
		{
			get => this.pinCode;
			set => this.pinCode = value;
		}

		/// <summary>
		/// Properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[] { nameof(this.PinCode) };

		/// <summary>
		/// Checks if the endpoint has a given device type.
		/// </summary>
		/// <param name="DeviceType">Device type ID.</param>
		/// <returns>If the endpoint implements the device type.</returns>
		public bool HasDeviceType(uint DeviceType)
		{
			return !(this.deviceTypes is null) && Array.IndexOf(this.deviceTypes, DeviceType) >= 0;
		}

		/// <summary>
		/// Gets information about a cluster on the endpoint.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <returns>Cluster information, or null if not available.</returns>
		public MatterClusterInfo GetCluster(uint ClusterId)
		{
			return MatterClusterInfo.Find(this.clusters, ClusterId);
		}

		#endregion

		#region Node interface

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 210, "Matter Endpoint");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent.
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is MatterDeviceNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child.
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new Int32Parameter("Endpoint", await Language.GetStringAsync(typeof(MatterControllerNode), 211, "Endpoint"),
				this.endpointId));

			string DeviceTypes = this.DeviceTypesDescription;
			if (!string.IsNullOrEmpty(DeviceTypes))
			{
				Result.AddLast(new StringParameter("DeviceType", await Language.GetStringAsync(typeof(MatterControllerNode), 212, "Device Type"),
					DeviceTypes));
			}

			return Result;
		}

		/// <summary>
		/// Gets the device node to which the endpoint belongs.
		/// </summary>
		/// <returns>Device node.</returns>
		public async Task<MatterDeviceNode> GetDeviceAsync()
		{
			if (await this.GetParent() is MatterDeviceNode Device)
				return Device;
			else
				throw new Exception("Matter endpoint node not placed under a Matter device node.");
		}

		/// <summary>
		/// Gets cluster contexts for the clusters on the endpoint.
		/// </summary>
		/// <returns>Cluster contexts, with corresponding handlers.</returns>
		private async Task<List<KeyValuePair<ClusterContext, IClusterHandler>>> GetClusterContexts()
		{
			List<KeyValuePair<ClusterContext, IClusterHandler>> Result = new List<KeyValuePair<ClusterContext, IClusterHandler>>();

			if (this.clusters is null)
				return Result;

			MatterDeviceNode Device = await this.GetDeviceAsync();

			foreach (MatterClusterInfo Cluster in this.clusters)
			{
				IClusterHandler Handler = ClusterHandlers.Get(Cluster.ClusterId);
				ClusterContext Context = new ClusterContext(Device, this, (ushort)this.endpointId, Cluster);

				Result.Add(new KeyValuePair<ClusterContext, IClusterHandler>(Context, Handler));
			}

			return Result;
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Result = new List<ICommand>();
			Result.AddRange(await base.Commands);

			try
			{
				MatterDeviceNode Device = await this.GetDeviceAsync();

				Result.Add(new ReadAllAttributesCommand(Device, this));
				Result.Add(new ReadEventsCommand(Device, this));

				foreach (KeyValuePair<ClusterContext, IClusterHandler> P in await this.GetClusterContexts())
				{
					try
					{
						P.Value.AddCommands(P.Key, Result);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return Result;
		}

		#endregion

		#region ISensor

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public override bool IsReadable => !this.Disabled;

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async Task StartReadout(ISensorReadout Request)
		{
			try
			{
				MatterDeviceNode Device = await this.GetDeviceAsync();
				await Device.ReadoutAsync(Request, false, new MatterEndpointNode[] { this }, true);
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		#endregion

		#region IActuator

		/// <summary>
		/// If the node can be controlled.
		/// </summary>
		public override bool IsControllable
		{
			get
			{
				if (this.Disabled || this.clusters is null)
					return false;

				foreach (MatterClusterInfo Cluster in this.clusters)
				{
					if (ClusterHandlers.Get(Cluster.ClusterId).IsControllable(Cluster))
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Get control parameters for the actuator.
		/// </summary>
		/// <returns>Collection of control parameters for actuator.</returns>
		public async Task<ControlParameter[]> GetControlParameters()
		{
			List<ControlParameter> Result = new List<ControlParameter>();

			foreach (KeyValuePair<ClusterContext, IClusterHandler> P in await this.GetClusterContexts())
			{
				if (!P.Value.IsControllable(P.Key.Cluster))
					continue;

				try
				{
					P.Value.AddControlParameters(P.Key, Result);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Gets a control parameter, given its name.
		/// </summary>
		/// <param name="Name">Name of control parameter.</param>
		/// <returns>Control parameter, or null if not found.</returns>
		public async Task<ControlParameter> GetControlParameter(string Name)
		{
			foreach (ControlParameter Parameter in await this.GetControlParameters())
			{
				if (Parameter.Name == Name)
					return Parameter;
			}

			return null;
		}

		#endregion

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.NodeId + " (" + this.endpointId.ToString() + ")";
		}
	}
}
