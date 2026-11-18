using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ip;
using Waher.Things.Matter.Clusters;
using Waher.Things.Matter.Commands;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Things.Matter
{
	/// <summary>
	/// Node representing a Matter device (operational node) commissioned into the fabric of the parent
	/// <see cref="MatterControllerNode"/>. The endpoints of the device are represented by
	/// <see cref="MatterEndpointNode"/> child nodes.
	/// </summary>
	public class MatterDeviceNode : IpHost
	{
		/// <summary>
		/// Default UDP port used by Matter devices.
		/// </summary>
		public const int DefaultPort = 5540;

		/// <summary>
		/// Maximum number of paths to include in a single read request.
		/// </summary>
		public const int MaxPathsPerRead = 9;

		private readonly object synchObject = new object();
		private readonly Dictionary<(ushort, uint, uint), TlvElement> cache = new Dictionary<(ushort, uint, uint), TlvElement>();
		private readonly Dictionary<ushort, FieldNameRegistry> fieldNames = new Dictionary<ushort, FieldNameRegistry>();
		private MatterNode matterNode = null;
		private MatterController matterNodeController = null;
		private Subscription subscription = null;
		private Timer resubscribeTimer = null;
		private Timer rescanTimer = null;
		private HashSet<ushort> knownEndpoints = null;
		private ulong? lastEventNumber = null;
		private int resubscribeAttempts = 0;
		private bool subscriptionRequested = false;
		private bool subscribing = false;
		private bool useWildcardSubscription = false;
		private bool unpaired = false;
		private string subscriptionSettings = null;

		private MatterClusterInfo[] rootClusters = null;
		private string operationalNodeId = string.Empty;
		private string vendorName = string.Empty;
		private string productName = string.Empty;
		private string softwareVersion = string.Empty;
		private string hardwareVersion = string.Empty;
		private string uniqueId = string.Empty;
		private int vendorIdentifier = 0;
		private int productIdentifier = 0;
		private int port = DefaultPort;
		private int minInterval = 0;
		private int maxInterval = 60;
		private bool subscribe = true;
		private bool unpairOnRemoval = true;
		private bool includeEndpointsInReadout = true;

		/// <summary>
		/// Node representing a Matter device (operational node).
		/// </summary>
		public MatterDeviceNode()
			: base()
		{
			this.PingHost = false;
		}

		#region Properties

		/// <summary>
		/// Operational node ID of the device in the fabric, as a hexadecimal string.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(101, "Operational Node ID:")]
		[ToolTip(102, "Operational node ID of the device in the fabric (hexadecimal).")]
		[RegularExpression("^[0-9A-Fa-f]{1,16}$")]
		[Required]
		public string OperationalNodeId
		{
			get => this.operationalNodeId;
			set => this.operationalNodeId = value;
		}

		/// <summary>
		/// UDP port of the device.
		/// </summary>
		[Page(136, "IP")]
		[Header(103, "UDP Port:")]
		[ToolTip(104, "UDP port of the device. The address is updated automatically if the device announces a new address on the network.")]
		[Range(1, 65535)]
		[DefaultValue(DefaultPort)]
		public int Port
		{
			get => this.port;
			set => this.port = value;
		}

		/// <summary>
		/// If changes on the device should be subscribed to, and reported as momentary values.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(105, "Subscribe to changes.")]
		[ToolTip(106, "If the gateway should subscribe to changes on the device, and report them as momentary values.")]
		[DefaultValue(true)]
		public bool Subscribe
		{
			get => this.subscribe;
			set => this.subscribe = value;
		}

		/// <summary>
		/// Minimum interval between reports, in seconds.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(107, "Minimum interval (s):")]
		[ToolTip(108, "Minimum interval between change reports, in seconds.")]
		[Range(0, 3600)]
		[DefaultValue(0)]
		public int MinInterval
		{
			get => this.minInterval;
			set => this.minInterval = value;
		}

		/// <summary>
		/// Maximum interval between reports, in seconds. Used to detect loss of connection.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(109, "Maximum interval (s):")]
		[ToolTip(110, "Maximum interval between reports, in seconds. The device may choose a longer interval. Used to detect if the device is lost.")]
		[Range(1, 3600)]
		[DefaultValue(60)]
		public int MaxInterval
		{
			get => this.maxInterval;
			set => this.maxInterval = value;
		}

		/// <summary>
		/// If the device should be removed from the fabric, when the node is deleted.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(111, "Remove device from fabric when node is deleted.")]
		[ToolTip(112, "If the gateway should remove itself as administrator from the device, when the node is deleted.")]
		[DefaultValue(true)]
		public bool UnpairOnRemoval
		{
			get => this.unpairOnRemoval;
			set => this.unpairOnRemoval = value;
		}

		/// <summary>
		/// If endpoints should be included when the device is read.
		/// </summary>
		[Page(100, "Matter", 100)]
		[Header(113, "Include endpoints in readout.")]
		[ToolTip(114, "If values from all endpoints should be included when reading the device node.")]
		[DefaultValue(true)]
		public bool IncludeEndpointsInReadout
		{
			get => this.includeEndpointsInReadout;
			set => this.includeEndpointsInReadout = value;
		}

		/// <summary>
		/// Vendor name, as reported by the device.
		/// </summary>
		[Page(115, "Device", 110)]
		[Header(116, "Vendor:")]
		[ToolTip(117, "Vendor name, as reported by the device.")]
		[ReadOnly]
		[DefaultValueStringEmpty]
		public string VendorName
		{
			get => this.vendorName;
			set => this.vendorName = value;
		}

		/// <summary>
		/// Vendor ID, as reported by the device.
		/// </summary>
		[Page(115, "Device", 110)]
		[Header(118, "Vendor ID:")]
		[ToolTip(119, "Vendor ID, as reported by the device.")]
		[ReadOnly]
		[DefaultValue(0)]
		public int VendorIdentifier
		{
			get => this.vendorIdentifier;
			set => this.vendorIdentifier = value;
		}

		/// <summary>
		/// Product name, as reported by the device.
		/// </summary>
		[Page(115, "Device", 110)]
		[Header(120, "Product:")]
		[ToolTip(121, "Product name, as reported by the device.")]
		[ReadOnly]
		[DefaultValueStringEmpty]
		public string ProductName
		{
			get => this.productName;
			set => this.productName = value;
		}

		/// <summary>
		/// Product ID, as reported by the device.
		/// </summary>
		[Page(115, "Device", 110)]
		[Header(122, "Product ID:")]
		[ToolTip(123, "Product ID, as reported by the device.")]
		[ReadOnly]
		[DefaultValue(0)]
		public int ProductIdentifier
		{
			get => this.productIdentifier;
			set => this.productIdentifier = value;
		}

		/// <summary>
		/// Software version, as reported by the device.
		/// </summary>
		[Page(115, "Device", 110)]
		[Header(126, "Software Version:")]
		[ToolTip(127, "Software version, as reported by the device.")]
		[ReadOnly]
		[DefaultValueStringEmpty]
		public string SoftwareVersion
		{
			get => this.softwareVersion;
			set => this.softwareVersion = value;
		}

		/// <summary>
		/// Hardware version, as reported by the device.
		/// </summary>
		[Page(115, "Device", 110)]
		[Header(128, "Hardware Version:")]
		[ToolTip(129, "Hardware version, as reported by the device.")]
		[ReadOnly]
		[DefaultValueStringEmpty]
		public string HardwareVersion
		{
			get => this.hardwareVersion;
			set => this.hardwareVersion = value;
		}

		/// <summary>
		/// Unique ID, as reported by the device.
		/// </summary>
		[Page(115, "Device", 110)]
		[Header(130, "Unique ID:")]
		[ToolTip(131, "Unique ID, as reported by the device.")]
		[ReadOnly]
		[DefaultValueStringEmpty]
		public string UniqueId
		{
			get => this.uniqueId;
			set => this.uniqueId = value;
		}

		/// <summary>
		/// Clusters available on the root endpoint (endpoint 0) of the device.
		/// </summary>
		[DefaultValueNull]
		public MatterClusterInfo[] RootClusters
		{
			get => this.rootClusters;
			set => this.rootClusters = value;
		}

		/// <summary>
		/// Tries to get the operational node ID of the device.
		/// </summary>
		/// <param name="NodeId">Operational node ID.</param>
		/// <returns>If a valid node ID is available.</returns>
		public bool TryGetOperationalNodeId(out ulong NodeId)
		{
			return ulong.TryParse(this.operationalNodeId, NumberStyles.HexNumber, null, out NodeId);
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
			return Language.GetStringAsync(typeof(MatterControllerNode), 132, "Matter Device");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent.
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is MatterControllerNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child.
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is MatterEndpointNode);
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

			Result.AddLast(new StringParameter("MatterNodeId", await Language.GetStringAsync(typeof(MatterControllerNode), 133, "Node ID"),
				this.operationalNodeId));

			if (!string.IsNullOrEmpty(this.productName))
			{
				Result.AddLast(new StringParameter("Product", await Language.GetStringAsync(typeof(MatterControllerNode), 134, "Product"),
					this.productName));
			}

			if (!string.IsNullOrEmpty(this.vendorName))
			{
				Result.AddLast(new StringParameter("Vendor", await Language.GetStringAsync(typeof(MatterControllerNode), 135, "Vendor"),
					this.vendorName));
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

			Result.Add(new RescanEndpointsCommand(this));
			Result.Add(new ReconnectCommand(this));
			Result.Add(new ReadAllAttributesCommand(this, null));
			Result.Add(new ReadEventsCommand(this, null));
			Result.Add(new ShareDeviceCommand(this));
			Result.Add(new RemoveFromFabricCommand(this));

			if (!(this.rootClusters is null))
			{
				foreach (MatterClusterInfo Cluster in this.rootClusters)
				{
					if (Cluster.ClusterId == ClusterIds.BasicInformation || Cluster.ClusterId == ClusterIds.GeneralDiagnostics)
						continue;

					IClusterHandler Handler = ClusterHandlers.Get(Cluster.ClusterId);
					if (Handler is InfrastructureClusterHandler)
						continue;

					try
					{
						Handler.AddCommands(new ClusterContext(this, null, 0, Cluster), Result);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Persists changes to the node, and generates a node updated event.
		/// </summary>
		protected override async Task NodeUpdated()
		{
			await base.NodeUpdated();

			string Settings = this.GetSubscriptionSettings();
			bool Restart;

			lock (this.synchObject)
			{
				Restart = this.subscriptionRequested && !(this.subscriptionSettings is null) && this.subscriptionSettings != Settings;
			}

			if (Restart)
			{
				this.StopSubscription();
				this.StartSubscription();
			}
			else if (this.subscribe && !this.Disabled && !this.subscriptionRequested && !(this.matterNode is null))
				this.StartSubscription();
			else if ((!this.subscribe || this.Disabled) && this.subscriptionRequested)
				this.StopSubscription();
		}

		private string GetSubscriptionSettings()
		{
			return this.operationalNodeId + "|" + this.subscribe.ToString() + "|" + this.minInterval.ToString() + "|" +
				this.maxInterval.ToString() + "|" + this.Disabled.ToString();
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public override async Task DestroyAsync()
		{
			this.StopSubscription();
			await base.DestroyAsync();
		}

		/// <summary>
		/// Called by the controller node, when the device node is about to be removed.
		/// </summary>
		/// <param name="Controller">Controller node.</param>
		internal async Task Removing(MatterControllerNode Controller)
		{
			this.StopSubscription();

			if (this.unpaired || !this.unpairOnRemoval || !this.TryGetOperationalNodeId(out ulong NodeId))
				return;

			try
			{
				MatterController MatterController = await Controller.GetControllerAsync();
				MatterNode Node = MatterController.GetNode(NodeId, await this.GetKnownAddressAsync());
				Task T = Node.RemoveFromFabricAsync();

				if (await Task.WhenAny(T, Task.Delay(15000)) != T)
					throw new TimeoutException("Timeout.");

				await T;
				await MatterController.ForgetNode(NodeId);

				Log.Informational("Matter device removed from fabric.", this.NodeId);
			}
			catch (Exception ex)
			{
				Log.Warning("Unable to remove Matter device from fabric: " + ex.Message, this.NodeId);
			}
		}

		/// <summary>
		/// Removes the device from the fabric (unpairs the device). After this, the device is no longer accessible
		/// by the controller.
		/// </summary>
		public async Task UnpairAsync()
		{
			if (!this.TryGetOperationalNodeId(out ulong NodeId))
				throw new Exception("Invalid operational node ID.");

			this.StopSubscription();

			MatterControllerNode ControllerNode = await this.GetControllerNodeAsync();
			MatterController Controller = await ControllerNode.GetControllerAsync();
			MatterNode Node = await this.GetMatterNodeAsync();

			await Node.RemoveFromFabricAsync();
			await Controller.ForgetNode(NodeId);

			lock (this.synchObject)
			{
				if (!(this.matterNode is null))
				{
					this.matterNode.OnAddressChanged -= this.Node_OnAddressChanged;
					this.matterNode = null;
				}

				this.matterNodeController = null;
				this.unpaired = true;
			}

			await this.LogInformationAsync("Commissioning", "Device removed from fabric.");
		}

		#endregion

		#region Connection

		/// <summary>
		/// Gets the parent controller node.
		/// </summary>
		/// <returns>Controller node.</returns>
		public async Task<MatterControllerNode> GetControllerNodeAsync()
		{
			if (await this.GetParent() is MatterControllerNode ControllerNode)
				return ControllerNode;
			else
				throw new Exception("Matter device node not placed under a Matter controller node.");
		}

		/// <summary>
		/// Gets the last known IP endpoint of the device, if known.
		/// </summary>
		/// <returns>IP endpoint, or null if not known.</returns>
		public async Task<IPEndPoint> GetKnownAddressAsync()
		{
			if (string.IsNullOrEmpty(this.Host))
				return null;

			if (IPAddress.TryParse(this.Host, out IPAddress Address))
				return new IPEndPoint(Address, this.port);

			try
			{
				IPAddress[] Addresses = await Dns.GetHostAddressesAsync(this.Host);

				if (Addresses.Length > 0)
					return new IPEndPoint(Addresses[0], this.port);
			}
			catch (Exception)
			{
				// Ignore
			}

			return null;
		}

		/// <summary>
		/// Gets the Matter node object representing the device, managing the secure session.
		/// </summary>
		/// <returns>Matter node.</returns>
		public async Task<MatterNode> GetMatterNodeAsync()
		{
			if (this.unpaired)
				throw new Exception("Device has been removed from the fabric.");

			if (!this.TryGetOperationalNodeId(out ulong NodeId))
				throw new Exception("Invalid operational node ID.");

			MatterControllerNode ControllerNode = await this.GetControllerNodeAsync();
			MatterController Controller = await ControllerNode.GetControllerAsync();

			lock (this.synchObject)
			{
				if (!(this.matterNode is null) && this.matterNodeController == Controller && this.matterNode.NodeId == NodeId)
					return this.matterNode;
			}

			IPEndPoint Address = await this.GetKnownAddressAsync();
			MatterNode Node = Controller.GetNode(NodeId, Address);

			lock (this.synchObject)
			{
				if (this.matterNode != Node)
				{
					if (!(this.matterNode is null))
						this.matterNode.OnAddressChanged -= this.Node_OnAddressChanged;

					Node.OnAddressChanged += this.Node_OnAddressChanged;
					this.matterNode = Node;
				}

				this.matterNodeController = Controller;
			}

			return Node;
		}

		private async Task Node_OnAddressChanged(object Sender, IPEndPoint Address)
		{
			try
			{
				string Host = Address.Address.ToString();

				if (Host != this.Host || Address.Port != this.port)
				{
					this.Host = Host;
					this.port = Address.Port;

					await this.UpdateAsync();
					await this.LogInformationAsync("Address", "Device address changed to " + Address.ToString());
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Closes the secure session with the device.
		/// </summary>
		public async Task DisconnectAsync()
		{
			MatterNode Node;

			lock (this.synchObject)
			{
				Node = this.matterNode;
			}

			if (!(Node is null))
				await Node.DisconnectAsync();
		}

		#endregion

		#region Interactions

		/// <summary>
		/// Reads attributes from the device, and updates the attribute cache. Large requests are divided into
		/// multiple read requests.
		/// </summary>
		/// <param name="Paths">Attribute paths.</param>
		/// <returns>Read result.</returns>
		public async Task<ReadResult> ReadAttributesAsync(params AttributePath[] Paths)
		{
			MatterNode Node = await this.GetMatterNodeAsync();
			ReadResult Result;
			int c = Paths.Length;

			if (c <= MaxPathsPerRead)
				Result = await Node.ReadAttributesAsync(Paths);
			else
			{
				Result = new ReadResult();

				for (int i = 0; i < c; i += MaxPathsPerRead)
				{
					int n = Math.Min(MaxPathsPerRead, c - i);
					AttributePath[] Part = new AttributePath[n];
					Array.Copy(Paths, i, Part, 0, n);

					ReadResult PartResult = await Node.ReadAttributesAsync(Part);

					foreach (AttributeReport Report in PartResult.Attributes)
						Result.Add(Report);
				}
			}

			this.UpdateCache(Result);

			return Result;
		}

		/// <summary>
		/// Reads an attribute from the device, and updates the attribute cache.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value.</returns>
		public async Task<TlvElement> ReadAttributeAsync(ushort EndpointId, uint ClusterId, uint AttributeId)
		{
			ReadResult Result = await this.ReadAttributesAsync(new AttributePath(EndpointId, ClusterId, AttributeId));
			AttributeReport Report = Result.GetReport(EndpointId, ClusterId, AttributeId);

			if (Report is null)
				throw new MatterException("Attribute not reported.");

			if (!Report.HasValue)
				throw new ImStatusException(Report.Status, "Unable to read attribute " + Report.Path.ToString());

			return Report.Data;
		}

		/// <summary>
		/// Writes an attribute on the device.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value to write.</param>
		/// <param name="Timed">If a timed interaction is required.</param>
		public async Task WriteAttributeAsync(ushort EndpointId, uint ClusterId, uint AttributeId, object Value, bool Timed)
		{
			MatterNode Node = await this.GetMatterNodeAsync();
			await Node.WriteAttributeAsync(EndpointId, ClusterId, AttributeId, Value, Timed);
		}

		/// <summary>
		/// Invokes a command on the device.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="CommandId">Command ID.</param>
		/// <param name="Fields">Command fields.</param>
		/// <param name="Timed">If a timed interaction is required.</param>
		/// <returns>Invoke result.</returns>
		public async Task<InvokeResult> InvokeAsync(ushort EndpointId, uint ClusterId, uint CommandId, TlvElement Fields, bool Timed)
		{
			MatterNode Node = await this.GetMatterNodeAsync();
			return await Node.InvokeAsync(EndpointId, ClusterId, CommandId, Fields, Timed);
		}

		#endregion

		#region Attribute cache

		private void UpdateCache(ReadResult Result)
		{
			lock (this.cache)
			{
				foreach (AttributeReport Report in Result.Attributes)
				{
					AttributePath Path = Report.Path;

					if (Report.HasValue && Path.EndpointId.HasValue && Path.ClusterId.HasValue && Path.AttributeId.HasValue)
						this.cache[(Path.EndpointId.Value, Path.ClusterId.Value, Path.AttributeId.Value)] = Report.Data;
				}
			}
		}

		/// <summary>
		/// Gets the last known value of an attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value, or null if not known.</returns>
		public TlvElement GetCachedValue(ushort EndpointId, uint ClusterId, uint AttributeId)
		{
			lock (this.cache)
			{
				return this.cache.TryGetValue((EndpointId, ClusterId, AttributeId), out TlvElement Value) ? Value : null;
			}
		}

		/// <summary>
		/// Gets the field name registry of an endpoint.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <returns>Field name registry.</returns>
		public FieldNameRegistry GetFieldNames(ushort EndpointId)
		{
			lock (this.fieldNames)
			{
				if (!this.fieldNames.TryGetValue(EndpointId, out FieldNameRegistry Result))
				{
					Result = new FieldNameRegistry();
					this.fieldNames[EndpointId] = Result;
				}

				return Result;
			}
		}

		#endregion

		#region Endpoints

		/// <summary>
		/// Gets the endpoint nodes of the device.
		/// </summary>
		/// <returns>Endpoint nodes.</returns>
		public async Task<MatterEndpointNode[]> GetEndpointNodesAsync()
		{
			List<MatterEndpointNode> Result = new List<MatterEndpointNode>();

			foreach (INode Child in await this.ChildNodes)
			{
				if (Child is MatterEndpointNode Endpoint)
					Result.Add(Endpoint);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Finds an endpoint node, given its endpoint ID.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <returns>Endpoint node, or null if not found.</returns>
		public async Task<MatterEndpointNode> FindEndpointNodeAsync(ushort EndpointId)
		{
			foreach (INode Child in await this.ChildNodes)
			{
				if (Child is MatterEndpointNode Endpoint && Endpoint.EndpointId == EndpointId)
					return Endpoint;
			}

			return null;
		}

		/// <summary>
		/// Scans the device for endpoints and clusters, updating device information, and creating or updating
		/// endpoint nodes.
		/// </summary>
		/// <returns>Endpoint nodes.</returns>
		public async Task<MatterEndpointNode[]> RescanAsync()
		{
			MatterNode Node = await this.GetMatterNodeAsync();
			EndpointInfo[] Endpoints = await Node.GetEndpointsAsync();

			ReadResult Globals = await this.ReadAttributesAsync(
				new AttributePath(null, null, GlobalAttributeIds.AttributeList),
				new AttributePath(null, null, GlobalAttributeIds.AcceptedCommandList),
				new AttributePath(null, null, GlobalAttributeIds.FeatureMap),
				new AttributePath(null, null, GlobalAttributeIds.ClusterRevision));

			await this.UpdateDeviceInformation();
			MatterClusterInfo[] RootClusters = null;
			Dictionary<ushort, MatterEndpointNode> Existing = new Dictionary<ushort, MatterEndpointNode>();
			List<MatterEndpointNode> Result = new List<MatterEndpointNode>();

			foreach (MatterEndpointNode EndpointNode in await this.GetEndpointNodesAsync())
				Existing[(ushort)EndpointNode.EndpointId] = EndpointNode;

			foreach (EndpointInfo Endpoint in Endpoints)
			{
				MatterClusterInfo[] Clusters = GetClusterInfo(Endpoint, Globals);

				if (Endpoint.EndpointId == 0)
				{
					RootClusters = Clusters;
					continue;
				}

				if (IsAggregatorOnly(Endpoint))
					continue;

				uint[] DeviceTypes = new uint[Endpoint.DeviceTypes.Count];
				for (int i = 0; i < DeviceTypes.Length; i++)
					DeviceTypes[i] = Endpoint.DeviceTypes[i].DeviceType;

				if (Existing.TryGetValue(Endpoint.EndpointId, out MatterEndpointNode EndpointNode))
				{
					Existing.Remove(Endpoint.EndpointId);

					EndpointNode.DeviceTypes = DeviceTypes;
					EndpointNode.Clusters = Clusters;

					if (!string.IsNullOrEmpty(Endpoint.Label))
						EndpointNode.Label = Endpoint.Label;

					await EndpointNode.UpdateAsync();
					await EndpointNode.RemoveWarningAsync("Endpoint");
				}
				else
				{
					EndpointNode = new MatterEndpointNode()
					{
						NodeId = await GetUniqueNodeId(this.NodeId + ", " + GetEndpointName(Endpoint)),
						EndpointId = Endpoint.EndpointId,
						DeviceTypes = DeviceTypes,
						Clusters = Clusters,
						Label = Endpoint.Label ?? string.Empty
					};

					await this.AddAsync(EndpointNode);
				}

				Result.Add(EndpointNode);
			}

			foreach (MatterEndpointNode Stale in Existing.Values)
				await Stale.LogWarningAsync("Endpoint", "Endpoint is no longer available on the device.");

			HashSet<ushort> Known = new HashSet<ushort>();

			foreach (EndpointInfo Endpoint in Endpoints)
			{
				if (Endpoint.EndpointId != 0)
					Known.Add(Endpoint.EndpointId);
			}

			lock (this.synchObject)
			{
				this.knownEndpoints = Known;
			}

			this.rootClusters = RootClusters ?? Array.Empty<MatterClusterInfo>();

			await this.UpdateAsync();

			await this.RemoveErrorAsync("Rescan");

			bool Resubscribe;

			lock (this.synchObject)
			{
				Resubscribe = this.subscriptionRequested && !(this.subscription is null);
			}

			if (Resubscribe)
			{
				this.StopSubscription();
				this.StartSubscription();
			}

			return Result.ToArray();
		}

		private static bool IsAggregatorOnly(EndpointInfo Endpoint)
		{
			if (Endpoint.DeviceTypes.Count == 0)
				return false;

			foreach (DeviceTypeInfo DeviceType in Endpoint.DeviceTypes)
			{
				if (DeviceType.DeviceType != DeviceTypeIds.Aggregator)
					return false;
			}

			return true;
		}

		private static string GetEndpointName(EndpointInfo Endpoint)
		{
			if (!string.IsNullOrEmpty(Endpoint.Label))
				return Endpoint.Label;

			foreach (DeviceTypeInfo DeviceType in Endpoint.DeviceTypes)
			{
				DeviceTypeDefinition Definition = MatterDataModel.GetDeviceType(DeviceType.DeviceType);

				if (!(Definition is null) && Definition.IsApplication)
					return Definition.Name;
			}

			foreach (DeviceTypeInfo DeviceType in Endpoint.DeviceTypes)
			{
				DeviceTypeDefinition Definition = MatterDataModel.GetDeviceType(DeviceType.DeviceType);

				if (!(Definition is null) && DeviceType.DeviceType != DeviceTypeIds.BridgedNode)
					return Definition.Name;
			}

			return "Endpoint " + Endpoint.EndpointId.ToString();
		}

		private static MatterClusterInfo[] GetClusterInfo(EndpointInfo Endpoint, ReadResult Globals)
		{
			List<MatterClusterInfo> Result = new List<MatterClusterInfo>();

			foreach (uint ClusterId in Endpoint.ServerClusters)
			{
				MatterClusterInfo Info = new MatterClusterInfo(ClusterId)
				{
					FeatureMap = Globals.GetValue(Endpoint.EndpointId, ClusterId, GlobalAttributeIds.FeatureMap)?.AsUInt32() ?? 0,
					Revision = Globals.GetValue(Endpoint.EndpointId, ClusterId, GlobalAttributeIds.ClusterRevision)?.AsUInt32() ?? 0,
					Attributes = GetIdList(Globals.GetValue(Endpoint.EndpointId, ClusterId, GlobalAttributeIds.AttributeList)),
					Commands = GetIdList(Globals.GetValue(Endpoint.EndpointId, ClusterId, GlobalAttributeIds.AcceptedCommandList))
				};

				Result.Add(Info);
			}

			Result.Sort((x, y) => x.ClusterId.CompareTo(y.ClusterId));

			return Result.ToArray();
		}

		private static uint[] GetIdList(TlvElement List)
		{
			if (List is null || !(List.IsArray || List.IsList))
				return null;

			List<uint> Result = new List<uint>();

			foreach (TlvElement Item in List.Children)
			{
				if (Item.IsInteger)
					Result.Add(Item.AsUInt32());
			}

			Result.Sort();

			return Result.ToArray();
		}

		private async Task<bool> UpdateDeviceInformation()
		{
			ReadResult Info = await this.ReadAttributesAsync(
				new AttributePath(0, ClusterIds.BasicInformation, 0x0001),
				new AttributePath(0, ClusterIds.BasicInformation, 0x0002),
				new AttributePath(0, ClusterIds.BasicInformation, 0x0003),
				new AttributePath(0, ClusterIds.BasicInformation, 0x0004),
				new AttributePath(0, ClusterIds.BasicInformation, 0x0008),
				new AttributePath(0, ClusterIds.BasicInformation, 0x000a),
				new AttributePath(0, ClusterIds.BasicInformation, 0x000d),
				new AttributePath(0, ClusterIds.BasicInformation, 0x000f),
				new AttributePath(0, ClusterIds.BasicInformation, 0x0012));

			string VendorName = Info.GetValue(0, ClusterIds.BasicInformation, 0x0001)?.AsString() ?? this.vendorName;
			int VendorId = (int)(Info.GetValue(0, ClusterIds.BasicInformation, 0x0002)?.AsUInt16() ?? (ushort)this.vendorIdentifier);
			string ProductName = Info.GetValue(0, ClusterIds.BasicInformation, 0x0003)?.AsString() ?? this.productName;
			int ProductId = (int)(Info.GetValue(0, ClusterIds.BasicInformation, 0x0004)?.AsUInt16() ?? (ushort)this.productIdentifier);
			string HardwareVersion = Info.GetValue(0, ClusterIds.BasicInformation, 0x0008)?.AsString() ?? this.hardwareVersion;
			string SoftwareVersion = Info.GetValue(0, ClusterIds.BasicInformation, 0x000a)?.AsString() ?? this.softwareVersion;
			string SerialNumber = Info.GetValue(0, ClusterIds.BasicInformation, 0x000f)?.AsString();
			string ProductUrl = Info.GetValue(0, ClusterIds.BasicInformation, 0x000d)?.AsString();
			string UniqueId = Info.GetValue(0, ClusterIds.BasicInformation, 0x0012)?.AsString() ?? this.uniqueId;

			bool Changed =
				VendorName != this.vendorName ||
				VendorId != this.vendorIdentifier ||
				ProductName != this.productName ||
				ProductId != this.productIdentifier ||
				HardwareVersion != this.hardwareVersion ||
				SoftwareVersion != this.softwareVersion ||
				(!string.IsNullOrEmpty(SerialNumber) && SerialNumber != this.SerialNumber) ||
				UniqueId != this.uniqueId;

			this.vendorName = VendorName;
			this.vendorIdentifier = VendorId;
			this.productName = ProductName;
			this.productIdentifier = ProductId;
			this.hardwareVersion = HardwareVersion;
			this.softwareVersion = SoftwareVersion;
			this.uniqueId = UniqueId;

			if (!string.IsNullOrEmpty(SerialNumber))
				this.SerialNumber = SerialNumber;

			if (string.IsNullOrEmpty(this.Model) && !string.IsNullOrEmpty(ProductName))
			{
				this.Model = ProductName;
				Changed = true;
			}

			if (string.IsNullOrEmpty(this.ProductUrl) && !string.IsNullOrEmpty(ProductUrl))
			{
				this.ProductUrl = ProductUrl;
				Changed = true;
			}

			return Changed;
		}

		private void ScheduleRescan()
		{
			lock (this.synchObject)
			{
				this.rescanTimer?.Dispose();
				this.rescanTimer = new Timer(async (_) =>
				{
					try
					{
						await this.RescanAsync();
					}
					catch (Exception ex)
					{
						await this.LogErrorAsync("Rescan", "Unable to scan device endpoints: " + ex.Message);
					}
				}, null, 2000, Timeout.Infinite);
			}
		}

		#endregion

		#region Readout

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public override bool IsReadable => !this.Disabled;

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public override async Task StartReadout(ISensorReadout Request)
		{
			try
			{
				if (this.PingHost && !string.IsNullOrEmpty(this.Host))
				{
					try
					{
						using (Ping Icmp = new Ping())
						{
							PingReply Reply = await Icmp.SendPingAsync(this.Host, 2000);

							if (Reply.Status == IPStatus.Success)
							{
								await Request.ReportFields(false, new QuantityField(this, DateTime.UtcNow, "Ping", Reply.RoundtripTime, 0, "ms",
									FieldType.Status, FieldQoS.AutomaticReadout));
							}
							else
								await Request.ReportErrors(false, new ThingError(this, "Ping failed: " + Reply.Status.ToString()));
						}
					}
					catch (Exception ex)
					{
						await Request.ReportErrors(false, new ThingError(this, "Ping failed: " + ex.Message));
					}
				}

				MatterEndpointNode[] Endpoints = this.includeEndpointsInReadout ? await this.GetEndpointNodesAsync() : Array.Empty<MatterEndpointNode>();

				await this.ReadoutAsync(Request, true, Endpoints, true);
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		private class ReadTarget
		{
			public ClusterContext Context;
			public IClusterHandler Handler;
			public MeteringNode Node;
		}

		/// <summary>
		/// Performs a readout of the device and/or a set of its endpoints.
		/// </summary>
		/// <param name="Request">Readout request.</param>
		/// <param name="IncludeDevice">If device-level values (from the root endpoint) are to be included.</param>
		/// <param name="Endpoints">Endpoints to read.</param>
		/// <param name="Done">If the readout is done after reporting the values.</param>
		public async Task ReadoutAsync(ISensorReadout Request, bool IncludeDevice, IEnumerable<MatterEndpointNode> Endpoints, bool Done)
		{
			DateTime Now = DateTime.UtcNow;
			List<ReadTarget> Targets = new List<ReadTarget>();
			List<AttributePath> Paths = new List<AttributePath>();
			List<ThingError> Errors = new List<ThingError>();

			if (IncludeDevice && !(this.rootClusters is null))
			{
				foreach (MatterClusterInfo Cluster in this.rootClusters)
					this.AddTarget(Targets, Paths, null, 0, Cluster, Request);
			}

			if (!(Endpoints is null))
			{
				foreach (MatterEndpointNode Endpoint in Endpoints)
				{
					if (Endpoint.Disabled || Endpoint.Clusters is null)
						continue;

					foreach (MatterClusterInfo Cluster in Endpoint.Clusters)
						this.AddTarget(Targets, Paths, Endpoint, (ushort)Endpoint.EndpointId, Cluster, Request);
				}
			}

			ReadResult Result = Paths.Count > 0 ? await this.ReadAttributesAsync(Paths.ToArray()) : new ReadResult();
			Dictionary<(ushort, uint), Dictionary<uint, TlvElement>> Grouped = Group(Result, Errors, this);

			foreach (ReadTarget Target in Targets)
			{
				if (!Grouped.TryGetValue((Target.Context.EndpointId, Target.Context.ClusterId), out Dictionary<uint, TlvElement> Attributes))
					Attributes = new Dictionary<uint, TlvElement>();

				FieldBuilder Fields = new FieldBuilder(Target.Node, Now, Request, this.GetFieldNames(Target.Context.EndpointId));
				ClusterValues Values = new ClusterValues(Target.Context, Attributes)
				{
					IsReadout = true
				};

				try
				{
					Target.Handler.AddFields(Target.Context, Values, Fields);
				}
				catch (Exception ex)
				{
					Errors.Add(new ThingError(Target.Node, Now, Target.Context.ClusterName + ": " + ex.Message));
				}

				if (Fields.Count > 0)
					await Request.ReportFields(false, Fields.Fields);
			}

			if (Errors.Count > 0)
				await Request.ReportErrors(Done, Errors.ToArray());
			else if (Done)
				await Request.ReportFields(true);
		}

		private void AddTarget(List<ReadTarget> Targets, List<AttributePath> Paths, MatterEndpointNode Endpoint, ushort EndpointId,
			MatterClusterInfo Cluster, ISensorReadout Request)
		{
			IClusterHandler Handler = ClusterHandlers.Get(Cluster.ClusterId);
			if (!Handler.HasValues(Cluster))
				return;

			if (Cluster.ClusterId == ClusterIds.BasicInformation && !(Request is null) && !Request.IsIncluded(FieldType.Identity | FieldType.Status))
				return;

			ClusterContext Context = new ClusterContext(this, Endpoint, EndpointId, Cluster);
			AttributePath[] ClusterPaths = Handler.GetReadoutPaths(EndpointId, Cluster);

			if (ClusterPaths is null || ClusterPaths.Length == 0)
				return;

			Paths.AddRange(ClusterPaths);
			Targets.Add(new ReadTarget()
			{
				Context = Context,
				Handler = Handler,
				Node = (MeteringNode)Endpoint ?? this
			});
		}

		private static Dictionary<(ushort, uint), Dictionary<uint, TlvElement>> Group(ReadResult Result, List<ThingError> Errors,
			MeteringNode Node)
		{
			Dictionary<(ushort, uint), Dictionary<uint, TlvElement>> Grouped = new Dictionary<(ushort, uint), Dictionary<uint, TlvElement>>();

			foreach (AttributeReport Report in Result.Attributes)
			{
				AttributePath Path = Report.Path;

				if (!Path.EndpointId.HasValue || !Path.ClusterId.HasValue || !Path.AttributeId.HasValue)
					continue;

				if (!Report.HasValue)
				{
					if (!(Errors is null) && !(Report.Status is null))
					{
						switch (Report.Status.Status)
						{
							case ImStatusCode.UnsupportedAttribute:
							case ImStatusCode.UnsupportedCluster:
							case ImStatusCode.UnsupportedEndpoint:
							case ImStatusCode.UnsupportedRead:
								break;

							default:
								Errors.Add(new ThingError(Node, Path.ToString() + ": " + Report.Status.ToString()));
								break;
						}
					}

					continue;
				}

				(ushort, uint) Key = (Path.EndpointId.Value, Path.ClusterId.Value);

				if (!Grouped.TryGetValue(Key, out Dictionary<uint, TlvElement> Attributes))
				{
					Attributes = new Dictionary<uint, TlvElement>();
					Grouped[Key] = Attributes;
				}

				Attributes[Path.AttributeId.Value] = Report.Data;
			}

			return Grouped;
		}

		#endregion

		#region Subscriptions

		/// <summary>
		/// Current subscription, if any.
		/// </summary>
		public Subscription CurrentSubscription
		{
			get
			{
				lock (this.synchObject)
				{
					return this.subscription;
				}
			}
		}

		/// <summary>
		/// Starts subscribing to changes on the device, if enabled. If the subscription fails, or is lost,
		/// the device will be resubscribed automatically.
		/// </summary>
		public void StartSubscription()
		{
			if (!this.subscribe || this.Disabled)
				return;

			lock (this.synchObject)
			{
				this.subscriptionRequested = true;

				if (this.subscribing || (!(this.subscription is null) && this.subscription.IsActive))
					return;

				this.subscribing = true;
			}

			Task.Run(this.SubscribeAsync);
		}

		/// <summary>
		/// Stops subscribing to changes on the device.
		/// </summary>
		public void StopSubscription()
		{
			Subscription Subscription;

			lock (this.synchObject)
			{
				this.subscriptionRequested = false;
				this.resubscribeTimer?.Dispose();
				this.resubscribeTimer = null;
				this.rescanTimer?.Dispose();
				this.rescanTimer = null;

				Subscription = this.subscription;
				this.subscription = null;
				this.subscriptionSettings = null;
			}

			if (!(Subscription is null))
			{
				Subscription.OnReport -= this.Subscription_OnReport;
				Subscription.OnLost -= this.Subscription_OnLost;
				Subscription.Cancel();
			}
		}

		/// <summary>
		/// Waits for the subscription to be established.
		/// </summary>
		/// <param name="Timeout">Maximum time to wait.</param>
		/// <returns>Subscription, if established within the time, null otherwise.</returns>
		public async Task<Subscription> WaitForSubscriptionAsync(TimeSpan Timeout)
		{
			DateTime Until = DateTime.UtcNow + Timeout;

			while (DateTime.UtcNow < Until)
			{
				Subscription Subscription = this.CurrentSubscription;

				if (!(Subscription is null) && Subscription.IsActive)
					return Subscription;

				await Task.Delay(50);
			}

			return null;
		}

		private async Task SubscribeAsync()
		{
			try
			{
				if (!this.subscriptionRequested || !this.subscribe || this.Disabled)
					return;

				if (this.rootClusters is null)
					await this.RescanAsync();

				MatterNode Node = await this.GetMatterNodeAsync();
				Subscription Subscription;
				bool Wildcard;

				lock (this.synchObject)
				{
					Wildcard = this.useWildcardSubscription;
				}

				(AttributePath[] AttributePaths, EventPath[] EventPaths) = await this.GetSubscriptionPathsAsync(Wildcard);

				ulong? EventMin = this.lastEventNumber.HasValue ? this.lastEventNumber.Value + 1 : (ulong?)null;
				ushort Min = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, this.minInterval));
				ushort Max = (ushort)Math.Max(Min, Math.Max(1, Math.Min(ushort.MaxValue, this.maxInterval)));

				try
				{
					Subscription = await Node.SubscribeAsync(AttributePaths, EventPaths, Min, Max, false, EventMin);
				}
				catch (ImStatusException ex) when (!Wildcard)
				{
					Log.Notice("Device rejected subscription with multiple paths (" + ex.Message + "). Using wildcard subscription instead.",
						this.NodeId);

					lock (this.synchObject)
					{
						this.useWildcardSubscription = true;
					}

					(AttributePaths, EventPaths) = await this.GetSubscriptionPathsAsync(true);
					Subscription = await Node.SubscribeAsync(AttributePaths, EventPaths, Min, Max, false, EventMin);
				}

				bool Keep;

				lock (this.synchObject)
				{
					Keep = this.subscriptionRequested;

					if (Keep)
					{
						this.subscription = Subscription;
						this.subscriptionSettings = this.GetSubscriptionSettings();
						this.resubscribeAttempts = 0;
					}
				}

				if (!Keep)
				{
					Subscription.Cancel();
					return;
				}

				Subscription.OnReport += this.Subscription_OnReport;
				Subscription.OnLost += this.Subscription_OnLost;

				await this.ProcessReport(Subscription.PrimingReport, !EventMin.HasValue);
				await this.RemoveErrorAsync("Subscription");
			}
			catch (Exception ex)
			{
				await this.LogErrorAsync("Subscription", "Unable to subscribe to device: " + ex.Message);
				this.ScheduleResubscribe();
			}
			finally
			{
				lock (this.synchObject)
				{
					this.subscribing = false;
				}
			}
		}

		private async Task<(AttributePath[], EventPath[])> GetSubscriptionPathsAsync(bool Wildcard)
		{
			if (Wildcard)
			{
				return (new AttributePath[] { new AttributePath(null, null, null) },
					new EventPath[] { new EventPath(null, null, null) });
			}

			SortedSet<uint> AttributeClusters = new SortedSet<uint>();
			SortedSet<uint> EventClusters = new SortedSet<uint>();

			if (!(this.rootClusters is null))
				AddSubscriptionClusters(this.rootClusters, AttributeClusters, EventClusters);

			foreach (MatterEndpointNode Endpoint in await this.GetEndpointNodesAsync())
			{
				if (!Endpoint.Disabled && !(Endpoint.Clusters is null))
					AddSubscriptionClusters(Endpoint.Clusters, AttributeClusters, EventClusters);
			}

			List<AttributePath> Attributes = new List<AttributePath>()
			{
				new AttributePath(0, ClusterIds.Descriptor, 0x0003)    // PartsList of root endpoint, to detect changes in endpoints.
			};

			foreach (uint ClusterId in AttributeClusters)
				Attributes.Add(new AttributePath(null, ClusterId, null));

			List<EventPath> Events = new List<EventPath>();

			foreach (uint ClusterId in EventClusters)
				Events.Add(new EventPath(null, ClusterId, null));

			return (Attributes.ToArray(), Events.Count == 0 ? null : Events.ToArray());
		}

		private static void AddSubscriptionClusters(MatterClusterInfo[] Clusters, SortedSet<uint> AttributeClusters, SortedSet<uint> EventClusters)
		{
			foreach (MatterClusterInfo Cluster in Clusters)
			{
				switch (Cluster.ClusterId)
				{
					case ClusterIds.BasicInformation:
					case ClusterIds.GeneralDiagnostics:
					case ClusterIds.WiFiNetworkDiagnostics:
					case ClusterIds.ThreadNetworkDiagnostics:
						continue;
				}

				IClusterHandler Handler = ClusterHandlers.Get(Cluster.ClusterId);

				if (Handler.HasValues(Cluster))
					AttributeClusters.Add(Cluster.ClusterId);

				if (Handler.HasEvents(Cluster))
					EventClusters.Add(Cluster.ClusterId);
			}
		}

		private void ScheduleResubscribe()
		{
			lock (this.synchObject)
			{
				if (!this.subscriptionRequested)
					return;

				int Attempt = ++this.resubscribeAttempts;
				double Seconds = Math.Min(300, 5 * Math.Pow(2, Math.Min(Attempt - 1, 6)));

				this.resubscribeTimer?.Dispose();
				this.resubscribeTimer = new Timer((_) => this.StartSubscription(), null, TimeSpan.FromSeconds(Seconds), Timeout.InfiniteTimeSpan);
			}
		}

		private async Task Subscription_OnReport(object Sender, ReadResult Report)
		{
			try
			{
				await this.ProcessReport(Report, false);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, this.NodeId);
			}
		}

		private async Task Subscription_OnLost(object Sender, EventArgs e)
		{
			bool Resubscribe;

			lock (this.synchObject)
			{
				if (this.subscription == Sender)
					this.subscription = null;

				Resubscribe = this.subscriptionRequested;
			}

			if (Resubscribe)
			{
				await this.LogWarningAsync("Subscription", "Subscription lost. Trying to resubscribe.");

				lock (this.synchObject)
				{
					this.resubscribeAttempts = 0;
				}

				this.ScheduleResubscribe();
			}
		}

		/// <summary>
		/// Processes a report (subscription priming report, or change report), reporting momentary values.
		/// </summary>
		/// <param name="Report">Report.</param>
		/// <param name="SuppressEvents">If events should be suppressed (only event numbers registered).</param>
		internal async Task ProcessReport(ReadResult Report, bool SuppressEvents)
		{
			if (Report is null)
				return;

			DateTime Now = DateTime.UtcNow;
			Dictionary<MeteringNode, List<Field>> FieldsByNode = new Dictionary<MeteringNode, List<Field>>();
			Dictionary<ushort, MatterEndpointNode> EndpointNodes = new Dictionary<ushort, MatterEndpointNode>();
			bool Rescan = false;

			this.UpdateCache(Report);

			foreach (MatterEndpointNode Endpoint in await this.GetEndpointNodesAsync())
				EndpointNodes[(ushort)Endpoint.EndpointId] = Endpoint;

			Dictionary<(ushort, uint), Dictionary<uint, TlvElement>> Grouped = Group(Report, null, this);

			foreach (KeyValuePair<(ushort, uint), Dictionary<uint, TlvElement>> P in Grouped)
			{
				ushort EndpointId = P.Key.Item1;
				uint ClusterId = P.Key.Item2;
				MatterEndpointNode Endpoint = null;
				MatterClusterInfo Cluster;

				if (ClusterId == ClusterIds.Descriptor)
				{
					if (EndpointId == 0 && P.Value.TryGetValue(0x0003, out TlvElement PartsList) && this.PartsChanged(PartsList))
						Rescan = true;

					continue;
				}

				if (EndpointId == 0)
					Cluster = MatterClusterInfo.Find(this.rootClusters, ClusterId);
				else if (EndpointNodes.TryGetValue(EndpointId, out Endpoint))
					Cluster = MatterClusterInfo.Find(Endpoint.Clusters, ClusterId);
				else
					continue;

				if (Cluster is null || (!(Endpoint is null) && Endpoint.Disabled))
					continue;

				IClusterHandler Handler = ClusterHandlers.Get(ClusterId);
				ClusterContext Context = new ClusterContext(this, Endpoint, EndpointId, Cluster);
				MeteringNode Node = (MeteringNode)Endpoint ?? this;
				FieldBuilder Fields = new FieldBuilder(Node, Now, null, this.GetFieldNames(EndpointId));

				try
				{
					Handler.AddFields(Context, new ClusterValues(Context, P.Value), Fields);
				}
				catch (Exception ex)
				{
					Log.Exception(ex, this.NodeId);
				}

				AddFields(FieldsByNode, Node, Fields.Fields);
			}

			foreach (EventReport Event in Report.Events)
			{
				if (!this.lastEventNumber.HasValue || Event.EventNumber > this.lastEventNumber.Value)
					this.lastEventNumber = Event.EventNumber;

				if (SuppressEvents || Event.Data is null)
					continue;

				EventPath Path = Event.Path;
				if (!Path.EndpointId.HasValue || !Path.ClusterId.HasValue)
					continue;

				ushort EndpointId = Path.EndpointId.Value;
				uint ClusterId = Path.ClusterId.Value;
				MatterEndpointNode Endpoint = null;
				MatterClusterInfo Cluster;

				if (EndpointId == 0)
					Cluster = MatterClusterInfo.Find(this.rootClusters, ClusterId);
				else if (EndpointNodes.TryGetValue(EndpointId, out Endpoint))
					Cluster = MatterClusterInfo.Find(Endpoint.Clusters, ClusterId);
				else
					continue;

				if (Cluster is null || (!(Endpoint is null) && Endpoint.Disabled))
					continue;

				IClusterHandler Handler = ClusterHandlers.Get(ClusterId);
				ClusterContext Context = new ClusterContext(this, Endpoint, EndpointId, Cluster);
				MeteringNode Node = (MeteringNode)Endpoint ?? this;
				DateTime Timestamp = Event.Timestamp ?? Now;
				FieldBuilder Fields = new FieldBuilder(Node, Timestamp, null, this.GetFieldNames(EndpointId));

				try
				{
					Handler.AddEventFields(Context, Event, Fields);
				}
				catch (Exception ex)
				{
					Log.Exception(ex, this.NodeId);
				}

				AddFields(FieldsByNode, Node, Fields.Fields);
			}

			foreach (KeyValuePair<MeteringNode, List<Field>> P in FieldsByNode)
			{
				if (P.Value.Count > 0)
					await P.Key.NewMomentaryValues(P.Value);
			}

			if (Rescan)
				this.ScheduleRescan();
		}

		private static void AddFields(Dictionary<MeteringNode, List<Field>> FieldsByNode, MeteringNode Node, List<Field> Fields)
		{
			if (Fields.Count == 0)
				return;

			if (!FieldsByNode.TryGetValue(Node, out List<Field> List))
			{
				List = new List<Field>();
				FieldsByNode[Node] = List;
			}

			List.AddRange(Fields);
		}

		private bool PartsChanged(TlvElement PartsList)
		{
			if (PartsList is null || !(PartsList.IsArray || PartsList.IsList))
				return false;

			HashSet<ushort> Parts = new HashSet<ushort>();

			foreach (TlvElement Item in PartsList.Children)
			{
				if (Item.IsInteger)
					Parts.Add(Item.AsUInt16());
			}

			lock (this.synchObject)
			{
				if (this.knownEndpoints is null)
				{
					this.knownEndpoints = Parts;
					return false;
				}
				else
					return !this.knownEndpoints.SetEquals(Parts);
			}
		}

		#endregion

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.NodeId + " (" + this.operationalNodeId + ")";
		}
	}
}
