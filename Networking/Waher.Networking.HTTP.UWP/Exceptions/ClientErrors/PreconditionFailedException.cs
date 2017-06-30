using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The precondition given in one or more of the request-header fields evaluated to false when it was tested on the server. This response code 
	/// allows the client to place preconditions on the current resource metainformation (header field data) and thus prevent the requested method 
	/// from being applied to a resource other than the one intended. 
	/// </summary>
	public class PreconditionFailedException : HttpException
	{
		/// <summary>
		/// The precondition given in one or more of the request-header fields evaluated to false when it was tested on the server. This response code 
		/// allows the client to place preconditions on the current resource metainformation (header field data) and thus prevent the requested method 
		/// from being applied to a resource other than the one intended. 
		/// </summary>
		public PreconditionFailedException()
			: base(411, "Precondition Failed")
		{
		}
	}
}
