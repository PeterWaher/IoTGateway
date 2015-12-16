using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The sending entity has provided (e.g., during resource binding) or communicated (e.g., in the 'to' address of a stanza) an XMPP address
	/// or aspect thereof that violates the rules defined in [XMPP-ADDR]; the associated error type SHOULD be "modify".
	/// </summary>
	public class JidMalformedException : StanzaExceptionException
	{
		/// <summary>
		/// The sending entity has provided (e.g., during resource binding) or communicated (e.g., in the 'to' address of a stanza) an XMPP address
		/// or aspect thereof that violates the rules defined in [XMPP-ADDR]; the associated error type SHOULD be "modify".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public JidMalformedException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "JID Malformed." : Message, Stanza)
		{
		}
	}
}
