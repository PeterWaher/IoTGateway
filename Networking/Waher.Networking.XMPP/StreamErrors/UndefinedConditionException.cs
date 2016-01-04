using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The error condition is not one of those defined by the other conditions in this list; this error condition SHOULD NOT be used
	/// except in conjunction with an application-specific condition.
	/// </summary>
	public class UndefinedConditionException : StreamException
	{
		/// <summary>
		/// The error condition is not one of those defined by the other conditions in this list; this error condition SHOULD NOT be used
		/// except in conjunction with an application-specific condition.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public UndefinedConditionException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Undefined Condition." : Message, Stanza)
		{
		}
	}
}
