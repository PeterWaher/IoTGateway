using System;
using Waher.Things.Matter.Certificates;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Fabric
{
	/// <summary>
	/// Represents a Matter fabric administered by this controller: root of trust, controller operational identity and
	/// identity protection key (Matter Core Specification, 2.5.1 and 4.15).
	/// </summary>
	public class MatterFabric : IDisposable
	{
		/// <summary>
		/// Test vendor ID 1 (0xFFF1).
		/// </summary>
		public const ushort TestVendorId = 0xFFF1;

		private EcKeyPair rootKey;
		private EcKeyPair operationalKey;

		private MatterFabric()
		{
		}

		/// <summary>
		/// Fabric ID.
		/// </summary>
		public ulong FabricId { get; private set; }

		/// <summary>
		/// Vendor ID of the administrator.
		/// </summary>
		public ushort VendorId { get; private set; }

		/// <summary>
		/// Root key pair.
		/// </summary>
		public EcKeyPair RootKey => this.rootKey;

		/// <summary>
		/// Root CA certificate.
		/// </summary>
		public MatterCertificate Rcac { get; private set; }

		/// <summary>
		/// Operational node ID of the controller.
		/// </summary>
		public ulong ControllerNodeId { get; private set; }

		/// <summary>
		/// Operational key pair of the controller.
		/// </summary>
		public EcKeyPair OperationalKey => this.operationalKey;

		/// <summary>
		/// Node operational certificate of the controller.
		/// </summary>
		public MatterCertificate ControllerNoc { get; private set; }

		/// <summary>
		/// Identity Protection Key (IPK) epoch key.
		/// </summary>
		public byte[] IpkEpochKey { get; private set; }

		/// <summary>
		/// Operational IPK, derived from the epoch key.
		/// </summary>
		public byte[] OperationalIpk { get; private set; }

		/// <summary>
		/// Compressed fabric ID.
		/// </summary>
		public ulong CompressedFabricId { get; private set; }

		/// <summary>
		/// Root public key.
		/// </summary>
		public byte[] RootPublicKey => this.Rcac.PublicKey;

		/// <summary>
		/// Creates a new fabric.
		/// </summary>
		/// <param name="FabricId">Fabric ID.</param>
		/// <param name="VendorId">Vendor ID of the administrator.</param>
		/// <param name="ControllerNodeId">Operational node ID of the controller.</param>
		/// <returns>New fabric.</returns>
		public static MatterFabric Create(ulong FabricId, ushort VendorId, ulong ControllerNodeId)
		{
			if (FabricId == 0)
				throw new ArgumentException("Fabric ID must be non-zero.", nameof(FabricId));

			DateTime NotBefore = DateTime.UtcNow.AddDays(-1);
			EcKeyPair RootKey = EcKeyPair.Generate();
			EcKeyPair OperationalKey = EcKeyPair.Generate();
			ulong RcacId = MatterCrypto.RandomUInt64() & 0x7fffffffffffffff;
			MatterCertificate Rcac = CertificateFactory.CreateRcac(RootKey, RcacId, FabricId, NotBefore);
			MatterCertificate Noc = CertificateFactory.CreateNoc(Rcac, RootKey, OperationalKey.PublicKey, ControllerNodeId, FabricId, null, NotBefore);

			MatterFabric Result = new MatterFabric()
			{
				FabricId = FabricId,
				VendorId = VendorId,
				rootKey = RootKey,
				Rcac = Rcac,
				ControllerNodeId = ControllerNodeId,
				operationalKey = OperationalKey,
				ControllerNoc = Noc,
				IpkEpochKey = MatterCrypto.RandomBytes(16)
			};

			Result.ComputeDerivedValues();

			return Result;
		}

		/// <summary>
		/// Generates a random fabric ID.
		/// </summary>
		/// <returns>Fabric ID.</returns>
		public static ulong NewFabricId()
		{
			ulong Result;

			do
			{
				Result = MatterCrypto.RandomUInt64();
			}
			while (Result == 0);

			return Result;
		}

		/// <summary>
		/// Generates a random operational node ID.
		/// </summary>
		/// <returns>Node ID.</returns>
		public static ulong NewOperationalNodeId()
		{
			ulong Result;

			do
			{
				Result = MatterCrypto.RandomUInt64();
			}
			while (!DistinguishedName.IsOperationalNodeId(Result));

			return Result;
		}

		private void ComputeDerivedValues()
		{
			this.CompressedFabricId = ComputeCompressedFabricId(this.Rcac.PublicKey, this.FabricId);
			this.OperationalIpk = DeriveOperationalGroupKey(this.IpkEpochKey, this.CompressedFabricId);
		}

		/// <summary>
		/// Computes the compressed fabric ID (Matter Core Specification, 4.3.2.2).
		/// </summary>
		/// <param name="RootPublicKey">Root public key (65 bytes).</param>
		/// <param name="FabricId">Fabric ID.</param>
		/// <returns>Compressed fabric ID.</returns>
		public static ulong ComputeCompressedFabricId(byte[] RootPublicKey, ulong FabricId)
		{
			byte[] Salt = ToBigEndian(FabricId);
			byte[] Key = MatterCrypto.Slice(RootPublicKey, 1, 64);
			byte[] Bin = MatterCrypto.Hkdf(Key, Salt, "CompressedFabric", 8);

			return FromBigEndian(Bin);
		}

		/// <summary>
		/// Derives an operational group key from an epoch key (Matter Core Specification, 4.17.2).
		/// </summary>
		/// <param name="EpochKey">Epoch key.</param>
		/// <param name="CompressedFabricId">Compressed fabric ID.</param>
		/// <returns>Operational group key.</returns>
		public static byte[] DeriveOperationalGroupKey(byte[] EpochKey, ulong CompressedFabricId)
		{
			return MatterCrypto.Hkdf(EpochKey, ToBigEndian(CompressedFabricId), "GroupKey v1.0", 16);
		}

		/// <summary>
		/// Computes the destination identifier used in CASE Sigma1 (Matter Core Specification, 4.14.2.4).
		/// </summary>
		/// <param name="InitiatorRandom">Initiator random.</param>
		/// <param name="NodeId">Destination node ID.</param>
		/// <returns>Destination identifier.</returns>
		public byte[] ComputeDestinationId(byte[] InitiatorRandom, ulong NodeId)
		{
			return MatterCrypto.HmacSha256(this.OperationalIpk, InitiatorRandom, this.Rcac.PublicKey,
				ToLittleEndian(this.FabricId), ToLittleEndian(NodeId));
		}

		/// <summary>
		/// Issues a node operational certificate for a node of this fabric.
		/// </summary>
		/// <param name="NodePublicKey">Public key of node.</param>
		/// <param name="NodeId">Operational node ID.</param>
		/// <param name="CaseAuthenticatedTags">Optional CASE Authenticated Tags.</param>
		/// <returns>NOC.</returns>
		public MatterCertificate IssueNoc(byte[] NodePublicKey, ulong NodeId, uint[] CaseAuthenticatedTags = null)
		{
			if (this.rootKey is null || !this.rootKey.HasPrivateKey)
				throw new InvalidOperationException("Root key not available.");

			return CertificateFactory.CreateNoc(this.Rcac, this.rootKey, NodePublicKey, NodeId, this.FabricId,
				CaseAuthenticatedTags, DateTime.UtcNow.AddHours(-1));
		}

		/// <summary>
		/// Gets the DNS-SD instance name of an operational node of the fabric (Matter Core Specification, 4.3.2).
		/// </summary>
		/// <param name="NodeId">Operational node ID.</param>
		/// <returns>Instance name.</returns>
		public string GetOperationalInstanceName(ulong NodeId)
		{
			return this.CompressedFabricId.ToString("X16") + "-" + NodeId.ToString("X16");
		}

		/// <summary>
		/// Validates the operational certificate chain of a peer node, against the root of this fabric.
		/// </summary>
		/// <param name="NocTlv">NOC of peer (TLV).</param>
		/// <param name="IcacTlv">ICAC of peer (TLV), or null.</param>
		/// <returns>Parsed NOC.</returns>
		public MatterCertificate ValidatePeerCredentials(byte[] NocTlv, byte[] IcacTlv)
		{
			MatterCertificate Noc = MatterCertificate.DecodeTlv(NocTlv);
			MatterCertificate Icac = IcacTlv is null ? null : MatterCertificate.DecodeTlv(IcacTlv);

			CertificateFactory.ValidateChain(Noc, Icac, this.Rcac, null);

			if (Noc.Subject.FabricId.Value != this.FabricId)
				throw new MatterSecurityException("Peer belongs to another fabric.");

			return Noc;
		}

		#region Serialization

		/// <summary>
		/// Exports the fabric, including private keys, as TLV. The output must be protected.
		/// </summary>
		/// <returns>Encoded fabric.</returns>
		public byte[] Export()
		{
			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteUInt(1, this.FabricId);
			w.WriteUInt(2, this.VendorId);
			w.WriteBytes(3, this.rootKey.PrivateKey);
			w.WriteBytes(4, this.Rcac.EncodeTlv());
			w.WriteUInt(5, this.ControllerNodeId);
			w.WriteBytes(6, this.operationalKey.PrivateKey);
			w.WriteBytes(7, this.ControllerNoc.EncodeTlv());
			w.WriteBytes(8, this.IpkEpochKey);
			w.EndContainer();

			return w.ToArray();
		}

		/// <summary>
		/// Imports a fabric exported using <see cref="Export"/>.
		/// </summary>
		/// <param name="Encoded">Encoded fabric.</param>
		/// <returns>Fabric.</returns>
		public static MatterFabric Import(byte[] Encoded)
		{
			TlvElement Root = TlvElement.Decode(Encoded);
			MatterCertificate Rcac = MatterCertificate.DecodeTlv(Root.Required(4).AsBytes());
			MatterCertificate Noc = MatterCertificate.DecodeTlv(Root.Required(7).AsBytes());

			MatterFabric Result = new MatterFabric()
			{
				FabricId = Root.Required(1).AsUInt64(),
				VendorId = Root.Required(2).AsUInt16(),
				rootKey = new EcKeyPair(Root.Required(3).AsBytes(), Rcac.PublicKey),
				Rcac = Rcac,
				ControllerNodeId = Root.Required(5).AsUInt64(),
				operationalKey = new EcKeyPair(Root.Required(6).AsBytes(), Noc.PublicKey),
				ControllerNoc = Noc,
				IpkEpochKey = Root.Required(8).AsBytes()
			};

			Result.ComputeDerivedValues();

			return Result;
		}

		/// <summary>
		/// Exports the public parts of the fabric (identifiers and certificates), as TLV. Private keys are exported
		/// separately, using <see cref="RootKey"/>, <see cref="OperationalKey"/> and <see cref="IpkEpochKey"/>.
		/// </summary>
		/// <returns>Encoded public fabric information.</returns>
		public byte[] ExportPublic()
		{
			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteUInt(1, this.FabricId);
			w.WriteUInt(2, this.VendorId);
			w.WriteBytes(4, this.Rcac.EncodeTlv());
			w.WriteUInt(5, this.ControllerNodeId);
			w.WriteBytes(7, this.ControllerNoc.EncodeTlv());
			w.EndContainer();

			return w.ToArray();
		}

		/// <summary>
		/// Imports a fabric from its public parts, exported using <see cref="ExportPublic"/>, and its private keys.
		/// </summary>
		/// <param name="Public">Public fabric information.</param>
		/// <param name="RootPrivateKey">Private key of the root certificate authority (32 bytes).</param>
		/// <param name="OperationalPrivateKey">Private operational key of the controller (32 bytes).</param>
		/// <param name="IpkEpochKey">Identity Protection Key (IPK) epoch key (16 bytes).</param>
		/// <returns>Fabric.</returns>
		public static MatterFabric Import(byte[] Public, byte[] RootPrivateKey, byte[] OperationalPrivateKey, byte[] IpkEpochKey)
		{
			if (RootPrivateKey is null || RootPrivateKey.Length != 32)
				throw new ArgumentException("Invalid root private key.", nameof(RootPrivateKey));

			if (OperationalPrivateKey is null || OperationalPrivateKey.Length != 32)
				throw new ArgumentException("Invalid operational private key.", nameof(OperationalPrivateKey));

			if (IpkEpochKey is null || IpkEpochKey.Length != 16)
				throw new ArgumentException("Invalid IPK epoch key.", nameof(IpkEpochKey));

			TlvElement Root = TlvElement.Decode(Public);
			MatterCertificate Rcac = MatterCertificate.DecodeTlv(Root.Required(4).AsBytes());
			MatterCertificate Noc = MatterCertificate.DecodeTlv(Root.Required(7).AsBytes());
			EcKeyPair RootKey = new EcKeyPair(RootPrivateKey, null);
			EcKeyPair OperationalKey = new EcKeyPair(OperationalPrivateKey, null);

			if (!MatterCrypto.AreEqual(RootKey.PublicKey, Rcac.PublicKey))
				throw new MatterSecurityException("Root private key does not match root certificate.");

			if (!MatterCrypto.AreEqual(OperationalKey.PublicKey, Noc.PublicKey))
				throw new MatterSecurityException("Operational private key does not match operational certificate.");

			MatterFabric Result = new MatterFabric()
			{
				FabricId = Root.Required(1).AsUInt64(),
				VendorId = Root.Required(2).AsUInt16(),
				rootKey = RootKey,
				Rcac = Rcac,
				ControllerNodeId = Root.Required(5).AsUInt64(),
				operationalKey = OperationalKey,
				ControllerNoc = Noc,
				IpkEpochKey = IpkEpochKey
			};

			Result.ComputeDerivedValues();

			return Result;
		}

		#endregion

		internal static byte[] ToBigEndian(ulong Value)
		{
			byte[] Result = new byte[8];
			int i;

			for (i = 7; i >= 0; i--)
			{
				Result[i] = (byte)Value;
				Value >>= 8;
			}

			return Result;
		}

		internal static byte[] ToLittleEndian(ulong Value)
		{
			byte[] Result = new byte[8];
			int i;

			for (i = 0; i < 8; i++)
			{
				Result[i] = (byte)Value;
				Value >>= 8;
			}

			return Result;
		}

		internal static ulong FromBigEndian(byte[] Data)
		{
			ulong Result = 0;

			foreach (byte b in Data)
				Result = (Result << 8) | b;

			return Result;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.rootKey?.Dispose();
			this.operationalKey?.Dispose();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Fabric " + this.FabricId.ToString("X16") + " (compressed " + this.CompressedFabricId.ToString("X16") + ")";
		}
	}
}
