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
		change_cipher_spec = 20,
		alert = 21,
		handshake = 22,
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

	public enum HandshakeType
	{
		hello_request = 0,
		client_hello = 1,
		server_hello = 2,
		certificate = 11,
		server_key_exchange = 12,
		certificate_request = 13,
		server_hello_done = 14,
		certificate_verify = 15,
		client_key_exchange = 16,
		finished = 20
	}


}
