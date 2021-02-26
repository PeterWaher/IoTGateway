using System;
using System.Collections.Generic;

namespace Waher.Content.QR.Encoding
{
	/// <summary>
	/// Class representing arithmentic over the Galois Field GF(256) ~
	/// ~ Z2[x]/P(x) over some irreducible polinomial in Z2[x] of order 8.
	/// 
	/// Elements x = b0b1b2b3b4b5b6b7 ~ b0+b1x+b2x^2+...+b7x^7 represented
	/// as bytes.
	/// </summary>
	public class GF256
	{
		private readonly byte[] powersOfTwo;
		private readonly byte[] log2;

		/// <summary>
		/// Class representing arithmentic over the Galois Field GF(256) ~
		/// ~ Z2[x]/P(x) over some irreducible polinomial in Z2[x] of order 8.
		/// This implementation uses P(x)=x^8+x^4+x^3+x^2+1~285, and
		/// minimum privitive element generator x~2.
		/// 
		/// Elements x = b7b6b5b4b3b2b1b0 ~ b7x^7+b6x^6+...+b1x+b0 represented
		/// as bytes.
		/// </summary>
		public GF256()
		{
			SortedDictionary<byte, byte> Inv = new SortedDictionary<byte, byte>();
			int i, j;

			this.powersOfTwo = new byte[256];

			for (i = 0, j = 1; i < 256; i++)
			{
				this.powersOfTwo[i] = (byte)j;

				if (i != 255)
					Inv[(byte)j] = (byte)i;

				j <<= 1;
				if (j >= 256)
					j ^= 285;    // equal to j-=poly = rest of j/poly as polynomial division
			}

			this.log2 = new byte[256];
			Inv.Values.CopyTo(this.log2, 1);
		}

		/// <summary>
		/// Power of 2 table.
		/// </summary>
		public byte[] PowerOf2Table => this.powersOfTwo;

		/// <summary>
		/// Log2 table.
		/// </summary>
		public byte[] Log2Table => this.log2;

		/// <summary>
		/// Adds two numbers
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <returns>x+y</returns>
		public byte Add(byte x, byte y) => (byte)(x ^ y);

		/// <summary>
		/// Subtracts one number from another
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <returns>x-y</returns>
		public byte Subtract(byte x, byte y) => (byte)(x ^ y);

		/// <summary>
		/// Multiplies two numbers
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <returns>x*y</returns>
		public byte Multiply(byte x, byte y) => this.powersOfTwo[(this.log2[x] + this.log2[y]) & 255];
	}
}
