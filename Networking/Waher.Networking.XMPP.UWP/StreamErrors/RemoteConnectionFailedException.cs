using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server is unable to properly connect to a remote entity that is needed for authentication or authorization (e.g., in certain
	/// scenarios related to Server Dialback [XEP-0220]); this condition is not to be used when the cause of the error is within the
	/// administrative domain of the XMPP service provider, in which case the <internal-server-error/> condition is more appropriate.
	/// </summary>
	public class RemoteConnectionFailedException : StreamException
	{
		/// <summary>
		/// The server is unable to properly connect to a remote entity that is needed for authentication or authorization (e.g., in certain
		/// scenarios related to Server Dialback [XEP-0220]); this condition is not to be used when the cause of the error is within the
		/// administrative domain of the XMPP service provider, in which case the <internal-server-error/> condition is more appropriate.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RemoteConnectionFailedException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Remote Connection Failed." : Message, Stanza)
		{
		}
	}
}
