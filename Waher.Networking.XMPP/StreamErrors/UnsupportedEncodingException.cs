using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The initiating entity has encoded the stream in an encoding that is not supported by the server (see Section 11.6) or has otherwise
	/// improperly encoded the stream (e.g., by violating the rules of the [UTF-8] encoding).
	/// </summary>
	public class UnsupportedEncodingException : XmppException
	{
		/// <summary>
		/// The initiating entity has encoded the stream in an encoding that is not supported by the server (see Section 11.6) or has otherwise
		/// improperly encoded the stream (e.g., by violating the rules of the [UTF-8] encoding).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public UnsupportedEncodingException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Unsupported Encoding." : Message, Stanza)
		{
		}
	}
}
