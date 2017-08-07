using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// TLS_PSK_WITH_AES_128_CCM, as defined in RFC 6655:
	/// https://tools.ietf.org/html/rfc6655
	/// </summary>
	public class TLS_PSK_WITH_AES_128_CCM : AesCcmCipher
	{
		/// <summary>
		/// TLS_PSK_WITH_AES_128_CCM, as defined in RFC 6655:
		/// https://tools.ietf.org/html/rfc6655
		/// </summary>
		public TLS_PSK_WITH_AES_128_CCM()
			: base(32, 16, 4, 128, 16)
		{
		}

		/// <summary>
		/// Cipher name.
		/// </summary>
		public override string Name => "TLS_PSK_WITH_AES_128_CCM";

		/// <summary>
		/// IANA cipher suite code:
		/// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-4
		/// </summary>
		public override ushort IanaCipherSuite => 0xc0a4;

		/// <summary>
		/// Priority. The higher the number, the higher priority.
		/// </summary>
		public override int Priority => 150;
	}
}
