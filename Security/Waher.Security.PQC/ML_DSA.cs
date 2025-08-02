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
		private readonly int γ1;                        // coefficient range of y 
		private readonly int γ2;                        // low-order rounding range 
		private readonly byte k;                        // Rows of matrix A
		private readonly byte l;                        // Columns of matrix A
		private readonly int η;                         // private key range
		private readonly int β;                         // τ*η
		private readonly int ω;                         // max # of 1’s in the hint h

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
		public ML_DSA(int τ, int λ, int γ1, int γ2, byte k, byte l, int η, int ω,
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

			short[][] s1 = this.ExpandS(B, this.k);
			short[][] s2 = this.ExpandS(B, this.l);

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
			// (Algorithm 35, §7,4)

			short[][] t0 = new short[this.k][];
			short[][] t1 = new short[this.k][];

			for (i = 0; i < this.k; i++)
			{
				short[] f0, f1;
				uint[] f;
				short r0, r1;
				uint r;

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
		/// Encodes a public key for ML-DSA into a byte string.
		/// Algorithm 22, §7.2.
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

			int b = 1 << (d - 1);
			int a = b - 1;

			for (i = 0; i < this.k; i++)
				Pos += BitPack(t0[i], PrivateKey, Pos, a, b);

			return PrivateKey;
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
					byte z = Context.Squeeze(1)[0];
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
					byte z = Context.Squeeze(1)[0];
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
			int i, c = f.Length;
			uint[] f0 = new uint[n];
			short v;

			for (i = 0; i < c; i++)
			{
				v = f[i];
				f0[i] = (uint)(v < 0 ? q + v : v);
			}

			NTT(f0);

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
		/// Computes the product (in the ring 𝑇𝑞) of two NTT representations.
		/// (Algorithm 45 in §7.6)
		/// </summary>
		/// <param name="f">Polynomial 1</param>
		/// <param name="g">Polynomial 2</param>
		/// <returns>f*g in 𝑇𝑞</returns>
		/// <exception cref="ArgumentException">If polynomials are not of the correct size.</exception>
		public static uint[] MultiplyNTTs(uint[] f, uint[] g)
		{
			uint[] Result = new uint[n];
			MultiplyNTTsAndAdd(f, g, Result);
			return Result;
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
		/// Encodes an array of integers (mod 2^d) into a byte array, as defined by
		/// Algorithm 16 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="d">Number of bits, between 1 and 15.</param>
		/// <returns>Byte array.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static byte[] SimpleBitPack(short[] Values, int d)
		{
			int c = Values.Length;
			int NrBits = c * d;
			int NrBytes = (NrBits + 7) >> 3;
			byte[] Result = new byte[NrBytes];

			SimpleBitPack(Values, d, Result, 0);

			return Result;
		}

		/// <summary>
		/// Encodes an array of integers (mod 2^d) into a byte array, as defined by
		/// Algorithm 16 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="d">Number of bits, between 1 and 12.</param>
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

				Value &= bitMask[d];
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
		/// Encodes an array of integers (mod 2^d) into a byte array, as defined by
		/// Algorithm 16 in §7.1. Canonical
		/// extension of <see cref="SimpleBitPack(short[], int, byte[], int)"/>.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="d">Number of bits, between 1 and 12.</param>
		/// <param name="Output">Bytes will be encoded into this array.</param>
		/// <param name="Index">Index into output array where encoding will begin.</param>
		/// <returns>Number of bytes encoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int SimpleBitPack(short[][] Values, int d, byte[] Output, int Index)
		{
			int Pos = Index;
			int i, c = Values.Length;

			for (i = 0; i < c; i++)
			{
				int NrBytes = SimpleBitPack(Values[i], d, Output, Pos);
				Pos += NrBytes;
			}

			return Pos - Index;
		}

		/// <summary>
		/// Encodes an array of integers between [-a,b] into a byte array, as defined by
		/// Algorithm 17 in §7.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="a">-a is the smallest integer to encode.</param>
		/// <param name="b">b is the largest integer to encode.</param>
		/// <returns>Byte array.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static byte[] BitPack(short[] Values, int a, int b)
		{
			int c = Values.Length;
			int NrBits = c * d;
			int NrBytes = (NrBits + 7) >> 3;
			byte[] Result = new byte[NrBytes];

			BitPack(Values, Result, 0, a, b);

			return Result;
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
		public static int BitPack(short[] Values, byte[] Output, int Index, int a, int b)
		{
			int d = 0;
			int i = b + a;

			while (i > 0)
			{
				i >>= 1;
				d++;
			}

			if (d < 1 || d > 15)
				throw new ArgumentOutOfRangeException(nameof(b), "d must be between 1 and 15.");

			int c = Values.Length;
			int BitOffset = 0;
			int Index0 = Index;

			for (i = 0; i < c; i++)
			{
				int Value = b - Values[i];

				Value &= bitMask[d];
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
		/// extension of <see cref="BitPack(short[], byte[], int, int, int)"/>.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="Output">Bytes will be encoded into this array.</param>
		/// <param name="Index">Index into output array where encoding will begin.</param>
		/// <param name="a">-a is the smallest integer to encode.</param>
		/// <param name="b">b is the largest integer to encode.</param>
		/// <returns>Number of bytes encoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int SimpleBitPack(short[][] Values, byte[] Output, int Index, int a, int b)
		{
			int Pos = Index;
			int i, c = Values.Length;

			for (i = 0; i < c; i++)
			{
				int NrBytes = BitPack(Values[i], Output, Pos, a, b);
				Pos += NrBytes;
			}

			return Pos - Index;
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

		private byte[] Sign_Internal(byte[] PrivateKey, byte[] Message, byte[] Signature)
		{
			throw new NotImplementedException();    // TODO
		}

		private bool Verify_Internal(byte[] PublicKey, byte[] Message, byte[] Signature)
		{
			throw new NotImplementedException();    // TODO
		}
	}
}
