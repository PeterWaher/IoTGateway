using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Ranged PUT Interface for HTTP resources.
	/// </summary>
	public interface IHttpPutRangesMethod
	{
		/// <summary>
		/// Executes the ranged PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		void PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval);
	}
}
