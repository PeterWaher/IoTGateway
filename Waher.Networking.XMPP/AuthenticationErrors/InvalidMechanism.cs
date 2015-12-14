using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The initiating entity did not specify a mechanism, or requested a mechanism that is not supported by the receiving entity; sent in
	/// reply to an <auth/> element.
	/// </summary>
	public class InvalidMechanism : AuthenticationException
	{
		/// <summary>
		/// The initiating entity did not specify a mechanism, or requested a mechanism that is not supported by the receiving entity; sent in
		/// reply to an <auth/> element.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public InvalidMechanism(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Invalid Mechanism." : Message, Stanza)
		{
		}
	}
}
