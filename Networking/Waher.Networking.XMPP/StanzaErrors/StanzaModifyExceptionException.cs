using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Base class for all stanza exceptions of type 'modify'.
	/// </summary>
	public abstract class StanzaModifyExceptionException : StanzaExceptionException 
	{
		/// <summary>
		/// Base class for all stanza exceptions of type 'modify'.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StanzaModifyExceptionException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}

		/// <summary>
		/// Error Type.
		/// </summary>
		public override string ErrorType
		{
			get { return "modify"; }
		}
	}
}
