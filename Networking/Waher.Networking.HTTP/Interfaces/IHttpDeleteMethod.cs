using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// DELETE Interface for HTTP resources.
	/// </summary>
	public interface IHttpDeleteMethod
	{
		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		void DELETE(HttpRequest Request, HttpResponse Response);

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		bool AllowsDELETE
		{
			get;
		}
	}
}
