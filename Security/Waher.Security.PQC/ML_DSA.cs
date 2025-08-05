using System;
using Waher.Security.SHA3;

namespace Waher.Security.PQC
{
	/// <summary>
	/// Implements the ML-DSA algorithm for post-quantum cryptography, as defined in
	/// NIST FIPS 204: https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.204.pdf
	/// </summary>
	public class ML_DSA : ML_Common
	{
		/// <summary>
		/// Model parameters for a required RBG strength 128 (cryptographic security strength),
		/// as defined in §4.
		/// </summary>
		public static readonly ML_DSA ML_DSA_44 = new ML_DSA(39, 128, 1 << 17, (q - 1) / 88, 4, 4, 2, 80, 2560, 1312, 2420);

		/// <summary>
		/// Model parameters for a required RBG strength 192 (cryptographic security strength),
		/// as defined in §4.
		/// </summary>
		public static readonly ML_DSA ML_DSA_65 = new ML_DSA(49, 192, 1 << 19, (q - 1) / 32, 6, 5, 4, 55, 4032, 1952, 3309);

		/// <summary>
		/// Model parameters for a required RBG strength 256 (cryptographic security strength),
		/// as defined in §4.
		/// </summary>
		public static readonly ML_DSA ML_DSA_87 = new ML_DSA(60, 256, 1 << 19, (q - 1) / 32, 8, 7, 2, 75, 4896, 2592, 4627);

		/// <summary>
		/// Gets a model by name, as defined in §8.
		/// </summary>
		/// <param name="Name">Name of model.</param>
		/// <returns>Reference to model.</returns>
		/// <exception cref="ArgumentException">Model name not recognized.</exception>
		public static ML_DSA GetModel(string Name)
		{
			switch (Name.ToUpper())
			{
				case "ML-DSA-44":
					return ML_DSA_44;

				case "ML-DSA-65":
					return ML_DSA_65;

				case "ML-DSA-87":
					return ML_DSA_87;

				default:
					throw new ArgumentException("Unknown model name: " + Name, nameof(Name));
			}
		}

		private const int n = 256;
		private const int q = 8380417;
		private const int ζ = 1753;                     // 512th root of unity in ℤ𝑞
		private const int d = 13;                       // #dropped bits from t
		private const uint dBitMask = 0x1fff;           // & dBitMask corresponds to mod 2^d.
		private const short twoDHalf = 1 << (d - 1);    // 2^(d-1)
		private const short twoD = 1 << d;              // 2^d

		private readonly int τ;                         // # of ±1’s in polynomial c
		private readonly int λ;                         // collision strength of c
		private readonly uint γ1;                       // coefficient range of y 
		private readonly uint γ2;                       // low-order rounding range 
		private readonly byte k;                        // Rows of matrix A
		private readonly byte l;                        // Columns of matrix A
		private readonly byte η;                        // private key range
		private readonly int β;                         // τ*η
		private readonly int ω;                         // max # of 1’s in the hint h

		private readonly int twoγ2;                     // 2*γ2

		private readonly int privateKeySize;
		private readonly int publicKeySize;
		private readonly int signatureSize;

		/// <summary>
		/// Implements the ML-DSA algorithm for post-quantum cryptography, as defined in
		/// NIST FIPS 204: https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.204.pdf
		/// </summary>
		/// <param name="τ"># of ±1’s in polynomial c</param>
		/// <param name="λ">collision strength of c</param>
		/// <param name="γ1">coefficient range of y</param>
		/// <param name="γ2">low-order rounding range</param>
		/// <param name="k">Rows of matrix A</param>
		/// <param name="l">Columns of matrix A</param>
		/// <param name="η">private key range</param>
		/// <param name="ω">max # of 1’s in the hint h</param>
		/// <param name="PrivateKeySize">Size of private key in bytes</param>
		/// <param name="PublicKeySize">Size of public key in bytes</param>
		/// <param name="SignatureSize">Size of signature in bytes</param>
		public ML_DSA(int τ, int λ, uint γ1, uint γ2, byte k, byte l, byte η, int ω,
			int PrivateKeySize, int PublicKeySize, int SignatureSize)
		{
			if (η != 2 && η != 4)
				throw new ArgumentException("η must be 2 or 4.", nameof(η));

			this.τ = τ;
			this.λ = λ;
			this.γ1 = γ1;
			this.γ2 = γ2;
			this.k = k;
			this.l = l;
			this.η = η;
			this.β = τ * η;
			this.ω = ω;

			this.twoγ2 = (int)(this.γ2 << 1);

			this.privateKeySize = PrivateKeySize;
			this.publicKeySize = PublicKeySize;
			this.signatureSize = SignatureSize;
		}

		/// <summary>
		/// Generates a public and private key. 
		/// (Algorithm 1 ML-DSA.KeyGen() in §5.1)
		/// </summary>
		/// <returns>Public Key pk (320k+32 bytes) and Private Key sk 
		/// (160*((l+k)*bitlen(2η)+dk) bytes).</returns>
		public ML_DSA_Keys KeyGen()
		{
			byte[] ξ = CreateSeed();

			ML_DSA_Keys Result = this.KeyGen_Internal(ξ);

			Clear(ξ);

			return Result;
		}

		/// <summary>
		/// Signs a message using the ML-DSA algorithm.
		/// (Algorithm 2 ML-DSA.Sign() in §5.2)
		/// </summary>
		/// <param name="PrivateKey">Private key</param>
		/// <param name="Message">Message to sign</param>
		/// <returns>Digital signature.</returns>
		public byte[] Sign(byte[] PrivateKey, byte[] Message)
		{
			return this.Sign(PrivateKey, Message, (byte[])null);
		}

		/// <summary>
		/// Signs a message using the ML-DSA algorithm.
		/// (Algorithm 2 ML-DSA.Sign() in §5.2)
		/// </summary>
		/// <param name="PrivateKey">Private key</param>
		/// <param name="Message">Message to sign</param>
		/// <param name="Context">Context, optionally empty.</param>
		/// <returns>Digital signature.</returns>
		public byte[] Sign(byte[] PrivateKey, byte[] Message, byte[] Context)
		{
			byte[] ξ = CreateSeed();
			byte[] Result = this.Sign(PrivateKey, Message, Context, ξ);
			Clear(ξ);

			return Result;
		}

		/// <summary>
		/// Signs a message using the ML-DSA algorithm.
		/// (Algorithm 2 ML-DSA.Sign() in §5.2)
		/// </summary>
		/// <param name="PrivateKey">Private key</param>
		/// <param name="Message">Message to sign</param>
		/// <param name="Context">Context, optionally empty.</param>
		/// <param name="Seed">32-byte randomness.</param>
		/// <returns>Digital signature.</returns>
		public byte[] Sign(byte[] PrivateKey, byte[] Message, byte[] Context, byte[] Seed)
		{
			int MessageLen = Message.Length;
			int ContextLen;

			if (Context is null)
			{
				Context = Array.Empty<byte>();
				ContextLen = 0;
			}
			else if ((ContextLen = Context.Length) > 255)
				throw new ArgumentException("Context must be 255 bytes or less.", nameof(Context));

			if (Seed is null)
				Seed = new byte[32];
			else if (Seed.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(Seed));

			byte[] Bin = new byte[2 + ContextLen + MessageLen];
			Bin[0] = 0;
			Bin[1] = (byte)ContextLen;

			if (ContextLen > 0)
				Array.Copy(Context, 0, Bin, 2, ContextLen);

			Array.Copy(Message, 0, Bin, 2 + ContextLen, MessageLen);

			return this.Sign_Internal(PrivateKey, Bin, Seed);
		}

		/// <summary>
		/// Verifies a digital signature using the ML-DSA algorithm.
		/// (Algorithm 3 ML-DSA.Verify() in §5.3)
		/// </summary>
		/// <param name="PublicKey">Public Key</param>
		/// <param name="Message">Message</param>
		/// <param name="Signature">Signature</param>
		/// <param name="Context">Context</param>
		/// <returns>If the signature is valid.</returns>
		public bool Verify(byte[] PublicKey, byte[] Message, byte[] Signature, byte[] Context)
		{
			int MessageLen = Message.Length;
			int ContextLen;

			if (Context is null)
			{
				Context = Array.Empty<byte>();
				ContextLen = 0;
			}
			else if ((ContextLen = Context.Length) > 255)
				return false;

			byte[] Bin = new byte[2 + ContextLen + MessageLen];
			Bin[0] = 0;
			Bin[1] = (byte)ContextLen;

			if (ContextLen > 0)
				Array.Copy(Context, 0, Bin, 2, ContextLen);

			Array.Copy(Message, 0, Bin, 2 + ContextLen, MessageLen);

			return this.Verify_Internal(PublicKey, Bin, Signature);
		}

		/// <summary>
		/// Signs a message using the ML-DSA pre-hash algorithm.
		/// (Algorithm 4 HashML-DSA.Sign() in §5.4.1)
		/// </summary>
		/// <param name="PrivateKey">Private key</param>
		/// <param name="Message">Message to sign</param>
		/// <param name="HashAlgorithm">Name of hash algorithm.</param>
		/// <returns>Digital signature.</returns>
		public byte[] Sign(byte[] PrivateKey, byte[] Message, string HashAlgorithm)
		{
			return this.Sign(PrivateKey, Message, null, HashAlgorithm);
		}

		/// <summary>
		/// Signs a message using the ML-DSA pre-hash algorithm.
		/// (Algorithm 4 HashML-DSA.Sign() in §5.4.1)
		/// </summary>
		/// <param name="PrivateKey">Private key</param>
		/// <param name="Message">Message to sign</param>
		/// <param name="Context">Context, optionally empty.</param>
		/// <param name="HashAlgorithm">Name of hash algorithm.</param>
		/// <returns>Digital signature.</returns>
		public byte[] Sign(byte[] PrivateKey, byte[] Message, byte[] Context, string HashAlgorithm)
		{
			byte[] ξ = CreateSeed();
			byte[] Result = this.Sign(PrivateKey, Message, Context, ξ, HashAlgorithm);
			Clear(ξ);

			return Result;
		}

		/// <summary>
		/// Signs a message using the ML-DSA pre-hash algorithm.
		/// (Algorithm 4 HashML-DSA.Sign() in §5.4.1)
		/// </summary>
		/// <param name="PrivateKey">Private key</param>
		/// <param name="Message">Message to sign</param>
		/// <param name="Context">Context, optionally empty.</param>
		/// <param name="Seed">32-byte randomness.</param>
		/// <param name="HashAlgorithm">Name of hash algorithm.</param>
		/// <returns>Digital signature.</returns>
		public byte[] Sign(byte[] PrivateKey, byte[] Message, byte[] Context, byte[] Seed,
			string HashAlgorithm)
		{
			int ContextLen;

			if (Context is null)
			{
				Context = Array.Empty<byte>();
				ContextLen = 0;
			}
			else if ((ContextLen = Context.Length) > 255)
				throw new ArgumentException("Context must be 255 bytes or less.", nameof(Context));

			if (Seed is null)
				Seed = new byte[32];
			else if (Seed.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(Seed));

			if (!TryGetHashFunction(HashAlgorithm, out HashFunctionArray H, out byte[] Oid))
				throw new ArgumentException("Unsupported hash algorithm: " + HashAlgorithm, nameof(HashAlgorithm));

			Message = H(Message);

			int MessageLen = Message.Length;
			int OidLen = Oid.Length;
			int Pos;

			byte[] Bin = new byte[2 + ContextLen + OidLen + MessageLen];
			Bin[0] = 1;
			Bin[1] = (byte)ContextLen;

			if (ContextLen > 0)
				Array.Copy(Context, 0, Bin, 2, ContextLen);

			Array.Copy(Oid, 0, Bin, Pos = 2 + ContextLen, OidLen);
			Pos += OidLen;

			Array.Copy(Message, 0, Bin, Pos, MessageLen);

			return this.Sign_Internal(PrivateKey, Bin, Seed);
		}

		private static bool TryGetHashFunction(string Name, out HashFunctionArray H, out byte[] Oid)
		{
			switch (Name.ToUpper())
			{
				case "SHA256":
				case "SHA-256":
					Oid = oidSha256;
					H = Hashes.ComputeSHA256Hash;
					return true;

				case "SHA384":
				case "SHA-384":
					Oid = oidSha384;
					H = Hashes.ComputeSHA384Hash;
					return true;

				case "SHA512":
				case "SHA-512":
					Oid = oidSha512;
					H = Hashes.ComputeSHA512Hash;
					return true;

				case "SHA3-224":
					Oid = oidSha3_224;
					H = new SHA3_224().ComputeVariable;
					return true;

				case "SHA3-256":
					Oid = oidSha3_256;
					H = new SHA3_256().ComputeVariable;
					return true;

				case "SHA3-384":
					Oid = oidSha3_384;
					H = new SHA3_384().ComputeVariable;
					return true;

				case "SHA3-512":
					Oid = oidSha3_512;
					H = new SHA3_512().ComputeVariable;
					return true;

				case "SHAKE128":
				case "SHAKE-128":
					Oid = oidShake128;
					H = new SHAKE128(256).ComputeVariable;
					return true;

				case "SHAKE256":
				case "SHAKE-256":
					Oid = oidShake256;
					H = new SHAKE256(256).ComputeVariable;
					return true;

				default:
					Oid = null;
					H = null;
					return false;
			}
		}

		private static readonly byte[] oidSha256 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01
		};

		private static readonly byte[] oidSha384 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x02
		};

		private static readonly byte[] oidSha512 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x03
		};

		private static readonly byte[] oidSha3_224 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x07
		};
		private static readonly byte[] oidSha3_256 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x08
		};

		private static readonly byte[] oidSha3_384 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x09
		};

		private static readonly byte[] oidSha3_512 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x0a
		};

		private static readonly byte[] oidShake128 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x0b
		};

		private static readonly byte[] oidShake256 = new byte[]
		{
			0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x0c
		};

		/// <summary>
		/// Verifies a digital signature using the ML-DSA pre-hash algorithm.
		/// (Algorithm 5 HashML-DSA.Verify() in §5.4.1)
		/// </summary>
		/// <param name="PublicKey">Public Key</param>
		/// <param name="Message">Message</param>
		/// <param name="Signature">Signature</param>
		/// <param name="Context">Context</param>
		/// <param name="HashAlgorithm">Name of hash algorithm.</param>
		/// <returns>If the signature is valid.</returns>
		public bool Verify(byte[] PublicKey, byte[] Message, byte[] Signature, byte[] Context,
			string HashAlgorithm)
		{
			int ContextLen;

			if (Context is null)
			{
				Context = Array.Empty<byte>();
				ContextLen = 0;
			}
			else if ((ContextLen = Context.Length) > 255)
				return false;

			if (!TryGetHashFunction(HashAlgorithm, out HashFunctionArray H, out byte[] Oid))
				throw new ArgumentException("Unsupported hash algorithm: " + HashAlgorithm, nameof(HashAlgorithm));

			Message = H(Message);

			int MessageLen = Message.Length;
			int OidLen = Oid.Length;
			int Pos;

			byte[] Bin = new byte[2 + ContextLen + OidLen + MessageLen];
			Bin[0] = 1;
			Bin[1] = (byte)ContextLen;

			if (ContextLen > 0)
				Array.Copy(Context, 0, Bin, 2, ContextLen);

			Array.Copy(Oid, 0, Bin, Pos = 2 + ContextLen, OidLen);
			Pos += OidLen;

			Array.Copy(Message, 0, Bin, Pos, MessageLen);

			return this.Verify_Internal(PublicKey, Bin, Signature);
		}

		/// <summary>
		/// Generates a public and private key. 
		/// (Algorithm 6 ML-DSA.KeyGen_Internal() in §6.1)
		/// </summary>
		/// <param name="ξ">Seed</param>
		/// <returns>Public Key pk (320k+32 bytes) and Private Key sk 
		/// (160*((l+k)*bitlen(2η)+dk) bytes).</returns>
		public ML_DSA_Keys KeyGen_Internal(byte[] ξ)
		{
			if (ξ.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(ξ));

			byte[] Bin = new byte[34];
			Array.Copy(ξ, 0, Bin, 0, 32);
			Bin[32] = this.k;
			Bin[33] = this.l;

			byte[] Bin2 = H(Bin, 128);
			Clear(Bin);

			byte[] ρ = new byte[32];
			Array.Copy(Bin2, 0, ρ, 0, 32);

			byte[] K = new byte[32];
			Array.Copy(Bin2, 96, K, 0, 32);

			uint[,][] Â = this.ExpandÂ(ρ);

			byte[] B = new byte[66];            // ρ´ || 2 index bytes
			Array.Copy(Bin2, 32, B, 0, 64);

			short[][] s1 = this.ExpandS(B, this.l);
			short[][] s2 = this.ExpandS(B, this.k);

			Clear(B);

			uint[][] NTTs1 = NTT(s1);

			uint[][] t = new uint[this.k][];
			int i, j;

			for (i = 0; i < this.k; i++)
			{
				t[i] = new uint[n];

				for (j = 0; j < this.l; j++)
					MultiplyNTTsAndAdd(Â[i, j], NTTs1[j], t[i]);
			}

			InverseNTT(t);
			AddTo(t, s2);

			// Power2Round, decomposes t into two arrays t1, t0, where t=t1*2^d+t0 mod q
			// (Algorithm 35, §7.4)

			short[][] t0 = new short[this.k][];
			short[][] t1 = new short[this.k][];
			short[] f0, f1;
			uint[] f;
			short r0, r1;
			uint r;

			for (i = 0; i < this.k; i++)
			{
				f = t[i];
				t0[i] = f0 = new short[n];
				t1[i] = f1 = new short[n];

				for (j = 0; j < n; j++)
				{
					r = f[j];
					r0 = (short)(r & dBitMask);

					if (r0 > twoDHalf)
						r0 -= twoD;

					f0[j] = r0;

					r1 = (short)((r - r0) >> d);

					f1[j] = r1;
				}
			}

			byte[] PublicKey = this.PublicKeyEncode(ρ, t1);
			byte[] tr = H(PublicKey, 64);
			byte[] PrivateKey = this.PrivateKeyEncode(ρ, K, tr, s1, s2, t0);

			Clear(Bin2);
			Clear(t);
			Clear(ρ);
			Clear(K);
			Clear(s1);
			Clear(s2);
			Clear(t0);

			return new ML_DSA_Keys(Â, PublicKey, PrivateKey);
		}

		/// <summary>
		/// Samples a 𝑘 × ℓ matrix 𝐀 of elements of 𝑇𝑞.
		/// (Algorithm 32, in §7.3)
		/// </summary>
		/// <param name="ρ">Seed</param>
		/// <returns>Matrix of polynomials</returns>
		private uint[,][] ExpandÂ(byte[] ρ)
		{
			if (ρ.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(ρ));

			uint[,][] Â = new uint[this.k, this.l][];
			byte r, c;

			byte[] B = new byte[34];
			Array.Copy(ρ, 0, B, 0, 32);

			for (r = 0; r < this.k; r++)
			{
				B[33] = r;

				for (c = 0; c < this.l; c++)
				{
					B[32] = c;
					Â[r, c] = RejNttPoly(B);
				}
			}

			Clear(B);

			return Â;
		}

		/// <summary>
		/// Samples elements of 𝑇𝑞.
		/// (Algorithm 33, in §7.3)
		/// </summary>
		/// <param name="Seed">Seed</param>
		/// <param name="Nr">Number of polynomials to generate.</param>
		/// <returns>Vector of polynomials</returns>
		private short[][] ExpandS(byte[] Seed, int Nr)
		{
			short[][] s = new short[Nr][];
			byte r = 0;

			while (Nr-- > 0)
			{
				s[r++] = this.RejBoundedPoly(Seed);
				Seed[64]++;
			}

			return s;
		}

		/// <summary>
		/// Encodes a public key for ML-DSA into a byte string.
		/// Algorithm 22, §7.2.
		/// </summary>
		/// <param name="ρ">Seed</param>
		/// <param name="t1">Polynomials to encode.</param>
		/// <returns>Public Key</returns>
		private byte[] PublicKeyEncode(byte[] ρ, short[][] t1)
		{
			if (ρ.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(ρ));

			if (t1.Length != this.k)
				throw new ArgumentException("Invalid polynomial size.", nameof(t1));

			byte[] PublicKey = new byte[this.publicKeySize];
			int Pos = 32;

			Array.Copy(ρ, 0, PublicKey, 0, 32);

			for (int i = 0; i < this.k; i++)
				Pos += SimpleBitPack(t1[i], 10, PublicKey, Pos);

			return PublicKey;
		}

		/// <summary>
		/// Encodes a private key for ML-DSA into a byte string.
		/// Algorithm 25, §7.2.
		/// </summary>
		/// <param name="ρ">Seed</param>
		/// <param name="K">Seed</param>
		/// <param name="s1">Polynomials</param>
		/// <param name="s2">Polynomials</param>
		/// <param name="tr">Polynomials</param>
		/// <param name="t0">Polynomials to encode.</param>
		/// <returns>Public Key</returns>
		private byte[] PrivateKeyEncode(byte[] ρ, byte[] K, byte[] tr, short[][] s1, short[][] s2, short[][] t0)
		{
			if (ρ.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(ρ));

			if (K.Length != 32)
				throw new ArgumentException("K must be 32 bytes long.", nameof(K));

			if (tr.Length != 64)
				throw new ArgumentException("tr must be 32 bytes long.", nameof(tr));

			if (s1.Length != this.l)
				throw new ArgumentException("s1 must be " + this.l + " polynomials long.", nameof(s1));

			if (s2.Length != this.k)
				throw new ArgumentException("s2 must be " + this.k + " polynomials long.", nameof(s2));

			if (t0.Length != this.k)
				throw new ArgumentException("t0 must be " + this.k + " polynomials long.", nameof(t0));

			byte[] PrivateKey = new byte[this.privateKeySize];
			int Pos = 128;
			int i;

			Array.Copy(ρ, 0, PrivateKey, 0, 32);
			Array.Copy(K, 0, PrivateKey, 32, 32);
			Array.Copy(tr, 0, PrivateKey, 64, 64);

			for (i = 0; i < this.l; i++)
				Pos += BitPack(s1[i], PrivateKey, Pos, this.η, this.η);

			for (i = 0; i < this.k; i++)
				Pos += BitPack(s2[i], PrivateKey, Pos, this.η, this.η);

			uint b = 1 << (d - 1);
			uint a = b - 1;

			for (i = 0; i < this.k; i++)
				Pos += BitPack(t0[i], PrivateKey, Pos, a, b);

			return PrivateKey;
		}

		/// <summary>
		/// Decodes a private key for ML-DSA into a byte string.
		/// Algorithm 25, §7.2.
		/// </summary>
		/// <param name="PrivateKey">Private key.</param>
		/// <param name="ρ">Seed</param>
		/// <param name="K">Seed</param>
		/// <param name="s1">Polynomials</param>
		/// <param name="s2">Polynomials</param>
		/// <param name="tr">Polynomials</param>
		/// <param name="t0">Polynomials to encode.</param>
		/// <returns>Public Key</returns>
		private bool TryDecodePrivateKey(byte[] PrivateKey, out byte[] ρ, out byte[] K,
			out byte[] tr, out short[][] s1, out short[][] s2, out short[][] t0)
		{
			if (PrivateKey is null || PrivateKey.Length != this.privateKeySize)
			{
				ρ = null;
				K = null;
				tr = null;
				s1 = null;
				s2 = null;
				t0 = null;

				return false;
			}

			int Pos = 128;
			int i;

			ρ = new byte[32];
			K = new byte[32];
			tr = new byte[64];
			s1 = new short[this.l][];
			s2 = new short[this.k][];
			t0 = new short[this.k][];

			Array.Copy(PrivateKey, 0, ρ, 0, 32);
			Array.Copy(PrivateKey, 32, K, 0, 32);
			Array.Copy(PrivateKey, 64, tr, 0, 64);

			for (i = 0; i < this.l; i++)
			{
				s1[i] = new short[n];
				Pos += BitUnpack(s1[i], PrivateKey, Pos, this.η, this.η);
			}

			for (i = 0; i < this.k; i++)
			{
				s2[i] = new short[n];
				Pos += BitUnpack(s2[i], PrivateKey, Pos, this.η, this.η);
			}

			uint b = 1 << (d - 1);
			uint a = b - 1;

			for (i = 0; i < this.k; i++)
			{
				t0[i] = new short[n];
				Pos += BitUnpack(t0[i], PrivateKey, Pos, a, b);
			}

			return Pos == this.privateKeySize;
		}

		/// <summary>
		/// The algorithm RejNttPoly (Algorithm 30, §7.3) converts a seed together with two 
		/// indexing bytes into a polynomial in the NTT domain.
		/// </summary>
		/// <param name="Seed">Seed value</param>
		/// <returns>Sample in 𝑇𝑞</returns>
		private static uint[] RejNttPoly(byte[] Seed)
		{
			SHAKE128 HashFunction = new SHAKE128(0);
			Keccak1600.Context Context = HashFunction.Absorb(Seed);
			uint[] Result = new uint[n];
			int Pos = 0;

			while (Pos < n)
			{
				byte[] C = Context.Squeeze(3);

				// CoeffFromThreeBytes, Algorithm 14, §7.1

				uint i = (uint)(C[2] & 0x7f);
				i <<= 8;
				i |= C[1];
				i <<= 8;
				i |= C[0];

				if (i < q)
					Result[Pos++] = i;
			}

			return Result;
		}

		/// <summary>
		/// The algorithm RejBoundedPoly (Algorithm 31, §7.3) converts a seed together with 
		/// an index into a polynomial with bounded coefficients.
		/// </summary>
		/// <param name="Seed">Seed value</param>
		/// <returns>Sample in R𝑞</returns>
		public short[] RejBoundedPoly(byte[] Seed)
		{
			SHAKE256 HashFunction = new SHAKE256(0);
			Keccak1600.Context Context = HashFunction.Absorb(Seed);
			short[] Result = new short[n];
			int Pos = 0;

			if (this.η == 2)
			{
				while (Pos < n)
				{
					byte z = Context.Squeeze1();
					byte b1 = (byte)(z & 0x0f);
					byte b2 = (byte)(z >> 4);

					// CoeffFromHalfByte, Algorithm 15, §7.1

					if (b1 < 15)
						Result[Pos++] = (short)(2 - (b1 % 5));

					if (Pos < n && b2 < 15)
						Result[Pos++] = (short)(2 - (b2 % 5));
				}
			}
			else if (this.η == 4)
			{
				while (Pos < n)
				{
					byte z = Context.Squeeze1();
					byte b1 = (byte)(z & 0x0f);
					byte b2 = (byte)(z >> 4);

					// CoeffFromHalfByte, Algorithm 15, §7.1

					if (b1 < 9)
						Result[Pos++] = (short)(4 - b1);

					if (Pos < n && b2 < 9)
						Result[Pos++] = (short)(4 - b2);
				}
			}
			else
				throw new InvalidOperationException("Invalid η.");

			return Result;
		}

		/// <summary>
		/// Computes the NTT representation f̂ of the given polynomial f ∈ 𝑅𝑞.
		/// (Algorithm 41 in §7.5)
		/// </summary>
		/// <param name="f">Polynomial in 𝑅𝑞</param>
		public static void NTT(uint[] f)
		{
			if (f.Length != n)
				throw new ArgumentException("Polynomial must have " + n + " coefficients.", nameof(f));

			int j;
			int Len;
			int Start;
			int m = 0;
			ulong ζ;
			uint t;

			for (Len = n >> 1; Len >= 1; Len >>= 1)
			{
				for (Start = 0; Start < n; Start += Len << 1)
				{
					ζ = nttTransformZeta[++m];

					for (j = Start; j < Start + Len; j++)
					{
						t = (uint)(ζ * f[j + Len] % q);
						f[j + Len] = (f[j] + q - t) % q;
						f[j] = (f[j] + t) % q;
					}
				}
			}
		}

		/// <summary>
		/// Canonical extension of <see cref="NTT(uint[])"/>.
		/// </summary>
		/// <param name="f">Array of polynomials in 𝑅𝑞</param>
		public static void NTT(uint[][] f)
		{
			int i, c = f.Length;

			for (i = 0; i < c; i++)
				NTT(f[i]);
		}

		/// <summary>
		/// Canonical extension of <see cref="NTT(uint[])"/>.
		/// </summary>
		/// <param name="f">Array of polynomials in 𝑅𝑞</param>
		/// <returns>NTT(f)</returns>
		public static uint[] NTT(short[] f)
		{
			uint[] f0 = ToUIntVector(f);
			NTT(f0);
			return f0;
		}

		private static uint[] ToUIntVector(short[] f)
		{
			int i, c = f.Length;
			uint[] f0 = new uint[n];
			short v;

			for (i = 0; i < c; i++)
			{
				v = f[i];
				f0[i] = (uint)(v < 0 ? q + v : v);
			}

			return f0;
		}

		/// <summary>
		/// Canonical extension of <see cref="NTT(short[])"/>.
		/// </summary>
		/// <param name="f">Array of polynomials in 𝑅𝑞</param>
		/// <returns>NTT(f)</returns>
		public static uint[][] NTT(short[][] f)
		{
			int i, c = f.Length;
			uint[][] Result = new uint[c][];

			for (i = 0; i < c; i++)
				Result[i] = NTT(f[i]);

			return Result;
		}

		/// <summary>
		/// Computes the NTT^-1 representation f of the given polynomial f̂ ∈ 𝑇𝑞.
		/// (Algorithm 42 in §7.5)
		/// </summary>
		/// <param name="f">polynomial f̂ ∈ 𝑇𝑞</param>
		public static void InverseNTT(uint[] f)
		{
			if (f.Length != n)
				throw new ArgumentException("Polynomial must have " + n + " coefficients.", nameof(f));

			int j;
			int Len;
			int Len2;
			int StartLen;
			int Start;
			int m = 256;
			ulong ζ;
			uint t;

			for (Len = 1; Len < n; Len <<= 1)
			{
				Len2 = Len << 1;

				for (Start = 0; Start < n; Start += Len2)
				{
					ζ = nttTransformZeta[--m];

					StartLen = Start + Len;
					for (j = Start; j < StartLen; j++)
					{
						t = f[j];
						f[j] = (t + f[j + Len]) % q;
						f[j + Len] = (uint)(ζ * (f[j + Len] + q - t) % q);
					}
				}
			}

			for (j = 0; j < n; j++)
				f[j] = (uint)(8347681ul * f[j] % q);   // 8347681 = 256^-1 mod q
		}

		/// <summary>
		/// Canonical extension of <see cref="InverseNTT(uint[])"/>.
		/// </summary>
		/// <param name="f">Array of polynomials f̂ ∈ 𝑇𝑞</param>
		public static void InverseNTT(uint[][] f)
		{
			int i, c = f.Length;

			for (i = 0; i < c; i++)
				InverseNTT(f[i]);
		}

		/// <summary>
		/// Computes the product (in the ring 𝑇𝑞) of two NTT representations 
		/// (Algorithm 45 in §7.6) and adds the result to a result vector
		/// (Algorithm 44 in §7.6).
		/// </summary>
		/// <param name="f">Polynomial 1</param>
		/// <param name="g">Polynomial 2</param>
		/// <param name="Result">Result vector.</param>
		/// <returns>f*g in 𝑇𝑞</returns>
		/// <exception cref="ArgumentException">If polynomials are not of the correct size.</exception>
		public static void MultiplyNTTsAndAdd(uint[] f, uint[] g, uint[] Result)
		{
			if (f.Length != n || g.Length != n)
				throw new ArgumentException("Polynomials must have " + n + " coefficients.", nameof(f));

			int i;

			for (i = 0; i < n; i++)
				Result[i] = (uint)((Result[i] + (ulong)f[i] * g[i]) % q);
		}

		/// <summary>
		/// Computes the product (in the ring 𝑇𝑞) of two NTT representations 
		/// (Algorithm 45 in §7.6) and adds the result to a result vector
		/// (Algorithm 44 in §7.6).
		/// </summary>
		/// <param name="f">Polynomial 1</param>
		/// <param name="g">Polynomial 2</param>
		/// <param name="Result">Result vector.</param>
		/// <returns>f*g in 𝑇𝑞</returns>
		/// <exception cref="ArgumentException">If polynomials are not of the correct size.</exception>
		public static void MultiplyNTTsAndAdd(uint[] f, short[] g, uint[] Result)
		{
			if (f.Length != n || g.Length != n)
				throw new ArgumentException("Polynomials must have " + n + " coefficients.", nameof(f));

			int i;
			short v;

			for (i = 0; i < n; i++)
			{
				v = g[i];

				if (v < 0)
					Result[i] = (uint)((Result[i] + (ulong)f[i] * (uint)(q + v)) % q);
				else
					Result[i] = (uint)((Result[i] + (ulong)f[i] * (uint)v) % q);
			}
		}

		/// <summary>
		/// Adds <paramref name="g"/> to <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Polynomial that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Polynomial to add to <paramref name="f"/>.</param>
		public static void AddTo(int[] f, short[] g)
		{
			int i;

			for (i = 0; i < n; i++)
				f[i] += g[i];
		}

		/// <summary>
		/// Adds vector <paramref name="g"/> to vector <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Vector of polynomials that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Vector of polynomials to add to <paramref name="f"/>.</param>
		public static void AddTo(int[][] f, short[][] g)
		{
			int i, c = f.Length;
			if (g.Length != c)
				throw new ArgumentException("Vectors must have the same number of polynomials.", nameof(g));

			for (i = 0; i < c; i++)
				AddTo(f[i], g[i]);
		}

		/// <summary>
		/// Adds <paramref name="g"/> to <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Polynomial that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Polynomial to add to <paramref name="f"/>.</param>
		public static void AddTo(uint[] f, short[] g)
		{
			int i;
			short v;

			for (i = 0; i < n; i++)
			{
				v = g[i];

				if (v < 0)
					f[i] = (uint)((f[i] + q + v) % q);
				else
					f[i] = (uint)((f[i] + v) % q);
			}
		}

		/// <summary>
		/// Adds vector <paramref name="g"/> to vector <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Vector of polynomials that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Vector of polynomials to add to <paramref name="f"/>.</param>
		public static void AddTo(uint[][] f, short[][] g)
		{
			int i, c = f.Length;
			if (g.Length != c)
				throw new ArgumentException("Vectors must have the same number of polynomials.", nameof(g));

			for (i = 0; i < c; i++)
				AddTo(f[i], g[i]);
		}

		/// <summary>
		/// Adds <paramref name="g"/> to <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Polynomial that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Polynomial to add to <paramref name="f"/>.</param>
		public static void AddTo(uint[] f, uint[] g)
		{
			int i;

			for (i = 0; i < n; i++)
				f[i] = (uint)((f[i] + g[i]) % q);
		}

		/// <summary>
		/// Adds vector <paramref name="g"/> to vector <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Vector of polynomials that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Vector of polynomials to add to <paramref name="f"/>.</param>
		public static void AddTo(uint[][] f, uint[][] g)
		{
			int i, c = f.Length;
			if (g.Length != c)
				throw new ArgumentException("Vectors must have the same number of polynomials.", nameof(g));

			for (i = 0; i < c; i++)
				AddTo(f[i], g[i]);
		}

		/// <summary>
		/// Subtracts <paramref name="g"/> from <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Polynomial that will be decremented by <paramref name="g"/>.</param>
		/// <param name="g">Polynomial to subtract from <paramref name="f"/>.</param>
		public static void SubtractFrom(uint[] f, short[] g)
		{
			int i;
			short v;

			for (i = 0; i < n; i++)
			{
				v = g[i];

				if (v < 0)
					f[i] = (uint)((f[i] - v) % q);
				else
					f[i] = (uint)((f[i] + q - v) % q);
			}
		}

		/// <summary>
		/// Subtracts <paramref name="g"/> from <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Vector of polynomials that will be decremented by <paramref name="g"/>.</param>
		/// <param name="g">Vector of polynomials to subtract from <paramref name="f"/>.</param>
		public static void SubtractFrom(uint[][] f, short[][] g)
		{
			int i, c = f.Length;
			if (g.Length != c)
				throw new ArgumentException("Vectors must have the same number of polynomials.", nameof(g));

			for (i = 0; i < c; i++)
				SubtractFrom(f[i], g[i]);
		}

		/// <summary>
		/// Subtracts <paramref name="g"/> from <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Polynomial that will be decremented by <paramref name="g"/>.</param>
		/// <param name="g">Polynomial to subtract from <paramref name="f"/>.</param>
		public static void SubtractFrom(uint[] f, uint[] g)
		{
			int i;

			for (i = 0; i < n; i++)
				f[i] = (f[i] + q - g[i]) % q;
		}

		/// <summary>
		/// Subtracts <paramref name="g"/> from <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Vector of polynomials that will be decremented by <paramref name="g"/>.</param>
		/// <param name="g">Vector of polynomials to subtract from <paramref name="f"/>.</param>
		public static void SubtractFrom(uint[][] f, uint[][] g)
		{
			int i, c = f.Length;
			if (g.Length != c)
				throw new ArgumentException("Vectors must have the same number of polynomials.", nameof(g));

			for (i = 0; i < c; i++)
				SubtractFrom(f[i], g[i]);
		}

		/// <summary>
		/// Negates a polynomial in 𝑅𝑞.
		/// </summary>
		/// <param name="f">Polynomial</param>
		public static void Negate(uint[] f)
		{
			int i, c = f.Length;

			for (i = 0; i < c; i++)
				f[i] = (q - f[i]) % q;   // To avoid different CPU instructions to execute based on if bit is 0 or 1.
		}

		/// <summary>
		/// Negates an array of polynomials in 𝑅𝑞.
		/// </summary>
		/// <param name="f">Polynomials</param>
		public static void Negate(uint[][] f)
		{
			int i, c = f.Length;

			for (i = 0; i < c; i++)
				Negate(f[i]);
		}

		/// <summary>
		/// Computes the dot product of two vectors of polynomials in 𝑇𝑞.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Dot product in 𝑇𝑞</returns>
		/// <exception cref="ArgumentException"></exception>
		public static uint[] DotProductNTT(uint[][] v1, uint[][] v2)
		{
			int i, c = v1.Length;

			if (v2.Length != c)
				throw new ArgumentException("Vectors must have the same number of polynomials.", nameof(v2));

			uint[] Result = new uint[n];

			for (i = 0; i < c; i++)
				MultiplyNTTsAndAdd(v1[i], v2[i], Result);

			return Result;
		}

		/// <summary>
		/// Computes the scalar product of a polynomial in 𝑇𝑞 with a vector of 
		/// polynomials in 𝑇𝑞.
		/// </summary>
		/// <param name="c">Scalar polynomial</param>
		/// <param name="s">Vector of polynomials</param>
		/// <returns>Vector of polynomials</returns>
		public static uint[][] ScalarProductNTT(uint[] c, uint[][] s)
		{
			if (c.Length != n)
				throw new ArgumentException("Polynomials must have " + n + " coefficients.", nameof(c));

			int i, j, k = s.Length;
			uint[][] Result = new uint[k][];
			uint[] f;
			uint[] si;

			for (i = 0; i < k; i++)
			{
				Result[i] = f = new uint[n];
				si = s[i];

				if (si.Length != n)
					throw new ArgumentException("Polynomials must have " + n + " coefficients.", nameof(s));

				for (j = 0; j < n; j++)
					f[j] = (uint)((long)c[j] * si[j] % q);
			}

			return Result;
		}

		/// <summary>
		/// Encodes an array of integers (mod 2^d) into a byte array, as defined by
		/// Algorithm 16 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="d">Number of bits, between 1 and 15.</param>
		/// <param name="Output">Bytes will be encoded into this array.</param>
		/// <param name="Index">Index into output array where encoding will begin.</param>
		/// <returns>Number of bytes encoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int SimpleBitPack(short[] Values, int d, byte[] Output, int Index)
		{
			if (d < 1 || d > 15)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 15.");

			int i, c = Values.Length;
			int BitOffset = 0;
			int Index0 = Index;

			for (i = 0; i < c; i++)
			{
				ushort Value = (ushort)Values[i];

				Value &= ushortBitMask[d];
				Output[Index] |= (byte)(Value << BitOffset);
				BitOffset += d;
				if (BitOffset >= 8)
				{
					Index++;
					BitOffset -= 8;

					if (BitOffset > 0)
					{
						Output[Index] = (byte)(Value >> (d - BitOffset));

						if (BitOffset >= 8)
						{
							BitOffset -= 8;
							Index++;

							if (BitOffset > 0)
								Output[Index] = (byte)(Value >> (d - BitOffset));
						}
					}
				}
			}

			return Index - Index0;
		}

		/// <summary>
		/// Encodes an array of integers between [-a,b] into a byte array, as defined by
		/// Algorithm 17 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="Output">Bytes will be encoded into this array.</param>
		/// <param name="Index">Index into output array where encoding will begin.</param>
		/// <param name="a">-a is the smallest integer to encode.</param>
		/// <param name="b">b is the largest integer to encode.</param>
		/// <returns>Number of bytes encoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int BitPack(short[] Values, byte[] Output, int Index, uint a, uint b)
		{
			int d = BitLen(b + a);
			if (d < 1 || d > 15)
				throw new ArgumentOutOfRangeException(nameof(b), "d must be between 1 and 15.");

			int c = Values.Length;
			int BitOffset = 0;
			int Index0 = Index;
			int Value;
			int i;

			for (i = 0; i < c; i++)
			{
				Value = (int)(b - Values[i]);
				if (Value < 0)
					Value += q;

				Value &= ushortBitMask[d];
				Output[Index] |= (byte)(Value << BitOffset);
				BitOffset += d;

				while (BitOffset >= 8)
				{
					Index++;
					BitOffset -= 8;

					if (BitOffset > 0)
						Output[Index] = (byte)(Value >> (d - BitOffset));
				}
			}

			return Index - Index0;
		}

		/// <summary>
		/// Encodes an array of integers between [-a,b] into a byte array, as defined by
		/// Algorithm 17 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="Output">Bytes will be encoded into this array.</param>
		/// <param name="Index">Index into output array where encoding will begin.</param>
		/// <param name="a">-a is the smallest integer to encode.</param>
		/// <param name="b">b is the largest integer to encode.</param>
		/// <param name="MakeSigned">If integers modulus q should be represented as a 
		/// signed residue value.</param>
		/// <returns>Number of bytes encoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int BitPack(uint[] Values, byte[] Output, int Index, uint a, uint b, bool MakeSigned)
		{
			int d = BitLen(b + a);
			if (d < 1 || d > 30)
				throw new ArgumentOutOfRangeException(nameof(b), "d must be between 1 and 30.");

			int c = Values.Length;
			int BitOffset = 0;
			int BitsLeft;
			int Index0 = Index;
			int Value;
			int i, j;

			for (i = 0; i < c; i++)
			{
				Value = (int)Values[i];

				if (MakeSigned)
				{
					Value %= q;
					if (q - Value < Value)
						Value -= q;
				}

				Value = (int)(b - Value);

				Value &= intBitMask[d];
				BitsLeft = d;

				if (BitOffset > 0)
				{
					Output[Index] |= (byte)(Value << BitOffset);
					j = 8 - BitOffset;

					if (BitsLeft >= j)
					{
						BitsLeft -= j;
						Value >>= j;
						BitOffset = 0;
						Index++;
					}
					else
					{
						BitOffset += BitsLeft;
						continue;
					}
				}

				while (BitsLeft >= 8)
				{
					Output[Index++] = (byte)Value;
					Value >>= 8;
					BitsLeft -= 8;
				}

				if (BitsLeft > 0)
				{
					Output[Index] = (byte)Value;
					BitOffset += BitsLeft;
				}
			}

			if (BitOffset > 0)
				Index++;

			return Index - Index0;
		}

		/// <summary>
		/// Number of bits required to represent an integer i in binary.
		/// </summary>
		/// <param name="i">Integer</param>
		/// <returns>Number of bits</returns>
		private static int BitLen(uint i)
		{
			int d = 0;

			while (i > 0)
			{
				i >>= 1;
				d++;
			}

			return d;
		}

		/// <summary>
		/// Decodes an array of integers between [-a,b] from a byte array, as defined by
		/// Algorithm 19 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="Input">Bytes will be decoded from this array.</param>
		/// <param name="Index">Index into input array where decoding will begin.</param>
		/// <param name="a">-a is the smallest integer to decode.</param>
		/// <param name="b">b is the largest integer to dencode.</param>
		/// <returns>Number of bytes decoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int BitUnpack(short[] Values, byte[] Input, int Index, uint a, uint b)
		{
			int d = BitLen(b + a);
			if (d < 1 || d > 15)
				throw new ArgumentOutOfRangeException(nameof(b), "Bitlength of a+b must be between 1 and 15.");

			int c = Values.Length;
			int BitOffset = 0;
			int Index0 = Index;
			int i, j;
			int Value;

			for (i = 0; i < c; i++)
			{
				Value = Input[Index] >> BitOffset;
				BitOffset += d;

				while (BitOffset >= 8)
				{
					Index++;
					BitOffset -= 8;

					if (BitOffset > 0)
					{
						j = d - BitOffset;
						Value &= ushortBitMask[j];
						Value |= Input[Index] << j;
					}
				}

				Value &= ushortBitMask[d];
				Values[i] = (short)(b - Value);
			}

			return Index - Index0;
		}

		/// <summary>
		/// Decodes an array of integers between [-a,b] from a byte array, as defined by
		/// Algorithm 19 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="Input">Bytes will be decoded from this array.</param>
		/// <param name="Index">Index into input array where decoding will begin.</param>
		/// <param name="a">-a is the smallest integer to decode.</param>
		/// <param name="b">b is the largest integer to dencode.</param>
		/// <param name="MakeUnsigned">If integers modulus q should be represented as non-negative
		/// integers [0,q) (true).</param>
		/// <returns>Number of bytes decoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int BitUnpack(uint[] Values, byte[] Input, int Index, uint a, uint b, bool MakeUnsigned)
		{
			int d = BitLen(b + a);
			if (d < 1 || d > 30)
				throw new ArgumentOutOfRangeException(nameof(b), "Bitlength of a+b must be between 1 and 30.");

			int c = Values.Length;
			int BitOffset = 0;
			int Index0 = Index;
			int i, j;
			int Value;

			for (i = 0; i < c; i++)
			{
				Value = Input[Index] >> BitOffset;
				BitOffset += d;

				while (BitOffset >= 8)
				{
					Index++;
					BitOffset -= 8;

					if (BitOffset > 0)
					{
						j = d - BitOffset;
						Value &= intBitMask[j];
						Value |= Input[Index] << j;
					}
				}

				Value &= intBitMask[d];
				j = (int)(b - Value) % q;
				if (MakeUnsigned)
				{
					if (j < 0)
						j += q;
				}
				else
				{
					if (q - j < j)
						j -= q;
				}

				Values[i] = (uint)j;
			}

			if (BitOffset > 0)
				Index++;

			return Index - Index0;
		}

		/// <summary>
		/// Hash function H, as defined in §3.7.
		/// </summary>
		/// <param name="Data">Input data.</param>
		/// <param name="l">Number of bytes</param>
		/// <returns>Hash digest.</returns>
		public static byte[] H(byte[] Data, int l)
		{
			return new SHAKE256(l << 3).ComputeVariable(Data);
		}

		/// <summary>
		/// Hash function G, as defined in §3.7.
		/// </summary>
		/// <param name="Data">Input data.</param>
		/// <param name="l">Number of bytes</param>
		/// <returns>Hash digest.</returns>
		public static byte[] G(byte[] Data, int l)
		{
			return new SHAKE128(l << 3).ComputeVariable(Data);
		}

		/// <summary>
		/// The values 𝜁BitRev8(𝑘) mod 𝑞 for 𝑘 = 1,…,255 used in the NTT Algorithms 41 and 42.
		/// See Appendix B.
		/// </summary>
		private static readonly uint[] nttTransformZeta = new uint[256]
		{
			0, 4808194, 3765607, 3761513, 5178923, 5496691, 5234739, 5178987,
			7778734, 3542485, 2682288, 2129892, 3764867, 7375178, 557458, 7159240,
			5010068, 4317364, 2663378, 6705802, 4855975, 7946292, 676590, 7044481,
			5152541, 1714295, 2453983, 1460718, 7737789, 4795319, 2815639, 2283733,
			3602218, 3182878, 2740543, 4793971, 5269599, 2101410, 3704823, 1159875,
			394148, 928749, 1095468, 4874037, 2071829, 4361428, 3241972, 2156050,
			3415069, 1759347, 7562881, 4805951, 3756790, 6444618, 6663429, 4430364,
			5483103, 3192354, 556856, 3870317, 2917338, 1853806, 3345963, 1858416,
			3073009, 1277625, 5744944, 3852015, 4183372, 5157610, 5258977, 8106357,
			2508980, 2028118, 1937570, 4564692, 2811291, 5396636, 7270901, 4158088,
			1528066, 482649, 1148858, 5418153, 7814814, 169688, 2462444, 5046034,
			4213992, 4892034, 1987814, 5183169, 1736313, 235407, 5130263, 3258457,
			5801164, 1787943, 5989328, 6125690, 3482206, 4197502, 7080401, 6018354,
			7062739, 2461387, 3035980, 621164, 3901472, 7153756, 2925816, 3374250,
			1356448, 5604662, 2683270, 5601629, 4912752, 2312838, 7727142, 7921254,
			348812, 8052569, 1011223, 6026202, 4561790, 6458164, 6143691, 1744507,
			1753, 6444997, 5720892, 6924527, 2660408, 6600190, 8321269, 2772600,
			1182243, 87208, 636927, 4415111, 4423672, 6084020, 5095502, 4663471,
			8352605, 822541, 1009365, 5926272, 6400920, 1596822, 4423473, 4620952,
			6695264, 4969849, 2678278, 4611469, 4829411, 635956, 8129971, 5925040,
			4234153, 6607829, 2192938, 6653329, 2387513, 4768667, 8111961, 5199961,
			3747250, 2296099, 1239911, 4541938, 3195676, 2642980, 1254190, 8368000,
			2998219, 141835, 8291116, 2513018, 7025525, 613238, 7070156, 6161950,
			7921677, 6458423, 4040196, 4908348, 2039144, 6500539, 7561656, 6201452,
			6757063, 2105286, 6006015, 6346610, 586241, 7200804, 527981, 5637006,
			6903432, 1994046, 2491325, 6987258, 507927, 7192532, 7655613, 6545891,
			5346675, 8041997, 2647994, 3009748, 5767564, 4148469, 749577, 4357667,
			3980599, 2569011, 6764887, 1723229, 1665318, 2028038, 1163598, 5011144,
			3994671, 8368538, 7009900, 3020393, 3363542, 214880, 545376, 7609976,
			3105558, 7277073, 508145, 7826699, 860144, 3430436, 140244, 6866265,
			6195333, 3123762, 2358373, 6187330, 5365997, 6663603, 2926054, 7987710,
			8077412, 3531229, 4405932, 4606686, 1900052, 7598542, 1054478, 7648983
		};

		private byte[] Sign_Internal(byte[] PrivateKey, byte[] Message, byte[] Seed)
		{
			if (!this.TryDecodePrivateKey(PrivateKey, out byte[] ρ, out byte[] K,
				out byte[] tr, out short[][] s1, out short[][] s2, out short[][] t0))
			{
				throw new ArgumentException("Invalid private key.", nameof(PrivateKey));
			}

			uint[][] NTTs1 = NTT(s1);
			uint[][] NTTs2 = NTT(s2);
			uint[][] NTTt0 = NTT(t0);

			uint[,][] Â = this.ExpandÂ(ρ);

			int MessageLen = Message.Length;
			byte[] Bin = new byte[64 + MessageLen];

			Array.Copy(tr, 0, Bin, 0, 64);
			Array.Copy(Message, 0, Bin, 64, MessageLen);

			byte[] μ = H(Bin, 64);
			Clear(Bin);

			Bin = new byte[128];
			Array.Copy(K, 0, Bin, 0, 32);
			Array.Copy(Seed, 0, Bin, 32, 32);
			Array.Copy(μ, 0, Bin, 64, 64);

			byte[] ρ2 = H(Bin, 64);
			Clear(Bin);

			ushort κ = 0;
			byte[] h = null;
			byte[] cSeed = null;
			uint[][] z = null;
			bool Found = false;
			int IterationLimit = 10000;

			while (!Found)
			{
				if (--IterationLimit <= 0)
					throw new InvalidOperationException("Unable to calculate signature.");

				uint[][] y = this.ExpandMask(ρ2, κ);

				if (!(z is null))
					Clear(z);

				z = Clone(y);
				NTT(y);

				uint[][] w = new uint[this.k][];
				int i, j;

				for (i = 0; i < this.k; i++)
				{
					w[i] = new uint[n];

					for (j = 0; j < this.l; j++)
						MultiplyNTTsAndAdd(Â[i, j], y[j], w[i]);
				}

				InverseNTT(w);

				short[][] w1 = this.HighBits(w);

				// w1Encode, Algorithm 28, §7.2

				int w1Len = BitLen((uint)((q - 1) / this.twoγ2 - 1));

				Bin = new byte[64 + this.k * (w1Len << 5)];
				Array.Copy(μ, 0, Bin, 0, 64);

				int Pos = 64;

				for (i = 0; i < this.k; i++)
					Pos += SimpleBitPack(w1[i], w1Len, Bin, Pos);

				cSeed = H(Bin, this.λ >> 2);
				Clear(Bin);

				short[] c = this.SampleInBall(cSeed);
				uint[] NTTc = NTT(c);

				uint[][] cs1 = ScalarProductNTT(NTTc, NTTs1);
				InverseNTT(cs1);

				uint[][] cs2 = ScalarProductNTT(NTTc, NTTs2);
				InverseNTT(cs2);

				AddTo(z, cs1);

				SubtractFrom(w, cs2);
				int[][] r0 = this.LowBits(w);

				uint nz = InfinityNorm(z);

				if (nz < this.γ1 - this.β && InfinityNorm(r0) < this.γ2 - this.β)
				{
					uint[][] ct0 = ScalarProductNTT(NTTc, NTTt0);
					InverseNTT(ct0);

					AddTo(w, ct0);
					Negate(ct0);

					if (InfinityNorm(ct0) < this.γ2)
						Found = true;

					h = this.MakeAndEncodeHint(ct0, w); // Alters ct0
					Clear(ct0);
				}

				Clear(y);
				Clear(w);
				Clear(cs1);
				Clear(cs2);
				Clear(r0);

				κ += this.l;
			}

			Clear(μ);
			Clear(tr);
			Clear(ρ);
			Clear(K);
			Clear(NTTs1);
			Clear(NTTs2);
			Clear(NTTt0);

			return this.EncodeSignature(cSeed, z, h);
		}

		private static uint[][] Clone(uint[][] v)
		{
			int i, c = v.Length;
			uint[][] Result = new uint[c][];

			for (i = 0; i < c; i++)
				Result[i] = (uint[])v[i].Clone();

			return Result;
		}

		/// <summary>
		/// Inifinity norm, as defined in §2.3
		/// </summary>
		/// <param name="f">Polynomial over ℤ𝑞</param>
		/// <returns>‖f‖∞</returns>
		private static uint InfinityNorm(uint[] f)
		{
			uint Result = 0;
			int i, c = f.Length;
			uint v, w;

			for (i = 0; i < c; i++)
			{
				v = f[i];

				w = q - v;
				if (w < v)
					v = w;

				if (v > Result)
					Result = v;
			}

			return Result;
		}

		/// <summary>
		/// Inifinity norm, as defined in §2.3
		/// </summary>
		/// <param name="f">Polynomials over ℤ𝑞</param>
		/// <returns>‖f‖∞</returns>
		private static uint InfinityNorm(uint[][] f)
		{
			uint Result = 0;
			int i, c = f.Length;
			uint v;

			for (i = 0; i < c; i++)
			{
				v = InfinityNorm(f[i]);
				if (v > Result)
					Result = v;
			}

			return Result;
		}

		/// <summary>
		/// Inifinity norm, as defined in §2.3
		/// </summary>
		/// <param name="f">Polynomial over ℤ𝑞</param>
		/// <returns>‖f‖∞</returns>
		private static int InfinityNorm(int[] f)
		{
			int Result = 0;
			int i, c = f.Length;
			int v, w;

			for (i = 0; i < c; i++)
			{
				v = f[i];

				w = q - v;
				if (w < v)
					v = w;

				if (v > Result)
					Result = v;
			}

			return Result;
		}

		/// <summary>
		/// Inifinity norm, as defined in §2.3
		/// </summary>
		/// <param name="f">Polynomials over ℤ𝑞</param>
		/// <returns>‖f‖∞</returns>
		private static int InfinityNorm(int[][] f)
		{
			int Result = 0;
			int i, c = f.Length;
			int v;

			for (i = 0; i < c; i++)
			{
				v = InfinityNorm(f[i]);
				if (v > Result)
					Result = v;
			}

			return Result;
		}

		private uint[][] ExpandMask(byte[] ρ, ushort μ)
		{
			uint[][] y = new uint[this.l][];
			int c = BitLen(this.γ1 - 1) + 1;
			int r;
			byte[] v;

			byte[] ρ1 = new byte[66];
			Array.Copy(ρ, 0, ρ1, 0, 64);

			for (r = 0; r < this.l; r++)
			{
				ρ1[64] = (byte)μ;
				ρ1[65] = (byte)(μ >> 8);
				μ++;

				v = H(ρ1, c << 5);

				y[r] = new uint[n];
				BitUnpack(y[r], v, 0, this.γ1 - 1, this.γ1, true);
			}

			return y;
		}

		/// <summary>
		/// High bits of vector.
		/// (Algorithm 37, §7.4)
		/// </summary>
		/// <param name="w">Input vector of polynomials.</param>
		/// <returns>High-order bits.</returns>
		private short[][] HighBits(uint[][] w)
		{
			this.Decompose(w, out _, out short[][] Result);
			return Result;
		}

		/// <summary>
		/// Low bits of vector.
		/// (Algorithm 38, §7.4)
		/// </summary>
		/// <param name="w">Input vector of polynomials.</param>
		/// <returns>High-order bits.</returns>
		private int[][] LowBits(uint[][] w)
		{
			this.Decompose(w, out int[][] Result, out _);
			return Result;
		}

		/// <summary>
		/// Decompose, decomposes w into two arrays w1, w0, where w=w1*2*γ2+w0 mod q
		/// (Algorithm 36, §7.4)
		/// </summary>
		/// <param name="w">Input vector of polynomials.</param>
		/// <param name="w0">Low bits.</param>
		/// <param name="w1">High bits.</param>
		private void Decompose(uint[][] w, out int[][] w0, out short[][] w1)
		{
			w0 = new int[this.k][]; // LowBits
			w1 = new short[this.k][]; // HighBits

			int[] f0;
			short[] f1;
			uint[] f;
			int r0, r1;
			int r;
			int i, j;

			for (i = 0; i < this.k; i++)
			{
				f = w[i];
				w0[i] = f0 = new int[n];
				w1[i] = f1 = new short[n];

				for (j = 0; j < n; j++)
				{
					r = (int)f[j];
					r0 = r % this.twoγ2;

					if (r0 > this.γ2)
						r0 -= this.twoγ2;

					r1 = r - r0;
					if (r1 == q - 1)
					{
						r1 = 0;
						r0--;
					}
					else
						r1 /= this.twoγ2;

					f0[j] = r0;
					f1[j] = (short)r1;
				}
			}
		}

		private short[] SampleInBall(byte[] Seed)
		{
			short[] c = new short[n];
			SHAKE256 HashFunction = new SHAKE256(0);
			Keccak1600.Context Context = HashFunction.Absorb(Seed);
			byte[] s = Context.Squeeze(8);
			int i, k;
			byte j;

			for (i = n - this.τ, k = 0; i < n; i++, k++)
			{
				j = Context.Squeeze1();
				while (j > i)
					j = Context.Squeeze1();

				c[i] = c[j];
				c[j] = (s[k >> 3] & (1 << (k & 7))) == 0 ? (short)1 : (short)-1;
			}

			return c;
		}

		/// <summary>
		/// Computes hint bit indicating whether adding 𝑧 to 𝑟 alters the high bits of 𝑟.
		/// (Algorithm 39, §7.4), together with encoding of hint in signature
		/// (Algorithm 20, §7.1).
		/// </summary>
		/// <param name="z"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		private byte[] MakeAndEncodeHint(uint[][] z, uint[][] r)
		{
			// MakeHint, (Algorithm 39, §7.4)

			short[][] r1 = this.HighBits(r);

			AddTo(r, z);

			short[][] v1 = this.HighBits(r);

			// HintBitPack, (Algorithm 20, §7.1)

			byte[] y = new byte[this.ω + this.k];
			byte Index = 0;
			int i, j;
			short[] f, g;

			for (i = 0; i < this.k; i++)
			{
				f = r1[i];
				g = v1[i];

				for (j = 0; j < n; j++)
				{
					if (f[j] != g[j])
						y[Index++] = (byte)j;
				}

				y[this.ω + i] = Index;
			}

			return y;
		}

		/// <summary>
		/// Encodes a signature into a byte string.
		/// (Algorithm 26, §7.2)
		/// </summary>
		/// <param name="c">Commitment hash</param>
		/// <param name="z">Signer's response.</param>
		/// <param name="h">Signer's hint.</param>
		/// <returns>Encoded signature.</returns>
		private byte[] EncodeSignature(byte[] c, uint[][] z, byte[] h)
		{
			int l1 = c.Length;
			int l2 = this.l * ((1 + BitLen(this.γ1 - 1)) << 5);
			int l3 = h.Length;
			int Len = l1 + l2 + l3;
			int i;
			byte[] Result = new byte[Len];

			Array.Copy(c, 0, Result, 0, l1);

			for (i = 0; i < this.l; i++)
				l1 += BitPack(z[i], Result, l1, this.γ1 - 1, this.γ1, true);

			Array.Copy(h, 0, Result, l1, l3);

			return Result;
		}

		private bool Verify_Internal(byte[] PublicKey, byte[] Message, byte[] Signature)
		{
			throw new NotImplementedException();    // TODO
		}
	}
}
