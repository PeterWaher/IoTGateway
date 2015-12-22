using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// The feature represented in the XML stanza is not implemented by the intended recipient or an intermediate server and therefore the stanza
	/// cannot be processed (e.g., the entity understands the namespace but does not recognize the element name); the associated error type
	/// SHOULD be "cancel" or "modify".
	/// </summary>
	public class FeatureNotImplementedException : StanzaCancelExceptionException
	{
		/// <summary>
		/// The feature represented in the XML stanza is not implemented by the intended recipient or an intermediate server and therefore the stanza
		/// cannot be processed (e.g., the entity understands the namespace but does not recognize the element name); the associated error type
		/// SHOULD be "cancel" or "modify".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public FeatureNotImplementedException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Feature Not Implemented." : Message, Stanza)
		{
		}

		/// <summary>
		/// <see cref="StanzaExceptionException.ErrorStanzaName"/>
		/// </summary>
		public override string ErrorStanzaName
		{
			get { return "feature-not-implemented"; }
		}
	}
}
