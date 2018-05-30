using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.PKCS.Passwords
{
	/// <summary>
	/// Implements a password-based encryption algorithm, as defined in §C, RFC 7292 (PKCS#12).
	/// </summary>
	public abstract class PbePkcs12 : PasswordEncryption
	{
		private readonly byte[] salt;
		private readonly byte[] key;
		private readonly int iterations;

		/// <summary>
		/// Implements a password-based encryption algorithm, as defined in §C, RFC 7292 (PKCS#12).
		/// </summary>
		/// <param name="Password">Password</param>
		/// <param name="Iterations">Number of iterations</param>
		/// <param name="KeyLength">Length of generated keys.</param>
		/// <param name="HashFunction">Hash function.</param>
		public PbePkcs12(string Password, int Iterations, int KeyLength, HashFunction HashFunction)
			: base(Password)
		{
			if (Iterations <= 0)
				throw new ArgumentException("Must be postitive.", nameof(Iterations));

			if (KeyLength <= 0)
				throw new ArgumentException("Must be postitive.", nameof(Iterations));

			this.iterations = Iterations;
			this.salt = PfxEncoder.GetRandomBytes(8);

			this.key = PfxEncoder.PRF(HashFunction, Iterations,
				PfxEncoder.FormatPassword(Password), this.salt, KeyLength, 1);
		}

		/// <summary>
		/// Generated key.
		/// </summary>
		protected byte[] Key => this.key;

		/// <summary>
		/// Number of iterations.
		/// </summary>
		protected int Iterations => this.iterations;

		/// <summary>
		/// Salt
		/// </summary>
		protected byte[] Salt => this.salt;

		/// <summary>
		/// Encodes the AlgorithmIdentifier, as defined in PKCS#5 (RFC 2898).
		/// </summary>
		/// <param name="Der">DER output.</param>
		public override void EncodePkcs5AlgorithmIdentifier(DerEncoder Der)
		{
			Der.StartSEQUENCE();
			Der.OBJECT_IDENTIFIER(this.AlgorithmOID);
			Der.StartSEQUENCE();            // pkcs-12PbeParams
			Der.OCTET_STRING(this.salt);
			Der.INTEGER(this.iterations);
			Der.EndSEQUENCE();              // End of pkcs-12PbeParams
			Der.EndSEQUENCE();
		}

	}
}
