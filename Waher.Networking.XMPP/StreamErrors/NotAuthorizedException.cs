using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The entity has attempted to send XML stanzas or other outbound data before the stream has been authenticated, or otherwise is not
	/// authorized to perform an action related to stream negotiation; the receiving entity MUST NOT process the offending data before sending
	/// the stream error.
	/// </summary>
	public class NotAuthorizedException : XmppException
	{
		/// <summary>
		/// The entity has attempted to send XML stanzas or other outbound data before the stream has been authenticated, or otherwise is not
		/// authorized to perform an action related to stream negotiation; the receiving entity MUST NOT process the offending data before sending
		/// the stream error.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public NotAuthorizedException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Not Authorized." : Message, Stanza)
		{
		}
	}
}
