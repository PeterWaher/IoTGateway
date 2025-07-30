using System;

namespace Waher.Security.PQC
{
	/// <summary>
	/// Implements the ML-DSA algorithm for post-quantum cryptography, as defined in
	/// NIST FIPS 204: https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.204.pdf
	/// </summary>
	public class ML_DSA
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
		private readonly int k;                     // Rows of matrix A
		private readonly int l;                     // Columns of matrix A
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
		public ML_DSA(int τ, int λ, int γ1, int γ2, int k, int l, int η, int ω,
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
	}
}
