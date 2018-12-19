using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Waher.Security.PKCS.Passwords;

namespace Waher.Security.PKCS
{
	/// <summary>
	/// Encodes certificates and keys into PKCS#12 or PFX files.
	/// </summary>
	public class PfxEncoder
	{
		private const string bagTypes = "1.2.840.113549.1.12.10.1";
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		private LinkedList<DerEncoder> stack = null;
		private DerEncoder der = null;
		private byte[] macSalt = null;

		/// <summary>
		/// Encodes certificates and keys into PKCS#12 or PFX files.
		/// </summary>
		public PfxEncoder()
		{
		}

		/// <summary>
		/// Gets a number of random bytes.
		/// </summary>
		/// <param name="NrBytes">Number of bytes to generate.</param>
		public static byte[] GetRandomBytes(int NrBytes)
		{
			byte[] Result = new byte[NrBytes];

			lock (rnd)
			{
				rnd.GetBytes(Result);
			}

			return Result;
		}

		/// <summary>
		/// Begins PKCS#12 encoding.
		/// </summary>
		public void Begin()
		{
			this.der = new DerEncoder();
			this.macSalt = GetRandomBytes(8);

			this.der.StartSEQUENCE();   // PFX (RFC 7292, §4)
			this.der.INTEGER(3);        // version
		}

		private void Push()
		{
			if (this.stack is null)
				this.stack = new LinkedList<DerEncoder>();

			this.stack.AddLast(this.der);
			this.der = new DerEncoder();
		}

		private byte[] Pop()
		{
			if (this.stack is null || this.stack.Last is null)
				throw new InvalidOperationException("Stack empty.");

			byte[] Result = this.der.ToArray();

			this.der = this.stack.Last.Value;
			this.stack.RemoveLast();

			return Result;
		}

		/// <summary>
		/// Starts a block of safe content, in accordance with §5 of RFC 7292.
		/// </summary>
		public void StartSafeContent()
		{
			this.der.StartSEQUENCE();   // authSafe:ContentInfo, §4.1 RFC 7292
			this.der.OBJECT_IDENTIFIER("1.2.840.113549.1.7.1");     // PKCS#7 data
			this.der.StartContent(Asn1TypeClass.ContextSpecific);
		}

		/// <summary>
		/// Ends a block of safe content, in accordance with §5 of RFC 7292.
		/// </summary>
		public void EndSafeContent()
		{
			this.der.EndContent(Asn1TypeClass.ContextSpecific);    // End of PKCS#7 data
			this.der.EndSEQUENCE();   // End of authSafe:ContentInfo
		}

		/// <summary>
		/// Starts a block of encrypted safe content, in accordance with §5 of RFC 7292.
		/// </summary>
		public void StartEncryptedSafeContent()
		{
			this.der.StartSEQUENCE();   // EncryptedData, §13 RFC 2315 (PKCS#7)
			this.der.INTEGER(0);		// version

			this.der.OBJECT_IDENTIFIER("1.2.840.113549.1.7.1");     // PKCS#7 data
			this.der.StartContent(Asn1TypeClass.ContextSpecific);
		}

		/// <summary>
		/// Ends a block of encrypted safe content, in accordance with §5 of RFC 7292.
		/// </summary>
		public void EndEncryptedSafeContent()
		{
			this.der.EndContent(Asn1TypeClass.ContextSpecific);    // End of PKCS#7 data
			this.der.EndSEQUENCE();   // End of authSafe:ContentInfo
		}

		/// <summary>
		/// Ends PKCS#12 encoding and returns the encoded result.
		/// </summary>
		/// <returns>PKCS#12 encoded data.</returns>
		public byte[] End()
		{
			this.AssertBegun();

			if (this.stack != null && this.stack.First != null)
				throw new InvalidOperationException("Stack not empty.");

			this.der.StartSEQUENCE();   // macData:MacData
			this.der.StartSEQUENCE();   // mac:DigestInfo
										// TODO
			this.der.EndSEQUENCE();     // End of mac:DigestInfo

			this.der.OCTET_STRING(this.macSalt);    // macSalt
			this.der.INTEGER(2048);     // iterations
			this.der.EndSEQUENCE();     // End of macData:MacData
			this.der.EndSEQUENCE();

			byte[] Result = this.der.ToArray();
			this.der = null;

			return Result;
		}

		private void AssertBegun()
		{
			if (this.der is null)
				throw new InvalidOperationException("Encoding not begun.");
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

			this.der.StartSEQUENCE();   // SafeBag
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
			EncodePrivateKeyInfo(this.der, Algorithm);
			this.EndSafeBag();
		}

		private static void EncodePrivateKeyInfo(DerEncoder Der, SignatureAlgorithm Algorithm)
		{
			Der.StartSEQUENCE();                               // PrivateKeyInfo
			Der.INTEGER(0);                                    // version
			Der.OBJECT_IDENTIFIER(Algorithm.PkiAlgorithmOID);  // privateKeyAlgorithm
			Der.StartOCTET_STRING();
			Algorithm.ExportPrivateKey(Der);                   // privateKey
			Der.EndOCTET_STRING();
			Der.NULL();                                        // Attributes
			Der.EndSEQUENCE();                                 // End of PrivateKeyInfo
		}

		/// <summary>
		/// Encodes a ShroudedKeyBag (§4.2.2 RFC 7292, §6, RFC 5208)
		/// </summary>
		/// <param name="Password">Password used to protect the key.</param>
		/// <param name="Algorithm">Algorithm containing private key.</param>
		public void ShroudedKeyBag(string Password, SignatureAlgorithm Algorithm)
		{
			this.ShroudedKeyBag(new PbeWithShaAnd3KeyTripleDesCbc(Password, 4096), Algorithm);
		}

		/// <summary>
		/// Encodes a ShroudedKeyBag (§4.2.2 RFC 7292, §6, RFC 5208)
		/// </summary>
		/// <param name="Encryption">Encryption algorithm.</param>
		/// <param name="Algorithm">Algorithm containing private key.</param>
		public void ShroudedKeyBag(PasswordEncryption Encryption, SignatureAlgorithm Algorithm)
		{
			this.StartSafeBag(bagTypes + ".2");

			DerEncoder Key = new DerEncoder();
			EncodePrivateKeyInfo(Key, Algorithm);
			byte[] PrivateKey = Key.ToArray();

			this.der.StartSEQUENCE();                                   // EncryptedPrivateKeyInfo
			Encryption.EncodePkcs5AlgorithmIdentifier(this.der);
			this.der.OCTET_STRING(Encryption.Encrypt(PrivateKey));
			this.der.NULL();                                        // Attributes

			this.der.EndSEQUENCE();                                 // End of EncryptedPrivateKeyInfo

			this.EndSafeBag();  // TODO: attributes
		}

		/// <summary>
		/// Formats a password, in accordance with RFC 7292, §B.1.
		/// </summary>
		/// <param name="Password">Password</param>
		/// <returns>Formatted password</returns>
		internal static byte[] FormatPassword(string Password)
		{
			return Primitives.CONCAT(Encoding.BigEndianUnicode.GetBytes(Password), new byte[] { 0, 0 });
		}

		/// <summary>
		/// Generates a seed, in accordance with RFC 7292, §B.2
		/// </summary>
		/// <param name="H">Hash function</param>
		/// <param name="r">Iteration count</param>
		/// <param name="P">Formatted password.</param>
		/// <param name="S">Salt.</param>
		/// <param name="n">Number of pseudo-random bits to generate.</param>
		/// <param name="ID">Purpose of key:
		/// 
		/// If ID=1, then the pseudorandom bits being produced are to be used
		/// as key material for performing encryption or decryption.
		/// 
		/// 2.  If ID=2, then the pseudorandom bits being produced are to be used
		/// as an IV (Initial Value) for encryption or decryption.
		/// 
		/// 3.  If ID=3, then the pseudorandom bits being produced are to be used
		/// as an integrity key for MACing.
		/// </param>
		internal static byte[] PRF(HashFunction H, int r, byte[] P, byte[] S, int n, byte ID)
		{
			int u, v;

			if ((n & 7) != 0)
				throw new ArgumentException("Must be a factor of 8.", nameof(n));

			switch (H)
			{
				case HashFunction.MD5:
					u = 128;
					v = 512;
					break;

				case HashFunction.SHA1:
					u = 160;
					v = 512;
					break;

				case HashFunction.SHA256:
					u = 256;
					v = 512;
					break;

				case HashFunction.SHA384:
					u = 384;
					v = 1024;
					break;

				case HashFunction.SHA512:
					u = 512;
					v = 1024;
					break;

				default:
					throw new ArgumentException("Hash function not supported.", nameof(H));
			}

			int v8 = v / 8;
			int u8 = u / 8;

			byte[] D = new byte[v8];
			int i, j, c;

			for (i = 0; i < v8; i++)
				D[i] = ID;

			S = Extend(S, v);
			P = Extend(P, v);

			byte[] I = Primitives.CONCAT(S, P);
			int i8 = I.Length;

			c = (n + u - 1) / u;

			byte[][] As = new byte[c][];

			for (i = 0; i < c; i++)
			{
				As[i] = Primitives.CONCAT(D, I);

				for (j = 0; j < r; j++)
					As[i] = Hashes.ComputeHash(H, As[i]);

				byte[] B = Extend(As[i], v);

				for (j = 0; j < i8; j += v8)
					AddTo(I, j, B, true);
			}

			byte[] A = Primitives.CONCAT(As);

			if (A.Length != n)
				Array.Resize<byte>(ref A, n);

			return A;
		}

		private static void AddTo(byte[] Dest, int Offset, byte[] Term, bool Carry)
		{
			int i = Term.Length;
			bool Carry2;
			byte b, b2;

			Offset += i;

			while (--i >= 0)
			{
				b = Dest[--Offset];
				b2 = b = Term[i];
				b += b2;
				Carry2 = b < b2;
				if (Carry)
				{
					b++;
					Carry2 |= b == 0;
				}
				Dest[Offset] = b;
				Carry = Carry2;
			}
		}

		private static byte[] Extend(byte[] Bin, int v)
		{
			int Len = Bin.Length;
			int c = (v / 8) * ((Len + v - 1) / v);
			byte[] Result = new byte[c];
			int i, j;

			for (i = 0; i < c; i += Len)
			{
				j = c - i;
				if (j < Len)
					Array.Copy(Bin, 0, Result, i, j);
				else
					Array.Copy(Bin, 0, Result, i, Len);
			}

			return Result;
		}

		/// <summary>
		/// Encodes a CertBag (§4.2.3 RFC 7292, §6, RFC 5208)
		/// </summary>
		/// <param name="Certificate">Certificate</param>
		public void CertificateBag(X509Certificate2 Certificate)
		{
			this.StartSafeBag(bagTypes + ".3");
			this.der.StartSEQUENCE();                               // CertBag
			this.der.OBJECT_IDENTIFIER("1.2.840.113549.1.9.22.1");  // x509Certificate
			this.der.OCTET_STRING(Certificate.Export(X509ContentType.Pkcs7));
			this.der.EndSEQUENCE();                                 // End of CertBag
			this.EndSafeBag();
		}

		/*/// <summary>
		/// Encodes a Certificate Revocation List (CRL) (§4.2.4 RFC 7292, §6, RFC 5208)
		/// </summary>
		public void CrlBag()
		{
			this.StartSafeBag(bagTypes + ".4");
		}*/

		/*public void SecretBag()
		{
			this.StartSafeBag(bagTypes + ".5");
		}*/

		/*public void StartContentsBag()
		{
			this.StartSafeBag(bagTypes + ".6");
		}

		public void EndContentsBag()
		{
			this.EndSafeBag();
		}*/

	}
}
