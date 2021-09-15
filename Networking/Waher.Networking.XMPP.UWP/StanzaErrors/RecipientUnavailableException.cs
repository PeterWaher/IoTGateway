using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The intended recipient is temporarily unavailable, undergoing maintenance, etc.; the associated error type SHOULD be "wait".
	/// </summary>
	public class RecipientUnavailableException : StanzaWaitExceptionException
	{
		/// <summary>
		/// The intended recipient is temporarily unavailable, undergoing maintenance, etc.; the associated error type SHOULD be "wait".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RecipientUnavailableException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Recipient Unavailable." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "recipient-unavailable"; }
		}
	}
}
