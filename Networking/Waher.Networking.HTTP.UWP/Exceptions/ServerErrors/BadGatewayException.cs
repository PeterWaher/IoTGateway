using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
	/// fulfill the request. 
	/// </summary>
	public class BadGatewayException : HttpException
	{
		/// <summary>
		/// The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to 
		/// fulfill the request. 
		/// </summary>
		public BadGatewayException()
			: base(502, "Bad Gateway")
		{
		}
	}
}
