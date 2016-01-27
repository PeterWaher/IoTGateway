using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// HTTP resource defined by a GET delegate method.
	/// </summary>
	public class HttpGetDelegateResource : HttpResource, IHttpGetMethod
	{
		private HttpMethodHandler get;
		private bool synchronous;
		private bool handlesSubPaths;

		/// <summary>
		/// HTTP resource defined by a GET delegate method.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="GET">GET Method.</param>
		/// <param name="Synchronous">If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).</param>
		/// <param name="HandlesSubPaths">If sub-paths are handled.</param>
		public HttpGetDelegateResource(string ResourceName, HttpMethodHandler GET, bool Synchronous, bool HandlesSubPaths)
			: base(ResourceName)
		{
			this.get = GET;
			this.synchronous = Synchronous;
			this.handlesSubPaths = HandlesSubPaths;
		}

		/// <summary>
		/// If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).
		/// </summary>
		public override bool Synchronous
		{
			get { return this.synchronous; }
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get { return this.handlesSubPaths; }
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void GET(HttpRequest Request, HttpResponse Response)
		{
			this.get(Request, Response);
		}
	}
}
