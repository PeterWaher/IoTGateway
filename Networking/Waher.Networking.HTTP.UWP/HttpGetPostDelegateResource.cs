using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// HTTP resource defined by GET and POST delegate methods.
	/// </summary>
	public class HttpGetPostDelegateResource : HttpGetDelegateResource, IHttpPostMethod
	{
		private readonly HttpMethodHandlerAsync post;

		/// <summary>
		/// HTTP resource defined by GET and POST delegate methods.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="GET">GET Method.</param>
		/// <param name="POST">POST Method.</param>
		/// <param name="Synchronous">If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).</param>
		/// <param name="HandlesSubPaths">If sub-paths are handled.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpGetPostDelegateResource(string ResourceName, HttpMethodHandlerAsync GET, HttpMethodHandlerAsync POST, bool Synchronous, bool HandlesSubPaths,
			bool UserSessions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName, GET, Synchronous, HandlesSubPaths, UserSessions, AuthenticationSchemes)
		{
			this.post = POST;
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST
		{
			get
			{
				return this.post != null;
			}
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (this.post is null)
				throw new MethodNotAllowedException(this.AllowedMethods);
			else
				return this.post(Request, Response);
		}
	}
}
