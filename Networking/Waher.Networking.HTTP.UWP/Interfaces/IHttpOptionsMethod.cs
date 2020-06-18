using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		Task OPTIONS(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the OPTIONS method is allowed.
		/// </summary>
		bool AllowsOPTIONS
		{
			get;
		}
	}
}
