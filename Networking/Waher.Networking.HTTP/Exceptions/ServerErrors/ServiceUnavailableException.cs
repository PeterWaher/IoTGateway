using System;
using System.Collections.Generic;
using System.Text;

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
		/// The server is currently unable to handle the request due to a temporary overloading or maintenance of the server. The implication is that 
		/// this is a temporary condition which will be alleviated after some delay. If known, the length of the delay MAY be indicated in a Retry-After 
		/// header. If no Retry-After is given, the client SHOULD handle the response as it would for a 500 response. 
		/// </summary>
		public ServiceUnavailableException()
			: base(503, "Service Unavailable")
		{
		}
	}
}
