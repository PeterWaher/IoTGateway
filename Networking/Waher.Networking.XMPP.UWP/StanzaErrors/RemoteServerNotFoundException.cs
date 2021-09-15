using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// A remote server or service specified as part or all of the JID of the intended recipient does not exist or cannot be resolved (e.g., there
	/// is no _xmpp-server._tcp DNS SRV record, the A or AAAA fallback resolution fails, or A/AAAA lookups succeed but there is no response
	/// on the IANA-registered port 5269); the associated error type SHOULD be "cancel".
	/// </summary>
	public class RemoteServerNotFoundException : StanzaCancelExceptionException
	{
		/// <summary>
		/// A remote server or service specified as part or all of the JID of the intended recipient does not exist or cannot be resolved (e.g., there
		/// is no _xmpp-server._tcp DNS SRV record, the A or AAAA fallback resolution fails, or A/AAAA lookups succeed but there is no response
		/// on the IANA-registered port 5269); the associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RemoteServerNotFoundException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Remote Server Not Found." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "remote-server-not-found"; }
		}
	}
}
