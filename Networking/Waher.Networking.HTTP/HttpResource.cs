using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Delegate for HTTP method handlers.
	/// </summary>
	/// <param name="Request">HTTP Request</param>
	/// <param name="Response">HTTP Response</param>
	/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
	public delegate void HttpMethodHandler(HttpRequest Request, HttpResponse Response);

	/// <summary>
	/// Base class for all HTTP resources.
	/// </summary>
	public abstract class HttpResource
	{
		private string[] allowedMethods;
		private IHttpGetMethod get;
		private IHttpPostMethod post;
		private IHttpPutMethod put;
		private IHttpDeleteMethod delete;
		private IHttpOptionsMethod options;
		private IHttpTraceMethod trace;
		private string resourceName;

		/// <summary>
		/// Base class for all HTTP resources.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public HttpResource(string ResourceName)
		{
			this.resourceName = ResourceName;
			this.get = this as IHttpGetMethod;
			this.post = this as IHttpPostMethod;
			this.put = this as IHttpPutMethod;
			this.delete = this as IHttpDeleteMethod;
			this.options = this as IHttpOptionsMethod;
			this.trace = this as IHttpTraceMethod;

			List<string> Methods = new List<string>();

			if (this.get != null)
			{
				Methods.Add("GET");
				Methods.Add("HEAD");
			}

			if (this.post != null)
				Methods.Add("POST");

			if (this.put != null)
				Methods.Add("PUT");

			if (this.delete != null)
				Methods.Add("DELETE");

			if (this.options != null)
				Methods.Add("OPTIONS");

			if (this.trace != null)
				Methods.Add("TRACE");

			this.allowedMethods = Methods.ToArray();
		}

		/// <summary>
		/// Name of resource.
		/// </summary>
		public string ResourceName
		{
			get { return this.resourceName; }
		}

		/// <summary>
		/// If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).
		/// </summary>
		public abstract bool Synchronous
		{
			get;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public abstract bool HandlesSubPaths
		{
			get;
		}

		/// <summary>
		/// Validates the request itself. This method is called prior to processing the request, to see if it is valid in the context of the resource 
		/// or not. If not, corresponding HTTP Exceptions should be thrown. Implementing validation checks in this method, instead of the corresponding
		/// execution method, allows the resource to respond correctly to requests using the "Expect: 100-continue" header.
		/// </summary>
		/// <param name="Request">Request to validate.</param>
		public virtual void Validate(HttpRequest Request)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Executes a method on the resource. The default behaviour is to call the corresponding execution methods defined in the specialized
		/// interfaces <see cref="IHttpGetMethod"/>, <see cref="IHttpPostMethod"/>, <see cref="IHttpPutMethod"/> and <see cref="IHttpDeleteMethod"/>
		/// if they are defined for the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public virtual void Execute(HttpRequest Request, HttpResponse Response)
		{
			switch (Request.Header.Method)
			{
				case "GET":
					if (this.get == null)
						throw new MethodNotAllowed(this.allowedMethods);
					else
						this.get.GET(Request, Response);
					break;

				case "HEAD":
					if (this.get == null)
						throw new MethodNotAllowed(this.allowedMethods);
					else
					{
						Response.OnlyHeader = true;
						this.get.GET(Request, Response);
					}
					break;

				case "POST":
					if (this.post == null)
						throw new MethodNotAllowed(this.allowedMethods);
					else
						this.post.POST(Request, Response);
					break;

				case "PUT":
					if (this.put == null)
						throw new MethodNotAllowed(this.allowedMethods);
					else
						this.put.PUT(Request, Response);
					break;

				case "DELETE":
					if (this.delete == null)
						throw new MethodNotAllowed(this.allowedMethods);
					else
						this.delete.DELETE(Request, Response);
					break;

				case "OPTIONS":
					if (this.options == null)
						throw new MethodNotAllowed(this.allowedMethods);
					else
						this.options.OPTIONS(Request, Response);
					break;

				case "TRACE":
					if (this.trace == null)
						throw new MethodNotAllowed(this.allowedMethods);
					else
						this.trace.TRACE(Request, Response);
					break;

				default:
					throw new MethodNotAllowed(this.allowedMethods);
			}

			if (this.Synchronous)
			{
				Response.SendResponse();
				Response.Dispose();
			}
		}

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted.
		/// </summary>
		public virtual HttpAuthenticationScheme[] AuthenticationSchemes
		{
			get { return null; }
		}

	}
}
