using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// POST Interface for HTTP resources.
	/// </summary>
	public interface IHttpPostMethod
	{
		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		void POST(HttpRequest Request, HttpResponse Response);
	}
}
