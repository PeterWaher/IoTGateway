using System;
using System.Xml;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Base class of XMPP exceptions
	/// </summary>
	public class XmppException : Exception
	{
		private readonly XmlElement stanza;

		/// <summary>
		/// Base class of XMPP exceptions
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public XmppException(string Message)
			: base(Message)
		{
			this.stanza = null;
		}

		/// <summary>
		/// Base class of XMPP exceptions
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public XmppException(string Message, XmlElement Stanza)
			: base(Message)
		{
			this.stanza = Stanza;
		}

		/// <summary>
		/// Stanza causing exception.
		/// </summary>
		public XmlElement Stanza
		{
			get { return this.stanza; }
		}
	}
}
