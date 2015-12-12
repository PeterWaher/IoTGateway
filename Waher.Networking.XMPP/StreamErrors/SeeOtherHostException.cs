using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server will not provide service to the initiating entity but is redirecting traffic to another host under the administrative control
	/// of the same service provider.  The XML character data of the <see-other-host/> element returned by the server MUST specify the
	/// alternate FQDN or IP address at which to connect, which MUST be a valid domainpart or a domainpart plus port number (separated by the
	/// ':' character in the form "domainpart:port").  If the domainpart is the same as the source domain, derived domain, or resolved IPv4 or
	/// IPv6 address to which the initiating entity originally connected (differing only by the port number), then the initiating entity
	/// SHOULD simply attempt to reconnect at that address.  (The format of an IPv6 address MUST follow [IPv6-ADDR], which includes the enclosing
	/// the IPv6 address in square brackets '[' and ']' as originally defined by [URI].)  Otherwise, the initiating entity MUST resolve the FQDN
	/// specified in the <see-other-host/> element as described under Section 3.2.
	/// </summary>
	public class SeeOtherHostException : XmppException
	{
		/// <summary>
		/// The server will not provide service to the initiating entity but is redirecting traffic to another host under the administrative control
		/// of the same service provider.  The XML character data of the <see-other-host/> element returned by the server MUST specify the
		/// alternate FQDN or IP address at which to connect, which MUST be a valid domainpart or a domainpart plus port number (separated by the
		/// ':' character in the form "domainpart:port").  If the domainpart is the same as the source domain, derived domain, or resolved IPv4 or
		/// IPv6 address to which the initiating entity originally connected (differing only by the port number), then the initiating entity
		/// SHOULD simply attempt to reconnect at that address.  (The format of an IPv6 address MUST follow [IPv6-ADDR], which includes the enclosing
		/// the IPv6 address in square brackets '[' and ']' as originally defined by [URI].)  Otherwise, the initiating entity MUST resolve the FQDN
		/// specified in the <see-other-host/> element as described under Section 3.2.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public SeeOtherHostException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "See Other Host." : Message, Stanza)
		{
		}
	}
}
