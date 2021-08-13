using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
	/// fulfill the request. 
	/// </summary>
	public class BadGatewayException : HttpException
	{
		/// <summary>
		/// 502
		/// </summary>
		public const int Code = 502;

		/// <summary>
		/// Bad Gateway
		/// </summary>
		public const string StatusMessage = "Bad Gateway";

		/// <summary>
		/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
		/// fulfill the request. 
		/// </summary>
		public BadGatewayException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
		/// fulfill the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public BadGatewayException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
		/// fulfill the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public BadGatewayException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
