using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The sender needs to provide credentials before being allowed to perform the action, or has provided improper credentials (the name
	/// "not-authorized", which was borrowed from the "401 Unauthorized" error of [HTTP], might lead the reader to think that this condition
	/// relates to authorization, but instead it is typically used in relation to authentication); the associated error type SHOULD be
	/// "auth".
	/// </summary>
	public class NotAuthorizedException : StanzaExceptionException
	{
		/// <summary>
		/// The sender needs to provide credentials before being allowed to perform the action, or has provided improper credentials (the name
		/// "not-authorized", which was borrowed from the "401 Unauthorized" error of [HTTP], might lead the reader to think that this condition
		/// relates to authorization, but instead it is typically used in relation to authentication); the associated error type SHOULD be
		/// "auth".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public NotAuthorizedException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Not Authorized." : Message, Stanza)
		{
		}
	}
}
