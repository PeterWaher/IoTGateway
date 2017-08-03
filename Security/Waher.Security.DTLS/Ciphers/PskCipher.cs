using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// Base class for all ciphers based on Pre-shared keys (PSK).
	/// </summary>
	public abstract class PskCipher : Cipher
	{
		/// <summary>
		/// If the cipher can be used by the endpoint.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <returns>If the cipher can be used.</returns>
		public override bool CanBeUsed(DtlsEndpoint Endpoint)
		{
			return Endpoint.UsesPsk;
		}

		/// <summary>
		/// Sends the Client Key Exchange message.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		public override void SendClientKeyExchange(DtlsEndpoint Endpoint)
		{
			Endpoint.SendClientPskKeyExchange();
		}
	}
}
