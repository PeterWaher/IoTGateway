using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The entity has sent XML that cannot be processed.
	/// </summary>
	public abstract class StanzaException : XmppException
	{
		/// <summary>
		/// The entity has sent XML that cannot be processed.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StanzaException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}
	}
}
