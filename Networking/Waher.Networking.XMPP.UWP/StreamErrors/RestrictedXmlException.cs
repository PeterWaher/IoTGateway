using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The entity has attempted to send restricted XML features such as a comment, processing instruction, DTD subset, or XML entity reference
	/// (see Section 11.1).
	/// </summary>
	public class RestrictedXmlException : StreamException
	{
		/// <summary>
		/// The entity has attempted to send restricted XML features such as a comment, processing instruction, DTD subset, or XML entity reference
		/// (see Section 11.1).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public RestrictedXmlException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Restricted XML." : Message, Stanza)
		{
		}
	}
}
