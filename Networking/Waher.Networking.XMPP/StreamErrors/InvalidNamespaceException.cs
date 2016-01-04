using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The stream namespace name is something other than "http://etherx.jabber.org/streams" (see Section 11.2) or the content
	/// namespace declared as the default namespace is not supported (e.g., something other than "jabber:client" or "jabber:server").
	/// </summary>
	public class InvalidNamespaceException : StreamException
	{
		/// <summary>
		/// The stream namespace name is something other than "http://etherx.jabber.org/streams" (see Section 11.2) or the content
		/// namespace declared as the default namespace is not supported (e.g., something other than "jabber:client" or "jabber:server").
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public InvalidNamespaceException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Invalid Namespace." : Message, Stanza)
		{
		}
	}
}
