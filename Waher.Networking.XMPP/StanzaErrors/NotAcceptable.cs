using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The recipient or server understands the request but cannot process it because the request does not meet criteria defined by the recipient
	/// or server (e.g., a request to subscribe to information that does not simultaneously include configuration parameters needed by the
	/// recipient); the associated error type SHOULD be "modify".
	/// </summary>
	public class NotAcceptable : StanzaException
	{
		/// <summary>
		/// The recipient or server understands the request but cannot process it because the request does not meet criteria defined by the recipient
		/// or server (e.g., a request to subscribe to information that does not simultaneously include configuration parameters needed by the
		/// recipient); the associated error type SHOULD be "modify".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public NotAcceptable(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Not Acceptable." : Message, Stanza)
		{
		}
	}
}
