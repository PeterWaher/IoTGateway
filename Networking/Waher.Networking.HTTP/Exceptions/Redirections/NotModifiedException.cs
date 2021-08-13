using System;

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
		/// <summary>
		/// 304
		/// </summary>
		public const int Code = 304;

		/// <summary>
		/// Not Modified
		/// </summary>
		public const string StatusMessage = "Not Modified";

		/// <summary>
		/// If the client has performed a conditional GET request and access is allowed, but the document has not been modified, the server SHOULD respond 
		/// with this status code. The 304 response MUST NOT contain a message-body, and thus is always terminated by the first empty line after the header 
		/// fields.
		/// 
		/// Note: 304 does not allow content in response.
		/// </summary>
		public NotModifiedException()
			: base(Code, StatusMessage)
		{
		}
	}
}
