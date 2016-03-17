using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Base class for all stanza exceptions of type 'auth'.
	/// </summary>
	public abstract class StanzaAuthExceptionException : StanzaExceptionException 
	{
		/// <summary>
		/// Base class for all stanza exceptions of type 'auth'.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StanzaAuthExceptionException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}

		/// <summary>
		/// Error Type.
		/// </summary>
		public override string ErrorType
		{
			get { return "auth"; }
		}
	}
}
