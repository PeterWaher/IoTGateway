using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.QR.Encoding
{
	/// <summary>
	/// Polynomial over GF(256), where coefficients are defined as bytes in a 
	/// byte array (Least significant byte first, i.e. Coefficients[i] corresponds to
	/// coefficient for x^i.)
	/// </summary>
	public class GF256Px
	{
		private readonly byte[] coefficients;
		private readonly int degree;

		/// <summary>
		/// Polynomial over GF(256), where coefficients are defined as bytes in a 
		/// byte array (Least significant byte first, i.e. Coefficients[i] corresponds to
		/// coefficient for x^i.)
		/// </summary>
		/// <param name="Coefficients">Polynomial coefficients.</param>
		public GF256Px(byte[] Coefficients)
		{
			this.coefficients = Coefficients;
			this.degree = this.coefficients.Length - 1;
		}

		/// <summary>
		/// Polynomial coefficients.
		/// </summary>
		public byte[] Coefficients => this.coefficients;

		/// <summary>
		/// Multiplies the polynomial with the polynomial defined by
		/// <paramref name="P"/>.
		/// </summary>
		/// <param name="P">Polynomial</param>
		/// <returns>New polynomial corresponding to the multiplication of the
		/// current polynomial and <paramref name="P"/>.</returns>
		public GF256Px Mul(GF256Px P)
		{
			byte[] C = new byte[this.degree + P.degree + 1];
			int i, j;

			for (i = 0; i <= P.degree; i++)
			{
				for (j = 0; j <= this.degree; j++)
					C[i + j] ^= GF256.Multiply(this.coefficients[j], P.coefficients[i]);
			}

			return new GF256Px(C);
		}
	}
}
