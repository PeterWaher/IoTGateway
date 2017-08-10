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
		internal IncrementalHash handshakeHashCalculator = null;
		internal IncrementalHash handshakeHashCalculator2 = null;
		internal DtlsState state = DtlsState.Created;
		internal DtlsEndpoint localEndpoint;
		internal byte[] pskIdentity;
		internal byte[] pskKey;
		internal byte[] handshakeHash = null;
		internal byte[] handshakeHash2 = null;
		internal byte[] cookie = new byte[1] { 0 };
		internal byte[] sessionId = new byte[1] { 0 };
		internal byte[] cookieRandom = new byte[32];
		internal byte[] clientRandom = new byte[32];
		internal byte[] serverRandom = new byte[0];
		internal ulong receivedPacketsWindow = 0;
		internal ulong leftEdgeSeqNr = 0;
		internal ulong currentSeqNr = 0;
		internal ulong previousReceivedPacketsWindow = 0;
		internal ulong previousLeftEdgeSeqNr = 0;
		internal ulong previousSeqNr = 0;
		internal ushort currentEpoch = 0;
		internal ushort message_seq = 0;
		internal ushort? next_receive_seq = null;
		internal bool acceptRollbackPrevEpoch = false;
		internal bool isClient = true;

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

			if (this.handshakeHashCalculator != null)
			{
				this.handshakeHashCalculator.Dispose();
				this.handshakeHashCalculator = null;
			}

			if (this.handshakeHashCalculator2 != null)
			{
				this.handshakeHashCalculator2.Dispose();
				this.handshakeHashCalculator2 = null;
			}

			this.handshakeHash = null;
			this.handshakeHash2 = null;
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

		internal byte[] HandshakeHash
		{
			get
			{
				if (this.handshakeHash == null)
					this.handshakeHash = this.handshakeHashCalculator.GetHashAndReset();

				return this.handshakeHash;
			}
		}

		internal byte[] HandshakeHash2
		{
			get
			{
				if (this.handshakeHash2 == null)
					this.handshakeHash2 = this.handshakeHashCalculator2.GetHashAndReset();

				return this.handshakeHash2;
			}
		}

	}
}
