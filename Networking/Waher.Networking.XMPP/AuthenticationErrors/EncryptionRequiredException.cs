using System;
using System.Xml;

namespace Waher.Networking.XMPP.AuthenticationErrors
{
	/// <summary>
	/// The mechanism requested by the initiating entity cannot be used unless the confidentiality and integrity of the underlying stream are
	/// protected (typically via TLS); sent in reply to an <auth/> element (with or without initial response data).
	/// </summary>
	public class EncryptionRequiredException : AuthenticationException
	{
		/// <summary>
		/// The mechanism requested by the initiating entity cannot be used unless the confidentiality and integrity of the underlying stream are
		/// protected (typically via TLS); sent in reply to an <auth/> element (with or without initial response data).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public EncryptionRequiredException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Encryption Required." : Message, Stanza)
		{
		}
	}
}
