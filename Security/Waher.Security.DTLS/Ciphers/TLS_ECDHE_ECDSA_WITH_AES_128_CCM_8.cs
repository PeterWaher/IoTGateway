using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8, as defined in RFC 7251:
	/// https://tools.ietf.org/html/rfc7251
	/// </summary>
	public class TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8 : ICipher
	{
		/// <summary>
		/// Cipher name.
		/// </summary>
		public string Name => "TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8";

		/// <summary>
		/// IANA cipher suite code:
		/// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-4
		/// </summary>
		public ushort IanaCipherSuite => 0xc0ae;

		/// <summary>
		/// Priority. The higher the number, the higher priority.
		/// </summary>
		public int Priority => 200;
	}
}
