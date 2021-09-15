using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The recipient or server can no longer be contacted at this address, typically on a permanent basis (as opposed to the <redirect/> error
	/// condition, which is used for temporary addressing failures); the associated error type SHOULD be "cancel" and the error stanza SHOULD
	/// include a new address (if available) as the XML character data of the <gone/> element (which MUST be a Uniform Resource Identifier [URI] or
	/// Internationalized Resource Identifier [IRI] at which the entity can be contacted, typically an XMPP IRI as specified in [XMPP-URI]).
	/// </summary>
	public class GoneException : StanzaCancelExceptionException
	{
		/// <summary>
		/// The recipient or server can no longer be contacted at this address, typically on a permanent basis (as opposed to the <redirect/> error
		/// condition, which is used for temporary addressing failures); the associated error type SHOULD be "cancel" and the error stanza SHOULD
		/// include a new address (if available) as the XML character data of the <gone/> element (which MUST be a Uniform Resource Identifier [URI] or
		/// Internationalized Resource Identifier [IRI] at which the entity can be contacted, typically an XMPP IRI as specified in [XMPP-URI]).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public GoneException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Gone." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "gone"; }
		}
	}
}
