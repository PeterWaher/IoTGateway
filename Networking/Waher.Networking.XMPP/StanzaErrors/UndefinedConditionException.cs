using System;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The error condition is not one of those defined by the other conditions in this list; any error type can be associated with this
	/// condition, and it SHOULD NOT be used except in conjunction with an application-specific condition.
	/// </summary>
	public class UndefinedConditionException : StanzaCancelExceptionException
	{
		/// <summary>
		/// The error condition is not one of those defined by the other conditions in this list; any error type can be associated with this
		/// condition, and it SHOULD NOT be used except in conjunction with an application-specific condition.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public UndefinedConditionException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Undefined Condition." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "undefined-condition"; }
		}
	}
}
