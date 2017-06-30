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
		/// <summary>
		/// The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server specified by the URI 
		/// (e.g. HTTP, FTP, LDAP) or some other auxiliary server (e.g. DNS) it needed to access in attempting to complete the request. 
		/// </summary>
		public GatewayTimeoutException()
			: base(504, "Gateway Timeout")
		{
		}
	}
}
