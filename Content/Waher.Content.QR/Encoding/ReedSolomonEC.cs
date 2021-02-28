using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.QR.Encoding
{
	/// <summary>
	/// Implements Reed-Solomon Error Correction using polynomial division
	/// over GF(256)[x].
	/// </summary>
	public class ReedSolomonEC
	{
		private readonly GF256Px generatorPolynomial;

		/// <summary>
		/// Implements Reed-Solomon Error Correction using polynomial division
		/// over GF(256)[x].
		/// </summary>
		/// <param name="NrCorrectionCodeWords">Number of correction code words
		/// to support.</param>
		public ReedSolomonEC(int NrCorrectionCodeWords)
		{
			if (NrCorrectionCodeWords <= 0)
				throw new ArgumentException("Number of correction code words must be positive.", nameof(NrCorrectionCodeWords));

			GF256Px P = new GF256Px(new byte[] { 1, 1 });   // X-1 = X-α^0
			int i = 0;

			while (++i < NrCorrectionCodeWords)
				P = P.Mul(new GF256Px(new byte[] { 1, GF256.PowerOf2Table[i] })); // X-α^i

			this.generatorPolynomial = P;
		}

		/// <summary>
		/// Generator polynomial.
		/// </summary>
		public GF256Px GeneratorPolynomial => this.generatorPolynomial;
	}
}
