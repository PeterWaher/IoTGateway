using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// A remote server or service specified as part or all of the JID of the intended recipient (or needed to fulfill a request) was resolved but
	/// communications could not be established within a reasonable amount of time (e.g., an XML stream cannot be established at the resolved IP
	/// address and port, or an XML stream can be established but stream negotiation fails because of problems with TLS, SASL, Server
	/// Dialback, etc.); the associated error type SHOULD be "wait" (unless the error is of a more permanent nature, e.g., the remote server is
	/// found but it cannot be authenticated or it violates security policies).
	/// </summary>
	public class RemoteServerTimeoutException : StanzaExceptionException
	{
		/// <summary>
		/// A remote server or service specified as part or all of the JID of the intended recipient (or needed to fulfill a request) was resolved but
		/// communications could not be established within a reasonable amount of time (e.g., an XML stream cannot be established at the resolved IP
		/// address and port, or an XML stream can be established but stream negotiation fails because of problems with TLS, SASL, Server
		/// Dialback, etc.); the associated error type SHOULD be "wait" (unless the error is of a more permanent nature, e.g., the remote server is
		/// found but it cannot be authenticated or it violates security policies).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RemoteServerTimeoutException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Remote Server Timeout." : Message, Stanza)
		{
		}
	}
}
