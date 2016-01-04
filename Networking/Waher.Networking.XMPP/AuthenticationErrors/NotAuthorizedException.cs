using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The authentication failed because the initiating entity did not provide proper credentials, or because some generic authentication
	/// failure has occurred but the receiving entity does not wish to disclose specific information about the cause of the failure; sent in
	/// reply to a <response/> element or an <auth/> element with initial response data.
	/// </summary>
	public class NotAuthorizedException : AuthenticationException
	{
		/// <summary>
		/// The authentication failed because the initiating entity did not provide proper credentials, or because some generic authentication
		/// failure has occurred but the receiving entity does not wish to disclose specific information about the cause of the failure; sent in
		/// reply to a <response/> element or an <auth/> element with initial response data.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public NotAuthorizedException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Not Authorized." : Message, Stanza)
		{
		}
	}
}
