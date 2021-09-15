using System;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The authentication failed because the initiating entity provided credentials that have expired; sent in reply to a <response/> element
	/// or an <auth/> element with initial response data.
	/// </summary>
	public class CredentialsExpiredException : AuthenticationException
	{
		/// <summary>
		/// The authentication failed because the initiating entity provided credentials that have expired; sent in reply to a <response/> element
		/// or an <auth/> element with initial response data.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public CredentialsExpiredException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Credentials Expired." : Message, Stanza)
		{
		}
	}
}
