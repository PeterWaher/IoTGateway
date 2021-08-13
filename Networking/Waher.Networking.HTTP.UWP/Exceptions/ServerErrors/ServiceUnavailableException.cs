using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server is currently unable to handle the request due to a temporary overloading or maintenance of the server. The implication is that 
	/// this is a temporary condition which will be alleviated after some delay. If known, the length of the delay MAY be indicated in a Retry-After 
	/// header. If no Retry-After is given, the client SHOULD handle the response as it would for a 500 response. 
	/// </summary>
	public class ServiceUnavailableException : HttpException
	{
		/// <summary>
		/// 503
		/// </summary>
		public const int Code = 503;

		/// <summary>
		/// Service Unavailable
		/// </summary>
		public const string StatusMessage = "Service Unavailable";

		/// <summary>
		/// The server is currently unable to handle the request due to a temporary overloading or maintenance of the server. The implication is that 
		/// this is a temporary condition which will be alleviated after some delay. If known, the length of the delay MAY be indicated in a Retry-After 
		/// header. If no Retry-After is given, the client SHOULD handle the response as it would for a 500 response. 
		/// </summary>
		public ServiceUnavailableException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The server is currently unable to handle the request due to a temporary overloading or maintenance of the server. The implication is that 
		/// this is a temporary condition which will be alleviated after some delay. If known, the length of the delay MAY be indicated in a Retry-After 
		/// header. If no Retry-After is given, the client SHOULD handle the response as it would for a 500 response. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public ServiceUnavailableException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The server is currently unable to handle the request due to a temporary overloading or maintenance of the server. The implication is that 
		/// this is a temporary condition which will be alleviated after some delay. If known, the length of the delay MAY be indicated in a Retry-After 
		/// header. If no Retry-After is given, the client SHOULD handle the response as it would for a 500 response. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public ServiceUnavailableException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
