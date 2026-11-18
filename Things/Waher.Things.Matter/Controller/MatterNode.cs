using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Things.Matter.Commissioning;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Discovery;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.Onboarding;
using Waher.Things.Matter.SecureChannel;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Controller
{
	/// <summary>
	/// Represents a commissioned node of the fabric, managing its CASE session, and providing interaction model operations.
	/// </summary>
	public class MatterNode
	{
		private readonly MatterController controller;
		private readonly SemaphoreSlim connectLock = new SemaphoreSlim(1, 1);
		private SecureSession session;
		private ResumptionRecord resumption;
		private SessionParameters announcedParameters;

		/// <summary>
		/// Represents a commissioned node of the fabric.
		/// </summary>
		/// <param name="Controller">Controller.</param>
		/// <param name="NodeId">Operational node ID.</param>
		/// <param name="Address">Last known address, or null.</param>
		internal MatterNode(MatterController Controller, ulong NodeId, IPEndPoint Address)
		{
			this.controller = Controller;
			this.NodeId = NodeId;
			this.Address = Address;
		}

		/// <summary>
		/// Controller.
		/// </summary>
		public MatterController Controller => this.controller;

		/// <summary>
		/// Operational node ID.
		/// </summary>
		public ulong NodeId { get; }

		/// <summary>
		/// Last known operational address.
		/// </summary>
		public IPEndPoint Address { get; set; }

		/// <summary>
		/// Current CASE session, or null if not connected.
		/// </summary>
		public SecureSession Session => this.session;

		/// <summary>
		/// If a CASE session is established.
		/// </summary>
		public bool IsConnected => !(this.session is null) && !this.session.IsClosed;

		/// <summary>
		/// Result of the commissioning, if the node was commissioned by this controller instance.
		/// </summary>
		public CommissioningResult CommissioningResult { get; internal set; }

		/// <summary>
		/// Resumption record of the current session, if any.
		/// </summary>
		public ResumptionRecord Resumption
		{
			get => this.resumption;
			set => this.resumption = value;
		}

		/// <summary>
		/// Event raised when the operational address of the node changes.
		/// </summary>
		public event EventHandlerAsync<IPEndPoint> OnAddressChanged = null;

		internal void SetSession(SecureSession Session, ResumptionRecord Resumption)
		{
			this.session = Session;
			this.resumption = Resumption;

			if (!(Session?.PeerAddress is null))
				this.Address = Session.PeerAddress;
		}

		internal void SessionClosed(SecureSession Session)
		{
			if (this.session == Session)
				this.session = null;
		}

		internal void AddressAnnounced(IPEndPoint Address, SessionParameters Parameters)
		{
			if (Address is null)
				return;

			this.announcedParameters = Parameters;

			if (this.Address is null || !this.Address.Equals(Address))
			{
				bool Changed = !(this.Address is null);
				this.Address = Address;

				if (Changed)
					this.OnAddressChanged.Raise(this, Address);
			}
		}

		/// <summary>
		/// Ensures a CASE session is established with the node.
		/// </summary>
		/// <returns>Secure session.</returns>
		public async Task<SecureSession> ConnectAsync()
		{
			SecureSession Session = this.session;
			if (!(Session is null) && !Session.IsClosed)
				return Session;

			await this.connectLock.WaitAsync();
			try
			{
				Session = this.session;
				if (!(Session is null) && !Session.IsClosed)
					return Session;

				Exception LastError = null;

				if (!(this.Address is null))
				{
					try
					{
						return await this.EstablishAsync(this.Address);
					}
					catch (MatterSecurityException)
					{
						throw;
					}
					catch (Exception ex)
					{
						LastError = ex;
						this.controller.Information("Unable to connect to node " + this.NodeId.ToString("X16") + " at " +
							this.Address.ToString() + ": " + ex.Message);
					}
				}

				if (!(this.controller.Mdns is null))
				{
					string FullName = this.controller.Fabric.GetOperationalInstanceName(this.NodeId) + "." + MatterDiscovery.OperationalServiceType;
					this.controller.Mdns.Forget(FullName);

					OperationalNode Op = await this.controller.ResolveOperationalAsync(this.NodeId);

					if (!(Op is null))
					{
						foreach (IPAddress A in Op.Instance.GetPreferredAddresses(this.controller.Transport.SupportsIPv4, this.controller.Transport.SupportsIPv6))
						{
							IPEndPoint EP = new IPEndPoint(A, Op.Instance.Port);
							this.announcedParameters = Op.SessionParameters;

							if (EP.Equals(this.Address) && !(LastError is null))
								continue;

							try
							{
								SecureSession Result = await this.EstablishAsync(EP);
								await this.OnAddressChanged.Raise(this, EP);
								return Result;
							}
							catch (MatterSecurityException)
							{
								throw;
							}
							catch (Exception ex)
							{
								LastError = ex;
							}
						}
					}
				}

				throw LastError ?? new MatterException("Unable to find node " + this.NodeId.ToString("X16") + " on the network.");
			}
			finally
			{
				this.connectLock.Release();
			}
		}

		private async Task<SecureSession> EstablishAsync(IPEndPoint Address)
		{
			SecureSession Session;
			ResumptionRecord Resumption;

			try
			{
				(Session, Resumption) = await CaseClient.EstablishAsync(this.controller.Exchanges, Address, this.controller.Fabric,
					this.NodeId, this.resumption);
			}
			catch (StatusReportException) when (!(this.resumption is null))
			{
				this.resumption = null;
				(Session, Resumption) = await CaseClient.EstablishAsync(this.controller.Exchanges, Address, this.controller.Fabric,
					this.NodeId, null);
			}

			if (!(this.announcedParameters is null) && Session.PeerParameters.IdleIntervalMs == SessionParameters.DefaultIdleIntervalMs)
				Session.PeerParameters = this.announcedParameters;

			this.session = Session;
			this.resumption = Resumption;
			this.Address = Address;

			return Session;
		}

		/// <summary>
		/// Closes the CASE session with the node.
		/// </summary>
		public async Task DisconnectAsync()
		{
			SecureSession Session = this.session;
			this.session = null;

			if (!(Session is null) && !Session.IsClosed)
				await this.controller.Exchanges.CloseSessionAsync(Session);
		}

		/// <summary>
		/// Gets an interaction client for the node, establishing a session if necessary.
		/// </summary>
		/// <returns>Interaction client.</returns>
		public async Task<InteractionClient> GetClientAsync()
		{
			SecureSession Session = await this.ConnectAsync();
			return new InteractionClient(this.controller.Exchanges, Session);
		}

		private async Task<T> Execute<T>(Func<InteractionClient, Task<T>> Action, bool RetryOnTimeout)
		{
			InteractionClient Client = await this.GetClientAsync();

			try
			{
				return await Action(Client);
			}
			catch (MatterTimeoutException) when (RetryOnTimeout)
			{
				await this.InvalidateSession(Client.Session);
				Client = await this.GetClientAsync();
				return await Action(Client);
			}
		}

		private async Task InvalidateSession(SecureSession Session)
		{
			if (this.session == Session)
				this.session = null;

			this.controller.Exchanges.RemoveSession(Session);
			await Task.CompletedTask;
		}

		#region Interaction Model

		/// <summary>
		/// Reads attributes.
		/// </summary>
		/// <param name="Paths">Attribute paths (wildcards allowed).</param>
		/// <returns>Read result.</returns>
		public Task<ReadResult> ReadAttributesAsync(params AttributePath[] Paths)
		{
			return this.Execute((Client) => Client.ReadAttributesAsync(Paths), true);
		}

		/// <summary>
		/// Reads attributes and events.
		/// </summary>
		/// <param name="AttributePaths">Attribute paths, or null.</param>
		/// <param name="EventPaths">Event paths, or null.</param>
		/// <param name="EventMin">Minimum event number, or null.</param>
		/// <returns>Read result.</returns>
		public Task<ReadResult> ReadAsync(AttributePath[] AttributePaths, EventPath[] EventPaths, ulong? EventMin = null)
		{
			return this.Execute((Client) => Client.ReadAsync(AttributePaths, EventPaths, EventMin, true, null), true);
		}

		/// <summary>
		/// Reads a single attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute value.</returns>
		public Task<TlvElement> ReadAttributeAsync(ushort EndpointId, uint ClusterId, uint AttributeId)
		{
			return this.Execute((Client) => Client.ReadAttributeAsync(EndpointId, ClusterId, AttributeId), true);
		}

		/// <summary>
		/// Writes a single attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value.</param>
		/// <param name="Timed">If a timed interaction is required.</param>
		public Task WriteAttributeAsync(ushort EndpointId, uint ClusterId, uint AttributeId, object Value, bool Timed = false)
		{
			return this.Execute(async (Client) =>
			{
				await Client.WriteAttributeAsync(EndpointId, ClusterId, AttributeId, Value, Timed);
				return true;
			}, false);
		}

		/// <summary>
		/// Writes attributes.
		/// </summary>
		/// <param name="Timed">If a timed interaction is required.</param>
		/// <param name="Writes">Writes.</param>
		/// <returns>Write results.</returns>
		public Task<WriteResult[]> WriteAsync(bool Timed, params AttributeWrite[] Writes)
		{
			return this.Execute((Client) => Client.WriteAsync(Timed, Writes), false);
		}

		/// <summary>
		/// Invokes a command.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="CommandId">Command ID.</param>
		/// <param name="Fields">Command fields, or null.</param>
		/// <param name="Timed">If a timed interaction is required.</param>
		/// <param name="ProcessingTime">Expected processing time, or null for default.</param>
		/// <returns>Invoke result.</returns>
		public Task<InvokeResult> InvokeAsync(ushort EndpointId, uint ClusterId, uint CommandId, TlvElement Fields = null,
			bool Timed = false, TimeSpan? ProcessingTime = null)
		{
			return this.Execute((Client) => Client.InvokeAsync(EndpointId, ClusterId, CommandId, Fields, Timed, ProcessingTime), false);
		}

		/// <summary>
		/// Subscribes to attributes and/or events.
		/// </summary>
		/// <param name="AttributePaths">Attribute paths, or null.</param>
		/// <param name="EventPaths">Event paths, or null.</param>
		/// <param name="MinInterval">Minimum interval floor, in seconds.</param>
		/// <param name="MaxInterval">Maximum interval ceiling, in seconds.</param>
		/// <param name="KeepSubscriptions">If existing subscriptions should be kept.</param>
		/// <param name="EventMin">Minimum event number, or null.</param>
		/// <returns>Subscription.</returns>
		public Task<Subscription> SubscribeAsync(AttributePath[] AttributePaths, EventPath[] EventPaths, ushort MinInterval, ushort MaxInterval,
			bool KeepSubscriptions = true, ulong? EventMin = null)
		{
			return this.Execute((Client) => Client.SubscribeAsync(this.controller.Subscriptions, AttributePaths, EventPaths,
				MinInterval, MaxInterval, KeepSubscriptions, true, EventMin), true);
		}

		#endregion

		#region Node information

		/// <summary>
		/// Reads the endpoint structure of the node (Descriptor clusters).
		/// </summary>
		/// <returns>Endpoints.</returns>
		public async Task<EndpointInfo[]> GetEndpointsAsync()
		{
			ReadResult Result = await this.ReadAttributesAsync(
				new AttributePath(null, ClusterIds.Descriptor, 0),
				new AttributePath(null, ClusterIds.Descriptor, 1),
				new AttributePath(null, ClusterIds.Descriptor, 2),
				new AttributePath(null, ClusterIds.Descriptor, 3));

			SortedDictionary<ushort, EndpointInfo> ById = new SortedDictionary<ushort, EndpointInfo>();

			foreach (AttributeReport Report in Result.Attributes)
			{
				if (!Report.HasValue || !Report.Path.EndpointId.HasValue)
					continue;

				ushort EndpointId = Report.Path.EndpointId.Value;

				if (!ById.TryGetValue(EndpointId, out EndpointInfo Info))
				{
					Info = new EndpointInfo(EndpointId);
					ById[EndpointId] = Info;
				}

				switch (Report.Path.AttributeId)
				{
					case 0:
						foreach (TlvElement DT in Report.Data.Children)
							Info.DeviceTypes.Add(new DeviceTypeInfo(DT.Required(0).AsUInt32(), DT[1]?.AsUInt16() ?? 0));
						break;

					case 1:
						foreach (TlvElement C in Report.Data.Children)
							Info.ServerClusters.Add(C.AsUInt32());
						break;

					case 2:
						foreach (TlvElement C in Report.Data.Children)
							Info.ClientClusters.Add(C.AsUInt32());
						break;

					case 3:
						foreach (TlvElement P in Report.Data.Children)
							Info.Parts.Add(P.AsUInt16());
						break;
				}
			}

			List<AttributePath> LabelPaths = new List<AttributePath>();

			foreach (EndpointInfo Info in ById.Values)
			{
				if (Info.ServerClusters.Contains(ClusterIds.BridgedDeviceBasicInformation))
				{
					LabelPaths.Add(new AttributePath(Info.EndpointId, ClusterIds.BridgedDeviceBasicInformation, 5));    // NodeLabel
					LabelPaths.Add(new AttributePath(Info.EndpointId, ClusterIds.BridgedDeviceBasicInformation, 3));    // ProductName
				}
			}

			if (LabelPaths.Count > 0)
			{
				try
				{
					ReadResult Labels = await this.ReadAttributesAsync(LabelPaths.ToArray());

					foreach (EndpointInfo Info in ById.Values)
					{
						string Label = Labels.GetValue(Info.EndpointId, ClusterIds.BridgedDeviceBasicInformation, 5)?.AsString();

						if (string.IsNullOrEmpty(Label))
							Label = Labels.GetValue(Info.EndpointId, ClusterIds.BridgedDeviceBasicInformation, 3)?.AsString();

						Info.Label = Label;
					}
				}
				catch (Exception ex)
				{
					Log.Warning("Unable to read bridged device labels: " + ex.Message);
				}
			}

			EndpointInfo[] Endpoints = new EndpointInfo[ById.Count];
			ById.Values.CopyTo(Endpoints, 0);

			return Endpoints;
		}

		/// <summary>
		/// Reads the fabric index of the controller's fabric on the node.
		/// </summary>
		/// <returns>Fabric index.</returns>
		public async Task<byte> GetCurrentFabricIndexAsync()
		{
			TlvElement E = await this.ReadAttributeAsync(0, ClusterIds.OperationalCredentials, 5);
			return E.AsByte();
		}

		/// <summary>
		/// Removes the controller's fabric from the node (unpairing). After this, the node is no longer accessible by the controller.
		/// </summary>
		public async Task RemoveFromFabricAsync()
		{
			byte FabricIndex = await this.GetCurrentFabricIndexAsync();
			InvokeResult Result = await this.InvokeAsync(0, ClusterIds.OperationalCredentials, 0x0a,
				TlvElement.Structure(TlvTag.Anonymous, TlvElement.UInt(0, FabricIndex)), false, TimeSpan.FromSeconds(10));

			Result.AssertSuccess();

			byte Status = Result.Fields?[0]?.AsByte() ?? 0;
			if (Status != 0)
				throw new MatterException("RemoveFabric failed with status " + Status.ToString() + ".");

			SecureSession Session = this.session;
			this.session = null;
			this.resumption = null;

			if (!(Session is null))
				this.controller.Exchanges.RemoveSession(Session);
		}

		/// <summary>
		/// Opens an enhanced commissioning window on the node, allowing another administrator (ecosystem) to commission it
		/// (multi-admin, Matter Core Specification, 11.19).
		/// </summary>
		/// <param name="Timeout">Duration of the commissioning window (180-900 seconds).</param>
		/// <param name="Passcode">Passcode to use, or null to generate a random passcode.</param>
		/// <param name="Discriminator">Discriminator to use, or null to generate a random discriminator.</param>
		/// <param name="Iterations">PBKDF2 iterations.</param>
		/// <returns>Onboarding payload to give to the other administrator.</returns>
		public async Task<SetupPayload> OpenCommissioningWindowAsync(TimeSpan Timeout, uint? Passcode = null, ushort? Discriminator = null,
			int Iterations = 1000)
		{
			int Seconds = (int)Math.Round(Timeout.TotalSeconds);
			if (Seconds < 180 || Seconds > 900)
				throw new ArgumentOutOfRangeException(nameof(Timeout), "Commissioning window must be open between 180 and 900 seconds.");

			uint Pin = Passcode ?? GenerateRandomPasscode();
			if (!SetupPayload.IsValidPasscode(Pin))
				throw new ArgumentException("Invalid passcode.", nameof(Passcode));

			ushort Disc = Discriminator ?? (ushort)(MatterCrypto.RandomUInt16() & 0x0fff);
			byte[] Salt = MatterCrypto.RandomBytes(32);
			byte[] Verifier = Spake2Plus.ComputeVerifier(Pin, Salt, Iterations);

			ReadResult Info = await this.ReadAttributesAsync(
				new AttributePath(0, ClusterIds.BasicInformation, 2),
				new AttributePath(0, ClusterIds.BasicInformation, 4));

			InvokeResult Result = await this.InvokeAsync(0, ClusterIds.AdministratorCommissioning, 0x00,
				TlvElement.Structure(TlvTag.Anonymous,
					TlvElement.UInt(0, (ushort)Seconds),
					TlvElement.Bytes(1, Verifier),
					TlvElement.UInt(2, Disc),
					TlvElement.UInt(3, (uint)Iterations),
					TlvElement.Bytes(4, Salt)), true, TimeSpan.FromSeconds(30));

			Result.AssertSuccess();

			return new SetupPayload()
			{
				VendorId = Info.GetValue(0, ClusterIds.BasicInformation, 2)?.AsUInt16() ?? 0,
				ProductId = Info.GetValue(0, ClusterIds.BasicInformation, 4)?.AsUInt16() ?? 0,
				Discriminator = Disc,
				Passcode = Pin,
				Flow = CommissioningFlow.Standard,
				Capabilities = DiscoveryCapabilities.OnNetwork
			};
		}

		/// <summary>
		/// Revokes an open commissioning window.
		/// </summary>
		public async Task RevokeCommissioningAsync()
		{
			InvokeResult Result = await this.InvokeAsync(0, ClusterIds.AdministratorCommissioning, 0x02, null, true);
			Result.AssertSuccess();
		}

		/// <summary>
		/// Generates a random, valid passcode.
		/// </summary>
		/// <returns>Passcode.</returns>
		public static uint GenerateRandomPasscode()
		{
			while (true)
			{
				uint Pin = MatterCrypto.RandomUInt32() & 0x07ffffff;

				if (SetupPayload.IsValidPasscode(Pin))
					return Pin;
			}
		}

		#endregion

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Node " + this.NodeId.ToString("X16") + (this.Address is null ? string.Empty : " @ " + this.Address.ToString());
		}
	}

	/// <summary>
	/// Device type information of an endpoint.
	/// </summary>
	public class DeviceTypeInfo
	{
		/// <summary>
		/// Device type information.
		/// </summary>
		/// <param name="DeviceType">Device type ID.</param>
		/// <param name="Revision">Revision.</param>
		public DeviceTypeInfo(uint DeviceType, ushort Revision)
		{
			this.DeviceType = DeviceType;
			this.Revision = Revision;
		}

		/// <summary>
		/// Device type ID.
		/// </summary>
		public uint DeviceType { get; }

		/// <summary>
		/// Revision.
		/// </summary>
		public ushort Revision { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return "0x" + this.DeviceType.ToString("X4") + " rev " + this.Revision.ToString();
		}
	}

	/// <summary>
	/// Information about an endpoint of a node.
	/// </summary>
	public class EndpointInfo
	{
		/// <summary>
		/// Information about an endpoint of a node.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		public EndpointInfo(ushort EndpointId)
		{
			this.EndpointId = EndpointId;
		}

		/// <summary>
		/// Endpoint ID.
		/// </summary>
		public ushort EndpointId { get; }

		/// <summary>
		/// Device types.
		/// </summary>
		public List<DeviceTypeInfo> DeviceTypes { get; } = new List<DeviceTypeInfo>();

		/// <summary>
		/// Server clusters.
		/// </summary>
		public List<uint> ServerClusters { get; } = new List<uint>();

		/// <summary>
		/// Client clusters.
		/// </summary>
		public List<uint> ClientClusters { get; } = new List<uint>();

		/// <summary>
		/// Child endpoints (parts).
		/// </summary>
		public List<ushort> Parts { get; } = new List<ushort>();

		/// <summary>
		/// Label of bridged device, if available.
		/// </summary>
		public string Label { get; internal set; }

		/// <summary>
		/// If the endpoint has a given device type.
		/// </summary>
		/// <param name="DeviceType">Device type ID.</param>
		/// <returns>If the endpoint has the device type.</returns>
		public bool HasDeviceType(uint DeviceType)
		{
			foreach (DeviceTypeInfo DT in this.DeviceTypes)
			{
				if (DT.DeviceType == DeviceType)
					return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Endpoint " + this.EndpointId.ToString() + ": " + string.Join(", ", this.DeviceTypes);
		}
	}
}
