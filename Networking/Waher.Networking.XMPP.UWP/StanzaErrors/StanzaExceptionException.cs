using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Base class for all stanza exceptions.
	/// </summary>
	public abstract class StanzaExceptionException : XmppException
	{
		/// <summary>
		/// Base class for all stanza exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StanzaExceptionException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}

		/// <summary>
		/// Error Type.
		/// </summary>
		public abstract string ErrorType
		{
			get;
		}

		/// <summary>
		/// Error Stanza Name.
		/// </summary>
		public abstract string ErrorStanzaName
		{
			get;
		}
	}
}
