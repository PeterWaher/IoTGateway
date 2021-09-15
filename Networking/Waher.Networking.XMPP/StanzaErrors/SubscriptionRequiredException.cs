using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The requesting entity is not authorized to access the requested service because a prior subscription is necessary (examples of prior
	/// subscription include authorization to receive presence information as defined in [XMPP-IM] and opt-in data feeds for XMPP publish-subscribe
	/// as defined in [XEP-0060]); the associated error type SHOULD be "auth".
	/// </summary>
	public class SubscriptionRequiredException : StanzaAuthExceptionException
	{
		/// <summary>
		/// The requesting entity is not authorized to access the requested service because a prior subscription is necessary (examples of prior
		/// subscription include authorization to receive presence information as defined in [XMPP-IM] and opt-in data feeds for XMPP publish-subscribe
		/// as defined in [XEP-0060]); the associated error type SHOULD be "auth".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public SubscriptionRequiredException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Subscription Required." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "subscription-required"; }
		}
	}
}
