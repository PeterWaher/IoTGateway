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
		/// Base class for all ciphers based on Pre-shared keys (PSK).
		/// </summary>
		/// <param name="MacKeyLength">MAC key length.</param>
		/// <param name="EncKeyLength">Encryption key size.</param>
		/// <param name="FixedIvLength">Fixed IV length.</param>
		public PskCipher(int MacKeyLength, int EncKeyLength, int FixedIvLength)
			: base(MacKeyLength, EncKeyLength, FixedIvLength)
		{
		}

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
