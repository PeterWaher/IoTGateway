using System;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// Base class for all autentication exceptions.
	/// </summary>
	public abstract class AuthenticationException : XmppException
	{
		/// <summary>
		/// Base class for all autentication exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public AuthenticationException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}
	}
}
