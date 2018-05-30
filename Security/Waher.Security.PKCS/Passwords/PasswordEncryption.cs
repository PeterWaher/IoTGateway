using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.PKCS.Passwords
{
	/// <summary>
	/// Abstract base class for password-based encryption algorithms
	/// </summary>
	public abstract class PasswordEncryption
    {
		private string password;

		/// <summary>
		/// Abstract base class for password-based encryption algorithms
		/// </summary>
		public PasswordEncryption(string Password)
		{
			this.password = Password;
		}

		/// <summary>
		/// Password.
		/// </summary>
		protected string Password => this.password;

		/// <summary>
		/// Object Identity for the algorithm.
		/// </summary>
		public abstract string AlgorithmOID
		{
			get;
		}

		/// <summary>
		/// Encodes the AlgorithmIdentifier, as defined in PKCS#5 (RFC 2898).
		/// </summary>
		/// <param name="Der">DER output.</param>
		public abstract void EncodePkcs5AlgorithmIdentifier(DerEncoder Der);

		/// <summary>
		/// Encrypts data.
		/// </summary>
		/// <param name="PlainText">Data to encrypt.</param>
		/// <returns>Encrypted data.</returns>
		public abstract byte[] Encrypt(byte[] PlainText);

	}
}
