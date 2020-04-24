using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
	/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
	/// fields.
	/// 
	/// Note: 304 does not allow content in response.
	/// </summary>
	public class NotModifiedException : HttpException
	{
		private const int Code = 304;
		private const string Msg = "Not Modified";

		/// <summary>
		/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
		/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
		/// fields.
		/// 
		/// Note: 304 does not allow content in response.
		/// </summary>
		public NotModifiedException()
			: base(Code, Msg)
		{
		}
	}
}
