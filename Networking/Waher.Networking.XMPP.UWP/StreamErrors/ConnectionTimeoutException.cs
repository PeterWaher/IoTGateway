using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// One party is closing the stream because it has reason to believe that the other party has permanently lost the ability to communicate over
	/// the stream.  The lack of ability to communicate can be discovered using various methods, such as whitespace keepalives as specified
	/// under Section 4.4, XMPP-level pings as defined in [XEP-0199], and XMPP Stream Management as defined in [XEP-0198].
	/// </summary>
	public class ConnectionTimeoutException : StreamException
	{
		/// <summary>
		/// One party is closing the stream because it has reason to believe that the other party has permanently lost the ability to communicate over
		/// the stream.  The lack of ability to communicate can be discovered using various methods, such as whitespace keepalives as specified
		/// under Section 4.4, XMPP-level pings as defined in [XEP-0199], and XMPP Stream Management as defined in [XEP-0198].
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ConnectionTimeoutException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Connection Timeout." : Message, Stanza)
		{
		}
	}
}
