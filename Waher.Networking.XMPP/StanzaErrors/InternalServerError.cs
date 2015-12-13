using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The server has experienced a misconfiguration or other internal error that prevents it from processing the stanza; the associated error
	/// type SHOULD be "cancel".
	/// </summary>
	public class InternalServerError : StanzaException
	{
		/// <summary>
		/// The server has experienced a misconfiguration or other internal error that prevents it from processing the stanza; the associated error
		/// type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public InternalServerError(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Internal Server Error." : Message, Stanza)
		{
		}
	}
}
