using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Access cannot be granted because an existing resource exists with the same name or address; the associated error type SHOULD be "cancel".
	/// </summary>
	public class ConflictException : StanzaCancelExceptionException
	{
		/// <summary>
		/// Access cannot be granted because an existing resource exists with the same name or address; the associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ConflictException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Conflict." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "conflict"; }
		}
	}
}
