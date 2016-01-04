using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The receiving entity has advertised a mandatory-to-negotiate stream feature that the initiating entity does not support, and has offered
	/// no other mandatory-to-negotiate feature alongside the unsupported feature.
	/// </summary>
	public class UnsupportedFeatureException : StreamException
	{
		/// <summary>
		/// The receiving entity has advertised a mandatory-to-negotiate stream feature that the initiating entity does not support, and has offered
		/// no other mandatory-to-negotiate feature alongside the unsupported feature.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public UnsupportedFeatureException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Unsupported Encoding." : Message, Stanza)
		{
		}
	}
}
