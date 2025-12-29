using System;

namespace Waher.Content.Zip
{
	/// <summary>
	/// Enumeration containing ZIP Encryption methods.
	/// </summary>
	public enum ZipEncryption
	{
		/// <summary>
		/// No encryption.
		/// </summary>
		None,

		/// <summary>
		/// Traditional PKWARE.
		/// </summary>
		/// <remarks>
		/// ZipCrypto is considered insecure.
		/// </remarks>
		ZipCrypto,

		/// <summary>
		/// PKWARE AES (AE-1), 128-bit.
		/// </summary>
		Aes128Ae1,

		/// <summary>
		/// PKWARE AES (AE-1), 192-bit
		/// </summary>
		Aes192Ae1,

		/// <summary>
		/// PKWARE AES (AE-1), 256-bit
		/// </summary>
		Aes256Ae1,

		/// <summary>
		/// PKWARE AES (AE-2), 128-bit (with auth tag)
		/// </summary>
		Aes128Ae2,

		/// <summary>
		/// PKWARE AES (AE-2), 192-bit (with auth tag)
		/// </summary>
		Aes192Ae2,

		/// <summary>
		/// PKWARE AES (AE-2), 256-bit (with auth tag)
		/// </summary>
		Aes256Ae2
	}
}
