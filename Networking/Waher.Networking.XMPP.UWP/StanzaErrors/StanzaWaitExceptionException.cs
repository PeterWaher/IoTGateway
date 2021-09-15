using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Base class for all stanza exceptions of type 'wait'.
	/// </summary>
	public abstract class StanzaWaitExceptionException : StanzaExceptionException 
	{
		/// <summary>
		/// Base class for all stanza exceptions of type 'wait'.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StanzaWaitExceptionException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}

		/// <summary>
		/// Error Type.
		/// </summary>
		public override string ErrorType
		{
			get { return "wait"; }
		}
	}
}
