using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// A stanza sent between two servers lacks a 'to' or 'from' attribute, the 'from' or 'to' attribute has no value, or the value violates the 
	/// rules for XMPP addresses [XMPP-ADDR].
	/// </summary>
	public class ImproperAddressingException : StreamException
	{
		/// <summary>
		/// A stanza sent between two servers lacks a 'to' or 'from' attribute, the 'from' or 'to' attribute has no value, or the value violates the 
		/// rules for XMPP addresses [XMPP-ADDR].
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ImproperAddressingException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Improper Addressing." : Message, Stanza)
		{
		}
	}
}
