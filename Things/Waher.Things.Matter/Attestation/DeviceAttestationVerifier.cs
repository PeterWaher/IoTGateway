using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things.Matter.Certificates;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Attestation
{
	/// <summary>
	/// How device attestation failures are handled during commissioning.
	/// </summary>
	public enum AttestationPolicy
	{
		/// <summary>
		/// Commissioning is aborted if device attestation fails (recommended).
		/// </summary>
		Strict,

		/// <summary>
		/// Device attestation failures are logged, but commissioning continues.
		/// </summary>
		Warn,

		/// <summary>
		/// Device attestation is not performed.
		/// </summary>
		Ignore
	}

	/// <summary>
	/// Result of a device attestation verification.
	/// </summary>
	public class AttestationResult
	{
		private readonly List<string> errors = new List<string>();
		private readonly List<string> warnings = new List<string>();

		/// <summary>
		/// Result of a device attestation verification.
		/// </summary>
		public AttestationResult()
		{
		}

		/// <summary>
		/// If attestation succeeded (no errors).
		/// </summary>
		public bool Success => this.errors.Count == 0;

		/// <summary>
		/// Errors found.
		/// </summary>
		public IReadOnlyList<string> Errors => this.errors;

		/// <summary>
		/// Warnings found.
		/// </summary>
		public IReadOnlyList<string> Warnings => this.warnings;

		/// <summary>
		/// Device Attestation Certificate.
		/// </summary>
		public X509CertificateInfo Dac { get; internal set; }

		/// <summary>
		/// Product Attestation Intermediate certificate.
		/// </summary>
		public X509CertificateInfo Pai { get; internal set; }

		/// <summary>
		/// Product Attestation Authority certificate (from the trust store), if found.
		/// </summary>
		public X509CertificateInfo Paa { get; internal set; }

		/// <summary>
		/// Certification declaration, if valid.
		/// </summary>
		public CertificationDeclaration CertificationDeclaration { get; internal set; }

		/// <summary>
		/// Vendor ID of the DAC.
		/// </summary>
		public ushort? VendorId => this.Dac?.VendorId;

		/// <summary>
		/// Product ID of the DAC.
		/// </summary>
		public ushort? ProductId => this.Dac?.ProductId;

		internal void Error(string Message) => this.errors.Add(Message);

		internal void Warning(string Message) => this.warnings.Add(Message);

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (this.Success)
				sb.Append("Device attestation succeeded.");
			else
			{
				sb.Append("Device attestation failed: ");
				sb.Append(string.Join(" ", this.errors));
			}

			if (this.warnings.Count > 0)
			{
				sb.Append(" Warnings: ");
				sb.Append(string.Join(" ", this.warnings));
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// Performs the device attestation procedure (Matter Core Specification, 6.2.3).
	/// </summary>
	public class DeviceAttestationVerifier
	{
		private readonly AttestationTrustStore trustStore;
		private readonly bool allowTestCertificationDeclarations;

		/// <summary>
		/// Performs the device attestation procedure.
		/// </summary>
		/// <param name="TrustStore">PAA trust store.</param>
		/// <param name="AllowTestCertificationDeclarations">If certification declarations signed with the publicly known
		/// test key are accepted (development and testing only).</param>
		public DeviceAttestationVerifier(AttestationTrustStore TrustStore, bool AllowTestCertificationDeclarations)
		{
			this.trustStore = TrustStore;
			this.allowTestCertificationDeclarations = AllowTestCertificationDeclarations;
		}

		/// <summary>
		/// PAA trust store.
		/// </summary>
		public AttestationTrustStore TrustStore => this.trustStore;

		/// <summary>
		/// Verifies device attestation information.
		/// </summary>
		/// <param name="DacDer">Device Attestation Certificate (DER).</param>
		/// <param name="PaiDer">Product Attestation Intermediate certificate (DER).</param>
		/// <param name="AttestationElements">Attestation elements (TLV).</param>
		/// <param name="AttestationSignature">Attestation signature (raw).</param>
		/// <param name="AttestationNonce">Nonce sent in the attestation request.</param>
		/// <param name="AttestationChallenge">Attestation challenge of the PASE session.</param>
		/// <param name="BasicVendorId">Vendor ID reported by the Basic Information cluster, if known.</param>
		/// <param name="BasicProductId">Product ID reported by the Basic Information cluster, if known.</param>
		/// <param name="Now">Current time, or null if unknown (validity periods are then not checked).</param>
		/// <returns>Attestation result.</returns>
		public AttestationResult Verify(byte[] DacDer, byte[] PaiDer, byte[] AttestationElements, byte[] AttestationSignature,
			byte[] AttestationNonce, byte[] AttestationChallenge, ushort? BasicVendorId, ushort? BasicProductId, DateTime? Now)
		{
			AttestationResult Result = new AttestationResult();

			try
			{
				Result.Dac = X509CertificateInfo.Parse(DacDer);
			}
			catch (Exception ex)
			{
				Result.Error("Invalid DAC: " + ex.Message);
				return Result;
			}

			try
			{
				Result.Pai = X509CertificateInfo.Parse(PaiDer);
			}
			catch (Exception ex)
			{
				Result.Error("Invalid PAI: " + ex.Message);
				return Result;
			}

			X509CertificateInfo Dac = Result.Dac;
			X509CertificateInfo Pai = Result.Pai;

			// Attestation signature

			if (!Dac.IsP256)
			{
				Result.Error("DAC does not contain a P-256 public key.");
				return Result;
			}

			if (!EcKeyPair.Verify(Dac.PublicKey, MatterCrypto.Concat(AttestationElements, AttestationChallenge), AttestationSignature))
				Result.Error("Invalid attestation signature.");

			// Attestation elements

			TlvElement Elements;

			try
			{
				Elements = TlvElement.Decode(AttestationElements);
			}
			catch (Exception ex)
			{
				Result.Error("Invalid attestation elements: " + ex.Message);
				return Result;
			}

			byte[] Nonce = Elements[2]?.AsBytes();
			if (!MatterCrypto.AreEqual(Nonce, AttestationNonce))
				Result.Error("Attestation nonce mismatch.");

			// Certificate chain

			this.VerifyChain(Result, Now);

			// Certification declaration

			byte[] Cd = Elements[1]?.AsBytes();

			if (Cd is null)
				Result.Error("Certification declaration missing.");
			else
			{
				try
				{
					CertificationDeclaration Declaration = CertificationDeclaration.ParseAndVerify(Cd, this.allowTestCertificationDeclarations);
					Result.CertificationDeclaration = Declaration;

					if (Declaration.SignedWithTestKey)
						Result.Warning("Certification declaration signed with the public test key.");

					if (Declaration.CertificationType == CertificationType.DevelopmentAndTest)
						Result.Warning("Device is certified for development and test only.");

					this.VerifyDeclaration(Result, Declaration, BasicVendorId, BasicProductId);
				}
				catch (Exception ex)
				{
					Result.Error("Invalid certification declaration: " + ex.Message);
				}
			}

			return Result;
		}

		private void VerifyChain(AttestationResult Result, DateTime? Now)
		{
			X509CertificateInfo Dac = Result.Dac;
			X509CertificateInfo Pai = Result.Pai;

			if (Dac.Version != 3 || Pai.Version != 3)
				Result.Error("Attestation certificates must be X.509 v3 certificates.");

			if (!Dac.HasBasicConstraints || Dac.IsCa)
				Result.Error("DAC must not be a CA certificate.");

			if (!Dac.KeyUsage.HasValue || (Dac.KeyUsage.Value & KeyUsage.DigitalSignature) == 0)
				Result.Error("DAC key usage must include digital signature.");

			if (!Pai.HasBasicConstraints || !Pai.IsCa)
				Result.Error("PAI must be a CA certificate.");

			if (Pai.PathLength.HasValue && Pai.PathLength.Value != 0)
				Result.Error("PAI path length must be 0.");

			if (!Pai.KeyUsage.HasValue || (Pai.KeyUsage.Value & KeyUsage.KeyCertSign) == 0)
				Result.Error("PAI key usage must include certificate signing.");

			if (!Dac.VendorId.HasValue || !Dac.ProductId.HasValue)
				Result.Error("DAC must contain Vendor ID and Product ID.");

			if (!Pai.VendorId.HasValue)
				Result.Error("PAI must contain a Vendor ID.");
			else if (Dac.VendorId.HasValue && Pai.VendorId.Value != Dac.VendorId.Value)
				Result.Error("Vendor ID mismatch between DAC and PAI.");

			if (Pai.ProductId.HasValue && Dac.ProductId.HasValue && Pai.ProductId.Value != Dac.ProductId.Value)
				Result.Error("Product ID mismatch between DAC and PAI.");

			if (!MatterCrypto.AreEqual(Dac.IssuerDer, Pai.SubjectDer))
				Result.Error("DAC not issued by PAI.");

			if (!Pai.IsP256 || !Dac.VerifySignature(Pai.PublicKey))
				Result.Error("Invalid DAC signature.");

			X509CertificateInfo Paa = this.trustStore.FindBySubjectKeyId(Pai.AuthorityKeyId);

			if (Paa is null)
				Result.Error("PAI not issued by a trusted PAA.");
			else
			{
				Result.Paa = Paa;

				if (!MatterCrypto.AreEqual(Pai.IssuerDer, Paa.SubjectDer))
					Result.Error("PAI issuer does not match PAA subject.");

				if (!Pai.VerifySignature(Paa.PublicKey))
					Result.Error("Invalid PAI signature.");

				if (Paa.VendorId.HasValue && Dac.VendorId.HasValue && Paa.VendorId.Value != Dac.VendorId.Value)
					Result.Error("Vendor ID mismatch between PAA and DAC.");
			}

			if (Now.HasValue)
			{
				if (!Dac.IsValidAt(Now.Value))
					Result.Error("DAC not valid at current time.");

				if (!Pai.IsValidAt(Now.Value))
					Result.Error("PAI not valid at current time.");

				if (!(Paa is null) && !Paa.IsValidAt(Now.Value))
					Result.Error("PAA not valid at current time.");
			}
		}

		private void VerifyDeclaration(AttestationResult Result, CertificationDeclaration Declaration, ushort? BasicVendorId, ushort? BasicProductId)
		{
			X509CertificateInfo Dac = Result.Dac;

			if (Declaration.DacOriginVendorId.HasValue || Declaration.DacOriginProductId.HasValue)
			{
				if (!Declaration.DacOriginVendorId.HasValue || !Declaration.DacOriginProductId.HasValue)
					Result.Error("Incomplete DAC origin in certification declaration.");
				else
				{
					if (Dac.VendorId.HasValue && Declaration.DacOriginVendorId.Value != Dac.VendorId.Value)
						Result.Error("DAC origin vendor ID mismatch.");

					if (Dac.ProductId.HasValue && Declaration.DacOriginProductId.Value != Dac.ProductId.Value)
						Result.Error("DAC origin product ID mismatch.");
				}
			}
			else
			{
				if (Dac.VendorId.HasValue && Declaration.VendorId != Dac.VendorId.Value)
					Result.Error("Vendor ID mismatch between certification declaration and DAC.");

				if (Dac.ProductId.HasValue && !Declaration.IncludesProduct(Dac.ProductId.Value))
					Result.Error("Product ID of DAC not included in certification declaration.");
			}

			if (BasicVendorId.HasValue && BasicVendorId.Value != Declaration.VendorId)
				Result.Error("Vendor ID of device does not match certification declaration.");

			if (BasicProductId.HasValue && !Declaration.IncludesProduct(BasicProductId.Value))
				Result.Error("Product ID of device not included in certification declaration.");

			if (!(Declaration.AuthorizedPaaList is null) && Declaration.AuthorizedPaaList.Length > 0)
			{
				bool Found = false;

				if (!(Result.Paa?.SubjectKeyId is null))
				{
					foreach (byte[] Kid in Declaration.AuthorizedPaaList)
					{
						if (MatterCrypto.AreEqual(Kid, Result.Paa.SubjectKeyId))
						{
							Found = true;
							break;
						}
					}
				}

				if (!Found)
					Result.Error("PAA not authorized by certification declaration.");
			}
		}
	}
}
