using System;
using System.Security.Cryptography;

namespace Waher.Security.PQC
{
	/// <summary>
	/// Methods common to ML algorithms.
	/// </summary>
	public abstract class ML_Common
	{
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		/// <summary>
		/// Length of the public key.
		/// </summary>
		public abstract int PublicKeyLength { get; }

		/// <summary>
		/// Length of the private key.
		/// </summary>
		public abstract int PrivateKeyLength { get; }

		/// <summary>
		/// Creates a random seed value.
		/// </summary>
		/// <param name="NrBytes">Number of random bytes to create.</param>
		/// <returns>Random seed value.</returns>
		protected static byte[] CreateSeed(int NrBytes)
		{
			byte[] Seed = new byte[NrBytes];

			lock (rnd)
			{
				rnd.GetBytes(Seed);
			}

			return Seed;
		}

		/// <summary>
		/// Creates a random seed value.
		/// </summary>
		/// <returns>Random 32-byte seed value.</returns>
		protected static byte[] CreateSeed()
		{
			return CreateSeed(32);
		}

		/// <summary>
		/// Clears a byte array.
		/// </summary>
		/// <param name="Bin">Byte array</param>
		protected static void Clear(byte[] Bin)
		{
			Array.Clear(Bin, 0, Bin.Length);
		}

		/// <summary>
		/// Clears a polynomial coefficient vector.
		/// </summary>
		/// <param name="f">Polynomial</param>
		protected static void Clear<T>(T[] f)
		{
			Array.Clear(f, 0, f.Length);
		}

		/// <summary>
		/// Clears a vector of polynomials.
		/// </summary>
		/// <param name="v">Vector of polynomials.</param>
		protected static void Clear<T>(T[][] v)
		{
			int i, c = v.Length;

			for (i = 0; i < c; i++)
				Clear(v[i]);
		}

		/// <summary>
		/// Bit masks corresponding to mod 2^d arithmetic, where d is the index of 
		/// the mask in the array.
		/// </summary>
		protected static readonly ushort[] ushortBitMask =
		{
			0x0000, 0x0001, 0x0003, 0x0007, 0x000F, 0x001F, 0x003F, 0x007F,
			0x00FF, 0x01FF, 0x03FF, 0x07FF, 0x0FFF, 0x1FFF, 0x3FFF, 0x7FFF,
			0xFFFF
		};

		/// <summary>
		/// Bit masks corresponding to mod 2^d arithmetic, where d is the index of 
		/// the mask in the array.
		/// </summary>
		protected static readonly int[] intBitMask =
		{
			0x00000000, 0x00000001, 0x00000003, 0x00000007, 
			0x0000000F, 0x0000001F, 0x0000003F, 0x0000007F,
			0x000000FF, 0x000001FF, 0x000003FF, 0x000007FF,
			0x00000FFF, 0x00001FFF, 0x00003FFF, 0x00007FFF,
			0x0000FFFF, 0x0001FFFF, 0x0003FFFF, 0x0007FFFF,
			0x000FFFFF, 0x001FFFFF, 0x003FFFFF, 0x007FFFFF,
			0x00FFFFFF, 0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF,
			0x0FFFFFFF, 0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF
		};

	}
}
