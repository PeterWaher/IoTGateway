using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The sender has sent a stanza containing XML that does not conform to the appropriate schema or that cannot be processed (e.g., an IQ
	/// stanza that includes an unrecognized value of the 'type' attribute, or an element that is qualified by a recognized namespace but that
	/// violates the defined syntax for the element); the associated error type SHOULD be "modify".
	/// </summary>
	public class BadRequestException : StanzaExceptionException
	{
		/// <summary>
		/// The sender has sent a stanza containing XML that does not conform to the appropriate schema or that cannot be processed (e.g., an IQ
		/// stanza that includes an unrecognized value of the 'type' attribute, or an element that is qualified by a recognized namespace but that
		/// violates the defined syntax for the element); the associated error type SHOULD be "modify".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public BadRequestException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Bad Request." : Message, Stanza)
		{
		}
	}
}
