using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server lacks the system resources necessary to service the stream.
	/// </summary>
	public class ResourceConstraintException : StreamException
	{
		/// <summary>
		/// The server lacks the system resources necessary to service the stream.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ResourceConstraintException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Resource Constraint." : Message, Stanza)
		{
		}
	}
}
