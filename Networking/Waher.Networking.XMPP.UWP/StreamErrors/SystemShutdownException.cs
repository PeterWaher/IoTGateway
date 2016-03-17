using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server is being shut down and all active streams are being closed.
	/// </summary>
	public class SystemShutdownException : StreamException
	{
		/// <summary>
		/// The server is being shut down and all active streams are being closed.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public SystemShutdownException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "System Shutdown." : Message, Stanza)
		{
		}
	}
}
