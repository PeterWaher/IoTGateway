using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
	/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
	/// fields.
	/// </summary>
	public class NotModifiedException : HttpException
	{
		private const int Code = 304;
		private const string Msg = "Not Modified";

		/// <summary>
		/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
		/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
		/// fields.
		/// </summary>
		public NotModifiedException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
		/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
		/// fields.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public NotModifiedException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
		/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
		/// fields.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public NotModifiedException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
