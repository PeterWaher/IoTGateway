using System;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The account of the initiating entity has been temporarily disabled; sent in reply to an <auth/> element (with or without initial response
	/// data) or a <response/> element.
	/// </summary>
	public class AccountDisabledException : AuthenticationException
	{
		/// <summary>
		/// The account of the initiating entity has been temporarily disabled; sent in reply to an <auth/> element (with or without initial response
		/// data) or a <response/> element.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public AccountDisabledException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Account Disabled." : Message, Stanza)
		{
		}
	}
}
