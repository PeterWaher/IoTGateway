using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server either (1) is closing the existing stream for this entity because a new stream has been initiated that conflicts with the
	/// existing stream, or (2) is refusing a new stream for this entity because allowing the new stream would conflict with an existing
	/// stream (e.g., because the server allows only a certain number of connections from the same IP address or allows only one server-to-
	/// server stream for a given domain pair as a way of helping to ensure in-order processing as described under Section 10.1).
	/// </summary>
	public class ConflictException : StreamException
	{
		/// <summary>
		/// The server either (1) is closing the existing stream for this entity because a new stream has been initiated that conflicts with the
		/// existing stream, or (2) is refusing a new stream for this entity because allowing the new stream would conflict with an existing
		/// stream (e.g., because the server allows only a certain number of connections from the same IP address or allows only one server-to-
		/// server stream for a given domain pair as a way of helping to ensure in-order processing as described under Section 10.1).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ConflictException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Conflict." : Message, Stanza)
		{
		}
	}
}
