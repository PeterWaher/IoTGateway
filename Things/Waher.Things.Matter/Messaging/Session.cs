using System;
using System.Net;
using Waher.Things.Matter.Crypto;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Base class for Matter sessions (Matter Core Specification, 4.13).
	/// </summary>
	public abstract class Session
	{
		private readonly ReceptionState reception;
		private IPEndPoint peerAddress;
		private SessionParameters peerParameters;
		private DateTime lastPeerActivity = DateTime.MinValue;
		private bool closed = false;

		/// <summary>
		/// Base class for Matter sessions.
		/// </summary>
		/// <param name="PeerAddress">Address of peer.</param>
		/// <param name="PeerParameters">Session parameters of peer.</param>
		/// <param name="AllowRollover">If received message counters are allowed to roll over.</param>
		protected Session(IPEndPoint PeerAddress, SessionParameters PeerParameters, bool AllowRollover)
		{
			this.peerAddress = PeerAddress;
			this.peerParameters = PeerParameters ?? new SessionParameters();
			this.reception = new ReceptionState(AllowRollover);
		}

		/// <summary>
		/// Address of peer.
		/// </summary>
		public IPEndPoint PeerAddress
		{
			get => this.peerAddress;
			set => this.peerAddress = value;
		}

		/// <summary>
		/// Session parameters of peer.
		/// </summary>
		public SessionParameters PeerParameters
		{
			get => this.peerParameters;
			set => this.peerParameters = value ?? new SessionParameters();
		}

		/// <summary>
		/// Timestamp of last message received from peer.
		/// </summary>
		public DateTime LastPeerActivity => this.lastPeerActivity;

		/// <summary>
		/// If the peer is considered active (a message was received within the active threshold).
		/// </summary>
		public bool IsPeerActive => (DateTime.UtcNow - this.lastPeerActivity).TotalMilliseconds < this.peerParameters.ActiveThresholdMs;

		/// <summary>
		/// If the session has been closed.
		/// </summary>
		public bool IsClosed
		{
			get => this.closed;
			internal set => this.closed = value;
		}

		/// <summary>
		/// If the session is secure (encrypted).
		/// </summary>
		public abstract bool IsSecure { get; }

		/// <summary>
		/// Reception state, for duplicate detection.
		/// </summary>
		internal ReceptionState Reception => this.reception;

		/// <summary>
		/// Registers activity from peer.
		/// </summary>
		/// <param name="From">Address of peer.</param>
		internal void PeerActivity(IPEndPoint From)
		{
			this.lastPeerActivity = DateTime.UtcNow;

			if (!(From is null))
				this.peerAddress = From;
		}

		/// <summary>
		/// Gets the next message counter to use when sending a message.
		/// </summary>
		/// <param name="Manager">Exchange manager.</param>
		/// <returns>Message counter.</returns>
		internal abstract uint NextMessageCounter(ExchangeManager Manager);
	}

	/// <summary>
	/// Unsecured session, used during session establishment (Matter Core Specification, 4.13.2).
	/// </summary>
	public class UnsecuredSession : Session
	{
		/// <summary>
		/// Unsecured session.
		/// </summary>
		/// <param name="PeerAddress">Address of peer.</param>
		/// <param name="EphemeralNodeId">Ephemeral initiator node ID.</param>
		/// <param name="IsInitiator">If the local node is the initiator.</param>
		internal UnsecuredSession(IPEndPoint PeerAddress, ulong EphemeralNodeId, bool IsInitiator)
			: base(PeerAddress, new SessionParameters(), true)
		{
			this.EphemeralNodeId = EphemeralNodeId;
			this.IsInitiator = IsInitiator;
		}

		/// <summary>
		/// Ephemeral initiator node ID.
		/// </summary>
		public ulong EphemeralNodeId { get; }

		/// <summary>
		/// If the local node is the initiator of the session.
		/// </summary>
		public bool IsInitiator { get; }

		/// <inheritdoc/>
		public override bool IsSecure => false;

		internal override uint NextMessageCounter(ExchangeManager Manager)
		{
			return Manager.NextUnsecuredCounter();
		}
	}

	/// <summary>
	/// Secure unicast session, established using PASE or CASE (Matter Core Specification, 4.13.1).
	/// </summary>
	public class SecureSession : Session
	{
		private readonly byte[] encryptKey;
		private readonly byte[] decryptKey;
		private uint counter;

		/// <summary>
		/// Secure unicast session.
		/// </summary>
		/// <param name="PeerAddress">Address of peer.</param>
		/// <param name="PeerParameters">Session parameters of peer.</param>
		/// <param name="LocalSessionId">Local session ID.</param>
		/// <param name="PeerSessionId">Peer session ID.</param>
		/// <param name="IsInitiator">If the local node was the session initiator.</param>
		/// <param name="IsPase">If the session was established using PASE.</param>
		/// <param name="LocalNodeId">Local operational node ID (0 for PASE).</param>
		/// <param name="PeerNodeId">Peer operational node ID (0 for PASE).</param>
		/// <param name="I2RKey">Initiator-to-responder key.</param>
		/// <param name="R2IKey">Responder-to-initiator key.</param>
		/// <param name="AttestationChallenge">Attestation challenge.</param>
		public SecureSession(IPEndPoint PeerAddress, SessionParameters PeerParameters, ushort LocalSessionId, ushort PeerSessionId,
			bool IsInitiator, bool IsPase, ulong LocalNodeId, ulong PeerNodeId, byte[] I2RKey, byte[] R2IKey, byte[] AttestationChallenge)
			: base(PeerAddress, PeerParameters, false)
		{
			this.LocalSessionId = LocalSessionId;
			this.PeerSessionId = PeerSessionId;
			this.IsInitiator = IsInitiator;
			this.IsPase = IsPase;
			this.LocalNodeId = LocalNodeId;
			this.PeerNodeId = PeerNodeId;
			this.AttestationChallenge = AttestationChallenge;
			this.encryptKey = IsInitiator ? I2RKey : R2IKey;
			this.decryptKey = IsInitiator ? R2IKey : I2RKey;
			this.counter = (MatterCrypto.RandomUInt32() & 0x0fffffff) + 1;
			this.Created = DateTime.UtcNow;
		}

		/// <summary>
		/// Creates a secure session from a shared secret, deriving session keys (Matter Core Specification, 4.13.2.6).
		/// </summary>
		/// <param name="PeerAddress">Address of peer.</param>
		/// <param name="PeerParameters">Session parameters of peer.</param>
		/// <param name="LocalSessionId">Local session ID.</param>
		/// <param name="PeerSessionId">Peer session ID.</param>
		/// <param name="IsInitiator">If the local node was the session initiator.</param>
		/// <param name="IsPase">If the session was established using PASE.</param>
		/// <param name="LocalNodeId">Local operational node ID (0 for PASE).</param>
		/// <param name="PeerNodeId">Peer operational node ID (0 for PASE).</param>
		/// <param name="SharedSecret">Shared secret.</param>
		/// <param name="Salt">Salt.</param>
		/// <param name="Info">KDF info string ("SessionKeys" or "SessionResumptionKeys").</param>
		/// <returns>Secure session.</returns>
		public static SecureSession FromSharedSecret(IPEndPoint PeerAddress, SessionParameters PeerParameters, ushort LocalSessionId,
			ushort PeerSessionId, bool IsInitiator, bool IsPase, ulong LocalNodeId, ulong PeerNodeId, byte[] SharedSecret, byte[] Salt,
			string Info = "SessionKeys")
		{
			byte[] Keys = MatterCrypto.Hkdf(SharedSecret, Salt, Info, 3 * MatterCrypto.SymmetricKeyLength);

			return new SecureSession(PeerAddress, PeerParameters, LocalSessionId, PeerSessionId, IsInitiator, IsPase,
				LocalNodeId, PeerNodeId,
				MatterCrypto.Slice(Keys, 0, 16),
				MatterCrypto.Slice(Keys, 16, 16),
				MatterCrypto.Slice(Keys, 32, 16));
		}

		/// <summary>
		/// Local session ID.
		/// </summary>
		public ushort LocalSessionId { get; }

		/// <summary>
		/// Peer session ID.
		/// </summary>
		public ushort PeerSessionId { get; }

		/// <summary>
		/// If the local node initiated the session.
		/// </summary>
		public bool IsInitiator { get; }

		/// <summary>
		/// If the session was established using PASE.
		/// </summary>
		public bool IsPase { get; }

		/// <summary>
		/// Local operational node ID (0 for PASE).
		/// </summary>
		public ulong LocalNodeId { get; }

		/// <summary>
		/// Peer operational node ID (0 for PASE).
		/// </summary>
		public ulong PeerNodeId { get; }

		/// <summary>
		/// Attestation challenge.
		/// </summary>
		public byte[] AttestationChallenge { get; }

		/// <summary>
		/// When the session was created.
		/// </summary>
		public DateTime Created { get; }

		/// <summary>
		/// Optional reference to the owner of the session (e.g. fabric).
		/// </summary>
		public object Tag { get; set; }

		/// <inheritdoc/>
		public override bool IsSecure => true;

		internal override uint NextMessageCounter(ExchangeManager Manager)
		{
			lock (this)
			{
				return this.counter++;
			}
		}

		/// <summary>
		/// Computes the nonce of a message.
		/// </summary>
		/// <param name="SecurityFlags">Security flags.</param>
		/// <param name="Counter">Message counter.</param>
		/// <param name="NodeId">Source node ID.</param>
		/// <returns>13-byte nonce.</returns>
		public static byte[] ComputeNonce(byte SecurityFlags, uint Counter, ulong NodeId)
		{
			byte[] Nonce = new byte[13];
			int i;

			Nonce[0] = SecurityFlags;

			for (i = 0; i < 4; i++)
			{
				Nonce[1 + i] = (byte)Counter;
				Counter >>= 8;
			}

			for (i = 0; i < 8; i++)
			{
				Nonce[5 + i] = (byte)NodeId;
				NodeId >>= 8;
			}

			return Nonce;
		}

		/// <summary>
		/// Encrypts a message payload.
		/// </summary>
		/// <param name="Header">Encoded message header (used as additional authenticated data).</param>
		/// <param name="SecurityFlags">Security flags.</param>
		/// <param name="Counter">Message counter.</param>
		/// <param name="Payload">Plaintext payload.</param>
		/// <returns>Encrypted payload, including MIC.</returns>
		public byte[] Encrypt(byte[] Header, byte SecurityFlags, uint Counter, byte[] Payload)
		{
			byte[] Nonce = ComputeNonce(SecurityFlags, Counter, this.IsPase ? 0 : this.LocalNodeId);
			return AesCcm128.Encrypt(this.encryptKey, Nonce, Payload, Header);
		}

		/// <summary>
		/// Decrypts a message payload.
		/// </summary>
		/// <param name="Header">Encoded message header (additional authenticated data).</param>
		/// <param name="SecurityFlags">Security flags.</param>
		/// <param name="Counter">Message counter.</param>
		/// <param name="Encrypted">Encrypted payload, including MIC.</param>
		/// <returns>Decrypted payload.</returns>
		public byte[] Decrypt(byte[] Header, byte SecurityFlags, uint Counter, byte[] Encrypted)
		{
			byte[] Nonce = ComputeNonce(SecurityFlags, Counter, this.IsPase ? 0 : this.PeerNodeId);
			return AesCcm128.Decrypt(this.decryptKey, Nonce, Encrypted, Header);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return (this.IsPase ? "PASE" : "CASE") + " session " + this.LocalSessionId.ToString() + "/" + this.PeerSessionId.ToString() +
				(this.IsPase ? string.Empty : " with node " + this.PeerNodeId.ToString("X16"));
		}
	}
}
