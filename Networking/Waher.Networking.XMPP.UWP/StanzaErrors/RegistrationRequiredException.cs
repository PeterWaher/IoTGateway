using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The requesting entity is not authorized to access the requested service because prior registration is necessary (examples of prior
	/// registration include members-only rooms in XMPP multi-user chat [XEP-0045] and gateways to non-XMPP instant messaging services, which
	/// traditionally required registration in order to use the gateway [XEP-0100]); the associated error type SHOULD be "auth".
	/// </summary>
	public class RegistrationRequiredException : StanzaAuthExceptionException
	{
		/// <summary>
		/// The requesting entity is not authorized to access the requested service because prior registration is necessary (examples of prior
		/// registration include members-only rooms in XMPP multi-user chat [XEP-0045] and gateways to non-XMPP instant messaging services, which
		/// traditionally required registration in order to use the gateway [XEP-0100]); the associated error type SHOULD be "auth".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RegistrationRequiredException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Registration Required." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "registration-required"; }
		}
	}
}
