using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource resides temporarily under a different URI. Since the redirection might be altered on occasion, the client SHOULD 
	/// continue to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
	/// </summary>
	public class FoundException : HttpException
	{
		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection might be altered on occasion, the client SHOULD 
		/// continue to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		public FoundException(string Location)
			: base(302, "Found", new KeyValuePair<string, string>("Location", Location))
		{
		}
	}
}
