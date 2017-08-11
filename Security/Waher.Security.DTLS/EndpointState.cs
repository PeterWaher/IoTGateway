using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Waher.Events;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Maintains the communication state for a remote endpoint.
	/// </summary>
	public class EndpointState : IDisposable
	{
		internal object remoteEndpoint;
		internal ICipher pendingCipher = null;
		internal ICipher currentCipher = null;
		internal ICipher previousCipher = null;
		internal MemoryStream buffer = null;
		internal IncrementalHash clientHandshakeHashCalculator = null;
		internal IncrementalHash serverHandshakeHashCalculator = null;
		internal DtlsState state = DtlsState.Created;
		internal DtlsEndpoint localEndpoint;
		internal byte[] psk_identity_hint = null;
		internal byte[] pskIdentity;
		internal byte[] pskKey;
		internal byte[] cookie = new byte[1] { 0 };
		internal byte[] sessionId = new byte[1] { 0 };
		internal byte[] cookieRandom = new byte[32];
		internal byte[] clientRandom = new byte[32];
		internal byte[] serverRandom = new byte[0];
		internal byte[] clientHandshakeHash = null;
		internal byte[] serverHandshakeHash = null;
		internal byte[] masterSecret = null;
		internal byte[] client_write_MAC_key = null;
		internal byte[] server_write_MAC_key = null;
		internal byte[] client_write_key = null;
		internal byte[] server_write_key = null;
		internal byte[] client_write_IV = null;
		internal byte[] server_write_IV = null;
		internal ulong nonceCount = 0;
		internal ulong receivedPacketsWindow = 0;
		internal ulong leftEdgeSeqNr = 0;
		internal ulong currentSeqNr = 0;
		internal ulong previousReceivedPacketsWindow = 0;
		internal ulong previousLeftEdgeSeqNr = 0;
		internal ulong previousSeqNr = 0;
		internal ushort currentEpoch = 0;
		internal ushort message_seq = 0;
		internal ushort next_receive_seq = 0;
		internal bool acceptRollbackPrevEpoch = false;
		internal bool isClient = true;
		internal bool clientFinished = false;
		internal bool serverFinished = false;

		/// <summary>
		/// Maintains the communication state for a remote endpoint.
		/// </summary>
		/// <param name="LocalEndpoint">Local endpoint.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public EndpointState(DtlsEndpoint LocalEndpoint, object RemoteEndpoint)
		{
			this.localEndpoint = LocalEndpoint;
			this.remoteEndpoint = RemoteEndpoint;
		}

		/// <summary>
		/// If pre-shared keys (PSK) are used.
		/// </summary>
		internal bool UsesPsk
		{
			get
			{
				return this.pskIdentity != null && this.pskKey != null;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.buffer != null)
			{
				this.buffer.Dispose();
				this.buffer = null;
			}

			if (this.clientHandshakeHashCalculator != null)
			{
				this.clientHandshakeHashCalculator.Dispose();
				this.clientHandshakeHashCalculator = null;
			}

			if (this.serverHandshakeHashCalculator != null)
			{
				this.serverHandshakeHashCalculator.Dispose();
				this.serverHandshakeHashCalculator = null;
			}

			this.clientHandshakeHash = null;
			this.serverHandshakeHash = null;
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public object RemoteEndpoint
		{
			get { return remoteEndpoint; }
		}

		/// <summary>
		/// Current DTLS state.
		/// </summary>
		public DtlsState State
		{
			get { return this.state; }
			internal set
			{
				if (this.state != value)
				{
					this.state = value;
					this.localEndpoint.StateChanged(this.remoteEndpoint, value);
				}
			}
		}

		internal void CalcClientHandshakeHash()
		{
			if (this.clientHandshakeHash == null)
				this.clientHandshakeHash = this.clientHandshakeHashCalculator.GetHashAndReset();
		}

		internal void CalcServerHandshakeHash()
		{
			if (this.serverHandshakeHash == null)
				this.serverHandshakeHash = this.serverHandshakeHashCalculator.GetHashAndReset();
		}

	}
}
