using System;
using System.Collections.Generic;
using Waher.Things.Matter.Asn1;
using Waher.Things.Matter.Certificates;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Attestation
{
	/// <summary>
	/// Certification type of a certification declaration.
	/// </summary>
	public enum CertificationType : byte
	{
		/// <summary>
		/// Development and test.
		/// </summary>
		DevelopmentAndTest = 0,

		/// <summary>
		/// Provisional.
		/// </summary>
		Provisional = 1,

		/// <summary>
		/// Official.
		/// </summary>
		Official = 2
	}

	/// <summary>
	/// Certification Declaration (CD), a CMS SignedData structure signed by the CSA, wrapping a TLV-encoded
	/// declaration (Matter Core Specification, 6.3).
	/// </summary>
	public class CertificationDeclaration
	{
		private static readonly List<KeyValuePair<byte[], byte[]>> officialSigningKeys = new List<KeyValuePair<byte[], byte[]>>()
		{
			Key("FE343F959947763B61EE4539131338494FE67D8E",
				"04cdeee93e44f8b72be3d1a9c07e21968b9afff3b403f05e1669d7b1e5caee6fc7714b42e7e23695e92cd7635473a280ae688f37bb9489e11629b9b94ff7b09929"),
			Key("DD04DB585B214C1C581587E6568DF487B6DDC701",
				"04031937e8f94251045df27457bb46253ee3754e8c12ae28557d8027b9d4c356d91b408cff32275017b25f8c8ba20516d5c83af2b724128013dfcc8f95d200a90b"),
			Key("471035E7C04EAAA8BE7C4D4C13E3E4C20995A84B",
				"049f575cd5fdb7521f10a4df31f073912b614728f2d37f5b6b96bc2cbf7c0b119690577d555d21a09660a8b082a339ea530f7a812d8693b56fd764185fabcd32f5"),
			Key("F68603A3692E981072419EA1E1AB3854BD7795D3",
				"047cfc8d8810a89cf4fa191778f2afec78f8517a97a3e57fc213bad888e3611e74ffb684bdeba8a68b25234a5c358f373bab9b6d304e1306dd7620a528d1161c0b"),
			Key("637F2634AD62EAFE6AF662EFB96F6FD2FCBFFC2F",
				"04438a52c662a2a6d72647f95eb753136ee4ae0fdb3aa9c16931426b3d0867f9103ae7d7a1b887e9f813a6f26f76bbcda93593787c12778ca8f2975dc45a2042f9")
		};

		private static readonly KeyValuePair<byte[], byte[]> testSigningKey = Key("62FA823359ACFAA9963E1CFA140ADDF504F37160",
			"043c398922452b55caf389c25bd1bca4656952ccb90e8869249ad8474653014cbf95d687965e036b521c51037e6b8cedefca1eb44046694fa08882eed6519decba");

		private static KeyValuePair<byte[], byte[]> Key(string Kid, string PublicKey)
		{
			return new KeyValuePair<byte[], byte[]>(MatterCrypto.FromHex(Kid), MatterCrypto.FromHex(PublicKey));
		}

		private CertificationDeclaration()
		{
		}

		/// <summary>
		/// Format version.
		/// </summary>
		public ushort FormatVersion { get; private set; }

		/// <summary>
		/// Vendor ID.
		/// </summary>
		public ushort VendorId { get; private set; }

		/// <summary>
		/// Product IDs.
		/// </summary>
		public ushort[] ProductIds { get; private set; }

		/// <summary>
		/// Device type ID.
		/// </summary>
		public uint DeviceTypeId { get; private set; }

		/// <summary>
		/// Certificate ID.
		/// </summary>
		public string CertificateId { get; private set; }

		/// <summary>
		/// Security level.
		/// </summary>
		public byte SecurityLevel { get; private set; }

		/// <summary>
		/// Security information.
		/// </summary>
		public ushort SecurityInformation { get; private set; }

		/// <summary>
		/// Version number.
		/// </summary>
		public ushort VersionNumber { get; private set; }

		/// <summary>
		/// Certification type.
		/// </summary>
		public CertificationType CertificationType { get; private set; }

		/// <summary>
		/// DAC origin vendor ID, if present.
		/// </summary>
		public ushort? DacOriginVendorId { get; private set; }

		/// <summary>
		/// DAC origin product ID, if present.
		/// </summary>
		public ushort? DacOriginProductId { get; private set; }

		/// <summary>
		/// Authorized PAA key identifiers, if present.
		/// </summary>
		public byte[][] AuthorizedPaaList { get; private set; }

		/// <summary>
		/// Key identifier of the signer.
		/// </summary>
		public byte[] SignerKeyId { get; private set; }

		/// <summary>
		/// If the declaration was signed using the public test key.
		/// </summary>
		public bool SignedWithTestKey { get; private set; }

		/// <summary>
		/// Parses a CMS-encoded certification declaration, and verifies its signature.
		/// </summary>
		/// <param name="Cms">CMS SignedData (DER).</param>
		/// <param name="AllowTestKey">If the publicly known test signing key is accepted.</param>
		/// <returns>Certification declaration.</returns>
		/// <exception cref="MatterSecurityException">If the declaration cannot be validated.</exception>
		public static CertificationDeclaration ParseAndVerify(byte[] Cms, bool AllowTestKey)
		{
			DerReader ContentInfo = new DerReader(Cms).ReadSequence();
			if (ContentInfo.ReadOid() != Oids.Pkcs7SignedData)
				throw new MatterSecurityException("Certification declaration is not a CMS SignedData structure.");

			DerReader SignedData = ContentInfo.ReadContextSpecific(0).ReadSequence();
			SignedData.ReadInteger();       // Version
			SignedData.ReadSet();           // Digest algorithms

			DerReader EncapContentInfo = SignedData.ReadSequence();
			if (EncapContentInfo.ReadOid() != Oids.Pkcs7Data)
				throw new MatterSecurityException("Unexpected content type in certification declaration.");

			byte[] Content = EncapContentInfo.ReadContextSpecific(0).ReadOctetString();

			while (SignedData.HasMore && (SignedData.IsContextSpecific(0) || SignedData.IsContextSpecific(1)))
				SignedData.Skip();          // Certificates, CRLs

			DerReader SignerInfos = SignedData.ReadSet();
			DerReader SignerInfo = SignerInfos.ReadSequence();
			SignerInfo.ReadInteger();       // Version

			byte[] Kid = SignerInfo.ReadElement(out byte Tag);
			if (Tag != 0x80)
				throw new MatterSecurityException("Certification declaration signer must be identified by subject key identifier.");

			DerReader DigestAlg = SignerInfo.ReadSequence();
			if (DigestAlg.ReadOid() != Oids.Sha256)
				throw new MatterSecurityException("Unsupported digest algorithm in certification declaration.");

			if (SignerInfo.IsContextSpecific(0))
				throw new MatterSecurityException("Signed attributes not supported in certification declarations.");

			DerReader SigAlg = SignerInfo.ReadSequence();
			if (SigAlg.ReadOid() != Oids.EcdsaWithSha256)
				throw new MatterSecurityException("Unsupported signature algorithm in certification declaration.");

			byte[] Signature = EcKeyPair.DerToRawSignature(SignerInfo.ReadOctetString());

			byte[] PublicKey = null;
			bool IsTestKey = false;

			foreach (KeyValuePair<byte[], byte[]> P in officialSigningKeys)
			{
				if (MatterCrypto.AreEqual(P.Key, Kid))
				{
					PublicKey = P.Value;
					break;
				}
			}

			if (PublicKey is null && MatterCrypto.AreEqual(testSigningKey.Key, Kid))
			{
				if (!AllowTestKey)
					throw new MatterSecurityException("Certification declaration signed with the test key, which is not accepted.");

				PublicKey = testSigningKey.Value;
				IsTestKey = true;
			}

			if (PublicKey is null)
				throw new MatterSecurityException("Certification declaration signed by unknown key " + MatterCrypto.ToHex(Kid) + ".");

			if (!EcKeyPair.Verify(PublicKey, Content, Signature))
				throw new MatterSecurityException("Invalid certification declaration signature.");

			CertificationDeclaration Result = Parse(Content);
			Result.SignerKeyId = Kid;
			Result.SignedWithTestKey = IsTestKey;

			return Result;
		}

		/// <summary>
		/// Parses the TLV content of a certification declaration.
		/// </summary>
		/// <param name="Tlv">TLV content.</param>
		/// <returns>Certification declaration.</returns>
		public static CertificationDeclaration Parse(byte[] Tlv)
		{
			TlvElement Root = TlvElement.Decode(Tlv);
			List<ushort> ProductIds = new List<ushort>();

			foreach (TlvElement Pid in Root.Required(2).Children)
				ProductIds.Add(Pid.AsUInt16());

			CertificationDeclaration Result = new CertificationDeclaration()
			{
				FormatVersion = Root.Required(0).AsUInt16(),
				VendorId = Root.Required(1).AsUInt16(),
				ProductIds = ProductIds.ToArray(),
				DeviceTypeId = Root.Required(3).AsUInt32(),
				CertificateId = Root.Required(4).AsString(),
				SecurityLevel = Root.Required(5).AsByte(),
				SecurityInformation = Root.Required(6).AsUInt16(),
				VersionNumber = Root.Required(7).AsUInt16(),
				CertificationType = (CertificationType)Root.Required(8).AsByte()
			};

			if (Root.TryGet(9, out TlvElement E))
				Result.DacOriginVendorId = E.AsUInt16();

			if (Root.TryGet(10, out E))
				Result.DacOriginProductId = E.AsUInt16();

			if (Root.TryGet(11, out E))
			{
				List<byte[]> Paas = new List<byte[]>();

				foreach (TlvElement Paa in E.Children)
					Paas.Add(Paa.AsBytes());

				Result.AuthorizedPaaList = Paas.ToArray();
			}

			return Result;
		}

		/// <summary>
		/// If the declaration includes a given product ID.
		/// </summary>
		/// <param name="ProductId">Product ID.</param>
		/// <returns>If included.</returns>
		public bool IncludesProduct(ushort ProductId)
		{
			return Array.IndexOf(this.ProductIds, ProductId) >= 0;
		}
	}
}
