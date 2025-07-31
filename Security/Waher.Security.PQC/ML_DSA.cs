using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
				case "ML-DSA-512":
					return ML_DSA_44;

				case "ML-DSA-768":
					return ML_DSA_65;

				case "ML-DSA-1024":
					return ML_DSA_87;

				default:
					throw new ArgumentException("Unknown model name: " + Name, nameof(Name));
			}
		}

		private const int n = 256;
		private const int q = 8380417;
		private const int ζ = 1753;                 // 512th root of unity in ℤ𝑞
		private const int d = 13;                   // #dropped bits from t

		private readonly int τ;                     // # of ±1’s in polynomial c
		private readonly int λ;                     // collision strength of c
		private readonly int γ1;                    // coefficient range of y 
		private readonly int γ2;                    // low-order rounding range 
		private readonly byte k;                    // Rows of matrix A
		private readonly byte l;                    // Columns of matrix A
		private readonly int η;                     // private key range
		private readonly int β;                     // τ*η
		private readonly int ω;                     // max # of 1’s in the hint h

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
		/// (160*((l+k)*bitlin(2η)+dk) bytes).</returns>
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
		/// (160*((l+k)*bitlin(2η)+dk) bytes).</returns>
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

			byte[] ρ2 = new byte[64];
			Array.Copy(Bin2, 32, ρ2, 0, 64);

			byte[] K = new byte[32];
			Array.Copy(Bin2, 96, K, 0, 32);
			Clear(Bin2);

			uint[,][] Â = this.ExpandÂ(ρ);


			throw new NotImplementedException();	// TODO
		}

		/// <summary>
		/// Samples a 𝑘 × ℓ matrix 𝐀 of elements of 𝑇𝑞.
		/// (Algorithm 32, in §7.3)
		/// </summary>
		/// <param name="ρ">Seed</param>
		/// <returns></returns>
		private uint[,][] ExpandÂ(byte[] ρ)
		{
			uint[,][] Â = new uint[this.k, this.l][];
			byte r, c;

			for (r = 0; r < this.k; r++)
			{
				for (c = 0; c < this.l; c++)
					Â[r, c] = RejNttPoly(ρ, c, r);
			}

			return Â;
		}

		/// <summary>
		/// The algorithm RejNttPoly (Algorithm 30, §7.3) converts a seed together with two 
		/// indexing bytes into a polynomial in the NTT domain.
		/// </summary>
		/// <param name="Index1">Byte index value 1.</param>
		/// <param name="Index2">Byte index value 2.</param>
		/// <returns>Sample in 𝑇𝑞</returns>
		public static uint[] RejNttPoly(byte Index1, byte Index2)
		{
			byte[] Seed = CreateSeed();

			uint[] Result = RejNttPoly(Seed, Index1, Index2);
			Clear(Seed);

			return Result;
		}

		/// <summary>
		/// The algorithm RejNttPoly (Algorithm 30, §7.3) converts a seed together with two 
		/// indexing bytes into a polynomial in the NTT domain.
		/// </summary>
		/// <param name="Seed">Seed value</param>
		/// <param name="Index1">Byte index value 1.</param>
		/// <param name="Index2">Byte index value 2.</param>
		/// <returns>Sample in 𝑇𝑞</returns>
		public static uint[] RejNttPoly(byte[] Seed, byte Index1, byte Index2)
		{
			SHAKE128 HashFunction = new SHAKE128(0);
			int c = Seed.Length;
			byte[] B = new byte[c + 2];
			Array.Copy(Seed, 0, B, 0, c);
			B[c] = Index1;
			B[c + 1] = Index2;

			Keccak1600.Context Context = HashFunction.Absorb(B);
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

		private byte[] Sign_Internal(byte[] PrivateKey, byte[] Message, byte[] Signature)
		{
			throw new NotImplementedException();    // TODO
		}

		private bool Verify_Internal(byte[] PublicKey, byte[] Message, byte[] Signature)
		{
			throw new NotImplementedException();	// TODO
		}
	}
}
