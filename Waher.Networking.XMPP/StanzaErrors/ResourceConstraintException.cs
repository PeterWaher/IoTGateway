using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The server or recipient is busy or lacks the system resources necessary to service the request; the associated error type SHOULD be "wait".
	/// </summary>
	public class ResourceConstraintException : StanzaExceptionException
	{
		/// <summary>
		/// The server or recipient is busy or lacks the system resources necessary to service the request; the associated error type SHOULD be "wait".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ResourceConstraintException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Resource Constraint." : Message, Stanza)
		{
		}
	}
}
