using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The value of the 'to' attribute provided in the initial stream header does not correspond to an FQDN that is serviced by the receiving entity.
	/// </summary>
	public class HostUnknownException : StreamException
	{
		/// <summary>
		/// The value of the 'to' attribute provided in the initial stream header does not correspond to an FQDN that is serviced by the receiving entity.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public HostUnknownException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Host Unknown." : Message, Stanza)
		{
		}
	}
}
