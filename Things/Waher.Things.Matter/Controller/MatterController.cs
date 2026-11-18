using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Things.Matter.Attestation;
using Waher.Things.Matter.Commissioning;
using Waher.Things.Matter.Discovery;
using Waher.Things.Matter.Fabric;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.Onboarding;

namespace Waher.Things.Matter.Controller
{
	/// <summary>
	/// Matter controller: a commissioner and administrator of a fabric, able to commission devices, and to interact with
	/// commissioned devices using the Interaction Model.
	/// </summary>
	public class MatterController : CommunicationLayer, IDisposable
	{
		private readonly Dictionary<ulong, MatterNode> nodes = new Dictionary<ulong, MatterNode>();
		private readonly MatterFabric fabric;
		private readonly MatterControllerOptions options;
		private UdpTransport transport;
		private ExchangeManager exchanges;
		private SubscriptionManager subscriptions;
		private MdnsClient mdns;
		private AttestationTrustStore trustStore;
		private bool started = false;

		/// <summary>
		/// Matter controller.
		/// </summary>
		/// <param name="Fabric">Fabric administered by the controller.</param>
		/// <param name="Options">Controller options.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public MatterController(MatterFabric Fabric, MatterControllerOptions Options, params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.fabric = Fabric;
			this.options = Options ?? new MatterControllerOptions();
		}

		/// <summary>
		/// Fabric administered by the controller.
		/// </summary>
		public MatterFabric Fabric => this.fabric;

		/// <summary>
		/// Controller options.
		/// </summary>
		public MatterControllerOptions Options => this.options;

		/// <summary>
		/// Exchange manager.
		/// </summary>
		public ExchangeManager Exchanges => this.exchanges;

		/// <summary>
		/// Subscription manager.
		/// </summary>
		public SubscriptionManager Subscriptions => this.subscriptions;

		/// <summary>
		/// mDNS client, or null if discovery is disabled.
		/// </summary>
		public MdnsClient Mdns => this.mdns;

		/// <summary>
		/// UDP transport.
		/// </summary>
		public UdpTransport Transport => this.transport;

		/// <summary>
		/// PAA trust store.
		/// </summary>
		public AttestationTrustStore TrustStore => this.trustStore;

		/// <summary>
		/// If the controller has been started.
		/// </summary>
		public bool IsStarted => this.started;

		/// <summary>
		/// Starts the controller: opens network sockets and starts discovery.
		/// </summary>
		public void Start()
		{
			if (this.started)
				return;

			this.transport = new UdpTransport(this.options.Port);
			this.exchanges = new ExchangeManager(this.transport, this);
			this.subscriptions = new SubscriptionManager(this.exchanges);
			this.exchanges.OnSessionClosed += this.Exchanges_OnSessionClosed;

			this.trustStore = new AttestationTrustStore(this.options.TrustProductionPaas, this.options.TrustDevelopmentPaas);

			if (!string.IsNullOrEmpty(this.options.PaaFolder))
				this.trustStore.LoadFolder(this.options.PaaFolder);

			if (this.options.EnableDiscovery)
			{
				try
				{
					this.mdns = new MdnsClient(this);
					this.mdns.Start();
					this.mdns.OnInstanceUpdated += this.Mdns_OnInstanceUpdated;
				}
				catch (Exception ex)
				{
					this.Warning("DNS-SD discovery not available: " + ex.Message);
					this.mdns?.Dispose();
					this.mdns = null;
				}
			}

			this.started = true;
		}

		private void AssertStarted()
		{
			if (!this.started)
				throw new InvalidOperationException("Controller not started.");
		}

		private Task Exchanges_OnSessionClosed(object Sender, SecureSession Session)
		{
			MatterNode Node;

			lock (this.nodes)
			{
				if (!this.nodes.TryGetValue(Session.PeerNodeId, out Node))
					Node = null;
			}

			Node?.SessionClosed(Session);

			return Task.CompletedTask;
		}

		private Task Mdns_OnInstanceUpdated(object Sender, DnsSdInstance Instance)
		{
			if (Instance.ServiceType.Equals(MatterDiscovery.OperationalServiceType, StringComparison.OrdinalIgnoreCase))
			{
				OperationalNode Op = new OperationalNode(Instance);

				if (Op.CompressedFabricId == this.fabric.CompressedFabricId)
				{
					MatterNode Node;

					lock (this.nodes)
					{
						if (!this.nodes.TryGetValue(Op.NodeId, out Node))
							Node = null;
					}

					if (!(Node is null) && Instance.IsResolved)
						Node.AddressAnnounced(MatterDiscovery.GetEndpoint(Instance, this.transport), Op.SessionParameters);
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Creates a device attestation verifier, according to the controller options.
		/// </summary>
		/// <returns>Verifier.</returns>
		public DeviceAttestationVerifier CreateAttestationVerifier()
		{
			this.AssertStarted();
			return new DeviceAttestationVerifier(this.trustStore, this.options.AllowTestCertificationDeclarations);
		}

		#region Discovery

		/// <summary>
		/// Discovers commissionable devices on the network.
		/// </summary>
		/// <param name="Timeout">Time to wait for responses, or null to use the default timeout.</param>
		/// <param name="Payload">Optional onboarding payload to filter by.</param>
		/// <returns>Commissionable devices.</returns>
		public Task<CommissionableNode[]> DiscoverCommissionableAsync(TimeSpan? Timeout = null, SetupPayload Payload = null)
		{
			this.AssertStarted();

			if (this.mdns is null)
				throw new MatterException("DNS-SD discovery not available.");

			return MatterDiscovery.DiscoverCommissionableAsync(this.mdns, Timeout ?? this.options.DiscoveryTimeout, Payload);
		}

		/// <summary>
		/// Browses for operational nodes of the controller's fabric.
		/// </summary>
		/// <param name="Timeout">Time to wait for responses, or null to use the default timeout.</param>
		/// <returns>Operational nodes.</returns>
		public Task<OperationalNode[]> BrowseOperationalAsync(TimeSpan? Timeout = null)
		{
			this.AssertStarted();

			if (this.mdns is null)
				throw new MatterException("DNS-SD discovery not available.");

			return MatterDiscovery.BrowseOperationalAsync(this.mdns, this.fabric.CompressedFabricId, Timeout ?? this.options.DiscoveryTimeout);
		}

		/// <summary>
		/// Resolves the operational address of a node of the fabric.
		/// </summary>
		/// <param name="NodeId">Operational node ID.</param>
		/// <param name="Timeout">Time to wait for responses, or null to use the default timeout.</param>
		/// <returns>Operational node, or null if not found.</returns>
		public async Task<OperationalNode> ResolveOperationalAsync(ulong NodeId, TimeSpan? Timeout = null)
		{
			this.AssertStarted();

			if (this.mdns is null)
				return null;

			return await MatterDiscovery.ResolveOperationalAsync(this.mdns, this.fabric.CompressedFabricId, NodeId,
				Timeout ?? this.options.DiscoveryTimeout);
		}

		#endregion

		#region Commissioning

		/// <summary>
		/// Commissions a device using an onboarding code (QR code or manual pairing code).
		/// </summary>
		/// <param name="OnboardingCode">QR code ("MT:...") or manual pairing code.</param>
		/// <param name="Address">Optional known address of the device. If null, the device is discovered using DNS-SD.</param>
		/// <param name="NodeId">Optional operational node ID to assign. If null, a random node ID is assigned.</param>
		/// <param name="Progress">Optional progress callback.</param>
		/// <returns>Commissioned node.</returns>
		public Task<MatterNode> CommissionAsync(string OnboardingCode, IPEndPoint Address = null, ulong? NodeId = null, Action<string> Progress = null)
		{
			return this.CommissionAsync(SetupPayload.Parse(OnboardingCode), Address, NodeId, Progress);
		}

		/// <summary>
		/// Commissions a device using an onboarding payload.
		/// </summary>
		/// <param name="Payload">Onboarding payload.</param>
		/// <param name="Address">Optional known address of the device. If null, the device is discovered using DNS-SD.</param>
		/// <param name="NodeId">Optional operational node ID to assign. If null, a random node ID is assigned.</param>
		/// <param name="Progress">Optional progress callback.</param>
		/// <returns>Commissioned node.</returns>
		public async Task<MatterNode> CommissionAsync(SetupPayload Payload, IPEndPoint Address = null, ulong? NodeId = null, Action<string> Progress = null)
		{
			this.AssertStarted();

			List<IPEndPoint> Candidates = new List<IPEndPoint>();

			if (Address is null)
			{
				Progress?.Invoke("Discovering commissionable device.");
				CommissionableNode[] Found = await this.DiscoverCommissionableAsync(null, Payload);

				foreach (CommissionableNode Node in Found)
				{
					foreach (IPAddress A in Node.Instance.GetPreferredAddresses(this.transport.SupportsIPv4, this.transport.SupportsIPv6))
						Candidates.Add(new IPEndPoint(A, Node.Instance.Port));
				}

				if (Candidates.Count == 0)
					throw new MatterException("Commissionable device not found on the network.");
			}
			else
				Candidates.Add(Address);

			ulong AssignedNodeId = NodeId ?? MatterFabric.NewOperationalNodeId();
			Commissioner Commissioner = new Commissioner(this.exchanges, this.fabric, this);
			CommissioningOptions Options = new CommissioningOptions()
			{
				AttestationPolicy = this.options.AttestationPolicy,
				AttestationVerifier = this.options.AttestationPolicy == AttestationPolicy.Ignore ? null : this.CreateAttestationVerifier(),
				RegulatoryLocation = this.options.RegulatoryLocation,
				CountryCode = this.options.CountryCode,
				FabricLabel = this.options.FabricLabel,
				Progress = Progress,
				OperationalAddressResolver = async (Id) =>
				{
					OperationalNode Op = await this.ResolveOperationalAsync(Id);
					return Op is null ? null : MatterDiscovery.GetEndpoint(Op.Instance, this.transport);
				}
			};

			Exception LastError = null;

			foreach (IPEndPoint Candidate in Candidates)
			{
				try
				{
					CommissioningResult Result = await Commissioner.CommissionAsync(Candidate, Payload.Passcode, AssignedNodeId, Options);
					MatterNode Node = this.GetNode(AssignedNodeId, Result.Address);
					Node.SetSession(Result.Session, Result.Resumption);
					Node.CommissioningResult = Result;
					return Node;
				}
				catch (MatterSecurityException)
				{
					throw;
				}
				catch (ImStatusException)
				{
					throw;
				}
				catch (Exception ex)
				{
					LastError = ex;
					Progress?.Invoke("Commissioning using " + Candidate.ToString() + " failed: " + ex.Message);
				}
			}

			throw LastError ?? new MatterException("Commissioning failed.");
		}

		#endregion

		#region Nodes

		/// <summary>
		/// Gets an object representing a node of the fabric.
		/// </summary>
		/// <param name="NodeId">Operational node ID.</param>
		/// <param name="KnownAddress">Last known address of the node, if any.</param>
		/// <returns>Node.</returns>
		public MatterNode GetNode(ulong NodeId, IPEndPoint KnownAddress = null)
		{
			MatterNode Node;

			lock (this.nodes)
			{
				if (!this.nodes.TryGetValue(NodeId, out Node))
				{
					Node = new MatterNode(this, NodeId, KnownAddress);
					this.nodes[NodeId] = Node;
					return Node;
				}
			}

			if (!(KnownAddress is null) && Node.Address is null)
				Node.Address = KnownAddress;

			return Node;
		}

		/// <summary>
		/// Gets known nodes.
		/// </summary>
		/// <returns>Nodes.</returns>
		public MatterNode[] GetNodes()
		{
			lock (this.nodes)
			{
				MatterNode[] Result = new MatterNode[this.nodes.Count];
				this.nodes.Values.CopyTo(Result, 0);
				return Result;
			}
		}

		/// <summary>
		/// Forgets a node (does not remove it from the fabric).
		/// </summary>
		/// <param name="NodeId">Operational node ID.</param>
		public async Task ForgetNode(ulong NodeId)
		{
			MatterNode Node;

			lock (this.nodes)
			{
				if (!this.nodes.TryGetValue(NodeId, out Node))
					return;

				this.nodes.Remove(NodeId);
			}

			await Node.DisconnectAsync();
		}

		#endregion

		/// <summary>
		/// Stops the controller.
		/// </summary>
		public async Task StopAsync()
		{
			if (!this.started)
				return;

			this.started = false;

			foreach (MatterNode Node in this.GetNodes())
			{
				try
				{
					await Node.DisconnectAsync();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			this.subscriptions?.Dispose();
			this.subscriptions = null;

			this.exchanges?.Dispose();
			this.exchanges = null;

			this.transport?.Dispose();
			this.transport = null;

			this.mdns?.Dispose();
			this.mdns = null;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.StopAsync().Wait(10000);
		}
	}
}
