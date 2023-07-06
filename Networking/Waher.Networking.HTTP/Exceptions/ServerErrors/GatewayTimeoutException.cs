using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
	/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
	/// </summary>
	public class GatewayTimeoutException : HttpException
	{
		/// <summary>
		/// 504
		/// </summary>
		public const int Code = 504;

		/// <summary>
		/// Gateway Timeout
		/// </summary>
		public const string StatusMessage = "Gateway Timeout";

		/// <summary>
		/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
		/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public GatewayTimeoutException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
		/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public GatewayTimeoutException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
		/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public GatewayTimeoutException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
