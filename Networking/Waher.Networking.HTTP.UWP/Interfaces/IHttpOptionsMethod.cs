using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// OPTIONS Interface for HTTP resources.
	/// </summary>
	public interface IHttpOptionsMethod
	{
		/// <summary>
		/// Executes the OPTIONS method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		void OPTIONS(HttpRequest Request, HttpResponse Response);
	}
}
