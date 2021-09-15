using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The server or recipient does not currently provide the requested service; the associated error type SHOULD be "cancel".
	/// </summary>
	public class ServiceUnavailableException : StanzaCancelExceptionException
	{
		/// <summary>
		/// The server or recipient does not currently provide the requested service; the associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ServiceUnavailableException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Service Unavailable." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "service-unavailable"; }
		}
	}
}
