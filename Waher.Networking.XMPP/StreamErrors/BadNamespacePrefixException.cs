using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The entity has sent a namespace prefix that is unsupported, or has sent no namespace prefix on an element that needs such a prefix .
	/// </summary>
	public class BadNamespacePrefixException : XmppException
	{
		/// <summary>
		/// The entity has sent a namespace prefix that is unsupported, or has sent no namespace prefix on an element that needs such a prefix .
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public BadNamespacePrefixException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Bad namespace prefix." : Message, Stanza)
		{
		}
	}
}
