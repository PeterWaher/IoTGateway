using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The initiating entity has sent a first-level child of the stream that is not supported by the server, either because the receiving entity
	/// does not understand the namespace or because the receiving entity does not understand the element name for the applicable namespace
	/// (which might be the content namespace declared as the default namespace).
	/// </summary>
	public class UnsupportedStanzaTypeException : StreamException
	{
		/// <summary>
		/// The initiating entity has sent a first-level child of the stream that is not supported by the server, either because the receiving entity
		/// does not understand the namespace or because the receiving entity does not understand the element name for the applicable namespace
		/// (which might be the content namespace declared as the default namespace).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public UnsupportedStanzaTypeException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Unsupported Stanza Type." : Message, Stanza)
		{
		}
	}
}
