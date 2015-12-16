using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The recipient or server is redirecting requests for this information to another entity, typically in a temporary fashion (as opposed to
	/// the <gone/> error condition, which is used for permanent addressing failures); the associated error type SHOULD be "modify" and the error
	/// stanza SHOULD contain the alternate address in the XML character data of the <redirect/> element (which MUST be a URI or IRI with which the
	/// sender can communicate, typically an XMPP IRI as specified in [XMPP-URI]).
	/// </summary>
	public class RedirectException : StanzaExceptionException
	{
		/// <summary>
		/// The recipient or server is redirecting requests for this information to another entity, typically in a temporary fashion (as opposed to
		/// the <gone/> error condition, which is used for permanent addressing failures); the associated error type SHOULD be "modify" and the error
		/// stanza SHOULD contain the alternate address in the XML character data of the <redirect/> element (which MUST be a URI or IRI with which the
		/// sender can communicate, typically an XMPP IRI as specified in [XMPP-URI]).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RedirectException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Redirect." : Message, Stanza)
		{
		}
	}
}
