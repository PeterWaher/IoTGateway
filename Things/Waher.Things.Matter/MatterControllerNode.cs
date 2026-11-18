using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Matter.Attestation;
using Waher.Things.Matter.Commands;
using Waher.Things.Matter.Commissioning;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.Fabric;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Matter
{
	/// <summary>
	/// Node representing a Matter controller (commissioner and administrator) with its own fabric. Matter devices
	/// commissioned into the fabric are represented by <see cref="MatterDeviceNode"/> child nodes.
	/// </summary>
	public class MatterControllerNode : ProvisionedMeteringNode, ICommunicationLayer, IEncryptedProperties
	{
		private static readonly Dictionary<Guid, MatterController> controllers = new Dictionary<Guid, MatterController>();
		private static readonly SemaphoreSlim controllersLock = new SemaphoreSlim(1, 1);

		private readonly CommunicationLayer sniffers = new CommunicationLayer(false);
		private AttestationPolicy attestationPolicy = AttestationPolicy.Strict;
		private RegulatoryLocation regulatoryLocation = RegulatoryLocation.IndoorOutdoor;
		private string fabricLabel = "IoT Gateway";
		private string countryCode = "XX";
		private string paaFolder = string.Empty;
		private string fabricInformation = string.Empty;
		private string rootCaPrivateKey = string.Empty;
		private string operationalPrivateKey = string.Empty;
		private string ipkEpochKey = string.Empty;
		private int vendorId = MatterFabric.TestVendorId;
		private int udpPort = 0;
		private int discoveryTimeoutSeconds = 5;
		private bool enableDiscovery = true;
		private bool trustDevelopmentPaas = false;
		private bool allowTestCertificationDeclarations = false;

		/// <summary>
		/// Node representing a Matter controller (commissioner and administrator) with its own fabric.
		/// </summary>
		public MatterControllerNode()
			: base()
		{
		}

		#region Properties

		/// <summary>
		/// Label of the fabric, as presented to devices and other administrators.
		/// </summary>
		[Page(1, "Matter", 100)]
		[Header(2, "Fabric label:")]
		[ToolTip(3, "Label of the fabric, as presented to devices and other administrators (max 32 characters).")]
		[DefaultValue("IoT Gateway")]
		public string FabricLabel
		{
			get => this.fabricLabel;
			set => this.fabricLabel = value;
		}

		/// <summary>
		/// Vendor ID of the administrator. Used when creating the fabric.
		/// </summary>
		[Page(1, "Matter", 100)]
		[Header(4, "Vendor ID:")]
		[ToolTip(5, "Vendor ID of the administrator, used when the fabric is created. 65521-65524 (0xFFF1-0xFFF4) are test vendor IDs.")]
		[Range(1, 65534)]
		[DefaultValue(MatterFabric.TestVendorId)]
		public int VendorId
		{
			get => this.vendorId;
			set => this.vendorId = value;
		}

		/// <summary>
		/// UDP port used by the controller. 0 means a dynamically assigned port.
		/// </summary>
		[Page(1, "Matter", 100)]
		[Header(6, "UDP Port:")]
		[ToolTip(7, "UDP port used by the controller. 0 means a dynamically assigned port.")]
		[Range(0, 65535)]
		[DefaultValue(0)]
		public int UdpPort
		{
			get => this.udpPort;
			set => this.udpPort = value;
		}

		/// <summary>
		/// If DNS-SD (mDNS) is used to discover devices.
		/// </summary>
		[Page(1, "Matter", 100)]
		[Header(8, "Discover devices using DNS-SD (mDNS).")]
		[ToolTip(9, "If DNS-SD (mDNS) should be used to discover commissionable devices, and to find the addresses of commissioned devices.")]
		[DefaultValue(true)]
		public bool EnableDiscovery
		{
			get => this.enableDiscovery;
			set => this.enableDiscovery = value;
		}

		/// <summary>
		/// Discovery timeout, in seconds.
		/// </summary>
		[Page(1, "Matter", 100)]
		[Header(10, "Discovery timeout (s):")]
		[ToolTip(11, "Maximum time to search for devices on the network, in seconds.")]
		[Range(1, 60)]
		[DefaultValue(5)]
		public int DiscoveryTimeoutSeconds
		{
			get => this.discoveryTimeoutSeconds;
			set => this.discoveryTimeoutSeconds = value;
		}

		/// <summary>
		/// How device attestation is to be handled.
		/// </summary>
		[Page(12, "Commissioning", 200)]
		[Header(13, "Device attestation:")]
		[ToolTip(14, "How device attestation (verification that the device is a certified Matter device) is to be handled during commissioning.")]
		[DefaultValue(AttestationPolicy.Strict)]
		[Option(AttestationPolicy.Strict, 15, "Strict: Only commission devices that pass attestation.")]
		[Option(AttestationPolicy.Warn, 16, "Warn: Commission devices failing attestation, but log a warning.")]
		[Option(AttestationPolicy.Ignore, 17, "Ignore: Do not perform device attestation.")]
		public AttestationPolicy AttestationPolicy
		{
			get => this.attestationPolicy;
			set => this.attestationPolicy = value;
		}

		/// <summary>
		/// If development and test Product Attestation Authorities (PAA) are trusted.
		/// </summary>
		[Page(12, "Commissioning", 200)]
		[Header(18, "Trust development and test PAAs.")]
		[ToolTip(19, "If devices with attestation certificates issued by development and test Product Attestation Authorities (PAA) should be accepted.")]
		[DefaultValue(false)]
		public bool TrustDevelopmentPaas
		{
			get => this.trustDevelopmentPaas;
			set => this.trustDevelopmentPaas = value;
		}

		/// <summary>
		/// If certification declarations signed by the public test key are accepted.
		/// </summary>
		[Page(12, "Commissioning", 200)]
		[Header(20, "Accept test certification declarations.")]
		[ToolTip(21, "If certification declarations signed by the public test key should be accepted. Required for development and test devices.")]
		[DefaultValue(false)]
		public bool AllowTestCertificationDeclarations
		{
			get => this.allowTestCertificationDeclarations;
			set => this.allowTestCertificationDeclarations = value;
		}

		/// <summary>
		/// Optional folder containing additional trusted Product Attestation Authority (PAA) certificates.
		/// </summary>
		[Page(12, "Commissioning", 200)]
		[Header(22, "Additional PAA folder:")]
		[ToolTip(23, "Optional folder containing additional trusted Product Attestation Authority (PAA) certificates, in DER or PEM format.")]
		[DefaultValueStringEmpty]
		public string PaaFolder
		{
			get => this.paaFolder;
			set => this.paaFolder = value;
		}

		/// <summary>
		/// Country code (ISO 3166-1 alpha-2) used when configuring devices.
		/// </summary>
		[Page(12, "Commissioning", 200)]
		[Header(24, "Country code:")]
		[ToolTip(25, "Country code (ISO 3166-1 alpha-2) used when configuring the regulatory settings of devices. XX means unknown.")]
		[RegularExpression("^[A-Z]{2}$")]
		[DefaultValue("XX")]
		public string CountryCode
		{
			get => this.countryCode;
			set => this.countryCode = value;
		}

		/// <summary>
		/// Regulatory location of devices.
		/// </summary>
		[Page(12, "Commissioning", 200)]
		[Header(26, "Regulatory location:")]
		[ToolTip(27, "Regulatory location of devices.")]
		[DefaultValue(RegulatoryLocation.IndoorOutdoor)]
		[Option(RegulatoryLocation.Indoor, 28, "Indoor")]
		[Option(RegulatoryLocation.Outdoor, 29, "Outdoor")]
		[Option(RegulatoryLocation.IndoorOutdoor, 30, "Indoor and outdoor")]
		public RegulatoryLocation RegulatoryLocation
		{
			get => this.regulatoryLocation;
			set => this.regulatoryLocation = value;
		}

		/// <summary>
		/// Public information about the fabric (identifiers and certificates), generated automatically when first needed.
		/// </summary>
		[DefaultValueStringEmpty]
		public string FabricInformation
		{
			get => this.fabricInformation;
			set => this.fabricInformation = value;
		}

		/// <summary>
		/// Private key of the root certificate authority of the fabric. Generated automatically when first needed.
		/// </summary>
		[Encrypted(64)]
		[DefaultValueStringEmpty]
		public string RootCaPrivateKey
		{
			get => this.rootCaPrivateKey;
			set => this.rootCaPrivateKey = value;
		}

		/// <summary>
		/// Private operational key of the controller. Generated automatically when first needed.
		/// </summary>
		[Encrypted(64)]
		[DefaultValueStringEmpty]
		public string OperationalPrivateKey
		{
			get => this.operationalPrivateKey;
			set => this.operationalPrivateKey = value;
		}

		/// <summary>
		/// Identity Protection Key (IPK) epoch key of the fabric. Generated automatically when first needed.
		/// </summary>
		[Encrypted(64)]
		[DefaultValueStringEmpty]
		public string IpkEpochKey
		{
			get => this.ipkEpochKey;
			set => this.ipkEpochKey = value;
		}

		/// <summary>
		/// Properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[]
		{
			nameof(this.RootCaPrivateKey),
			nameof(this.OperationalPrivateKey),
			nameof(this.IpkEpochKey)
		};

		/// <summary>
		/// If the node has fabric credentials.
		/// </summary>
		public bool HasFabric =>
			!string.IsNullOrEmpty(this.fabricInformation) &&
			!string.IsNullOrEmpty(this.rootCaPrivateKey) &&
			!string.IsNullOrEmpty(this.operationalPrivateKey) &&
			!string.IsNullOrEmpty(this.ipkEpochKey);

		#endregion

		#region Node interface

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 31, "Matter Controller");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent.
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is NodeCollection);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child.
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is MatterDeviceNode);
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
			MatterFabric Fabric = this.TryGetFabric();

			if (!(Fabric is null))
			{
				Result.AddLast(new StringParameter("FabricId", await Language.GetStringAsync(typeof(MatterControllerNode), 32, "Fabric ID"),
					Fabric.FabricId.ToString("X16")));

				Result.AddLast(new StringParameter("ControllerNodeId", await Language.GetStringAsync(typeof(MatterControllerNode), 33, "Node ID"),
					Fabric.ControllerNodeId.ToString("X16")));
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

			Result.Add(new CommissionDeviceCommand(this));
			Result.Add(new DiscoverDevicesCommand(this));
			Result.Add(new ScanFabricCommand(this));
			Result.Add(new RestartControllerCommand(this));

			return Result;
		}

		/// <summary>
		/// Persists changes to the node, and generates a node updated event.
		/// </summary>
		protected override async Task NodeUpdated()
		{
			await base.NodeUpdated();

			MatterController Controller = this.GetRunningController();
			if (!(Controller is null))
			{
				if (Controller.Options.Port != this.udpPort || Controller.Options.EnableDiscovery != this.enableDiscovery)
					await this.RestartControllerAsync();
				else
					this.UpdateOptions(Controller.Options);
			}
		}

		/// <summary>
		/// Removes a child node.
		/// </summary>
		/// <param name="Child">Child node to remove.</param>
		/// <returns>If the Child node was found and removed.</returns>
		public override async Task<bool> RemoveAsync(INode Child)
		{
			if (Child is MatterDeviceNode Device)
				await Device.Removing(this);

			return await base.RemoveAsync(Child);
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public override async Task DestroyAsync()
		{
			List<Task> Tasks = new List<Task>();

			foreach (INode Child in await this.ChildNodes)
			{
				if (Child is MatterDeviceNode Device)
					Tasks.Add(Device.Removing(this));
			}

			if (Tasks.Count > 0)
				await Task.WhenAll(Tasks);

			await this.StopControllerAsync();

			await base.DestroyAsync();
		}

		#endregion

		#region Fabric & Controller

		/// <summary>
		/// Gets the fabric, if available.
		/// </summary>
		/// <returns>Fabric, or null if not created yet.</returns>
		public MatterFabric TryGetFabric()
		{
			MatterController Controller = this.GetRunningController();
			if (!(Controller is null))
				return Controller.Fabric;

			if (!this.HasFabric)
				return null;

			try
			{
				return this.ImportFabric();
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the fabric. If no fabric exists, a new fabric is created, and persisted.
		/// </summary>
		/// <returns>Fabric.</returns>
		public async Task<MatterFabric> GetFabricAsync()
		{
			if (this.HasFabric)
				return this.ImportFabric();

			if (this.ObjectId == Guid.Empty)
				throw new InvalidOperationException("Node must be persisted before a fabric can be created.");

			MatterFabric Result = MatterFabric.Create(MatterFabric.NewFabricId(), (ushort)this.vendorId, MatterFabric.NewOperationalNodeId());

			this.fabricInformation = Convert.ToBase64String(Result.ExportPublic());
			this.rootCaPrivateKey = Convert.ToBase64String(Result.RootKey.PrivateKey);
			this.operationalPrivateKey = Convert.ToBase64String(Result.OperationalKey.PrivateKey);
			this.ipkEpochKey = Convert.ToBase64String(Result.IpkEpochKey);

			await Database.Update(this);
			await this.LogInformationAsync("Fabric", "Fabric " + Result.FabricId.ToString("X16") + " created. Controller node ID: " +
				Result.ControllerNodeId.ToString("X16"));

			return Result;
		}

		private MatterFabric ImportFabric()
		{
			return MatterFabric.Import(
				Convert.FromBase64String(this.fabricInformation),
				Convert.FromBase64String(this.rootCaPrivateKey),
				Convert.FromBase64String(this.operationalPrivateKey),
				Convert.FromBase64String(this.ipkEpochKey));
		}

		/// <summary>
		/// Gets the running controller of the node, if any.
		/// </summary>
		/// <returns>Controller, or null if not running.</returns>
		public MatterController GetRunningController()
		{
			lock (controllers)
			{
				if (controllers.TryGetValue(this.ObjectId, out MatterController Controller))
					return Controller;
				else
					return null;
			}
		}

		/// <summary>
		/// Gets the Matter controller of the node. If not running, it is started.
		/// </summary>
		/// <returns>Matter controller.</returns>
		public async Task<MatterController> GetControllerAsync()
		{
			MatterController Controller = this.GetRunningController();
			if (!(Controller is null))
				return Controller;

			if (this.ObjectId == Guid.Empty)
				throw new InvalidOperationException("Node must be persisted before the controller can be started.");

			await controllersLock.WaitAsync();
			try
			{
				Controller = this.GetRunningController();
				if (!(Controller is null))
					return Controller;

				MatterFabric Fabric = await this.GetFabricAsync();
				MatterControllerOptions Options = new MatterControllerOptions()
				{
					Port = this.udpPort,
					EnableDiscovery = this.enableDiscovery
				};

				this.UpdateOptions(Options);

				Controller = new MatterController(Fabric, Options, this.sniffers.Sniffers);
				Controller.Start();

				lock (controllers)
				{
					controllers[this.ObjectId] = Controller;
				}

				return Controller;
			}
			finally
			{
				controllersLock.Release();
			}
		}

		private void UpdateOptions(MatterControllerOptions Options)
		{
			Options.AttestationPolicy = this.attestationPolicy;
			Options.TrustProductionPaas = true;
			Options.TrustDevelopmentPaas = this.trustDevelopmentPaas;
			Options.AllowTestCertificationDeclarations = this.allowTestCertificationDeclarations;
			Options.PaaFolder = string.IsNullOrEmpty(this.paaFolder) ? null : this.paaFolder;
			Options.RegulatoryLocation = this.regulatoryLocation;
			Options.CountryCode = string.IsNullOrEmpty(this.countryCode) ? "XX" : this.countryCode;
			Options.FabricLabel = string.IsNullOrEmpty(this.fabricLabel) ? null :
				(this.fabricLabel.Length > 32 ? this.fabricLabel.Substring(0, 32) : this.fabricLabel);
			Options.DiscoveryTimeout = TimeSpan.FromSeconds(Math.Max(1, this.discoveryTimeoutSeconds));
		}

		/// <summary>
		/// Stops the controller of the node, if running.
		/// </summary>
		public async Task StopControllerAsync()
		{
			MatterController Controller;

			foreach (INode Child in await this.ChildNodes)
			{
				if (Child is MatterDeviceNode Device)
					Device.StopSubscription();
			}

			lock (controllers)
			{
				if (!controllers.TryGetValue(this.ObjectId, out Controller))
					return;

				controllers.Remove(this.ObjectId);
			}

			try
			{
				await Controller.StopAsync();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Restarts the controller, and the subscriptions of its devices.
		/// </summary>
		public async Task RestartControllerAsync()
		{
			await this.StopControllerAsync();
			await this.StartAsync();
		}

		/// <summary>
		/// Starts the controller, and the subscriptions of its devices.
		/// </summary>
		public async Task StartAsync()
		{
			if (this.Disabled)
				return;

			await this.GetControllerAsync();

			foreach (INode Child in await this.ChildNodes)
			{
				if (Child is MatterDeviceNode Device)
					Device.StartSubscription();
			}
		}

		/// <summary>
		/// Stops all running controllers.
		/// </summary>
		internal static async Task StopAllAsync()
		{
			MatterController[] ToStop;

			lock (controllers)
			{
				ToStop = new MatterController[controllers.Count];
				controllers.Values.CopyTo(ToStop, 0);
				controllers.Clear();
			}

			foreach (MatterController Controller in ToStop)
			{
				try
				{
					await Controller.StopAsync();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Creates commissioning options, based on the settings of the node.
		/// </summary>
		/// <param name="Controller">Controller.</param>
		/// <returns>Commissioning options.</returns>
		internal MatterControllerOptions GetOptions(MatterController Controller)
		{
			this.UpdateOptions(Controller.Options);
			return Controller.Options;
		}

		/// <summary>
		/// Finds a device node, given its operational node ID.
		/// </summary>
		/// <param name="OperationalNodeId">Operational node ID.</param>
		/// <returns>Device node, or null if not found.</returns>
		public async Task<MatterDeviceNode> FindDeviceAsync(ulong OperationalNodeId)
		{
			foreach (INode Child in await this.ChildNodes)
			{
				if (Child is MatterDeviceNode Device && Device.TryGetOperationalNodeId(out ulong Id) && Id == OperationalNodeId)
					return Device;
			}

			return null;
		}

		#endregion

		#region ICommunicationLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => this.sniffers.DecoupledEvents;

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers => this.sniffers.Sniffers;

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers => this.sniffers.HasSniffers;

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public void Add(ISniffer Sniffer)
		{
			this.sniffers.Add(Sniffer);
			this.GetRunningController()?.Add(Sniffer);
		}

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers.</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			this.sniffers.AddRange(Sniffers);
			this.GetRunningController()?.AddRange(Sniffers);
		}

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer)
		{
			this.GetRunningController()?.Remove(Sniffer);
			return this.sniffers.Remove(Sniffer);
		}

		/// <summary>
		/// Gets an enumerator of registered sniffers.
		/// </summary>
		/// <returns>Enumerator.</returns>
		public IEnumerator<ISniffer> GetEnumerator() => this.sniffers.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.sniffers.GetEnumerator();

		/// <inheritdoc/>
		public void ReceiveBinary(int Count) => this.sniffers.ReceiveBinary(Count);

		/// <inheritdoc/>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data) => this.sniffers.ReceiveBinary(ConstantBuffer, Data);

		/// <inheritdoc/>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.ReceiveBinary(ConstantBuffer, Data, Offset, Count);

		/// <inheritdoc/>
		public void TransmitBinary(int Count) => this.sniffers.TransmitBinary(Count);

		/// <inheritdoc/>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data) => this.sniffers.TransmitBinary(ConstantBuffer, Data);

		/// <inheritdoc/>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.TransmitBinary(ConstantBuffer, Data, Offset, Count);

		/// <inheritdoc/>
		public void ReceiveText(string Text) => this.sniffers.ReceiveText(Text);

		/// <inheritdoc/>
		public void TransmitText(string Text) => this.sniffers.TransmitText(Text);

		/// <inheritdoc/>
		public void Information(string Comment) => this.sniffers.Information(Comment);

		/// <inheritdoc/>
		public void Warning(string Warning) => this.sniffers.Warning(Warning);

		/// <inheritdoc/>
		public void Error(string Error) => this.sniffers.Error(Error);

		/// <inheritdoc/>
		public void Exception(Exception Exception) => this.sniffers.Exception(Exception);

		/// <inheritdoc/>
		public void Exception(string Exception) => this.sniffers.Exception(Exception);

		/// <inheritdoc/>
		public void ReceiveBinary(DateTime Timestamp, int Count) => this.sniffers.ReceiveBinary(Timestamp, Count);

		/// <inheritdoc/>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.sniffers.ReceiveBinary(Timestamp, ConstantBuffer, Data);

		/// <inheritdoc/>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <inheritdoc/>
		public void TransmitBinary(DateTime Timestamp, int Count) => this.sniffers.TransmitBinary(Timestamp, Count);

		/// <inheritdoc/>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.sniffers.TransmitBinary(Timestamp, ConstantBuffer, Data);

		/// <inheritdoc/>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <inheritdoc/>
		public void ReceiveText(DateTime Timestamp, string Text) => this.sniffers.ReceiveText(Timestamp, Text);

		/// <inheritdoc/>
		public void TransmitText(DateTime Timestamp, string Text) => this.sniffers.TransmitText(Timestamp, Text);

		/// <inheritdoc/>
		public void Information(DateTime Timestamp, string Comment) => this.sniffers.Information(Timestamp, Comment);

		/// <inheritdoc/>
		public void Warning(DateTime Timestamp, string Warning) => this.sniffers.Warning(Timestamp, Warning);

		/// <inheritdoc/>
		public void Error(DateTime Timestamp, string Error) => this.sniffers.Error(Timestamp, Error);

		/// <inheritdoc/>
		public void Exception(DateTime Timestamp, string Exception) => this.sniffers.Exception(Timestamp, Exception);

		/// <inheritdoc/>
		public void Exception(DateTime Timestamp, Exception Exception) => this.sniffers.Exception(Timestamp, Exception);

		#endregion
	}
}
