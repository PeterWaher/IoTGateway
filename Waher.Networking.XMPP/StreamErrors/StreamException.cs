using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// Base class for all stream exceptions.
	/// </summary>
	public abstract class StreamException : XmppException
	{
		/// <summary>
		/// Base class for all stream exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StreamException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}
	}
}
