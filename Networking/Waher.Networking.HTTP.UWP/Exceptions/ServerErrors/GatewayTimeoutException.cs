using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
	/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
	/// </summary>
	public class GatewayTimeoutException : HttpException
	{
		private const int Code = 504;
		private const string Msg = "Gateway Timeout";

		/// <summary>
		/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
		/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
		/// </summary>
		public GatewayTimeoutException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
		/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public GatewayTimeoutException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
		/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public GatewayTimeoutException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
