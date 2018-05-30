using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.PKCS.Passwords
{
	/// <summary>
	/// Implements the PBKDF1 key derivation function, as defined in §5.1, RFC 2898 (PKCS#5).
	/// </summary>
	public abstract class Pbkdf1 : PasswordEncryption
	{
		private readonly byte[] salt;
		private readonly byte[] key;
		private readonly int iterations;

		/// <summary>
		/// Implements the PBKDF1 key derivation function, as defined in §5.1, RFC 2898 (PKCS#5).
		/// </summary>
		/// <param name="Password">Password</param>
		/// <param name="Iterations">Number of iterations</param>
		/// <param name="KeyLength">Length of generated keys.</param>
		/// <param name="HashFunction">Hash function.</param>
		public Pbkdf1(string Password, int Iterations, int KeyLength, HashFunction HashFunction)
			: base(Password)
		{
			if (Iterations <= 0)
				throw new ArgumentException("Must be postitive.", nameof(Iterations));

			if (KeyLength <= 0)
				throw new ArgumentException("Must be postitive.", nameof(Iterations));

			this.iterations = Iterations;
			this.salt = PfxEncoder.GetRandomBytes(8);

			byte[] Bin = Hashes.ComputeHash(HashFunction, 
				Primitives.CONCAT(Encoding.UTF8.GetBytes(Password), this.salt));

			while (--Iterations >= 0)
				Bin = Hashes.ComputeHash(HashFunction, Bin);

			if (KeyLength > Bin.Length)
				throw new ArgumentException("Derived key too long.", nameof(KeyLength));

			this.key = new byte[KeyLength];

			Array.Copy(Bin, 0, this.key, 0, KeyLength);
		}
	}
}
