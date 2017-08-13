using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// DTLS Record content type.
	/// </summary>
	public enum ContentType
	{
		/// <summary>
		/// The change cipher spec protocol exists to signal transitions in
		/// ciphering strategies.
		/// </summary>
		change_cipher_spec = 20,

		/// <summary>
		/// Alert messages convey the severity of the message
		/// (warning or fatal) and a description of the alert.
		/// </summary>
		alert = 21,

		/// <summary>
		/// used to negotiate the secure attributes of a session.
		/// </summary>
		handshake = 22,

		/// <summary>
		/// Application data messages are carried by the record layer and are
		/// fragmented, compressed, and encrypted based on the current connection
		/// state.
		/// </summary>
		application_data = 23
	}

	/// <summary>
	/// D(TLS) Protocol version.
	/// </summary>
	public struct ProtocolVersion
	{
		/// <summary>
		/// Major version. DTLS uses 1's complement.
		/// </summary>
		public byte major;

		/// <summary>
		/// Minor version. DTLS uses 1's complement.
		/// </summary>
		public byte minor;
	}

	/// <summary>
	/// DTLS record.
	/// </summary>
	public struct DTLSPlaintext
	{
		/// <summary>
		/// The higher-level protocol used to process the enclosed fragment.
		/// </summary>
		public ContentType type;

		/// <summary>
		/// The version of the protocol being employed. This document
		/// describes DTLS version 1.2, which uses the version { 254, 253 }.
		/// The version value of 254.253 is the 1's complement of DTLS version
		/// 1.2. This maximal spacing between TLS and DTLS version numbers
		/// ensures that records from the two protocols can be easily
		/// distinguished.It should be noted that future on-the-wire version
		/// numbers of DTLS are decreasing in value (while the true version
		/// number is increasing in value.)
		/// </summary>
		public ProtocolVersion version;

		/// <summary>
		///  A counter value that is incremented on every cipher state change.
		/// </summary>
		public ushort epoch;

		/// <summary>
		/// The sequence number for this record.
		/// </summary>
		public ulong sequence_number;   // Only 6 bytes.

		/// <summary>
		/// The length (in bytes) of the following DTLSPlaintext.fragment. The
		/// length MUST NOT exceed 2^14.
		/// </summary>
		public ushort length;

		/// <summary>
		/// The application data.  This data is transparent and treated as an
		/// independent block to be dealt with by the higher-level protocol
		/// specified by the type field.
		/// </summary>
		public byte[] fragment; // length bytes.
	}

	/// <summary>
	/// Type of handshake message.
	/// </summary>
	public enum HandshakeType
	{
		/// <summary>
		/// HelloRequest is a simple notification that the client should begin
		/// the negotiation process anew.
		/// </summary>
		hello_request = 0,

		/// <summary>
		/// When a client first connects to a server, it is required to send
		/// the ClientHello as its first message.  The client can also send a
		/// ClientHello in response to a HelloRequest or on its own initiative
		/// in order to renegotiate the security parameters in an existing
		/// connection.
		/// </summary>
		client_hello = 1,

		/// <summary>
		/// The server will send this message in response to a ClientHello
		/// message when it was able to find an acceptable set of algorithms.
		/// </summary>
		server_hello = 2,

		/// <summary>
		/// Used to send a cookie to the client.
		/// </summary>
		hello_verify_request = 3,

		/// <summary>
		/// This message conveys the server's certificate chain to the client.
		/// </summary>
		certificate = 11,

		/// <summary>
		/// This message conveys cryptographic information to allow the client
		/// to communicate the premaster secret: a Diffie-Hellman public key
		/// with which the client can complete a key exchange (with the result
		/// being the premaster secret) or a public key for some other
		/// algorithm.
		/// </summary>
		server_key_exchange = 12,

		/// <summary>
		/// A non-anonymous server can optionally request a certificate from
		/// the client, if appropriate for the selected cipher suite.
		/// </summary>
		certificate_request = 13,

		/// <summary>
		/// The ServerHelloDone message is sent by the server to indicate the
		/// end of the ServerHello and associated messages.
		/// </summary>
		server_hello_done = 14,

		/// <summary>
		/// This message is used to provide explicit verification of a client
		/// certificate.
		/// </summary>
		certificate_verify = 15,

		/// <summary>
		/// With this message, the premaster secret is set, either by direct
		/// transmission of the RSA-encrypted secret or by the transmission of
		/// Diffie-Hellman parameters that will allow each side to agree upon
		/// the same premaster secret.
		/// </summary>
		client_key_exchange = 16,

		/// <summary>
		///  The Finished message is the first one protected with the just
		/// negotiated algorithms, keys, and secrets.  Recipients of Finished
		/// messages MUST verify that the contents are correct.  Once a side
		/// has sent its Finished message and received and validated the
		/// Finished message from its peer, it may begin to send and receive
		/// application data over the connection.
		/// </summary>
		finished = 20
	}

	/// <summary>
	/// Alert level.
	/// </summary>
	public enum AlertLevel
	{
		/// <summary>
		///  Warning message.
		/// </summary>
		warning = 1,

		/// <summary>
		/// Alert messages with a level of fatal result in the immediate termination of the
		/// connection.
		/// </summary>
		fatal = 2
	}

	/// <summary>
	/// DTLS state.
	/// </summary>
	public enum DtlsState
	{
		/// <summary>
		/// Endpoint created.
		/// </summary>
		Created,

		/// <summary>
		/// Handshake is underway.
		/// </summary>
		Handshake,

		/// <summary>
		/// Session established. Application data can be transmitted.
		/// </summary>
		SessionEstablished,

		/// <summary>
		/// Communication failed.
		/// </summary>
		Failed,

		/// <summary>
		/// Connections closed
		/// </summary>
		Closed
	}

	/// <summary>
	/// Alert description.
	/// </summary>
	public enum AlertDescription
	{
		/// <summary>
		/// This message notifies the recipient that the sender will not send
		/// any more messages on this connection.
		/// </summary>
		close_notify = 0,

		/// <summary>
		/// An inappropriate message was received.
		/// </summary>
		unexpected_message = 10,

		/// <summary>
		/// This alert is returned if a record is received with an incorrect MAC.
		/// </summary>
		bad_record_mac = 20,

		/// <summary>
		/// This alert was used in some earlier versions of TLS, and may have
		/// permitted certain attacks against the CBC mode [CBCATT].  It MUST
		/// NOT be sent by compliant implementations.
		/// </summary>
		decryption_failed_RESERVED = 21,

		/// <summary>
		/// A TLSCiphertext record was received that had a length more than
		/// 2^14+2048 bytes, or a record decrypted to a TLSCompressed record
		/// with more than 2^14+1024 bytes.
		/// </summary>
		record_overflow = 22,

		/// <summary>
		/// The decompression function received improper input (e.g., data
		/// that would expand to excessive length).
		/// </summary>
		decompression_failure = 30,

		/// <summary>
		/// Reception of a handshake_failure alert message indicates that the
		/// sender was unable to negotiate an acceptable set of security
		/// parameters given the options available.
		/// </summary>
		handshake_failure = 40,

		/// <summary>
		/// This alert was used in SSLv3 but not any version of TLS.  It MUST
		/// NOT be sent by compliant implementations.
		/// </summary>
		no_certificate_RESERVED = 41,

		/// <summary>
		/// A certificate was corrupt, contained signatures that did not
		/// verify correctly, etc.
		/// </summary>
		bad_certificate = 42,

		/// <summary>
		/// A certificate was of an unsupported type.
		/// </summary>
		unsupported_certificate = 43,

		/// <summary>
		/// A certificate was revoked by its signer.
		/// </summary>
		certificate_revoked = 44,

		/// <summary>
		/// A certificate has expired or is not currently valid.
		/// </summary>
		certificate_expired = 45,

		/// <summary>
		/// Some other (unspecified) issue arose in processing the
		/// certificate, rendering it unacceptable.
		/// </summary>
		certificate_unknown = 46,

		/// <summary>
		/// A field in the handshake was out of range or inconsistent with
		/// other fields.  This message is always fatal.
		/// </summary>
		illegal_parameter = 47,

		/// <summary>
		/// A valid certificate chain or partial chain was received, but the
		/// certificate was not accepted because the CA certificate could not
		/// be located or couldn't be matched with a known, trusted CA.  This
		/// message is always fatal.
		/// </summary>
		unknown_ca = 48,

		/// <summary>
		/// A valid certificate was received, but when access control was
		/// applied, the sender decided not to proceed with negotiation.  This
		/// message is always fatal.
		/// </summary>
		access_denied = 49,

		/// <summary>
		/// A message could not be decoded because some field was out of the
		/// specified range or the length of the message was incorrect.  This
		/// message is always fatal and should never be observed in
		/// communication between proper implementations (except when messages
		/// were corrupted in the network).
		/// </summary>
		decode_error = 50,

		/// <summary>
		/// A handshake cryptographic operation failed, including being unable
		/// to correctly verify a signature or validate a Finished message.
		/// This message is always fatal.
		/// </summary>
		decrypt_error = 51,

		/// <summary>
		/// This alert was used in some earlier versions of TLS.  It MUST NOT
		/// be sent by compliant implementations.
		/// </summary>
		export_restriction_RESERVED = 60,

		/// <summary>
		/// The protocol version the client has attempted to negotiate is
		/// recognized but not supported.  (For example, old protocol versions
		/// might be avoided for security reasons.)  This message is always
		/// fatal.
		/// </summary>
		protocol_version = 70,

		/// <summary>
		/// Returned instead of handshake_failure when a negotiation has
		/// failed specifically because the server requires ciphers more
		/// secure than those supported by the client.  This message is always
		/// fatal.
		/// </summary>
		insufficient_security = 71,

		/// <summary>
		/// An internal error unrelated to the peer or the correctness of the
		/// protocol (such as a memory allocation failure) makes it impossible
		/// to continue.  This message is always fatal.
		/// </summary>
		internal_error = 80,

		/// <summary>
		/// This handshake is being canceled for some reason unrelated to a
		/// protocol failure.  If the user cancels an operation after the
		/// handshake is complete, just closing the connection by sending a
		/// close_notify is more appropriate.  This alert should be followed
		/// by a close_notify.  This message is generally a warning.
		/// </summary>
		user_canceled = 90,

		/// <summary>
		/// Sent by the client in response to a hello request or by the server
		/// in response to a client hello after initial handshaking.  Either
		/// of these would normally lead to renegotiation; when that is not
		/// appropriate, the recipient should respond with this alert.  At
		/// that point, the original requester can decide whether to proceed
		/// with the connection.  One case where this would be appropriate is
		/// where a server has spawned a process to satisfy a request; the
		/// process might receive security parameters (key length,
		/// authentication, etc.) at startup, and it might be difficult to
		/// communicate changes to these parameters after that point.  This
		/// message is always a warning.
		/// </summary>
		no_renegotiation = 100,

		/// <summary>
		/// sent by clients that receive an extended server hello containing
		/// an extension that they did not put in the corresponding client
		/// hello.  This message is always fatal.
		/// </summary>
		unsupported_extension = 110
	}

	/// <summary>
	/// DTLS mode of operation.
	/// </summary>
	public enum DtlsMode
	{
		/// <summary>
		/// DTLS endpoint acts as a client only.
		/// </summary>
		Client,

		/// <summary>
		/// DTLS endpoint acts as a server only.
		/// </summary>
		Server,

		/// <summary>
		/// DTLS endpoint can act as both client and server.
		/// </summary>
		Both
	}

}
