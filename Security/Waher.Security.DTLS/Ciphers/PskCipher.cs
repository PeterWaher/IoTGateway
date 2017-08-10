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
		private byte[] psk_identity_hint = null;

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
		/// <param name="State">Endpoint state.</param>
		/// <returns>If the cipher can be used.</returns>
		public override bool CanBeUsed(EndpointState State)
		{
			return State.UsesPsk;
		}

		/// <summary>
		/// Sends the Client Key Exchange message.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public override void SendClientKeyExchange(DtlsEndpoint Endpoint, EndpointState State)
		{
			// Sends the Client Key Exchange message for Pre-shared key ciphers, 
			// as defined in §2 of RFC 4279: https://tools.ietf.org/html/rfc4279

			ushort N = (ushort)State.pskKey.Length;
			byte[] PremasterSecret = new byte[4 + (N << 1)];

			PremasterSecret[0] = (byte)(N >> 8);
			PremasterSecret[1] = (byte)N;
			PremasterSecret[N + 2] = (byte)(N >> 8);
			PremasterSecret[N + 3] = (byte)N;
			Array.Copy(State.pskKey, 0, PremasterSecret, N + 4, N);

			N = (ushort)State.pskIdentity.Length;
			byte[] ClientKeyExchange = new byte[2 + N];

			ClientKeyExchange[0] = (byte)(N >> 8);
			ClientKeyExchange[1] = (byte)N;

			Array.Copy(State.pskIdentity, 0, ClientKeyExchange, 2, N);

			Endpoint.SendHandshake(HandshakeType.client_key_exchange, ClientKeyExchange, true, State);

			// RFC 5246, §8.1, Computing the Master Secret:

			this.MasterSecret = this.PRF(PremasterSecret,
				"master secret", Concat(this.ClientRandom, this.ServerRandom), 48);

			PremasterSecret.Initialize();

			// RFC 5246, §7.1, Change Cipher Spec Protocol:

			Endpoint.SendRecord(ContentType.change_cipher_spec, new byte[] { 1 }, true, State);
			Endpoint.ChangeCipherSpec(State, true);

			this.SendFinished(Endpoint, true, State.HandshakeHash, State);
		}

		/// <summary>
		/// Allows the cipher to process any server key information sent by the DTLS server.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset where data begins.</param>
		public override void ServerKeyExchange(byte[] Data, ref int Offset)
		{
			// RFC 4279, §2:

			ushort Len = Data[Offset++];
			Len <<= 8;
			Len |= Data[Offset++];

			this.psk_identity_hint = new byte[Len];
			Array.Copy(Data, Offset, this.psk_identity_hint, 0, Len);
			Offset += Len;
		}

	}
}
