using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Waher.Networking.CoAP.Options;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Contains information about a CoAP message.
	/// </summary>
	public class CoapMessage
	{
		private Dictionary<string, string> uriQuery = null;
		private Dictionary<string, string> locationQuery = null;
		private CoapOption[] options;
		private CoapMessageType type;
		private CoapCode code;
		private IPEndPoint from;
		private Uri baseUri = null;
		private byte[] payload;
		private ushort messageId;
		private ulong token;
		private string host = null;
		private string path = null;
		private string locationPath = null;
		private ushort? contentFormat = null;
		private ulong? accept = null;
		private ushort? port = null;
		private uint maxAge = 60;
		private uint? size1 = null;
		private uint? size2 = null;
		private CoapOptionBlock1 block1 = null;
		private CoapOptionBlock2 block2 = null;
		private uint? observe = null;

		internal CoapMessage(CoapMessageType Type, CoapCode Code, ushort MessageId, ulong Token, CoapOption[] Options, byte[] Payload,
			IPEndPoint From)
		{
			this.type = Type;
			this.code = Code;
			this.messageId = MessageId;
			this.token = Token;
			this.options = Options;
			this.payload = Payload;
			this.from = From;
		}

		/// <summary>
		/// Type of message.
		/// </summary>
		public CoapMessageType Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Message code.
		/// </summary>
		public CoapCode Code
		{
			get { return this.code; }
		}

		/// <summary>
		/// Message ID.
		/// </summary>
		public ushort MessageId
		{
			get { return this.messageId; }
		}

		/// <summary>
		/// Token
		/// </summary>
		public ulong Token
		{
			get { return this.token; }
		}

		/// <summary>
		/// Available options.
		/// </summary>
		public CoapOption[] Options
		{
			get { return this.options; }
		}

		/// <summary>
		/// Payload, if available, or null otherwise.
		/// </summary>
		public byte[] Payload
		{
			get { return this.payload; }
			internal set { this.payload = value; }
		}

		/// <summary>
		/// Base URI, if available, or null otherwise.
		/// </summary>
		public Uri BaseUri
		{
			get { return this.baseUri; }
			internal set { this.baseUri = value; }
		}

		/// <summary>
		/// From where the message came.
		/// </summary>
		public IPEndPoint From
		{
			get { return this.from; }
		}

		/// <summary>
		/// Optional accept option.
		/// </summary>
		public ulong? Accept
		{
			get { return this.accept; }
			internal set { this.accept = value; }
		}

		/// <summary>
		/// Optional content format option.
		/// </summary>
		public ushort? ContentFormat
		{
			get { return this.contentFormat; }
			internal set { this.contentFormat = value; }
		}

		/// <summary>
		/// Optional URI query parameters.
		/// </summary>
		public Dictionary<string, string> UriQuery
		{
			get { return this.uriQuery; }
			internal set { this.uriQuery = value; }
		}

		/// <summary>
		/// Tries to get a URI query parameter value.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Value">Parameter value.</param>
		/// <returns>If a parameter with the gíven name was found.</returns>
		public bool TryGetUriQueryParameter(string Name, out string Value)
		{
			if (this.uriQuery == null)
			{
				Value = null;
				return false;
			}
			else
				return this.uriQuery.TryGetValue(Name, out Value);
		}

		/// <summary>
		/// Optional URI Port number option.
		/// </summary>
		public ushort? Port
		{
			get { return this.port; }
			internal set { this.port = value; }
		}

		/// <summary>
		/// Optional URI Host option.
		/// </summary>
		public string Host
		{
			get { return this.host; }
			internal set { this.host = value; }
		}

		/// <summary>
		/// Optional URI Path options, appended into a path string.
		/// </summary>
		public string Path
		{
			get { return this.path; }
			internal set { this.path = value; }
		}

		/// <summary>
		/// Max Age option (number of seconds).
		/// </summary>
		public uint MaxAge
		{
			get { return this.maxAge; }
			internal set { this.maxAge = value; }
		}

		/// <summary>
		/// Optional Location Path options, appended into a path string.
		/// </summary>
		public string LocationPath
		{
			get { return this.locationPath; }
			internal set { this.locationPath = value; }
		}

		/// <summary>
		/// Optional Location Query parameters.
		/// </summary>
		public Dictionary<string, string> LocationQuery
		{
			get { return this.locationQuery; }
			internal set { this.locationQuery = value; }
		}

		/// <summary>
		/// Tries to get a Location query parameter value.
		/// </summary>
		/// <param name="Name">Parameter  name.</param>
		/// <param name="Value">Parameter value.</param>
		/// <returns>If a location parameter was found with the given name.</returns>
		public bool TryGetLocationQueryParameter(string Name, out string Value)
		{
			if (this.locationQuery == null)
			{
				Value = null;
				return false;
			}
			else
				return this.locationQuery.TryGetValue(Name, out Value);
		}

		/// <summary>
		/// Optional Size1 option.
		/// </summary>
		public uint? Size1
		{
			get { return this.size1; }
			internal set { this.size1 = value; }
		}

		/// <summary>
		/// Optional Size2 option.
		/// </summary>
		public uint? Size2
		{
			get { return this.size2; }
			internal set { this.size2 = value; }
		}

		/// <summary>
		/// Optional Block1 option (request payload).
		/// </summary>
		public CoapOptionBlock1 Block1
		{
			get { return this.block1; }
			internal set { this.block1 = value; }
		}

		/// <summary>
		/// Optional Block2 option (response payload).
		/// </summary>
		public CoapOptionBlock2 Block2
		{
			get { return this.block2; }
			internal set { this.block2 = value; }
		}

		/// <summary>
		/// Optional Observe option.
		/// </summary>
		public uint? Observe
		{
			get { return this.observe; }
			internal set { this.observe = value; }
		}

		/// <summary>
		/// Generates an URI for the message.
		/// </summary>
		/// <returns>URI string.</returns>
		public string GetUri()
		{
			return CoapClient.GetUri(this.host, this.port, this.path, this.uriQuery);
		}

		/// <summary>
		/// Decodes the payload of the message.
		/// </summary>
		/// <returns>Decoded payload.</returns>
		public object Decode()
		{
			if (this.payload == null)
				return null;
			else if (!this.contentFormat.HasValue)
				return this.payload;
			else
				return CoapClient.Decode((int)this.contentFormat.Value, this.payload, this.baseUri);
		}

	}
}
