using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The entity has sent invalid XML over the stream to a server that performs validation (see Section 11.4).
	/// </summary>
	public class InvalidXmlException : StreamException
	{
		/// <summary>
		/// The entity has sent invalid XML over the stream to a server that performs validation (see Section 11.4).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public InvalidXmlException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Invalid XML." : Message, Stanza)
		{
		}
	}
}
