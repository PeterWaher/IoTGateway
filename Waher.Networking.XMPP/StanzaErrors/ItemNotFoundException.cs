using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The addressed JID or item requested cannot be found; the associated error type SHOULD be "cancel".
	/// </summary>
	public class ItemNotFoundException : StanzaCancelExceptionException
	{
		/// <summary>
		/// The addressed JID or item requested cannot be found; the associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ItemNotFoundException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Item Not Found." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "item-not-found"; }
		}
	}
}
