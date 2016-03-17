using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The value of the 'to' attribute provided in the initial stream header corresponds to an FQDN that is no longer serviced by the receiving entity.
	/// </summary>
	public class HostGoneException : StreamException
	{
		/// <summary>
		/// The value of the 'to' attribute provided in the initial stream header corresponds to an FQDN that is no longer serviced by the receiving entity.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public HostGoneException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Host Gone." : Message, Stanza)
		{
		}
	}
}
