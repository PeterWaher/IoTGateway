using System;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The authentication failed because of a temporary error condition within the receiving entity, and it is advisable for the initiating
	/// entity to try again later; sent in reply to an <auth/> element or a <response/> element.
	/// </summary>
	public class TemporaryAuthFailureException : AuthenticationException
	{
		/// <summary>
		/// The authentication failed because of a temporary error condition within the receiving entity, and it is advisable for the initiating
		/// entity to try again later; sent in reply to an <auth/> element or a <response/> element.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public TemporaryAuthFailureException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Temporary Authentication Failure." : Message, Stanza)
		{
		}
	}
}
