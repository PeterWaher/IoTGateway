using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The entity has violated some local service policy (e.g., a stanza exceeds a configured size limit); the server MAY choose to specify
	/// the policy in the <text/> element or in an application-specific condition element.
	/// </summary>
	public class PolicyViolationException : StreamException
	{
		/// <summary>
		/// The entity has violated some local service policy (e.g., a stanza exceeds a configured size limit); the server MAY choose to specify
		/// the policy in the <text/> element or in an application-specific condition element.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public PolicyViolationException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Policy Violation." : Message, Stanza)
		{
		}
	}
}
