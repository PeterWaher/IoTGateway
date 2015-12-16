using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The request is malformed (e.g., the <auth/> element includes initial response data but the mechanism does not allow that, or the data sent
	/// violates the syntax for the specified SASL mechanism); sent in reply to an <abort/>, <auth/>, <challenge/>, or <response/> element.
	/// </summary>
	public class MalformedRequestException : AuthenticationException
	{
		/// <summary>
		/// The request is malformed (e.g., the <auth/> element includes initial response data but the mechanism does not allow that, or the data sent
		/// violates the syntax for the specified SASL mechanism); sent in reply to an <abort/>, <auth/>, <challenge/>, or <response/> element.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public MalformedRequestException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Malformed Request." : Message, Stanza)
		{
		}
	}
}
