using System;
using System.Security.Cryptography;
using Waher.Security.SHA3;

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
		protected static void Clear(ushort[] f)
		{
			Array.Clear(f, 0, f.Length);
		}

		/// <summary>
		/// Clears a vector of polynomials.
		/// </summary>
		/// <param name="v">Vector of polynomials.</param>
		protected static void Clear(ushort[][] v)
		{
			int i, c = v.Length;

			for (i = 0; i < c; i++)
				Clear(v[i]);
		}

	}
}
