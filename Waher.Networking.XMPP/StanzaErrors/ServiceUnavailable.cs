using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The server or recipient does not currently provide the requested service; the associated error type SHOULD be "cancel".
	/// </summary>
	public class ServiceUnavailable : StanzaException
	{
		/// <summary>
		/// The server or recipient does not currently provide the requested service; the associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ServiceUnavailable(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Service Unavailable." : Message, Stanza)
		{
		}
	}
}
