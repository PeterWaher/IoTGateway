using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// PUT Interface for HTTP resources.
	/// </summary>
	public interface IHttpPutMethod
	{
		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task PUT(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		bool AllowsPUT
		{
			get;
		}
	}
}
