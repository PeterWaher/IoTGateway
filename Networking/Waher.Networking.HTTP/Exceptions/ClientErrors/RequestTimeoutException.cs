using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
	/// modifications at any later time. 
	/// </summary>
	public class RequestTimeoutException : HttpException
	{
		/// <summary>
		/// 408
		/// </summary>
		public const int Code = 408;

		/// <summary>
		/// Request Timeout
		/// </summary>
		public const string StatusMessage = "Request Timeout";

		/// <summary>
		/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
		/// modifications at any later time. 
		/// </summary>
		public RequestTimeoutException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
		/// modifications at any later time. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public RequestTimeoutException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
		/// modifications at any later time. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public RequestTimeoutException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
