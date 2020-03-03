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
		/// <param name="State">Endpoint state.</param>
		/// <returns>If the cipher can be used.</returns>
		public override bool CanBeUsed(EndpointState State)
		{
			return State.UsesPsk;
		}

		/// <summary>
		/// Sends the Client Key Exchange message flight.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public override void SendClientKeyExchange(DtlsEndpoint Endpoint, EndpointState State)
		{
			this.CalcMasterSecret(State);

			// Sends the Client Key Exchange message for Pre-shared key ciphers, 
			// as defined in §2 of RFC 4279: https://tools.ietf.org/html/rfc4279

			if (State.credentials is PresharedKey Psk)
			{
				ushort N = (ushort)Psk.Identity.Length;
				byte[] ClientKeyExchange = new byte[2 + N];

				ClientKeyExchange[0] = (byte)(N >> 8);
				ClientKeyExchange[1] = (byte)N;

				Array.Copy(Psk.Identity, 0, ClientKeyExchange, 2, N);

				Endpoint.SendHandshake(HandshakeType.client_key_exchange, ClientKeyExchange, true, true, State);

				// RFC 5246, §7.1, Change Cipher Spec Protocol:

				Endpoint.SendRecord(ContentType.change_cipher_spec, new byte[] { 1 }, true, true, State);
				Endpoint.ChangeCipherSpec(State, true);

				this.SendFinished(Endpoint, State, true);
			}
		}

		private void CalcMasterSecret(EndpointState State)
		{
			if (State.credentials is PresharedKey Psk)
			{
				ushort N = (ushort)Psk.Key.Length;
				byte[] PremasterSecret = new byte[4 + (N << 1)];

				PremasterSecret[0] = (byte)(N >> 8);
				PremasterSecret[1] = (byte)N;
				PremasterSecret[N + 2] = (byte)(N >> 8);
				PremasterSecret[N + 3] = (byte)N;

				if (N > 0)
					Array.Copy(Psk.Key, 0, PremasterSecret, N + 4, N);

				// RFC 5246, §8.1, Computing the Master Secret:

				this.SetMasterSecret(this.PRF(PremasterSecret, "master secret",
					Concat(State.clientRandom, State.serverRandom), 48), State);

				PremasterSecret.Initialize();
			}
			else
			{
				State.masterSecret = null;
				State.client_write_MAC_key = null;
				State.server_write_MAC_key = null;
				State.client_write_key = null;
				State.server_write_key = null;
				State.client_write_IV = null;
				State.server_write_IV = null;
			}
		}

		/// <summary>
		/// Sends the Server Key Exchange message flight.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public override void SendServerKeyExchange(DtlsEndpoint Endpoint, EndpointState State)
		{
			Endpoint.SendHandshake(HandshakeType.server_hello_done, new byte[0], false, true, State);
		}

		/// <summary>
		/// Allows the cipher to process any server key information sent by the DTLS server.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset where data begins.</param>
		/// <param name="State">Endpoint state.</param>
		public override void ServerKeyExchange(byte[] Data, ref int Offset, EndpointState State)
		{
			// RFC 4279, §2:

			ushort Len = Data[Offset++];
			Len <<= 8;
			Len |= Data[Offset++];

			State.psk_identity_hint = new byte[Len];
			Array.Copy(Data, Offset, State.psk_identity_hint, 0, Len);
			Offset += Len;
		}

		/// <summary>
		/// Allows the cipher to process any client key information sent by the DTLS client.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset where data begins.</param>
		/// <param name="State">Endpoint state.</param>
		public override void ClientKeyExchange(byte[] Data, ref int Offset, EndpointState State)
		{
			ushort N = Data[Offset++];
			N <<= 8;
			N |= Data[Offset++];

			byte[] Identity = new byte[N];
			Array.Copy(Data, Offset, Identity, 0, N);

			string UserId = Encoding.UTF8.GetString(Identity);

			Offset += N;

			if (State.localEndpoint.Users.TryGetUser(UserId, out IUser User) && 
				(State.localEndpoint.RequiredPrivilege is null ||
				User.HasPrivilege(State.localEndpoint.RequiredPrivilege)))
			{
				string s;
				byte[] Key = null;

				if (!string.IsNullOrEmpty(s = User.PasswordHash))
					Key = Hashes.StringToBinary(s);

				if (Key is null)
					Key = Encoding.UTF8.GetBytes(User.PasswordHash);

				State.credentials = new PresharedKey(Identity, Key);

				this.CalcMasterSecret(State);
			}
			else
				State.credentials = null;
		}

	}
}
