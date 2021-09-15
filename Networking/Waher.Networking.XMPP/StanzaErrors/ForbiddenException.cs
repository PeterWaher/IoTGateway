using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The requesting entity does not possess the necessary permissions to perform an action that only certain authorized roles or individuals
	/// are allowed to complete (i.e., it typically relates to authorization rather than authentication); the associated error type SHOULD be
	/// "auth".
	/// </summary>
	public class ForbiddenException : StanzaAuthExceptionException
	{
		/// <summary>
		/// The requesting entity does not possess the necessary permissions to perform an action that only certain authorized roles or individuals
		/// are allowed to complete (i.e., it typically relates to authorization rather than authentication); the associated error type SHOULD be
		/// "auth".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ForbiddenException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Forbidden." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "forbidden"; }
		}
	}
}
