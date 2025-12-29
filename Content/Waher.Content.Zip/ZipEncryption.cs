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
		/// ZipCrypto is considered insecure.
		/// </summary>
		ZipCrypto,

		/// <summary>
		/// PKWARE AES (AE-1), 128-bit.
		/// May require additional license from PKWARE, see §10.0:
		/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
		/// </summary>
		Aes128Ae1,

		/// <summary>
		/// PKWARE AES (AE-1), 192-bit
		/// May require additional license from PKWARE, see §10.0:
		/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
		/// </summary>
		Aes192Ae1,

		/// <summary>
		/// PKWARE AES (AE-1), 256-bit
		/// May require additional license from PKWARE, see §10.0:
		/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
		/// </summary>
		Aes256Ae1,

		/// <summary>
		/// PKWARE AES (AE-2), 128-bit (with auth tag)
		/// May require additional license from PKWARE, see §10.0:
		/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
		/// </summary>
		Aes128Ae2,

		/// <summary>
		/// PKWARE AES (AE-2), 192-bit (with auth tag)
		/// May require additional license from PKWARE, see §10.0:
		/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
		/// </summary>
		Aes192Ae2,

		/// <summary>
		/// PKWARE AES (AE-2), 256-bit (with auth tag)
		/// May require additional license from PKWARE, see §10.0:
		/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
		/// </summary>
		Aes256Ae2
	}
}
