using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Waher.Security.PKCS
{
	/// <summary>
	/// Encodes certificates and keys into PKCS#12 or PFX files.
	/// </summary>
	public class PfxEncoder
    {
		private const string bagTypes = "1.2.840.113549.1.12.10.1";

		private DerEncoder der = null;
		private byte[] macSalt = null;

		/// <summary>
		/// Encodes certificates and keys into PKCS#12 or PFX files.
		/// </summary>
		public PfxEncoder()
		{
		}

		/// <summary>
		/// Begins PKCS#12 encoding.
		/// </summary>
		public void Begin()
		{
			this.der = new DerEncoder();

			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				this.macSalt = new byte[8];
				Rnd.GetBytes(this.macSalt);
			}

			this.der.StartSEQUENCE();	// PFX (RFC 7292, §4)
			this.der.INTEGER(3);		// version

			this.der.StartSEQUENCE();   // authSafe:ContentInfo, §4.1 RFC 7292
			this.der.OBJECT_IDENTIFIER("1.2.840.113549.1.7.1");     // PKCS#7 data
			this.der.StartContent(Asn1TypeClass.ContextSpecific);



			this.der.EndContent(Asn1TypeClass.ContextSpecific);    // End of PKCS#7 data
			this.der.EndSEQUENCE();   // End of authSafe:ContentInfo

			this.der.StartSEQUENCE();   // macData:MacData
			this.der.StartSEQUENCE();   // mac:DigestInfo
			// TODO
			this.der.EndSEQUENCE();     // End of mac:DigestInfo

			this.der.OCTET_STRING(this.macSalt);	// macSalt
			this.der.INTEGER(2048);		// iterations
			this.der.EndSEQUENCE();     // End of macData:MacData
		}

		private void AssertBegun()
		{
			if (this.der == null)
				throw new InvalidOperationException("Encoding not begun.");
		}

		/// <summary>
		/// Ends PKCS#12 encoding and returns the encoded result.
		/// </summary>
		/// <returns>PKCS#12 encoded data.</returns>
		public byte[] End()
		{
			this.AssertBegun();

			this.der.EndSEQUENCE();

			byte[] Result = this.der.ToArray();
			this.der = null;

			return Result;
		}

		/// <summary>
		/// Starts a PKCS12BagSet
		/// </summary>
		public void StartBagSet()
		{
			this.AssertBegun();
			this.der.StartSEQUENCE();
		}

		/// <summary>
		/// Ends a PKCS12BagSet
		/// </summary>
		public void EndBagSet()
		{
			this.AssertBegun();
			this.der.EndSEQUENCE();
		}

		private void StartSafeBag(string OID)
		{
			this.AssertBegun();

			this.der.StartSEQUENCE();	// SafeBag
			this.der.OBJECT_IDENTIFIER(OID);
			this.der.StartContent(Asn1TypeClass.ContextSpecific);
		}

		private void EndSafeBag()
		{
			this.der.EndContent(Asn1TypeClass.ContextSpecific);
			// TODO: Attributes
			this.der.EndSEQUENCE();
		}

		/// <summary>
		/// Encodes a KeyBag (§4.2.1 RFC 7292, §5, RFC 5208)
		/// </summary>
		/// <param name="Algorithm">Algorithm containing private key.</param>
		public void KeyBag(SignatureAlgorithm Algorithm)
		{
			this.StartSafeBag(bagTypes + ".1");

			this.der.StartSEQUENCE();								// PrivateKeyInfo
			this.der.INTEGER(0);									// version
			this.der.OBJECT_IDENTIFIER(Algorithm.PkiAlgorithmOID);  // privateKeyAlgorithm
			this.der.StartOCTET_STRING();
			Algorithm.ExportPrivateKey(this.der);                   // privateKey
			this.der.EndOCTET_STRING();
			this.der.NULL();										// Attributes
			this.der.EndSEQUENCE();									// End of PrivateKeyInfo

			this.EndSafeBag();
		}

		public void StartShroudedKeyBag()
		{
			this.StartSafeBag(bagTypes + ".2");
		}

		public void EndShroudedKeyBag()
		{
			this.EndSafeBag();
		}

		public void StartCertificateBag()
		{
			this.StartSafeBag(bagTypes + ".3");
		}

		public void EndCertificateBag()
		{
			this.EndSafeBag();
		}

		public void StartCrlBag()
		{
			this.StartSafeBag(bagTypes + ".4");
		}

		public void EndCrlBag()
		{
			this.EndSafeBag();
		}

		public void StartSecretBag()
		{
			this.StartSafeBag(bagTypes + ".5");
		}

		public void EndSecretBag()
		{
			this.EndSafeBag();
		}

		public void StartContentsBag()
		{
			this.StartSafeBag(bagTypes + ".6");
		}

		public void EndContentsBag()
		{
			this.EndSafeBag();
		}

	}
}
