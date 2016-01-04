using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The data provided by the initiating entity could not be processed because the base 64 encoding is incorrect (e.g., because the encoding
	/// does not adhere to the definition in Section 4 of [BASE64]); sent in reply to a <response/> element or an <auth/> element with initial
	/// response data.
	/// </summary>
	public class IncorrectEncodingException : AuthenticationException
	{
		/// <summary>
		/// The data provided by the initiating entity could not be processed because the base 64 encoding is incorrect (e.g., because the encoding
		/// does not adhere to the definition in Section 4 of [BASE64]); sent in reply to a <response/> element or an <auth/> element with initial
		/// response data.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public IncorrectEncodingException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Incorrect Encoding." : Message, Stanza)
		{
		}
	}
}
