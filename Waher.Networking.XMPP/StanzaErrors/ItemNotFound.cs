using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The addressed JID or item requested cannot be found; the associated error type SHOULD be "cancel".
	/// </summary>
	public class ItemNotFound : StanzaException
	{
		/// <summary>
		/// The addressed JID or item requested cannot be found; the associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ItemNotFound(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Item Not Found." : Message, Stanza)
		{
		}
	}
}
