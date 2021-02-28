using System;
using System.Collections.Generic;

namespace Waher.Content.QR.Encoding
{
	/// <summary>
	/// Class representing arithmentic over the Galois Field GF(256) ~
	/// ~ Z2[x]/P(x) over some irreducible polinomial in Z2[x] of order 8.
	/// This implementation uses P(x)=x^8+x^4+x^3+x^2+1~285, and
	/// minimum privitive element generator x~2.
	/// 
	/// Elements x = b0b1b2b3b4b5b6b7 ~ b0+b1x+b2x^2+...+b7x^7 represented
	/// as bytes.
	/// </summary>
	public static class GF256
	{
		private readonly static byte[] powersOfTwo;
		private readonly static byte[] log2;

		static GF256()
		{
			SortedDictionary<byte, byte> Inv = new SortedDictionary<byte, byte>();
			int i, j;

			powersOfTwo = new byte[256];

			for (i = 0, j = 1; i < 256; i++)
			{
				powersOfTwo[i] = (byte)j;

				if (i != 255)
					Inv[(byte)j] = (byte)i;

				j <<= 1;
				if (j >= 256)
					j ^= 285;    // equal to j-=poly = rest of j/poly as polynomial division
			}

			log2 = new byte[256];
			Inv.Values.CopyTo(log2, 1);
		}

		/// <summary>
		/// Power of 2 table.
		/// </summary>
		public static byte[] PowerOf2Table => powersOfTwo;

		/// <summary>
		/// Log2 table.
		/// </summary>
		public static byte[] Log2Table => log2;

		/// <summary>
		/// Adds two numbers
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <returns>x+y</returns>
		public static byte Add(byte x, byte y) => (byte)(x ^ y);

		/// <summary>
		/// Subtracts one number from another
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <returns>x-y</returns>
		public static byte Subtract(byte x, byte y) => (byte)(x ^ y);

		/// <summary>
		/// Multiplies two numbers
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <returns>x*y</returns>
		public static byte Multiply(byte x, byte y) => powersOfTwo[(log2[x] + log2[y]) % 255];
	}
}
