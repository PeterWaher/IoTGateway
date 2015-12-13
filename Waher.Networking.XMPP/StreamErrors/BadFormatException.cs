using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The entity has sent XML that cannot be processed.
	/// </summary>
	public class BadFormatException : StreamException
	{
		/// <summary>
		/// The entity has sent XML that cannot be processed.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public BadFormatException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Bad format." : Message, Stanza)
		{
		}
	}
}
