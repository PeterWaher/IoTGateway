using System;
using System.Net;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;
using Waher.Things.Matter.Attestation;
using Waher.Things.Matter.Certificates;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Fabric;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.SecureChannel;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Commissioning
{
	/// <summary>
	/// Regulatory location types.
	/// </summary>
	public enum RegulatoryLocation : byte
	{
		/// <summary>
		/// Indoor.
		/// </summary>
		Indoor = 0,

		/// <summary>
		/// Outdoor.
		/// </summary>
		Outdoor = 1,

		/// <summary>
		/// Indoor and outdoor.
		/// </summary>
		IndoorOutdoor = 2
	}

	/// <summary>
	/// Options for commissioning.
	/// </summary>
	public class CommissioningOptions
	{
		/// <summary>
		/// Options for commissioning.
		/// </summary>
		public CommissioningOptions()
		{
		}

		/// <summary>
		/// How device attestation failures are handled.
		/// </summary>
		public AttestationPolicy AttestationPolicy { get; set; } = AttestationPolicy.Strict;

		/// <summary>
		/// Device attestation verifier. Required unless <see cref="AttestationPolicy"/> is <see cref="AttestationPolicy.Ignore"/>.
		/// </summary>
		public DeviceAttestationVerifier AttestationVerifier { get; set; }

		/// <summary>
		/// Regulatory location to configure.
		/// </summary>
		public RegulatoryLocation RegulatoryLocation { get; set; } = RegulatoryLocation.IndoorOutdoor;

		/// <summary>
		/// Two-letter ISO 3166-1 country code to configure ("XX" if unknown).
		/// </summary>
		public string CountryCode { get; set; } = "XX";

		/// <summary>
		/// Fail-safe expiry length, in seconds.
		/// </summary>
		public ushort FailSafeSeconds { get; set; } = 60;

		/// <summary>
		/// Optional fabric label to set on the device.
		/// </summary>
		public string FabricLabel { get; set; }

		/// <summary>
		/// Optional resolver of the operational address of the commissioned node. If null, or if the resolver returns null,
		/// the address used during PASE is used.
		/// </summary>
		public Func<ulong, Task<IPEndPoint>> OperationalAddressResolver { get; set; }

		/// <summary>
		/// Optional callback receiving progress messages.
		/// </summary>
		public Action<string> Progress { get; set; }
	}

	/// <summary>
	/// Result of a successful commissioning.
	/// </summary>
	public class CommissioningResult
	{
		/// <summary>
		/// Operational node ID assigned to the device.
		/// </summary>
		public ulong NodeId { get; internal set; }

		/// <summary>
		/// Operational address of the device.
		/// </summary>
		public IPEndPoint Address { get; internal set; }

		/// <summary>
		/// Established CASE session.
		/// </summary>
		public SecureSession Session { get; internal set; }

		/// <summary>
		/// Resumption record of the CASE session.
		/// </summary>
		public ResumptionRecord Resumption { get; internal set; }

		/// <summary>
		/// Device attestation result, if attestation was performed.
		/// </summary>
		public AttestationResult Attestation { get; internal set; }

		/// <summary>
		/// Vendor ID reported by the device.
		/// </summary>
		public ushort VendorId { get; internal set; }

		/// <summary>
		/// Product ID reported by the device.
		/// </summary>
		public ushort ProductId { get; internal set; }

		/// <summary>
		/// Fabric index assigned by the device.
		/// </summary>
		public byte FabricIndex { get; internal set; }

		/// <summary>
		/// NOC issued to the device.
		/// </summary>
		public MatterCertificate Noc { get; internal set; }
	}

	/// <summary>
	/// Commissions devices into a fabric, using on-network commissioning (Matter Core Specification, 5.5).
	/// </summary>
	public class Commissioner
	{
		private readonly ExchangeManager manager;
		private readonly MatterFabric fabric;
		private readonly ICommunicationLayer sniffers;

		/// <summary>
		/// Commissions devices into a fabric.
		/// </summary>
		/// <param name="Manager">Exchange manager.</param>
		/// <param name="Fabric">Fabric to commission devices into.</param>
		/// <param name="Sniffers">Optional communication layer for logging.</param>
		public Commissioner(ExchangeManager Manager, MatterFabric Fabric, ICommunicationLayer Sniffers)
		{
			this.manager = Manager;
			this.fabric = Fabric;
			this.sniffers = Sniffers;
		}

		private void Progress(CommissioningOptions Options, string Message)
		{
			this.sniffers?.Information(Message);
			Options.Progress?.Invoke(Message);
		}

		/// <summary>
		/// Matter epoch (2000-01-01T00:00:00Z).
		/// </summary>
		private static readonly DateTime MatterEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Commissions a device.
		/// </summary>
		/// <param name="Address">IP address and port of the commissionable device.</param>
		/// <param name="Passcode">Setup passcode.</param>
		/// <param name="NodeId">Operational node ID to assign to the device.</param>
		/// <param name="Options">Commissioning options.</param>
		/// <returns>Commissioning result.</returns>
		public async Task<CommissioningResult> CommissionAsync(IPEndPoint Address, uint Passcode, ulong NodeId, CommissioningOptions Options)
		{
			if (!DistinguishedName.IsOperationalNodeId(NodeId))
				throw new ArgumentException("Invalid operational node ID.", nameof(NodeId));

			Options = Options ?? new CommissioningOptions();

			if (Options.AttestationPolicy != AttestationPolicy.Ignore && Options.AttestationVerifier is null)
				throw new ArgumentException("An attestation verifier is required.", nameof(Options));

			CommissioningResult Result = new CommissioningResult()
			{
				NodeId = NodeId
			};

			this.Progress(Options, "Establishing PASE session with " + Address.ToString() + ".");

			SecureSession Pase = await PaseClient.EstablishAsync(this.manager, Address, Passcode);
			InteractionClient Client = new InteractionClient(this.manager, Pase);
			bool FailSafeArmed = false;
			ulong Breadcrumb = 1;

			try
			{
				// Read basic commissioning information

				this.Progress(Options, "Reading device information.");

				ReadResult Info = await Client.ReadAttributesAsync(
					new AttributePath(0, ClusterIds.BasicInformation, 2),      // VendorID
					new AttributePath(0, ClusterIds.BasicInformation, 4),      // ProductID
					new AttributePath(0, ClusterIds.GeneralCommissioning, 1),  // BasicCommissioningInfo
					new AttributePath(0, ClusterIds.GeneralCommissioning, 3),  // LocationCapability
					new AttributePath(0, ClusterIds.TimeSynchronization, GlobalAttributeIds.FeatureMap));

				Result.VendorId = Info.GetValue(0, ClusterIds.BasicInformation, 2)?.AsUInt16() ?? 0;
				Result.ProductId = Info.GetValue(0, ClusterIds.BasicInformation, 4)?.AsUInt16() ?? 0;

				ushort FailSafe = Options.FailSafeSeconds;
				TlvElement CommissioningInfo = Info.GetValue(0, ClusterIds.GeneralCommissioning, 1);
				ushort? MaxCumulative = CommissioningInfo?[1]?.AsUInt16();

				if (MaxCumulative.HasValue && MaxCumulative.Value > 0 && FailSafe > MaxCumulative.Value)
					FailSafe = MaxCumulative.Value;

				// Arm fail-safe

				this.Progress(Options, "Arming fail-safe timer (" + FailSafe.ToString() + " s).");
				await this.ArmFailSafe(Client, FailSafe, Breadcrumb++);
				FailSafeArmed = true;

				// Regulatory configuration

				RegulatoryLocation Location = Options.RegulatoryLocation;
				TlvElement Capability = Info.GetValue(0, ClusterIds.GeneralCommissioning, 3);

				if (!(Capability is null) && Capability.AsByte() != (byte)RegulatoryLocation.IndoorOutdoor)
					Location = (RegulatoryLocation)Capability.AsByte();

				this.Progress(Options, "Setting regulatory configuration.");

				InvokeResult Regulatory = await Client.InvokeAsync(0, ClusterIds.GeneralCommissioning, 0x02, TlvElement.Structure(TlvTag.Anonymous,
					TlvElement.UInt(0, (byte)Location),
					TlvElement.String(1, string.IsNullOrEmpty(Options.CountryCode) ? "XX" : Options.CountryCode),
					TlvElement.UInt(2, Breadcrumb++)));

				CheckCommissioningError(Regulatory, "SetRegulatoryConfig");

				// Time synchronization, if supported by the device

				if (!(Info.GetValue(0, ClusterIds.TimeSynchronization, GlobalAttributeIds.FeatureMap) is null))
				{
					this.Progress(Options, "Setting UTC time.");

					try
					{
						ulong UtcMicroseconds = (ulong)((DateTime.UtcNow - MatterEpoch).Ticks / 10);
						InvokeResult TimeResult = await Client.InvokeAsync(0, ClusterIds.TimeSynchronization, 0x00, TlvElement.Structure(TlvTag.Anonymous,
							TlvElement.UInt(0, UtcMicroseconds),
							TlvElement.UInt(1, (byte)3)));     // Milliseconds granularity

						if (!TimeResult.IsSuccess)
							this.Progress(Options, "Device did not accept UTC time: " + TimeResult.Status?.ToString());
					}
					catch (Exception ex)
					{
						this.Progress(Options, "Unable to set UTC time: " + ex.Message);
					}
				}

				// Device attestation

				byte[] DacDer;

				this.Progress(Options, "Requesting device attestation information.");

				DacDer = await this.GetCertificate(Client, 1);
				byte[] PaiDer = await this.GetCertificate(Client, 2);
				byte[] AttestationNonce = MatterCrypto.RandomBytes(32);

				InvokeResult Attestation = await Client.InvokeAsync(0, ClusterIds.OperationalCredentials, 0x00,
					TlvElement.Structure(TlvTag.Anonymous, TlvElement.Bytes(0, AttestationNonce)), false, TimeSpan.FromSeconds(30));

				Attestation.AssertSuccess();

				byte[] AttestationElements = Attestation.Fields.Required(0).AsBytes();
				byte[] AttestationSignature = Attestation.Fields.Required(1).AsBytes();

				if (Options.AttestationPolicy != AttestationPolicy.Ignore)
				{
					AttestationResult AttestationResult = Options.AttestationVerifier.Verify(DacDer, PaiDer, AttestationElements,
						AttestationSignature, AttestationNonce, Pase.AttestationChallenge, Result.VendorId, Result.ProductId, DateTime.UtcNow);

					Result.Attestation = AttestationResult;
					this.Progress(Options, AttestationResult.ToString());

					if (!AttestationResult.Success)
					{
						if (Options.AttestationPolicy == AttestationPolicy.Strict)
							throw new MatterSecurityException(AttestationResult.ToString());
						else
							Log.Warning(AttestationResult.ToString());
					}
				}

				// CSR

				this.Progress(Options, "Requesting operational certificate signing request.");

				byte[] CsrNonce = MatterCrypto.RandomBytes(32);
				InvokeResult CsrResponse = await Client.InvokeAsync(0, ClusterIds.OperationalCredentials, 0x04,
					TlvElement.Structure(TlvTag.Anonymous, TlvElement.Bytes(0, CsrNonce)), false, TimeSpan.FromSeconds(30));

				CsrResponse.AssertSuccess();

				byte[] NocsrElements = CsrResponse.Fields.Required(0).AsBytes();
				byte[] CsrSignature = CsrResponse.Fields.Required(1).AsBytes();

				if (Options.AttestationPolicy != AttestationPolicy.Ignore)
				{
					X509CertificateInfo Dac = X509CertificateInfo.Parse(DacDer);

					if (!EcKeyPair.Verify(Dac.PublicKey, MatterCrypto.Concat(NocsrElements, Pase.AttestationChallenge), CsrSignature))
					{
						string Msg = "Invalid attestation signature of NOCSR elements.";

						if (Options.AttestationPolicy == AttestationPolicy.Strict)
							throw new MatterSecurityException(Msg);
						else
							Log.Warning(Msg);
					}
				}

				TlvElement Nocsr = TlvElement.Decode(NocsrElements);
				byte[] CsrDer = Nocsr.Required(1).AsBytes();
				byte[] EchoedNonce = Nocsr.Required(2).AsBytes();

				if (!MatterCrypto.AreEqual(EchoedNonce, CsrNonce))
					throw new MatterSecurityException("CSR nonce mismatch.");

				CertificateSigningRequest Csr = CertificateSigningRequest.Parse(CsrDer);
				if (!Csr.VerifySignature())
					throw new MatterSecurityException("Invalid CSR signature.");

				// Operational credentials

				MatterCertificate Noc = this.fabric.IssueNoc(Csr.PublicKey, NodeId);
				Result.Noc = Noc;

				this.Progress(Options, "Installing trusted root certificate.");

				InvokeResult Root = await Client.InvokeAsync(0, ClusterIds.OperationalCredentials, 0x0b,
					TlvElement.Structure(TlvTag.Anonymous, TlvElement.Bytes(0, this.fabric.Rcac.EncodeTlv())));

				Root.AssertSuccess();

				this.Progress(Options, "Installing node operational certificate.");

				InvokeResult AddNoc = await Client.InvokeAsync(0, ClusterIds.OperationalCredentials, 0x06,
					TlvElement.Structure(TlvTag.Anonymous,
						TlvElement.Bytes(0, Noc.EncodeTlv()),
						TlvElement.Bytes(2, this.fabric.IpkEpochKey),
						TlvElement.UInt(3, this.fabric.ControllerNodeId),
						TlvElement.UInt(4, this.fabric.VendorId)), false, TimeSpan.FromSeconds(30));

				AddNoc.AssertSuccess();

				byte NocStatus = AddNoc.Fields?[0]?.AsByte() ?? 0xff;
				if (NocStatus != 0)
				{
					throw new MatterException("AddNOC failed with status " + NocStatus.ToString() + ": " +
						(AddNoc.Fields?[2]?.AsString() ?? string.Empty));
				}

				Result.FabricIndex = AddNoc.Fields?[1]?.AsByte() ?? 0;

				if (!string.IsNullOrEmpty(Options.FabricLabel))
				{
					try
					{
						InvokeResult Label = await Client.InvokeAsync(0, ClusterIds.OperationalCredentials, 0x09,
							TlvElement.Structure(TlvTag.Anonymous, TlvElement.String(0, Options.FabricLabel)));
					}
					catch (Exception ex)
					{
						this.Progress(Options, "Unable to set fabric label: " + ex.Message);
					}
				}

				// Operational discovery & CASE

				IPEndPoint OperationalAddress = null;

				if (!(Options.OperationalAddressResolver is null))
				{
					this.Progress(Options, "Resolving operational address.");

					try
					{
						OperationalAddress = await Options.OperationalAddressResolver(NodeId);
					}
					catch (Exception ex)
					{
						this.Progress(Options, "Unable to resolve operational address: " + ex.Message);
					}
				}

				OperationalAddress ??= Pase.PeerAddress;
				Result.Address = OperationalAddress;

				this.Progress(Options, "Establishing CASE session with " + OperationalAddress.ToString() + ".");

				(SecureSession Case, ResumptionRecord Resumption) = await CaseClient.EstablishAsync(this.manager, OperationalAddress, this.fabric, NodeId);

				Result.Session = Case;
				Result.Resumption = Resumption;

				// Complete commissioning

				this.Progress(Options, "Completing commissioning.");

				InteractionClient CaseClient2 = new InteractionClient(this.manager, Case);
				InvokeResult Complete = await CaseClient2.InvokeAsync(0, ClusterIds.GeneralCommissioning, 0x04, null);
				CheckCommissioningError(Complete, "CommissioningComplete");

				FailSafeArmed = false;

				this.Progress(Options, "Commissioning completed successfully.");

				return Result;
			}
			catch (Exception)
			{
				if (FailSafeArmed)
				{
					try
					{
						await this.ArmFailSafe(Client, 0, 0);
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				throw;
			}
			finally
			{
				await this.manager.CloseSessionAsync(Pase);
			}
		}

		private async Task ArmFailSafe(InteractionClient Client, ushort ExpiryLengthSeconds, ulong Breadcrumb)
		{
			InvokeResult Result = await Client.InvokeAsync(0, ClusterIds.GeneralCommissioning, 0x00, TlvElement.Structure(TlvTag.Anonymous,
				TlvElement.UInt(0, ExpiryLengthSeconds),
				TlvElement.UInt(1, Breadcrumb)));

			CheckCommissioningError(Result, "ArmFailSafe");
		}

		private async Task<byte[]> GetCertificate(InteractionClient Client, byte CertificateType)
		{
			InvokeResult Result = await Client.InvokeAsync(0, ClusterIds.OperationalCredentials, 0x02,
				TlvElement.Structure(TlvTag.Anonymous, TlvElement.UInt(0, CertificateType)));

			Result.AssertSuccess();

			return Result.Fields.Required(0).AsBytes();
		}

		private static void CheckCommissioningError(InvokeResult Result, string Command)
		{
			Result.AssertSuccess();

			byte ErrorCode = Result.Fields?[0]?.AsByte() ?? 0;
			if (ErrorCode != 0)
			{
				string Debug = Result.Fields?[1]?.AsString();
				throw new MatterException(Command + " failed with error code " + ErrorCode.ToString() +
					(string.IsNullOrEmpty(Debug) ? "." : ": " + Debug));
			}
		}
	}
}
