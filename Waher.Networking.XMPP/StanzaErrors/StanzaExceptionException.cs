using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Base class for all stanza exceptions.
	/// </summary>
	public abstract class StanzaExceptionException : XmppException
	{
		/// <summary>
		/// Base class for all stanza exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StanzaExceptionException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}
	}
}
