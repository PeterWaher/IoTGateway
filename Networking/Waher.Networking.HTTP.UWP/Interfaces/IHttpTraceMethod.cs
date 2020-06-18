using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// TRACE Interface for HTTP resources.
	/// </summary>
	public interface IHttpTraceMethod
	{
		/// <summary>
		/// Executes the TRACE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task TRACE(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the TRACE method is allowed.
		/// </summary>
		bool AllowsTRACE
		{
			get;
		}
	}
}
