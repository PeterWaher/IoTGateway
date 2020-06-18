using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Ranged POST Interface for HTTP resources.
	/// </summary>
	public interface IHttpPostRangesMethod
	{
		/// <summary>
		/// Executes the ranged POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		Task POST(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval);

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		bool AllowsPOST
		{
			get;
		}
	}
}
