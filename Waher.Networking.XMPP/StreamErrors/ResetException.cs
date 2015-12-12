using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server is closing the stream because it has new (typically security-critical) features to offer, because the keys or
	/// certificates used to establish a secure context for the stream have expired or have been revoked during the life of the stream
	/// (Section 13.7.2.3), because the TLS sequence number has wrapped (Section 5.3.5), etc.  The reset applies to the stream and to any
	/// security context established for that stream (e.g., via TLS and SASL), which means that encryption and authentication need to be
	/// negotiated again for the new stream (e.g., TLS session resumption cannot be used).
	/// </summary>
	public class ResetException : XmppException
	{
		/// <summary>
		/// The server is closing the stream because it has new (typically security-critical) features to offer, because the keys or
		/// certificates used to establish a secure context for the stream have expired or have been revoked during the life of the stream
		/// (Section 13.7.2.3), because the TLS sequence number has wrapped (Section 5.3.5), etc.  The reset applies to the stream and to any
		/// security context established for that stream (e.g., via TLS and SASL), which means that encryption and authentication need to be
		/// negotiated again for the new stream (e.g., TLS session resumption cannot be used).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ResetException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Reset." : Message, Stanza)
		{
		}
	}
}
