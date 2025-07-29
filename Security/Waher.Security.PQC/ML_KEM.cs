using System;
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
		/// <summary>
		/// Model parameters for a required RBG strength 128 (cryptographic security strength),
		/// as defined in §8.
		/// </summary>
		public static readonly ML_KEM ML_KEM_512 = new ML_KEM(2, 3, 2, 10, 4);

		/// <summary>
		/// Model parameters for a required RBG strength 192 (cryptographic security strength),
		/// as defined in §8.
		/// </summary>
		public static readonly ML_KEM ML_KEM_768 = new ML_KEM(3, 2, 2, 10, 4);

		/// <summary>
		/// Model parameters for a required RBG strength 256 (cryptographic security strength),
		/// as defined in §8.
		/// </summary>
		public static readonly ML_KEM ML_KEM_1024 = new ML_KEM(4, 2, 2, 11, 5);

		/// <summary>
		/// Gets a model by name, as defined in §8.
		/// </summary>
		/// <param name="Name">Name of model.</param>
		/// <returns>Reference to model.</returns>
		/// <exception cref="ArgumentException">Model name not recognized.</exception>
		public static ML_KEM GetModel(string Name)
		{
			switch (Name.ToUpper())
			{
				case "ML-KEM-512":
					return ML_KEM_512;

				case "ML-KEM-768":
					return ML_KEM_768;

				case "ML-KEM-1024":
					return ML_KEM_1024;

				default:
					throw new ArgumentException("Unknown model name: " + Name, nameof(Name));
			}
		}

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
		private static readonly ushort[] nttTransformZeta2 =
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
		/// <param name="k">Moduel dimension 𝑘 ∈ {2,3,4}</param>
		/// <param name="η1">Specifies the distribution for generating the vectors s and s.</param>
		/// <param name="η2">Specifies the distribution for generating the vectors 𝐞1 and e2.</param>
		/// <param name="dᵤ">Number of bits used to compress the vector u.</param>
		/// <param name="dᵥ">Number of bits used to compress the vector v.</param>
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
		/// Canonical extension of Compress function, as defined in §4.2.1.
		/// </summary>
		/// <param name="Value">Vector to be compressed.</param>
		/// <param name="d">Number of bits between 1 and 12.</param>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static void Compress(ushort[] Value, int d)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			int j, c = Value.Length;
			uint i;

			for (j = 0; j < c; j++)
			{
				i = Value[j];
				i <<= d;
				i += half_q;
				i /= q;
				Value[j] = (ushort)i;
			}
		}

		/// <summary>
		/// Canonical extension of Compress function, as defined in §4.2.1.
		/// </summary>
		/// <param name="Value">Array of vectors to be compressed.</param>
		/// <param name="d">Number of bits between 1 and 12.</param>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static void Compress(ushort[][] Value, int d)
		{
			int i, c = Value.Length;

			for (i = 0; i < c; i++)
				Compress(Value[i], d);
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
		/// Canonical extension of Decompress function, as defined in §4.2.1.
		/// </summary>
		/// <param name="Value">Vector to be decompressed.</param>
		/// <param name="d">Number of bits between 1 and 12.</param>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static void Decompress(ushort[] Value, int d)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			int j, c = Value.Length;
			uint i;

			for (j = 0; j < c; j++)
			{
				i = Value[j];
				i *= q;
				i >>= d - 1;
				i++;
				i >>= 1;
				Value[j] = (ushort)i;
			}
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
			int c = Values.Length;
			int NrBits = c * d;
			int NrBytes = (NrBits + 7) >> 3;
			byte[] Result = new byte[NrBytes];

			ByteEncode(Values, d, Result, 0);

			return Result;
		}

		/// <summary>
		/// Encodes an array of integers (mod 2^d) into a byte array, as defined by
		/// Algorithm 5 in §4.2.1.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="d">Number of bits, between 1 and 12.</param>
		/// <param name="Output">Bytes will be encoded into this array.</param>
		/// <param name="Index">Index into output array where encoding will begin.</param>
		/// <returns>Number of bytes encoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int ByteEncode(ushort[] Values, int d, byte[] Output, int Index)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			int i, c = Values.Length;
			int BitOffset = 0;
			int Index0 = Index;

			for (i = 0; i < c; i++)
			{
				ushort Value = Values[i];

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
						}
					}
				}
			}

			return Index - Index0;
		}

		/// <summary>
		/// Encodes an array of vectors of integers (mod 2^d) into a byte array. Canonical
		/// extension of <see cref="ByteEncode(ushort[], int, byte[], int)"/>.
		/// </summary>
		/// <param name="Values">Array of integers.</param>
		/// <param name="d">Number of bits, between 1 and 12.</param>
		/// <param name="Output">Bytes will be encoded into this array.</param>
		/// <param name="Index">Index into output array where encoding will begin.</param>
		/// <returns>Number of bytes encoded.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static int ByteEncode(ushort[][] Values, int d, byte[] Output, int Index)
		{
			int Pos = Index;
			int i, c = Values.Length;

			for (i = 0; i < c; i++)
			{
				int NrBytes = ByteEncode(Values[i], d, Output, Pos);
				Pos += NrBytes;
			}

			return Pos - Index;
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
			return ByteDecode(Data, 0, Data.Length, d);
		}

		/// <summary>
		/// Decodes an array of integers (mod 2^d) from a byte array, as defined by
		/// Algorithm 6 in §4.2.1.
		/// </summary>
		/// <param name="Data">Array of bytes.</param>
		/// <param name="Offset">Start offset into byte array to decode.</param>
		/// <param name="Length">Number of bytes to decode.</param>
		/// <param name="d">Number of bits, between 1 and 12.</param>
		/// <returns>Integer array.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If d lies outside of the valid range.</exception>
		public static ushort[] ByteDecode(byte[] Data, int Offset, int Length, int d)
		{
			if (d < 1 || d > 12)
				throw new ArgumentOutOfRangeException(nameof(d), "d must be between 1 and 12.");

			int NrBits = Length << 3;
			int NrIntegers = (NrBits + d - 1) / d;
			ushort[] Result = new ushort[NrIntegers];
			int Pos = 0;
			int BitOffset = 0;
			int BitsLeft;
			int BitsToUse;

			while (Length-- > 0)
			{
				byte b = Data[Offset++];
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

					if (BitOffset >= d)
					{
						Pos++;
						BitOffset = 0;
					}
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
			return SampleNTT(CreateSeed(), Index1, Index2);
		}

		/// <summary>
		/// Creates a random seed value.
		/// </summary>
		/// <returns>Random 32-byte seed value.</returns>
		private static byte[] CreateSeed()
		{
			return CreateSeed(32);
		}

		/// <summary>
		/// Creates a random seed value.
		/// </summary>
		/// <param name="NrBytes">Number of random bytes to create.</param>
		/// <returns>Random seed value.</returns>
		private static byte[] CreateSeed(int NrBytes)
		{
			byte[] Seed = new byte[NrBytes];

			lock (rnd)
			{
				rnd.GetBytes(Seed);
			}

			return Seed;
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

			return SamplePolyCBD(CreateSeed(η << 6));
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
			ushort dx, dy;
			ushort[] Result = new ushort[n];

			for (i = 0; i < n; i++)
			{
				for (j = x = y = 0; j < η; j++)
				{
					k = 2 * i * η + j;
					dx = (Seed[k >> 3] & (1 << (k & 7))) != 0 ? (ushort)1 : (ushort)0;
					x += dx;    // To avoid different CPU instructions to execute based on if bit is 0 or 1.

					k += η;
					dy = (Seed[k >> 3] & (1 << (k & 7))) != 0 ? (ushort)1 : (ushort)0;
					y += dy;    // To avoid different CPU instructions to execute based on if bit is 0 or 1.
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
			int Len2;
			int StartLen;
			int Start;
			ushort ζ;
			ushort t;

			for (Len = 2; Len <= 128; Len <<= 1)
			{
				Len2 = Len << 1;

				for (Start = 0; Start < n; Start += Len2)
				{
					ζ = nttTransformZeta[i--];

					StartLen = Start + Len;
					for (j = Start; j < StartLen; j++)
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
				γ = nttTransformZeta2[i >> 1];

				Result[i] = (ushort)((Result[i] + a0 * b0 + γ * a1 % q * b1) % q);  // Three multiplications without modulus can generate integer overflow.
				Result[i + 1] = (ushort)((Result[i + 1] + a0 * b1 + a1 * b0) % q);
			}
		}

		/// <summary>
		/// Adds <paramref name="g"/> to <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Polynomial that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Polynomial to add to <paramref name="f"/>.</param>
		public static void AddTo(ushort[] f, ushort[] g)
		{
			int i;

			for (i = 0; i < n; i++)
				f[i] = (ushort)((f[i] + g[i]) % q);
		}

		/// <summary>
		/// Adds vector <paramref name="g"/> to vector <paramref name="f"/>.
		/// </summary>
		/// <param name="f">Vector of polynomials that will be incremented by <paramref name="g"/>.</param>
		/// <param name="g">Vector of polynomials to add to <paramref name="f"/>.</param>
		public static void AddTo(ushort[][] f, ushort[][] g)
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
		public static void Negate(ushort[] f)
		{
			int i, c = f.Length;

			for (i = 0; i < c; i++)
				f[i] = (ushort)((q - f[i]) % q);   // To avoid different CPU instructions to execute based on if bit is 0 or 1.
		}

		/// <summary>
		/// Computes the dot product of two vectors of polynomials in 𝑇𝑞.
		/// </summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Dot product in 𝑇𝑞</returns>
		/// <exception cref="ArgumentException"></exception>
		public static ushort[] DotProductNTT(ushort[][] v1, ushort[][] v2)
		{
			int i, c = v1.Length;

			if (v2.Length != c)
				throw new ArgumentException("Vectors must have the same number of polynomials.", nameof(v2));

			ushort[] Result = new ushort[n];

			for (i = 0; i < c; i++)
				MultiplyNTTsAndAdd(v1[i], v2[i], Result);

			return Result;
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

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 13 K-PKE.KeyGen(𝑑) in §5.1)
		/// </summary>
		/// <returns>Public Encryption Key (384k+32 bytes) and Private Decryption Key 
		/// (384k bytes). Matrix used to calculate public key is also provided.</returns>
		public K_PKE_Keys K_PKE_KeyGen()
		{
			return this.K_PKE_KeyGen(CreateSeed(), true, false, false);
		}

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 13 K-PKE.KeyGen(𝑑) in §5.1)
		/// </summary>
		/// <param name="Seed">Randomness (32 bytes)</param>
		/// <returns>Public Encryption Key (384k+32 bytes) and Private Decryption Key 
		/// (384k bytes). Matrix used to calculate public key is also provided.</returns>
		public K_PKE_Keys K_PKE_KeyGen(byte[] Seed)
		{
			return this.K_PKE_KeyGen(Seed, true, false, false);
		}

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 13 K-PKE.KeyGen(𝑑) in §5.1)
		/// </summary>
		/// <param name="Seed">Randomness (32 bytes)</param>
		/// <param name="ConcatenateModelParameter">A common error in ML-KEM implementations
		/// is that the model parameter is not concatenated to the seed before calling G(x)
		/// in Algorithm 13. To interoperate with such systems, using seed as initial key,
		/// you need to turn this part of the algorithm off. Default is to concatenate it,
		/// in accordance with FIPS 203.</param>
		/// <param name="TransposeA">Another common error in ML-KEM implementations is that
		/// the evaluation of A is transposed compared to FIPS 203. For interoperability with
		/// such systems, this argument needs to be true. It is false by default.</param>
		/// <param name="PreserveIntermediates">If intermediate values should be preserved.
		/// Default is false. For testing and debugging, intermediate values may be required.
		/// By default, they should be cleared and forgotten.</param>
		/// <returns>Public Encryption Key (384k+32 bytes) and Private Decryption Key 
		/// (384k bytes). Matrix used to calculate public key is also provided.</returns>
		public K_PKE_Keys K_PKE_KeyGen(byte[] Seed, bool ConcatenateModelParameter,
			bool TransposeA, bool PreserveIntermediates)
		{
			if (Seed.Length != 32)
				throw new ArgumentException("Seed must be 32 bytes long.", nameof(Seed));

			byte[] Bin;

			if (ConcatenateModelParameter)
			{
				Bin = new byte[33];
				Array.Copy(Seed, 0, Bin, 0, 32);
				Bin[32] = this.k;
				Bin = G(Bin);
			}
			else
				Bin = G(Seed);

			byte[] ρ = new byte[32];
			Array.Copy(Bin, 0, ρ, 0, 32);

			byte[] σ = new byte[32];
			Array.Copy(Bin, 32, σ, 0, 32);
			Clear(Bin);

			byte N = 0;
			int i, j;
			ushort[,][] Â = new ushort[this.k, this.k][];
			ushort[][] s = new ushort[this.k][];
			ushort[][] e = new ushort[this.k][];
			ushort[][] s2 = PreserveIntermediates ? new ushort[this.k][] : null;
			ushort[][] e2 = PreserveIntermediates ? new ushort[this.k][] : null;

			for (i = 0; i < this.k; i++)
			{
				for (j = 0; j < this.k; j++)
				{
					if (TransposeA)
						Â[i, j] = SampleNTT(ρ, (byte)i, (byte)j);
					else
						Â[i, j] = SampleNTT(ρ, (byte)j, (byte)i);
				}
			}

			for (i = 0; i < this.k; i++)
			{
				s[i] = SamplePolyCBD(PRF(σ, N++, this.η1));

				if (PreserveIntermediates)
					s2[i] = (ushort[])s[i].Clone();
			}

			for (i = 0; i < this.k; i++)
			{
				e[i] = SamplePolyCBD(PRF(σ, N++, this.η1));

				if (PreserveIntermediates)
					e2[i] = (ushort[])e[i].Clone();
			}

			NTT(s);
			NTT(e);

			ushort[][] t = new ushort[this.k][];

			for (i = 0; i < this.k; i++)
			{
				t[i] = (ushort[])e[i].Clone();

				for (j = 0; j < this.k; j++)
					MultiplyNTTsAndAdd(Â[i, j], s[j], t[i]);
			}

			byte[] EncryptionKey = new byte[32 + 384 * this.k];
			byte[] DecryptionKey = new byte[384 * this.k];
			int Pos = 0;

			for (i = 0; i < this.k; i++)
			{
				ByteEncode(t[i], 12, EncryptionKey, Pos);
				ByteEncode(s[i], 12, DecryptionKey, Pos);
				Pos += 384;
			}

			Array.Copy(ρ, 0, EncryptionKey, Pos, 32);

			if (PreserveIntermediates)
				return new K_PKE_Keys(Â, EncryptionKey, DecryptionKey, ρ, σ, s2, e2, s, e, t);
			else
			{
				Clear(s);
				Clear(t);
				Clear(e);

				return new K_PKE_Keys(Â, EncryptionKey, DecryptionKey, ρ, σ);
			}
		}

		/// <summary>
		/// Uses the encryption key to encrypt a plaintext message using the given randomness
		/// and algorithm 14 (K-PKE.Encrypt) of §5.2.
		/// </summary>
		/// <param name="EncryptionKey">Encryption key (384k+32 bytes)</param>
		/// <param name="Message">Plain text message (32 bytes)</param>
		/// <returns>Ciphertext (32*(dᵤk+dᵥ) bytes)</returns>
		public byte[] K_PKE_Encrypt(byte[] EncryptionKey, byte[] Message)
		{
			return this.K_PKE_Encrypt(EncryptionKey, Message, CreateSeed(), null);
		}

		/// <summary>
		/// Uses the encryption key to encrypt a plaintext message using the given randomness
		/// and algorithm 14 (K-PKE.Encrypt) of §5.2.
		/// </summary>
		/// <param name="EncryptionKey">Encryption key (384k+32 bytes)</param>
		/// <param name="Message">Plain text message (32 bytes)</param>
		/// <param name="A">Optional matrix generating the encryption key.</param>
		/// <returns>Ciphertext (32*(dᵤk+dᵥ) bytes)</returns>
		public byte[] K_PKE_Encrypt(byte[] EncryptionKey, byte[] Message, ushort[,][] A)
		{
			return this.K_PKE_Encrypt(EncryptionKey, Message, CreateSeed(), A);
		}

		/// <summary>
		/// Uses the encryption key to encrypt a plaintext message using the given randomness
		/// and algorithm 14 (K-PKE.Encrypt) of §5.2.
		/// </summary>
		/// <param name="EncryptionKey">Encryption key (384k+32 bytes)</param>
		/// <param name="Message">Plain text message (32 bytes)</param>
		/// <param name="Seed">Randomness (32 bytes)</param>
		/// <returns>Ciphertext (32*(dᵤk+dᵥ) bytes)</returns>
		public byte[] K_PKE_Encrypt(byte[] EncryptionKey, byte[] Message, byte[] Seed)
		{
			return this.K_PKE_Encrypt(EncryptionKey, Message, Seed, null);
		}

		/// <summary>
		/// Uses the encryption key to encrypt a plaintext message using the given randomness
		/// and algorithm 14 (K-PKE.Encrypt) of §5.2.
		/// </summary>
		/// <param name="EncryptionKey">Encryption key (384k+32 bytes)</param>
		/// <param name="Message">Plain text message (32 bytes)</param>
		/// <param name="Seed">Randomness (32 bytes)</param>
		/// <param name="A">Optional matrix generating the encryption key.</param>
		/// <returns>Ciphertext (32*(dᵤk+dᵥ) bytes)</returns>
		public byte[] K_PKE_Encrypt(byte[] EncryptionKey, byte[] Message, byte[] Seed,
			ushort[,][] A)
		{
			if (EncryptionKey is null)
				throw new ArgumentNullException(nameof(EncryptionKey), "Encryption key cannot be null.");

			if (EncryptionKey.Length != 384 * this.k + 32)
				throw new ArgumentException("Encryption key must be 384k+32 bytes long.", nameof(EncryptionKey));

			if (Message.Length != 32)
				throw new ArgumentException("Message must be 32 bytes long.", nameof(Message));

			ushort[][] t = new ushort[this.k][];
			byte[] ρ = new byte[32];
			int Pos;
			int i, j;
			byte N = 0;
			byte k;

			for (i = Pos = 0; i < this.k; i++)
			{
				t[i] = ByteDecode(EncryptionKey, Pos, 384, 12);
				Pos += 384;
			}

			Array.Copy(EncryptionKey, Pos, ρ, 0, 32);

			ushort[][] y = new ushort[this.k][];
			ushort[][] e1 = new ushort[this.k][];
			ushort[] e2;

			if (A is null)
			{
				A = new ushort[this.k, this.k][];

				for (i = 0; i < this.k; i++)
				{
					for (j = 0; j < this.k; j++)
						A[i, j] = SampleNTT(ρ, (byte)j, (byte)i);
				}
			}
			else if (A.GetLength(0) != this.k || A.GetLength(1) != this.k)
				throw new ArgumentException("Matrix A must be " + this.k + "x" + this.k + ".", nameof(A));

			for (i = 0; i < this.k; i++)
				y[i] = SamplePolyCBD(PRF(Seed, N++, this.η1));

			for (i = 0; i < this.k; i++)
				e1[i] = SamplePolyCBD(PRF(Seed, N++, this.η2));

			e2 = SamplePolyCBD(PRF(Seed, N++, this.η2));

			NTT(y);

			ushort[][] u = new ushort[this.k][];

			for (i = 0; i < this.k; i++)
			{
				u[i] = new ushort[n];

				for (j = 0; j < this.k; j++)
					MultiplyNTTsAndAdd(A[j, i], y[j], u[i]);
			}

			InverseNTT(u);
			AddTo(u, e1);

			ushort[] v = DotProductNTT(t, y);
			ushort dv;

			InverseNTT(v);
			AddTo(v, e2);

			for (i = j = 0, k = 1; i < n; i++)
			{
				dv = (Message[j] & k) != 0 ? (ushort)1 : (ushort)0;
				v[i] = (ushort)((v[i] + dv) % q);   // To avoid different CPU instructions to execute based on if bit is 0 or 1.

				k <<= 1;
				if (k == 0)
				{
					k = 1;
					j++;
				}
			}

			int dᵤk32 = this.dᵤ * this.k << 5;
			int dᵥ32 = this.dᵥ << 5;
			byte[] CipherText = new byte[dᵤk32 + dᵥ32];

			Compress(u, this.dᵤ);
			ByteEncode(u, this.dᵤ, CipherText, 0);

			Compress(v, this.dᵥ);
			ByteEncode(v, this.dᵥ, CipherText, dᵤk32);

			return CipherText;
		}

		/// <summary>
		/// Uses the decryption key to decrypt a ciphertext message using
		/// algorithm 15 (K-PKE.Decrypt) of §5.3.
		/// </summary>
		/// <param name="DecryptionKey">Decryption key (384k bytes)</param>
		/// <param name="CipherText">Ciphertext (32*(dᵤk+dᵥ) bytes)</param>
		/// <returns>Clear text message (32 bytes)</returns>
		public byte[] K_PKE_Decrypt(byte[] DecryptionKey, byte[] CipherText)
		{
			if (DecryptionKey is null)
				throw new ArgumentNullException(nameof(DecryptionKey), "Decryption key cannot be null.");

			if (DecryptionKey.Length != 384 * this.k)
				throw new ArgumentException("Decryption key must be 384k bytes long.", nameof(DecryptionKey));

			int dᵤ32 = this.dᵤ << 5;
			int dᵤk32 = dᵤ32 * this.k;
			int dᵥ32 = this.dᵥ << 5;

			if (CipherText.Length != dᵤk32 + dᵥ32)
				throw new ArgumentException("Message must be " + (dᵤk32 + dᵥ32) + " bytes long.", nameof(CipherText));

			ushort[][] u = new ushort[this.k][];
			ushort[] v;
			int Pos;
			int i;

			for (i = Pos = 0; i < this.k; i++)
			{
				u[i] = ByteDecode(CipherText, Pos, dᵤ32, this.dᵤ);
				Decompress(u[i], this.dᵤ);
				Pos += dᵤ32;
			}

			v = ByteDecode(CipherText, Pos, dᵥ32, this.dᵥ);
			Decompress(v, this.dᵥ);

			ushort[][] s = new ushort[this.k][];

			for (i = Pos = 0; i < this.k; i++)
			{
				s[i] = ByteDecode(DecryptionKey, Pos, 384, 12);
				Pos += 384;
			}

			NTT(u);

			ushort[] w = DotProductNTT(s, u);

			InverseNTT(w);
			Negate(w);
			AddTo(w, v);

			Compress(w, 1);
			return ByteEncode(w, 1);
		}

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 16 ML-KEM.KeyGen(𝑑) in §6.1)
		/// </summary>
		/// <returns>Public Encryption Key (384k+32 bytes) and Private Decryption Key 
		/// (384k bytes). Matrix used to calculate public key is also provided.</returns>
		public ML_KEM_Keys KeyGen()
		{
			return this.KeyGen(CreateSeed(), CreateSeed(), true, false, false);
		}

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 13 ML-KEM.KeyGen(𝑑) in §5.1)
		/// </summary>
		/// <param name="Seed">Randomness (32 bytes)</param>
		/// <param name="Seed2">Randomness (32 bytes)</param>
		/// <returns>Public Encryption Key (384k+32 bytes) and Private Decryption Key 
		/// (384k bytes). Matrix used to calculate public key is also provided.</returns>
		public ML_KEM_Keys KeyGen(byte[] Seed, byte[] Seed2)
		{
			return this.KeyGen(Seed, Seed2, true, false, false);
		}

		/// <summary>
		/// Uses randomness to generate an encryption key and a corresponding decryption key. 
		/// (Algorithm 13 ML-KEM.KeyGen(𝑑) in §5.1)
		/// </summary>
		/// <param name="Seed">Randomness (32 bytes)</param>
		/// <param name="Seed2">Randomness (32 bytes)</param>
		/// <param name="ConcatenateModelParameter">A common error in ML-KEM implementations
		/// is that the model parameter is not concatenated to the seed before calling G(x)
		/// in Algorithm 13. To interoperate with such systems, using seed as initial key,
		/// you need to turn this part of the algorithm off. Default is to concatenate it,
		/// in accordance with FIPS 203.</param>
		/// <param name="TransposeA">Another common error in ML-KEM implementations is that
		/// the evaluation of A is transposed compared to FIPS 203. For interoperability with
		/// such systems, this argument needs to be true. It is false by default.</param>
		/// <param name="PreserveIntermediates">If intermediate values should be preserved.
		/// Default is false. For testing and debugging, intermediate values may be required.
		/// By default, they should be cleared and forgotten.</param>
		/// <returns>Public Encryption Key (384k+32 bytes) and Private Decryption Key 
		/// (384k bytes). Matrix used to calculate public key is also provided.</returns>
		public ML_KEM_Keys KeyGen(byte[] Seed, byte[] Seed2, bool ConcatenateModelParameter,
			bool TransposeA, bool PreserveIntermediates)
		{
			if (Seed2.Length != 32)
				throw new ArgumentException("Seed2 must be 32 bytes long.", nameof(Seed2));

			K_PKE_Keys Keys = K_PKE_KeyGen(Seed, ConcatenateModelParameter, TransposeA, PreserveIntermediates);
			byte[] DecryptionKey = new byte[768 * this.k + 96];
			int Pos;

			Array.Copy(Keys.DecryptionKey, 0, DecryptionKey, 0, Pos = 384 * this.k);
			Array.Copy(Keys.EncryptionKey, 0, DecryptionKey, Pos, Pos + 32);
			Pos += Pos + 32;

			byte[] Bin = H(Keys.EncryptionKey);
			Array.Copy(Bin, 0, DecryptionKey, Pos, 32);
			Pos += 32;

			Array.Copy(Seed2, 0, DecryptionKey, Pos, 32);

			return new ML_KEM_Keys(Keys, DecryptionKey);
		}

	}
}
