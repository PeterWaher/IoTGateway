using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The recipient or server does not allow any entity to perform the action (e.g., sending to entities at a blacklisted domain); the
	/// associated error type SHOULD be "cancel".
	/// </summary>
	public class NotAllowed : StanzaException
	{
		/// <summary>
		/// The recipient or server does not allow any entity to perform the action (e.g., sending to entities at a blacklisted domain); the
		/// associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public NotAllowed(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Not Allowed." : Message, Stanza)
		{
		}
	}
}
