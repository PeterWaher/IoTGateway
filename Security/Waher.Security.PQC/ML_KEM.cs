using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Waher.Security.SHA3;

namespace Waher.Security.PQC
{
	/// <summary>
	/// Implements the ML-KEM algorithm for post-quantum cryptography, as defined in
	/// NIST FIPS 203: https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.203.pdf
	/// </summary>
	public class ML_KEM
	{
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		private static readonly ushort[] bitMask =
		{
			0x0000, 0x0001, 0x0003, 0x0007, 0x000F, 0x001F, 0x003F, 0x007F,
			0x00FF, 0x01FF, 0x03FF, 0x07FF, 0x0FFF, 0x1FFF, 0x3FFF, 0x7FFF,
			0xFFFF
		};

		/// <summary>
		/// The following 128 numbers are the values of 𝜁^BitRev7(𝑖) mod 𝑞 for 𝑖 ∈ {0,…,127}. 
		/// These numbers are used in Algorithms 9 and 10.
		/// </summary>
		private static readonly ushort[] nttTransformZeta =
		{
			1, 1729, 2580, 3289, 2642, 630, 1897, 848,
			1062, 1919, 193, 797, 2786, 3260, 569, 1746,
			296, 2447, 1339, 1476, 3046, 56, 2240, 1333,
			1426, 2094, 535, 2882, 2393, 2879, 1974, 821,
			289, 331, 3253, 1756, 1197, 2304, 2277, 2055,
			650, 1977, 2513, 632, 2865, 33, 1320, 1915,
			2319, 1435, 807, 452, 1438, 2868, 1534, 2402,
			2647, 2617, 1481, 648, 2474, 3110, 1227, 910,
			17, 2761, 583, 2649, 1637, 723, 2288, 1100,
			1409, 2662, 3281, 233, 756, 2156, 3015, 3050,
			1703, 1651, 2789, 1789, 1847, 952, 1461, 2687,
			939, 2308, 2437, 2388, 733, 2337, 268, 641,
			1584, 2298, 2037, 3220, 375, 2549, 2090, 1645,
			1063, 319, 2773, 757, 2099, 561, 2466, 2594,
			2804, 1092, 403, 1026, 1143, 2150, 2775, 886,
			1722, 1212, 1874, 1029, 2110, 2935, 885, 2154
		};

		/// <summary>
		/// When implementing Algorithm 11, the values 𝜁^2BitRev7(𝑖)+1 mod 𝑞 need to be 
		/// computed. The following array contains these values for 𝑖 ∈ {0,…,127}: 
		/// </summary>
		private static readonly ushort[] nttTransformZetaRoots =
		{
			17, q - 17, 2761, q - 2761, 583, q - 583, 2649, q - 2649,
			1637, q - 1637, 723, q - 723, 2288, q - 2288, 1100, q - 1100,
			1409, q - 1409, 2662, q - 2662, 3281, q - 3281, 233, q - 233,
			756, q - 756, 2156, q - 2156, 3015, q - 3015, 3050, q - 3050,
			1703, q - 1703, 1651, q - 1651, 2789, q - 2789, 1789, q - 1789,
			1847, q - 1847, 952, q - 952, 1461, q - 1461, 2687, q - 2687,
			939, q - 939, 2308, q - 2308, 2437, q - 2437, 2388, q - 2388,
			733, q - 733, 2337, q - 2337, 268, q - 268, 641, q - 641,
			1584, q - 1584, 2298, q - 2298, 2037, q - 2037, 3220, q - 3220,
			375, q - 375, 2549, q - 2549, 2090, q - 2090, 1645, q - 1645,
			1063, q - 1063, 319, q - 319, 2773, q - 2773, 757, q - 757,
			2099, q - 2099, 561, q - 561, 2466, q - 2466, 2594, q - 2594,
			2804, q - 2804, 1092, q - 1092, 403, q - 403, 1026, q - 1026,
			1143, q - 1143, 2150, q - 2150, 2775, q - 2775, 886, q - 886,
			1722, q - 1722, 1212, q - 1212, 1874, q - 1874, 1029, q - 1029,
			2110, q - 2110, 2935, q - 2935, 885, q - 885, 2154, q - 2154
		};

		private const int n = 256;
		private const int q = 3329;
		private const int half_q = q >> 1;

		private readonly byte k;
		private readonly int η1;
		private readonly int η2;
		private readonly int dᵤ;
		private readonly int dᵥ;

		/// <summary>
		/// Implements the ML-KEM algorithm for post-quantum cryptography, as defined in
		/// NIST FIPS 203: https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.203.pdf
		/// </summary>
		/// <param name="k">Module dimension 𝑘 ∈ {2,3,4}</param>
		public ML_KEM(byte k, int η1, int η2, int dᵤ, int dᵥ)
		{
			this.k = k;
			this.η1 = η1;
			this.η2 = η2;
			this.dᵤ = dᵤ;
			this.dᵥ = dᵥ;
		}

		/// <summary>
		/// Pseudorandom function (PRF), as defined in §4.1.
		/// </summary>
		/// <param name="Seed">32-byte input.</param>
		/// <param name="Data">1-byte input.</param>
		/// <param name="η">2 or 3</param>
		/// <returns>Pseudo-random output.</returns>
		public static byte[] PRF(byte[] Seed, byte Data, int η)
		{
			SHAKE256 HashFunction = new SHAKE256(8 * 64 * η);
			int c = Seed.Length;
			byte[] Bin = new byte[c + 1];
			Array.Copy(Seed, 0, Bin, 0, c);
			Bin[c] = Data;
			return HashFunction.ComputeVariable(Bin);
		}

		/// <summary>
		/// Hash function H, as defined in §4.1.
		/// </summary>
		/// <param name="Data">Input data.</param>
		/// <returns>Hash digest.</returns>
		public static byte[] H(byte[] Data)
		{
			SHA3_256 HashFunction = new SHA3_256();
			return HashFunction.ComputeVariable(Data);
		}

		/// <summary>
		/// Hash function J, as defined in §4.1.
		/// </summary>
		/// <param name="Data">Input data.</param>
		/// <returns>Hash digest.</returns>
		public static byte[] J(byte[] Data)
		{
			SHAKE256 HashFunction = new SHAKE256(n);
			return HashFunction.ComputeVariable(Data);
		}

		/// <summary>
		/// Hash function G, as defined in §4.1.
		/// </summary>
		/// <param name="Data">Input data.</param>
		/// <returns>Hash digest.</returns>
		public static byte[] G(byte[] Data)
		{
			SHA3_512 HashFunction = new SHA3_512();
			return HashFunction.ComputeVariable(Data);
		}

		/// <summary>
		/// eXtendable-Output Function (XOF), as defined in §4.1.
		/// </summary>
		/// <param name="Input">Input data.</param>
		/// <param name="OutputLength">Number of bytes requested for the output data.</param>
		/// <returns>Output data.</returns>
		public static byte[] XOF(byte[] Input, int OutputLength)
		{
			SHAKE128 HashFunction = new SHAKE128(OutputLength << 3);
			return HashFunction.ComputeVariable(Input);
		}

		/// <summary>
		/// Compress function, as defined in §4.2.1.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="d">Number of bits between 1 and 12.</param>
		/// <returns>Compressed value.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static ushort Compress(ushort Value, int d)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			uint i = Value;
			i <<= d;
			i += half_q;
			i /= q;
			return (ushort)(i & bitMask[d]);
		}

		/// <summary>
		/// Decompress function, as defined in §4.2.1.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="d">Number of bits between 1 and 12.</param>
		/// <returns>Decompressed value.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static ushort Decompress(ushort Value, int d)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			uint i = Value;
			i *= q;
			i >>= d - 1;
			i++;
			i >>= 1;
			return (ushort)i;
		}

		/// <summary>
		/// Encodes an array of integers (mod 2^d) into a byte array, as defined by
		/// Algorithm 5 in §4.2.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="d">Number of bits, between 1 and 12.</param>
		/// <returns>Byte array.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static byte[] ByteEncode(ushort[] Values, int d)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			int i, c = Values.Length;
			int NrBits = c * d;
			int NrBytes = (NrBits + 7) >> 3;
			byte[] Result = new byte[NrBytes];
			int Pos = 0;
			int BitOffset = 0;

			for (i = 0; i < c; i++)
			{
				ushort Value = Values[i];

				Value &= bitMask[d];
				Result[Pos] |= (byte)(Value << BitOffset);
				BitOffset += d;
				if (BitOffset >= 8)
				{
					Pos++;
					BitOffset -= 8;
					Result[Pos] = (byte)(Value >> (d - BitOffset));
				}
			}

			return Result;
		}

		/// <summary>
		/// Decodes an array of integers (mod 2^d) from a byte array, as defined by
		/// Algorithm 6 in §4.2.1.
		/// </summary>
		/// <param name="Data">Array of bytes.</param>
		/// <param name="d">Number of bits, between 1 and 12.</param>
		/// <returns>Integer array.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static ushort[] ByteDecode(byte[] Data, int d)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			int i, c = Data.Length;
			int NrBits = c << 3;
			int NrIntegers = (NrBits + d - 1) / d;
			ushort[] Result = new ushort[NrIntegers];
			int Pos = 0;
			int BitOffset = 0;
			int BitsLeft;
			int BitsToUse;

			for (i = 0; i < c; i++)
			{
				byte b = Data[i];
				BitsLeft = 8;

				while (BitsLeft > 0)
				{
					BitsToUse = d - BitOffset;
					if (BitsToUse > BitsLeft)
						BitsToUse = BitsLeft;

					Result[Pos] |= (ushort)((b & bitMask[BitsToUse]) << BitOffset);
					BitOffset += BitsToUse;
					BitsLeft -= BitsToUse;
					b >>= BitsToUse;
				}
			}

			return Result;
		}

		/// <summary>
		/// The algorithm SampleNTT (Algorithm 7, §4.2.1) converts a seed together with two 
		/// indexing bytes into a polynomial in the NTT domain. If the seed is uniformly 
		/// random, the resulting polynomial will be drawn from a distribution that is 
		/// computationally indistinguishable from the uniform distribution 𝑇𝑞.
		/// </summary>
		/// <param name="Index1">Byte index value 1.</param>
		/// <param name="Index2">Byte index value 2.</param>
		/// <returns>Sample in 𝑇𝑞</returns>
		public static ushort[] SampleNTT(byte Index1, byte Index2)
		{
			byte[] Seed = new byte[32];

			lock (rnd)
			{
				rnd.GetBytes(Seed);
			}

			return SampleNTT(Seed, Index1, Index2);
		}

		/// <summary>
		/// The algorithm SampleNTT (Algorithm 7, §4.2.1) converts a seed together with two 
		/// indexing bytes into a polynomial in the NTT domain. If the seed is uniformly 
		/// random, the resulting polynomial will be drawn from a distribution that is 
		/// computationally indistinguishable from the uniform distribution 𝑇𝑞.
		/// </summary>
		/// <param name="Seed">Seed value</param>
		/// <param name="Index1">Byte index value 1.</param>
		/// <param name="Index2">Byte index value 2.</param>
		/// <returns>Sample in 𝑇𝑞</returns>
		public static ushort[] SampleNTT(byte[] Seed, byte Index1, byte Index2)
		{
			SHAKE128 HashFunction = new SHAKE128(0);
			int c = Seed.Length;
			byte[] B = new byte[c + 2];
			Array.Copy(Seed, 0, B, 0, c);
			B[c] = Index1;
			B[c + 1] = Index2;

			Keccak1600.Context Context = HashFunction.Absorb(B);
			ushort[] Result = new ushort[n];
			int Pos = 0;

			while (Pos < n)
			{
				byte[] C = Context.Squeeze(3);
				ushort d1 = (ushort)(C[0] + ((C[1] & 15) << 8));
				ushort d2 = (ushort)((C[1] >> 4) + (C[2] << 4));

				if (d1 < q)
					Result[Pos++] = d1;

				if (d2 < q && Pos < n)
					Result[Pos++] = d2;
			}

			return Result;
		}

		/// <summary>
		/// The algorithm SamplePolyCBD (Algorithm 8, §4.2.1) samples the coefficient array 
		/// of a polynomial 𝑓 ∈ 𝑅𝑞 according to the distribution D𝜂(𝑅𝑞), provided that
		/// its input is a stream of uniformly random bytes.
		/// </summary>
		/// <param name="η">Can be 2 or 3.</param>
		/// <returns>Sample polynomial</returns>
		public static ushort[] SamplePolyCBD(int η)
		{
			if (η < 2 || η > 3)
				throw new ArgumentOutOfRangeException(nameof(η), "η must be either 2 or 3.");

			byte[] Seed = new byte[η << 6];

			lock (rnd)
			{
				rnd.GetBytes(Seed);
			}

			return SamplePolyCBD(Seed);
		}

		/// <summary>
		/// The algorithm SamplePolyCBD (Algorithm 8, §4.2.1) samples the coefficient array 
		/// of a polynomial 𝑓 ∈ 𝑅𝑞 according to the distribution D𝜂(𝑅𝑞), provided that
		/// its input is a stream of uniformly random bytes.
		/// </summary>
		/// <param name="Seed">128 (𝜂=2) or 192 (𝜂=3) bytes seed value.</param>
		/// <returns>Sample polynomial</returns>
		public static ushort[] SamplePolyCBD(byte[] Seed)
		{
			int c = Seed.Length;

			if (c != 128 && c != 192)
				throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be either 128 or 192 bytes.");

			int η = c >> 6;
			int i, j, k;
			ushort x, y;
			ushort[] Result = new ushort[n];

			for (i = 0; i < n; i++)
			{
				for (j = x = y = 0; j < η; j++)
				{
					k = 2 * i * η + j;
					if ((Seed[k >> 3] & (1 << (k & 7))) != 0)
						x++;

					k += η;
					if ((Seed[k >> 3] & (1 << (k & 7))) != 0)
						y++;
				}

				if (x < y)
					Result[i] = (ushort)(q + x - y);
				else
					Result[i] = (ushort)(x - y);
			}

			return Result;
		}

		/// <summary>
		/// Computes the NTT representation f̂ of the given polynomial f ∈ 𝑅𝑞.
		/// (Algorithm 9 in §4.3)
		/// </summary>
		/// <param name="f">Polynomial in 𝑅𝑞</param>
		public static void NTT(ushort[] f)
		{
			if (f.Length != n)
				throw new ArgumentException("Polynomial must have " + n + " coefficients.", nameof(f));

			int i = 1;
			int j;
			int Len;
			int Start;
			ushort ζ;
			ushort t;

			for (Len = n >> 1; Len >= 2; Len >>= 1)
			{
				for (Start = 0; Start < n; Start += Len << 1)
				{
					ζ = nttTransformZeta[i++];

					for (j = Start; j < Start + Len; j++)
					{
						t = (ushort)(ζ * f[j + Len] % q);
						f[j + Len] = (ushort)((f[j] + q - t) % q);
						f[j] = (ushort)((f[j] + t) % q);
					}
				}
			}
		}

		/// <summary>
		/// Canonical extension of <see cref="NTT(ushort[])"/>.
		/// </summary>
		/// <param name="f">Array of polynomials in 𝑅𝑞</param>
		public static void NTT(ushort[][] f)
		{
			int i, c = f.Length;

			for (i = 0; i < c; i++)
				NTT(f[i]);
		}

		/// <summary>
		/// Computes the NTT^-1 representation f of the given polynomial f̂ ∈ 𝑇𝑞.
		/// (Algorithm 10 in §4.3)
		/// </summary>
		/// <param name="f">polynomial f̂ ∈ 𝑇𝑞</param>
		public static void InverseNTT(ushort[] f)
		{
			if (f.Length != n)
				throw new ArgumentException("Polynomial must have " + n + " coefficients.", nameof(f));

			int i = 127;
			int j;
			int Len;
			int Start;
			ushort ζ;
			ushort t;

			for (Len = 2; Len <= 128; Len <<= 1)
			{
				for (Start = 0; Start < n; Start += Len << 1)
				{
					ζ = nttTransformZetaRoots[i--];

					for (j = Start; j < Start + Len; j++)
					{
						t = f[j];
						f[j] = (ushort)((t + f[j + Len]) % q);
						f[j + Len] = (ushort)(ζ * (f[j + Len] + q - t) % q);
					}
				}
			}

			for (i = 0; i < n; i++)
				f[i] = (ushort)(3303 * f[i] % q);   // 3303 = 128^-1 mod q
		}

		/// <summary>
		/// Canonical extension of <see cref="NTT(ushort[])"/>.
		/// </summary>
		/// <param name="f">Array of polynomials f̂ ∈ 𝑇𝑞</param>
		public static void InverseNTT(ushort[][] f)
		{
			int i, c = f.Length;

			for (i = 0; i < c; i++)
				InverseNTT(f[i]);
		}

		/// <summary>
		/// Computes the product (in the ring 𝑇𝑞) of two NTT representations.
		/// (Algorithm 11 in §4.3.1)
		/// </summary>
		/// <param name="f">Polynomial 1</param>
		/// <param name="g">Polynomial 2</param>
		/// <returns>f*g in 𝑇𝑞</returns>
		/// <exception cref="ArgumentException">If polynomials are not of the correct size.</exception>
		public static ushort[] MultiplyNTTs(ushort[] f, ushort[] g)
		{
			ushort[] Result = new ushort[n];
			MultiplyNTTsAndAdd(f, g, Result);
			return Result;
		}

		/// <summary>
		/// Computes the product (in the ring 𝑇𝑞) of two NTT representations 
		/// (Algorithm 11 in §4.3.1) and adds the result to a result vector.
		/// </summary>
		/// <param name="f">Polynomial 1</param>
		/// <param name="g">Polynomial 2</param>
		/// <param name="Result">Result vector.</param>
		/// <returns>f*g in 𝑇𝑞</returns>
		/// <exception cref="ArgumentException">If polynomials are not of the correct size.</exception>
		public static void MultiplyNTTsAndAdd(ushort[] f, ushort[] g, ushort[] Result)
		{
			if (f.Length != n || g.Length != n)
				throw new ArgumentException("Polynomials must have " + n + " coefficients.", nameof(f));

			int i;
			ushort a0, a1, b0, b1, γ;

			for (i = 0; i < n; i += 2)
			{
				a0 = f[i];
				a1 = f[i + 1];
				b0 = g[i];
				b1 = g[i + 1];
				γ = nttTransformZetaRoots[i >> 1];

				Result[i] = (ushort)((Result[i] + a0 * b0 + γ * a1 * b1) % q);
				Result[i + 1] = (ushort)((Result[i + 1] + a0 * b1 + a1 * b0) % q);
			}
		}

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 13 K-PKE.KeyGen(𝑑) in §5.1)
		/// </summary>
		public KeyValuePair<byte[], byte[]> KeyGen()
		{
			byte[] Seed = new byte[32];

			lock (rnd)
			{
				rnd.GetBytes(Seed);
			}

			return this.KeyGen(Seed);
		}

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 13 K-PKE.KeyGen(𝑑) in §5.1)
		/// </summary>
		/// <param name="Seed">Randomness</param>
		/// <returns>Public Encryption Key (Key) and Private Decryption Key (Value).</returns>
		public KeyValuePair<byte[], byte[]> KeyGen(byte[] Seed)
		{
			if (Seed.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(Seed));

			byte[] Bin = new byte[33];
			Array.Copy(Seed, 0, Bin, 0, 32);
			Bin[32] = this.k;
			Bin = G(Bin);

			byte[] ρ = new byte[32];
			Array.Copy(Bin, 0, ρ, 0, 32);

			byte[] σ = new byte[32];
			Array.Copy(Bin, 32, σ, 0, 32);
			Clear(Bin);

			byte N = 0;
			int i, j;
			ushort[,][] A = new ushort[this.k, this.k][];
			ushort[][] s = new ushort[this.k][];
			ushort[][] e = new ushort[this.k][];

			for (i = 0; i < this.k; i++)
			{
				for (j = 0; j < this.k; j++)
					A[i, j] = SampleNTT(ρ, (byte)j, (byte)i);
			}

			for (i = 0; i < this.k; i++)
				s[i] = SamplePolyCBD(PRF(σ, N++, this.η1));

			for (i = 0; i < this.k; i++)
				e[i] = SamplePolyCBD(PRF(σ, N++, this.η1));

			NTT(s);
			NTT(e);

			ushort[][] t = new ushort[this.k][];

			for (i = 0; i < this.k; i++)
			{
				t[i] = (ushort[])e[i].Clone();

				for (j = 0; j < this.k; j++)
					MultiplyNTTsAndAdd(A[i, j], s[j], t[i]);
			}

			byte[] EncryptionKey = new byte[32 + 384 * this.k];
			byte[] DecryptionKey = new byte[384 * this.k];
			int Pos = 0;

			for (i = 0; i < this.k; i++)
			{
				Bin = ByteEncode(t[i], 12);
				Array.Copy(Bin, 0, EncryptionKey, Pos, 384);
				Clear(Bin);

				Bin = ByteEncode(s[i], 12);
				Array.Copy(Bin, 0, DecryptionKey, Pos, 384);
				Clear(Bin);

				Pos += 384;
			}

			Array.Copy(ρ, 0, EncryptionKey, Pos, 32);

			Clear(s);
			Clear(t);
			Clear(e);
			Clear(A);

			return new KeyValuePair<byte[], byte[]>(EncryptionKey, DecryptionKey);
		}

		private static void Clear(byte[] Bin)
		{
			Array.Clear(Bin, 0, Bin.Length);
		}

		private static void Clear(ushort[] f)
		{
			Array.Clear(f, 0, f.Length);
		}

		private static void Clear(ushort[][] v)
		{
			int i, c = v.Length;

			for (i = 0; i < c; i++)
				Clear(v[i]);
		}

		private static void Clear(ushort[,][] A)
		{
			int i, j, c = A.GetLength(0), d = A.GetLength(1);

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < d; j++)
					Clear(A[i, j]);
			}
		}

	}
}
