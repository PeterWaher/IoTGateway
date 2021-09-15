using System;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The authzid provided by the initiating entity is invalid, either because it is incorrectly formatted or because the initiating entity
	/// does not have permissions to authorize that ID; sent in reply to a <response/> element or an <auth/> element with initial response data.
	/// </summary>
	public class InvalidAuthzidException : AuthenticationException
	{
		/// <summary>
		/// The authzid provided by the initiating entity is invalid, either because it is incorrectly formatted or because the initiating entity
		/// does not have permissions to authorize that ID; sent in reply to a <response/> element or an <auth/> element with initial response data.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public InvalidAuthzidException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Invalid Authzid." : Message, Stanza)
		{
		}
	}
}
