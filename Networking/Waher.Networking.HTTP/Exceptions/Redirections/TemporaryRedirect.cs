using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource resides temporarily under a different URI. Since the redirection MAY be altered on occasion, the client SHOULD continue 
	/// to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
	/// </summary>
	public class TemporaryRedirect : HttpException
	{
		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection MAY be altered on occasion, the client SHOULD continue 
		/// to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		public TemporaryRedirect(string Location)
			: base(307, "Temporary Redirect", new KeyValuePair<string, string>("Location", Location))
		{
		}
	}
}
