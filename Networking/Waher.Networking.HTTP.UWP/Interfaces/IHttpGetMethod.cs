using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// GET Interface for HTTP resources.
	/// </summary>
	public interface IHttpGetMethod
	{
		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task GET(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		bool AllowsGET
		{
			get;
		}
	}
}
