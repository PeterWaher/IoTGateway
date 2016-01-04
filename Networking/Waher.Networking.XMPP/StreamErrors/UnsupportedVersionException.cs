using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The 'version' attribute provided by the initiating entity in the stream header specifies a version of XMPP that is not supported by the server.
	/// </summary>
	public class UnsupportedVersionException : StreamException
	{
		/// <summary>
		/// The 'version' attribute provided by the initiating entity in the stream header specifies a version of XMPP that is not supported by the server.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public UnsupportedVersionException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Unsupported Version." : Message, Stanza)
		{
		}
	}
}
