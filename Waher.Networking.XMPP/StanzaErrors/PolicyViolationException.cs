using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The entity has violated some local service policy (e.g., a message contains words that are prohibited by the service) and the server MAY
	/// choose to specify the policy in the <text/> element or in an application-specific condition element; the associated error type
	/// SHOULD be "modify" or "wait" depending on the policy being violated. 
	/// </summary>
	public class PolicyViolationException : StanzaExceptionException
	{
		/// <summary>
		/// The entity has violated some local service policy (e.g., a message contains words that are prohibited by the service) and the server MAY
		/// choose to specify the policy in the <text/> element or in an application-specific condition element; the associated error type
		/// SHOULD be "modify" or "wait" depending on the policy being violated. 
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public PolicyViolationException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Policy Violation." : Message, Stanza)
		{
		}
	}
}
