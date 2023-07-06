using System.Collections.Generic;

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
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public RequestTimeoutException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
		/// modifications at any later time. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public RequestTimeoutException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
		/// modifications at any later time. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public RequestTimeoutException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
