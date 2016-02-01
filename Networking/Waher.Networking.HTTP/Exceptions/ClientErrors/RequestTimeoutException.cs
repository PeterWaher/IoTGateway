using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
	/// modifications at any later time. 
	/// </summary>
	public class RequestTimeoutException : HttpException
	{
		/// <summary>
		/// The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without 
		/// modifications at any later time. 
		/// </summary>
		public RequestTimeoutException()
			: base(408, "Request Timeout")
		{
		}
	}
}
