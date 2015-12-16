using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The recipient or server understood the request but was not expecting it at this time (e.g., the request was out of order); the associated
	/// error type SHOULD be "wait" or "modify".
	/// </summary>
	public class UnexpectedRequestException : StanzaExceptionException
	{
		/// <summary>
		/// The recipient or server understood the request but was not expecting it at this time (e.g., the request was out of order); the associated
		/// error type SHOULD be "wait" or "modify".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public UnexpectedRequestException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Unexpected Request." : Message, Stanza)
		{
		}
	}
}
