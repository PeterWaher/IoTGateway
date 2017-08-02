using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Interface for ciphers recognized by the DTLS class library.
	/// </summary>
    public interface ICipher
    {
		/// <summary>
		/// Cipher name.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// IANA cipher suite code:
		/// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-4
		/// </summary>
		ushort IanaCipherSuite
		{
			get;
		}

		/// <summary>
		/// Priority. The higher the number, the higher priority.
		/// </summary>
		int Priority
		{
			get;
		}
    }
}
