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


}
