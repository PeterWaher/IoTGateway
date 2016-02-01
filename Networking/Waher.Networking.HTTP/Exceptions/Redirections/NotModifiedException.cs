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
		/// <summary>
		/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
		/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
		/// fields.
		/// </summary>
		public NotModifiedException()
			: base(304, "Not Modified")
		{
		}
	}
}
