using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The initiating entity has sent XML that violates the well-formedness rules of [XML] or [XML-NAMES].
	/// </summary>
	public class NotWellFormedException : XmppException
	{
		/// <summary>
		/// The initiating entity has sent XML that violates the well-formedness rules of [XML] or [XML-NAMES].
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public NotWellFormedException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Not Well Formed." : Message, Stanza)
		{
		}
	}
}
