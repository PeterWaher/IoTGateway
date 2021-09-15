using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Base class for all stanza exceptions of type 'cancel'.
	/// </summary>
	public abstract class StanzaCancelExceptionException : StanzaExceptionException 
	{
		/// <summary>
		/// Base class for all stanza exceptions of type 'cancel'.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public StanzaCancelExceptionException(string Message, XmlElement Stanza)
			: base(Message, Stanza)
		{
		}

		/// <summary>
		/// Error Type.
		/// </summary>
		public override string ErrorType
		{
			get { return "cancel"; }
		}
	}
}
