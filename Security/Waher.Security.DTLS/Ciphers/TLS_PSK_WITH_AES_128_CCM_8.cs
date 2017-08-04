using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// TLS_PSK_WITH_AES_128_CCM_8, as defined in RFC 6655:
	/// https://tools.ietf.org/html/rfc6655
	/// </summary>
	public class TLS_PSK_WITH_AES_128_CCM_8 : PskCipher
	{
		/// <summary>
		/// TLS_PSK_WITH_AES_128_CCM_8, as defined in RFC 6655:
		/// https://tools.ietf.org/html/rfc6655
		/// </summary>
		public TLS_PSK_WITH_AES_128_CCM_8()
			: base(32, 16, 4)
		{
		}

		/// <summary>
		/// Cipher name.
		/// </summary>
		public override string Name => "TLS_PSK_WITH_AES_128_CCM_8";

		/// <summary>
		/// IANA cipher suite code:
		/// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-4
		/// </summary>
		public override ushort IanaCipherSuite => 0xc0a8;

		/// <summary>
		/// Priority. The higher the number, the higher priority.
		/// </summary>
		public override int Priority => 100;

		/// <summary>
		/// Encrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to encrypt.</param>
		/// <returns>Encrypted data.</returns>
		public override byte[] Encrypt(byte[] Data)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Decrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <returns>Decrypted data, or null if authentication failed.</returns>
		public override byte[] Decrypt(byte[] Data)
		{
			throw new NotImplementedException();
		}
	}
}
