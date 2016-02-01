using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The response to the request can be found under a different URI and SHOULD be retrieved using a GET method on that resource. This method exists 
	/// primarily to allow the output of a POST-activated script to redirect the user agent to a selected resource. The new URI is not a substitute 
	/// reference for the originally requested resource. The 303 response MUST NOT be cached, but the response to the second (redirected) request might 
	/// be cacheable. 
	/// </summary>
	public class SeeOtherException : HttpException
	{
		/// <summary>
		/// The response to the request can be found under a different URI and SHOULD be retrieved using a GET method on that resource. This method exists 
		/// primarily to allow the output of a POST-activated script to redirect the user agent to a selected resource. The new URI is not a substitute 
		/// reference for the originally requested resource. The 303 response MUST NOT be cached, but the response to the second (redirected) request might 
		/// be cacheable. 
		/// </summary>
		/// <param name="Location">Location.</param>
		public SeeOtherException(string Location)
			: base(303, "See Other", new KeyValuePair<string, string>("Location", Location))
		{
		}
	}
}
