using System;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	///  The mechanism requested by the initiating entity is weaker than server policy permits for that initiating entity; sent in reply to an
	///  <auth/> element (with or without initial response data).
	/// </summary>
	public class MechanismTooWeakException : AuthenticationException
	{
		/// <summary>
		///  The mechanism requested by the initiating entity is weaker than server policy permits for that initiating entity; sent in reply to an
		///  <auth/> element (with or without initial response data).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public MechanismTooWeakException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Mechanism Too Weak." : Message, Stanza)
		{
		}
	}
}
