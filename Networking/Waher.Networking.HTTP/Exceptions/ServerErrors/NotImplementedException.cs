using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server does not support the functionality required to fulfill the request. This is the appropriate response when the server does not 
	/// recognize the request method and is not capable of supporting it for any resource. 
	/// </summary>
	public class NotImplementedException : HttpException
	{
		/// <summary>
		/// The server does not support the functionality required to fulfill the request. This is the appropriate response when the server does not 
		/// recognize the request method and is not capable of supporting it for any resource. 
		/// </summary>
		public NotImplementedException()
			: base(501, "Not Implemented")
		{
		}
	}
}
