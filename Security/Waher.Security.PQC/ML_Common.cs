using System;
using System.Security.Cryptography;

namespace Waher.Security.PQC
{
	/// <summary>
	/// Methods common to ML algorithms.
	/// </summary>
	public class ML_Common
	{
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

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
		protected static readonly ushort[] bitMask =
		{
			0x0000, 0x0001, 0x0003, 0x0007, 0x000F, 0x001F, 0x003F, 0x007F,
			0x00FF, 0x01FF, 0x03FF, 0x07FF, 0x0FFF, 0x1FFF, 0x3FFF, 0x7FFF,
			0xFFFF
		};

	}
}
