using System.Collections.Generic;

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
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public BadGatewayException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
		/// fulfill the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public BadGatewayException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
		/// fulfill the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public BadGatewayException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
