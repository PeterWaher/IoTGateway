using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The intended recipient is temporarily unavailable, undergoing maintenance, etc.; the associated error type SHOULD be "wait".
	/// </summary>
	public class RecipientUnavailable : StanzaException
	{
		/// <summary>
		/// The intended recipient is temporarily unavailable, undergoing maintenance, etc.; the associated error type SHOULD be "wait".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RecipientUnavailable(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Recipient Unavailable." : Message, Stanza)
		{
		}
	}
}
