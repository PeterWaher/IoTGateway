using Waher.Things.Matter.Asn1;
using Waher.Things.Matter.Crypto;

namespace Waher.Things.Matter.Certificates
{
	/// <summary>
	/// Parsed PKCS#10 certificate signing request, as returned by the CSRRequest command.
	/// </summary>
	public class CertificateSigningRequest
	{
		private CertificateSigningRequest()
		{
		}

		/// <summary>
		/// DER encoding of the CertificationRequestInfo.
		/// </summary>
		public byte[] InfoDer { get; private set; }

		/// <summary>
		/// Public key (uncompressed P-256 point).
		/// </summary>
		public byte[] PublicKey { get; private set; }

		/// <summary>
		/// Signature algorithm OID.
		/// </summary>
		public string SignatureAlgorithm { get; private set; }

		/// <summary>
		/// Signature (raw r || s).
		/// </summary>
		public byte[] Signature { get; private set; }

		/// <summary>
		/// Parses a DER-encoded PKCS#10 certificate signing request.
		/// </summary>
		/// <param name="Der">DER encoding.</param>
		/// <returns>Parsed CSR.</returns>
		public static CertificateSigningRequest Parse(byte[] Der)
		{
			CertificateSigningRequest Result = new CertificateSigningRequest();
			DerReader Reader = new DerReader(Der).ReadSequence();

			Result.InfoDer = Reader.ReadEncodedElement();

			DerReader Alg = Reader.ReadSequence();
			Result.SignatureAlgorithm = Alg.ReadOid();

			byte[] Sig = Reader.ReadBitString(out _);
			Result.Signature = Result.SignatureAlgorithm == Oids.EcdsaWithSha256 ? EcKeyPair.DerToRawSignature(Sig) : Sig;

			DerReader Info = new DerReader(Result.InfoDer).ReadSequence();
			long Version = Info.ReadInteger();

			if (Version != 0)
				throw new MatterDecodingException("Unsupported CSR version.");

			Info.Skip();    // Subject

			DerReader Spki = Info.ReadSequence();
			DerReader KeyAlg = Spki.ReadSequence();
			string KeyAlgorithm = KeyAlg.ReadOid();
			string Curve = KeyAlg.HasMore ? KeyAlg.ReadOid() : null;

			if (KeyAlgorithm != Oids.EcPublicKey || Curve != Oids.Prime256v1)
				throw new MatterSecurityException("CSR does not contain a P-256 public key.");

			Result.PublicKey = Spki.ReadBitString(out _);

			if (Result.PublicKey.Length != 65 || Result.PublicKey[0] != 4)
				throw new MatterSecurityException("Invalid public key in CSR.");

			return Result;
		}

		/// <summary>
		/// Verifies the self-signature of the CSR.
		/// </summary>
		/// <returns>If the signature is valid.</returns>
		public bool VerifySignature()
		{
			if (this.SignatureAlgorithm != Oids.EcdsaWithSha256)
				return false;

			return EcKeyPair.Verify(this.PublicKey, this.InfoDer, this.Signature);
		}
	}
}
