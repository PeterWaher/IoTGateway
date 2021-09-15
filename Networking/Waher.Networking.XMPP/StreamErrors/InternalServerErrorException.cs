using System;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server has experienced a misconfiguration or other internal error that prevents it from servicing the stream
	/// </summary>
	public class InternalServerErrorException : StreamException
	{
		/// <summary>
		/// The server has experienced a misconfiguration or other internal error that prevents it from servicing the stream
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public InternalServerErrorException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Internal Server Error." : Message, Stanza)
		{
		}
	}
}
