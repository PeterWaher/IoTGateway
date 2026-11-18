using System;
using Waher.Things.Matter.Crypto;

namespace Waher.Things.Matter.Certificates
{
	/// <summary>
	/// Creates Matter operational certificates (RCAC, ICAC and NOC), as required by a fabric administrator
	/// (Matter Core Specification, 6.5).
	/// </summary>
	public static class CertificateFactory
	{
		/// <summary>
		/// Creates a random, positive serial number.
		/// </summary>
		/// <returns>Serial number.</returns>
		public static byte[] NewSerialNumber()
		{
			byte[] Result = MatterCrypto.RandomBytes(8);
			Result[0] &= 0x7f;

			if (Result[0] == 0)
				Result[0] = 1;

			return Result;
		}

		/// <summary>
		/// Creates a self-signed Root CA Certificate (RCAC).
		/// </summary>
		/// <param name="RootKey">Root key pair.</param>
		/// <param name="RcacId">RCAC identifier.</param>
		/// <param name="FabricId">Fabric ID to include in the subject, or null.</param>
		/// <param name="NotBefore">Start of validity.</param>
		/// <param name="NotAfter">End of validity, or null for no well-defined expiration.</param>
		/// <returns>RCAC.</returns>
		public static MatterCertificate CreateRcac(EcKeyPair RootKey, ulong RcacId, ulong? FabricId, DateTime NotBefore, DateTime? NotAfter = null)
		{
			DistinguishedName Subject = new DistinguishedName();
			Subject.Add(DistinguishedName.TagRcacId, RcacId);

			if (FabricId.HasValue)
				Subject.Add(DistinguishedName.TagFabricId, FabricId.Value);

			byte[] KeyId = RootKey.KeyIdentifier();

			MatterCertificate Result = new MatterCertificate()
			{
				SerialNumber = NewSerialNumber(),
				Issuer = Subject,
				Subject = Subject,
				NotBefore = MatterCertificate.ToMatterTime(NotBefore),
				NotAfter = NotAfter.HasValue ? MatterCertificate.ToMatterTime(NotAfter.Value) : 0,
				PublicKey = RootKey.PublicKey,
				HasBasicConstraints = true,
				IsCa = true,
				KeyUsage = KeyUsage.KeyCertSign | KeyUsage.CrlSign,
				SubjectKeyId = KeyId,
				AuthorityKeyId = KeyId
			};

			Result.Sign(RootKey);

			return Result;
		}

		/// <summary>
		/// Creates an Intermediate CA Certificate (ICAC), signed by a root.
		/// </summary>
		/// <param name="Rcac">Root certificate.</param>
		/// <param name="RootKey">Root key pair.</param>
		/// <param name="IcacPublicKey">Public key of the intermediate CA.</param>
		/// <param name="IcacId">ICAC identifier.</param>
		/// <param name="FabricId">Fabric ID to include in the subject, or null.</param>
		/// <param name="NotBefore">Start of validity.</param>
		/// <param name="NotAfter">End of validity, or null for no well-defined expiration.</param>
		/// <returns>ICAC.</returns>
		public static MatterCertificate CreateIcac(MatterCertificate Rcac, EcKeyPair RootKey, byte[] IcacPublicKey, ulong IcacId,
			ulong? FabricId, DateTime NotBefore, DateTime? NotAfter = null)
		{
			DistinguishedName Subject = new DistinguishedName();
			Subject.Add(DistinguishedName.TagIcacId, IcacId);

			if (FabricId.HasValue)
				Subject.Add(DistinguishedName.TagFabricId, FabricId.Value);

			MatterCertificate Result = new MatterCertificate()
			{
				SerialNumber = NewSerialNumber(),
				Issuer = Rcac.Subject,
				Subject = Subject,
				NotBefore = MatterCertificate.ToMatterTime(NotBefore),
				NotAfter = NotAfter.HasValue ? MatterCertificate.ToMatterTime(NotAfter.Value) : 0,
				PublicKey = IcacPublicKey,
				HasBasicConstraints = true,
				IsCa = true,
				PathLength = 0,
				KeyUsage = KeyUsage.KeyCertSign | KeyUsage.CrlSign,
				SubjectKeyId = EcKeyPair.ComputeKeyIdentifier(IcacPublicKey),
				AuthorityKeyId = Rcac.SubjectKeyId
			};

			Result.Sign(RootKey);

			return Result;
		}

		/// <summary>
		/// Creates a Node Operational Certificate (NOC).
		/// </summary>
		/// <param name="Issuer">Issuing certificate (RCAC or ICAC).</param>
		/// <param name="IssuerKey">Key pair of issuer.</param>
		/// <param name="NodePublicKey">Public key of node.</param>
		/// <param name="NodeId">Operational node ID.</param>
		/// <param name="FabricId">Fabric ID.</param>
		/// <param name="CaseAuthenticatedTags">Optional CASE Authenticated Tags (max 3).</param>
		/// <param name="NotBefore">Start of validity.</param>
		/// <param name="NotAfter">End of validity, or null for no well-defined expiration.</param>
		/// <returns>NOC.</returns>
		public static MatterCertificate CreateNoc(MatterCertificate Issuer, EcKeyPair IssuerKey, byte[] NodePublicKey, ulong NodeId,
			ulong FabricId, uint[] CaseAuthenticatedTags, DateTime NotBefore, DateTime? NotAfter = null)
		{
			if (!DistinguishedName.IsOperationalNodeId(NodeId))
				throw new ArgumentException("Invalid operational node ID.", nameof(NodeId));

			if (!(CaseAuthenticatedTags is null) && CaseAuthenticatedTags.Length > 3)
				throw new ArgumentException("At most 3 CASE Authenticated Tags allowed.", nameof(CaseAuthenticatedTags));

			DistinguishedName Subject = new DistinguishedName();
			Subject.Add(DistinguishedName.TagNodeId, NodeId);
			Subject.Add(DistinguishedName.TagFabricId, FabricId);

			if (!(CaseAuthenticatedTags is null))
			{
				foreach (uint Cat in CaseAuthenticatedTags)
					Subject.Add(DistinguishedName.TagNocCat, (ulong)Cat);
			}

			MatterCertificate Result = new MatterCertificate()
			{
				SerialNumber = NewSerialNumber(),
				Issuer = Issuer.Subject,
				Subject = Subject,
				NotBefore = MatterCertificate.ToMatterTime(NotBefore),
				NotAfter = NotAfter.HasValue ? MatterCertificate.ToMatterTime(NotAfter.Value) : 0,
				PublicKey = NodePublicKey,
				HasBasicConstraints = true,
				IsCa = false,
				KeyUsage = KeyUsage.DigitalSignature,
				ExtendedKeyUsage = new ExtendedKeyUsage[] { ExtendedKeyUsage.ClientAuth, ExtendedKeyUsage.ServerAuth },
				SubjectKeyId = EcKeyPair.ComputeKeyIdentifier(NodePublicKey),
				AuthorityKeyId = Issuer.SubjectKeyId
			};

			Result.Sign(IssuerKey);

			return Result;
		}

		/// <summary>
		/// Validates an operational certificate chain (NOC, optional ICAC) against a trusted root (RCAC).
		/// </summary>
		/// <param name="Noc">Node operational certificate.</param>
		/// <param name="Icac">Intermediate certificate, or null.</param>
		/// <param name="Rcac">Trusted root certificate.</param>
		/// <param name="ValidationTime">Time to validate against (null to skip validity period checks).</param>
		/// <exception cref="MatterSecurityException">If the chain is not valid.</exception>
		public static void ValidateChain(MatterCertificate Noc, MatterCertificate Icac, MatterCertificate Rcac, DateTime? ValidationTime)
		{
			if (Noc.IsCa)
				throw new MatterSecurityException("NOC must not be a CA certificate.");

			if (!Noc.Subject.NodeId.HasValue || !DistinguishedName.IsOperationalNodeId(Noc.Subject.NodeId.Value))
				throw new MatterSecurityException("NOC lacks a valid node ID.");

			if (!Noc.Subject.FabricId.HasValue)
				throw new MatterSecurityException("NOC lacks a fabric ID.");

			if (!Noc.KeyUsage.HasValue || (Noc.KeyUsage.Value & KeyUsage.DigitalSignature) == 0)
				throw new MatterSecurityException("NOC key usage must include digital signature.");

			MatterCertificate Issuer;

			if (!(Icac is null))
			{
				if (!Icac.IsCa)
					throw new MatterSecurityException("ICAC must be a CA certificate.");

				if (!Icac.Subject.Equals(Noc.Issuer))
					throw new MatterSecurityException("NOC not issued by ICAC.");

				if (!Noc.VerifySignature(Icac.PublicKey))
					throw new MatterSecurityException("Invalid NOC signature.");

				if (Icac.Subject.FabricId.HasValue && Icac.Subject.FabricId.Value != Noc.Subject.FabricId.Value)
					throw new MatterSecurityException("Fabric ID mismatch between NOC and ICAC.");

				Issuer = Icac;
			}
			else
				Issuer = Noc;

			if (!Rcac.IsCa)
				throw new MatterSecurityException("RCAC must be a CA certificate.");

			if (!Rcac.Subject.Equals(Issuer.Issuer))
				throw new MatterSecurityException("Certificate chain does not lead to the trusted root.");

			if (!Issuer.VerifySignature(Rcac.PublicKey))
				throw new MatterSecurityException("Invalid certificate signature.");

			if (Rcac.Subject.FabricId.HasValue && Rcac.Subject.FabricId.Value != Noc.Subject.FabricId.Value)
				throw new MatterSecurityException("Fabric ID mismatch between NOC and RCAC.");

			if (ValidationTime.HasValue)
			{
				if (Noc.NotAfter != 0 && MatterCertificate.ToMatterTime(ValidationTime.Value) > Noc.NotAfter)
					throw new MatterSecurityException("NOC has expired.");

				if (!(Icac is null) && Icac.NotAfter != 0 && MatterCertificate.ToMatterTime(ValidationTime.Value) > Icac.NotAfter)
					throw new MatterSecurityException("ICAC has expired.");
			}
		}
	}
}
