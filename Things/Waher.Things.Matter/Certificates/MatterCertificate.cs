using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things.Matter.Asn1;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Certificates
{
	/// <summary>
	/// Key usage flags, as encoded in Matter certificates (Matter Core Specification, 6.5.11.2).
	/// </summary>
	[Flags]
	public enum KeyUsage : ushort
	{
		/// <summary>
		/// Digital signature.
		/// </summary>
		DigitalSignature = 0x0001,

		/// <summary>
		/// Non-repudiation.
		/// </summary>
		NonRepudiation = 0x0002,

		/// <summary>
		/// Key encipherment.
		/// </summary>
		KeyEncipherment = 0x0004,

		/// <summary>
		/// Data encipherment.
		/// </summary>
		DataEncipherment = 0x0008,

		/// <summary>
		/// Key agreement.
		/// </summary>
		KeyAgreement = 0x0010,

		/// <summary>
		/// Certificate signing.
		/// </summary>
		KeyCertSign = 0x0020,

		/// <summary>
		/// CRL signing.
		/// </summary>
		CrlSign = 0x0040,

		/// <summary>
		/// Encipher only.
		/// </summary>
		EncipherOnly = 0x0080,

		/// <summary>
		/// Decipher only.
		/// </summary>
		DecipherOnly = 0x0100
	}

	/// <summary>
	/// Extended key usage values, as encoded in Matter certificates.
	/// </summary>
	public enum ExtendedKeyUsage : byte
	{
		/// <summary>
		/// Server authentication.
		/// </summary>
		ServerAuth = 1,

		/// <summary>
		/// Client authentication.
		/// </summary>
		ClientAuth = 2,

		/// <summary>
		/// Code signing.
		/// </summary>
		CodeSigning = 3,

		/// <summary>
		/// E-mail protection.
		/// </summary>
		EmailProtection = 4,

		/// <summary>
		/// Time stamping.
		/// </summary>
		TimeStamping = 5,

		/// <summary>
		/// OCSP signing.
		/// </summary>
		OcspSigning = 6
	}

	/// <summary>
	/// Matter operational certificate in Matter TLV encoding (Matter Core Specification, 6.5), with conversion to
	/// X.509 DER for signature generation and verification.
	/// </summary>
	public class MatterCertificate
	{
		/// <summary>
		/// Matter epoch (2000-01-01T00:00:00Z).
		/// </summary>
		public static readonly DateTime MatterEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Date used to represent "no well-defined expiration date".
		/// </summary>
		public static readonly DateTime NoExpiration = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

		/// <summary>
		/// Maximum size of a TLV-encoded Matter certificate.
		/// </summary>
		public const int MaxTlvSize = 400;

		private readonly List<byte[]> futureExtensions = new List<byte[]>();
		private readonly List<int> extensionOrder = new List<int>();
		private byte[] encodedTlv = null;

		/// <summary>
		/// Matter operational certificate.
		/// </summary>
		public MatterCertificate()
		{
		}

		/// <summary>
		/// Serial number.
		/// </summary>
		public byte[] SerialNumber { get; set; }

		/// <summary>
		/// Signature algorithm (1 = ecdsa-with-SHA256).
		/// </summary>
		public byte SignatureAlgorithm { get; set; } = 1;

		/// <summary>
		/// Issuer.
		/// </summary>
		public DistinguishedName Issuer { get; set; } = new DistinguishedName();

		/// <summary>
		/// Not before, in seconds since the Matter epoch.
		/// </summary>
		public uint NotBefore { get; set; }

		/// <summary>
		/// Not after, in seconds since the Matter epoch (0 = no well-defined expiration).
		/// </summary>
		public uint NotAfter { get; set; }

		/// <summary>
		/// Subject.
		/// </summary>
		public DistinguishedName Subject { get; set; } = new DistinguishedName();

		/// <summary>
		/// Public key algorithm (1 = EC).
		/// </summary>
		public byte PublicKeyAlgorithm { get; set; } = 1;

		/// <summary>
		/// Elliptic curve identifier (1 = prime256v1).
		/// </summary>
		public byte EllipticCurveId { get; set; } = 1;

		/// <summary>
		/// Public key (uncompressed, 65 bytes).
		/// </summary>
		public byte[] PublicKey { get; set; }

		/// <summary>
		/// If basic constraints extension is present.
		/// </summary>
		public bool HasBasicConstraints { get; set; } = true;

		/// <summary>
		/// If the certificate belongs to a certificate authority.
		/// </summary>
		public bool IsCa { get; set; }

		/// <summary>
		/// Path length constraint, if any.
		/// </summary>
		public byte? PathLength { get; set; }

		/// <summary>
		/// Key usage.
		/// </summary>
		public KeyUsage? KeyUsage { get; set; }

		/// <summary>
		/// Extended key usage, if any.
		/// </summary>
		public ExtendedKeyUsage[] ExtendedKeyUsage { get; set; }

		/// <summary>
		/// Subject key identifier.
		/// </summary>
		public byte[] SubjectKeyId { get; set; }

		/// <summary>
		/// Authority key identifier.
		/// </summary>
		public byte[] AuthorityKeyId { get; set; }

		/// <summary>
		/// Future extensions (DER-encoded X.509 extensions).
		/// </summary>
		public IReadOnlyList<byte[]> FutureExtensions => this.futureExtensions;

		/// <summary>
		/// Signature (raw r || s, 64 bytes).
		/// </summary>
		public byte[] Signature { get; set; }

		/// <summary>
		/// Not before, as a date and time.
		/// </summary>
		public DateTime NotBeforeTime => MatterEpoch.AddSeconds(this.NotBefore);

		/// <summary>
		/// Not after, as a date and time.
		/// </summary>
		public DateTime NotAfterTime => this.NotAfter == 0 ? NoExpiration : MatterEpoch.AddSeconds(this.NotAfter);

		/// <summary>
		/// Converts a date and time to seconds since the Matter epoch.
		/// </summary>
		/// <param name="TP">Date and time.</param>
		/// <returns>Seconds since the Matter epoch.</returns>
		public static uint ToMatterTime(DateTime TP)
		{
			double s = (TP.ToUniversalTime() - MatterEpoch).TotalSeconds;

			if (s < 0)
				return 0;
			else if (s > uint.MaxValue)
				return uint.MaxValue;
			else
				return (uint)s;
		}

		/// <summary>
		/// Adds a future extension.
		/// </summary>
		/// <param name="DerExtension">DER-encoded extension.</param>
		public void AddFutureExtension(byte[] DerExtension)
		{
			this.futureExtensions.Add(DerExtension);
			this.encodedTlv = null;
		}

		#region TLV

		/// <summary>
		/// Encodes the certificate using Matter TLV. If the certificate was decoded from TLV, the original encoding is returned.
		/// </summary>
		/// <returns>TLV encoding.</returns>
		public byte[] EncodeTlv()
		{
			if (!(this.encodedTlv is null))
				return this.encodedTlv;

			TlvWriter w = new TlvWriter();

			w.StartStructure();
			w.WriteBytes(1, this.SerialNumber);
			w.WriteUInt(2, this.SignatureAlgorithm);
			this.Issuer.EncodeTlv(w, 3);
			w.WriteUInt(4, this.NotBefore);
			w.WriteUInt(5, this.NotAfter);
			this.Subject.EncodeTlv(w, 6);
			w.WriteUInt(7, this.PublicKeyAlgorithm);
			w.WriteUInt(8, this.EllipticCurveId);
			w.WriteBytes(9, this.PublicKey);

			w.StartList(10);

			foreach (int Ext in this.GetExtensionOrder())
			{
				switch (Ext)
				{
					case 1:
						w.StartStructure(1);
						w.WriteBoolean(1, this.IsCa);
						if (this.PathLength.HasValue)
							w.WriteUInt(2, this.PathLength.Value);
						w.EndContainer();
						break;

					case 2:
						w.WriteUInt(2, (ushort)this.KeyUsage.Value);
						break;

					case 3:
						w.StartArray(3);
						foreach (ExtendedKeyUsage Usage in this.ExtendedKeyUsage)
							w.WriteUInt(TlvTag.Anonymous, (byte)Usage);
						w.EndContainer();
						break;

					case 4:
						w.WriteBytes(4, this.SubjectKeyId);
						break;

					case 5:
						w.WriteBytes(5, this.AuthorityKeyId);
						break;

					case 6:
						foreach (byte[] Future in this.futureExtensions)
							w.WriteBytes(6, Future);
						break;
				}
			}

			w.EndContainer();

			if (!(this.Signature is null))
				w.WriteBytes(11, this.Signature);

			w.EndContainer();

			return w.ToArray();
		}

		private IEnumerable<int> GetExtensionOrder()
		{
			if (this.extensionOrder.Count > 0)
				return this.extensionOrder;

			List<int> Result = new List<int>();

			if (this.HasBasicConstraints)
				Result.Add(1);

			if (this.KeyUsage.HasValue)
				Result.Add(2);

			if (!(this.ExtendedKeyUsage is null))
				Result.Add(3);

			if (!(this.SubjectKeyId is null))
				Result.Add(4);

			if (!(this.AuthorityKeyId is null))
				Result.Add(5);

			if (this.futureExtensions.Count > 0)
				Result.Add(6);

			return Result;
		}

		/// <summary>
		/// Decodes a TLV-encoded Matter certificate.
		/// </summary>
		/// <param name="Tlv">TLV encoding.</param>
		/// <returns>Certificate.</returns>
		public static MatterCertificate DecodeTlv(byte[] Tlv)
		{
			if (Tlv.Length > MaxTlvSize)
				throw new MatterDecodingException("Matter certificate too large.");

			TlvElement Root = TlvElement.Decode(Tlv);
			if (!Root.IsStructure)
				throw new MatterDecodingException("Matter certificate must be a TLV structure.");

			MatterCertificate Result = new MatterCertificate()
			{
				SerialNumber = Root.Required(1).AsBytes(),
				SignatureAlgorithm = Root.Required(2).AsByte(),
				Issuer = DistinguishedName.DecodeTlv(Root.Required(3)),
				NotBefore = Root.Required(4).AsUInt32(),
				NotAfter = Root.Required(5).AsUInt32(),
				Subject = DistinguishedName.DecodeTlv(Root.Required(6)),
				PublicKeyAlgorithm = Root.Required(7).AsByte(),
				EllipticCurveId = Root.Required(8).AsByte(),
				PublicKey = Root.Required(9).AsBytes(),
				Signature = Root[11]?.AsBytes(),
				HasBasicConstraints = false
			};

			TlvElement Extensions = Root.Required(10);

			foreach (TlvElement Ext in Extensions.Children)
			{
				if (!Ext.Tag.IsContextSpecific)
					continue;

				int Tag = (int)Ext.Tag.Number;

				switch (Tag)
				{
					case 1:
						Result.HasBasicConstraints = true;
						Result.IsCa = Ext[1]?.AsBoolean() ?? false;
						if (Ext.TryGet(2, out TlvElement PathLen))
							Result.PathLength = PathLen.AsByte();
						break;

					case 2:
						Result.KeyUsage = (KeyUsage)Ext.AsUInt16();
						break;

					case 3:
						List<ExtendedKeyUsage> Usages = new List<ExtendedKeyUsage>();
						foreach (TlvElement U in Ext.Children)
							Usages.Add((ExtendedKeyUsage)U.AsByte());
						Result.ExtendedKeyUsage = Usages.ToArray();
						break;

					case 4:
						Result.SubjectKeyId = Ext.AsBytes();
						break;

					case 5:
						Result.AuthorityKeyId = Ext.AsBytes();
						break;

					case 6:
						Result.futureExtensions.Add(Ext.AsBytes());
						break;

					default:
						throw new MatterDecodingException("Unknown certificate extension: " + Tag.ToString());
				}

				if (!Result.extensionOrder.Contains(Tag))
					Result.extensionOrder.Add(Tag);
			}

			if (Result.SignatureAlgorithm != 1)
				throw new MatterDecodingException("Unsupported signature algorithm.");

			if (Result.PublicKeyAlgorithm != 1 || Result.EllipticCurveId != 1 || Result.PublicKey is null || Result.PublicKey.Length != 65)
				throw new MatterDecodingException("Unsupported public key.");

			Result.encodedTlv = Tlv;

			return Result;
		}

		#endregion

		#region X.509

		/// <summary>
		/// Encodes the TBSCertificate (to-be-signed part) of the corresponding X.509 certificate.
		/// </summary>
		/// <returns>DER encoding of TBSCertificate.</returns>
		public byte[] EncodeTbsDer()
		{
			DerWriter w = new DerWriter();

			w.StartSequence();

			w.StartContextSpecific(0);
			w.WriteInteger(2);
			w.EndContextSpecific();

			w.WriteUnsignedInteger(this.SerialNumber);

			w.StartSequence();
			w.WriteOid(Oids.EcdsaWithSha256);
			w.EndSequence();

			this.Issuer.EncodeDer(w);

			w.StartSequence();
			w.WriteTime(this.NotBeforeTime);
			w.WriteTime(this.NotAfterTime);
			w.EndSequence();

			this.Subject.EncodeDer(w);

			w.StartSequence();
			w.StartSequence();
			w.WriteOid(Oids.EcPublicKey);
			w.WriteOid(Oids.Prime256v1);
			w.EndSequence();
			w.WriteBitString(this.PublicKey, 0);
			w.EndSequence();

			w.StartContextSpecific(3);
			w.StartSequence();

			foreach (int Ext in this.GetExtensionOrder())
			{
				switch (Ext)
				{
					case 1:
						DerWriter v = new DerWriter();
						v.StartSequence();
						if (this.IsCa)
							v.WriteBoolean(true);
						if (this.PathLength.HasValue)
							v.WriteInteger(this.PathLength.Value);
						v.EndSequence();
						WriteExtension(w, Oids.BasicConstraints, true, v.ToArray());
						break;

					case 2:
						v = new DerWriter();
						v.WriteNamedBitString((ushort)this.KeyUsage.Value);
						WriteExtension(w, Oids.KeyUsage, true, v.ToArray());
						break;

					case 3:
						v = new DerWriter();
						v.StartSequence();
						foreach (ExtendedKeyUsage Usage in this.ExtendedKeyUsage)
							v.WriteOid(Oids.GetExtendedKeyUsageOid(Usage));
						v.EndSequence();
						WriteExtension(w, Oids.ExtendedKeyUsage, true, v.ToArray());
						break;

					case 4:
						v = new DerWriter();
						v.WriteOctetString(this.SubjectKeyId);
						WriteExtension(w, Oids.SubjectKeyIdentifier, false, v.ToArray());
						break;

					case 5:
						v = new DerWriter();
						v.StartSequence();
						v.WriteElement(0x80, this.AuthorityKeyId);
						v.EndSequence();
						WriteExtension(w, Oids.AuthorityKeyIdentifier, false, v.ToArray());
						break;

					case 6:
						foreach (byte[] Future in this.futureExtensions)
							w.WriteRaw(Future);
						break;
				}
			}

			w.EndSequence();
			w.EndContextSpecific();

			w.EndSequence();

			return w.ToArray();
		}

		private static void WriteExtension(DerWriter w, string Oid, bool Critical, byte[] Value)
		{
			w.StartSequence();
			w.WriteOid(Oid);
			if (Critical)
				w.WriteBoolean(true);
			w.WriteOctetString(Value);
			w.EndSequence();
		}

		/// <summary>
		/// Encodes the certificate as an X.509 certificate (DER).
		/// </summary>
		/// <returns>DER encoding.</returns>
		public byte[] EncodeX509Der()
		{
			if (this.Signature is null)
				throw new InvalidOperationException("Certificate not signed.");

			DerWriter w = new DerWriter();

			w.StartSequence();
			w.WriteRaw(this.EncodeTbsDer());
			w.StartSequence();
			w.WriteOid(Oids.EcdsaWithSha256);
			w.EndSequence();
			w.WriteBitString(EcKeyPair.RawToDerSignature(this.Signature), 0);
			w.EndSequence();

			return w.ToArray();
		}

		/// <summary>
		/// Signs the certificate.
		/// </summary>
		/// <param name="IssuerKey">Key pair of issuer.</param>
		public void Sign(EcKeyPair IssuerKey)
		{
			this.encodedTlv = null;
			this.Signature = IssuerKey.Sign(this.EncodeTbsDer());
		}

		/// <summary>
		/// Verifies the signature of the certificate.
		/// </summary>
		/// <param name="IssuerPublicKey">Public key of issuer.</param>
		/// <returns>If signature is valid.</returns>
		public bool VerifySignature(byte[] IssuerPublicKey)
		{
			if (this.Signature is null)
				return false;

			return EcKeyPair.Verify(IssuerPublicKey, this.EncodeTbsDer(), this.Signature);
		}

		#endregion

		/// <summary>
		/// If the certificate is valid at a given point in time.
		/// </summary>
		/// <param name="TP">Point in time.</param>
		/// <returns>If valid.</returns>
		public bool IsValidAt(DateTime TP)
		{
			uint T = ToMatterTime(TP);

			if (T < this.NotBefore)
				return false;

			if (this.NotAfter != 0 && T > this.NotAfter)
				return false;

			return true;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Subject: ");
			sb.Append(this.Subject.ToString());
			sb.Append("; Issuer: ");
			sb.Append(this.Issuer.ToString());

			return sb.ToString();
		}
	}

	/// <summary>
	/// Object identifiers used in certificates.
	/// </summary>
	public static class Oids
	{
		/// <summary>
		/// ecdsa-with-SHA256
		/// </summary>
		public const string EcdsaWithSha256 = "1.2.840.10045.4.3.2";

		/// <summary>
		/// id-ecPublicKey
		/// </summary>
		public const string EcPublicKey = "1.2.840.10045.2.1";

		/// <summary>
		/// prime256v1
		/// </summary>
		public const string Prime256v1 = "1.2.840.10045.3.1.7";

		/// <summary>
		/// Basic constraints extension.
		/// </summary>
		public const string BasicConstraints = "2.5.29.19";

		/// <summary>
		/// Key usage extension.
		/// </summary>
		public const string KeyUsage = "2.5.29.15";

		/// <summary>
		/// Extended key usage extension.
		/// </summary>
		public const string ExtendedKeyUsage = "2.5.29.37";

		/// <summary>
		/// Subject key identifier extension.
		/// </summary>
		public const string SubjectKeyIdentifier = "2.5.29.14";

		/// <summary>
		/// Authority key identifier extension.
		/// </summary>
		public const string AuthorityKeyIdentifier = "2.5.29.35";

		/// <summary>
		/// PKCS#7 signed data.
		/// </summary>
		public const string Pkcs7SignedData = "1.2.840.113549.1.7.2";

		/// <summary>
		/// PKCS#7 data.
		/// </summary>
		public const string Pkcs7Data = "1.2.840.113549.1.7.1";

		/// <summary>
		/// SHA-256 digest algorithm.
		/// </summary>
		public const string Sha256 = "2.16.840.1.101.3.4.2.1";

		/// <summary>
		/// Matter Vendor ID attribute (device attestation certificates).
		/// </summary>
		public const string MatterVendorId = "1.3.6.1.4.1.37244.2.1";

		/// <summary>
		/// Matter Product ID attribute (device attestation certificates).
		/// </summary>
		public const string MatterProductId = "1.3.6.1.4.1.37244.2.2";

		/// <summary>
		/// Gets the OID of an extended key usage value.
		/// </summary>
		/// <param name="Usage">Usage.</param>
		/// <returns>OID.</returns>
		public static string GetExtendedKeyUsageOid(ExtendedKeyUsage Usage)
		{
			switch (Usage)
			{
				case Certificates.ExtendedKeyUsage.ServerAuth: return "1.3.6.1.5.5.7.3.1";
				case Certificates.ExtendedKeyUsage.ClientAuth: return "1.3.6.1.5.5.7.3.2";
				case Certificates.ExtendedKeyUsage.CodeSigning: return "1.3.6.1.5.5.7.3.3";
				case Certificates.ExtendedKeyUsage.EmailProtection: return "1.3.6.1.5.5.7.3.4";
				case Certificates.ExtendedKeyUsage.TimeStamping: return "1.3.6.1.5.5.7.3.8";
				case Certificates.ExtendedKeyUsage.OcspSigning: return "1.3.6.1.5.5.7.3.9";
				default: throw new ArgumentException("Unknown extended key usage.", nameof(Usage));
			}
		}
	}
}
