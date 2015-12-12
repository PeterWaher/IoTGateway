using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The data provided in a 'from' attribute does not match an authorized JID or validated domain as negotiated (1) between two servers using
	/// SASL or Server Dialback, or (2) between a client and a server via SASL authentication and resource binding.
	/// </summary>
	public class InvalidFromException : XmppException
	{
		/// <summary>
		/// The data provided in a 'from' attribute does not match an authorized JID or validated domain as negotiated (1) between two servers using
		/// SASL or Server Dialback, or (2) between a client and a server via SASL authentication and resource binding.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public InvalidFromException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Invalid From." : Message, Stanza)
		{
		}
	}
}
