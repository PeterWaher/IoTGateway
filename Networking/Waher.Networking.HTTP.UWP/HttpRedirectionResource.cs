﻿using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// An HTTP redirection resource. Incoming requests are redirected to another location.
	/// </summary>
	public class HttpRedirectionResource : HttpSynchronousResource, IHttpGetMethod, IHttpGetRangesMethod,
		IHttpPostMethod, IHttpPostRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IHttpOptionsMethod,
		IHttpDeleteMethod, IHttpPatchMethod, IHttpPatchRangesMethod, IHttpTraceMethod
	{
		private readonly string location;
		private readonly bool includeSubPaths;
		private readonly bool permanent;

		/// <summary>
		/// An HTTP Reverse proxy resource. Incoming requests are reverted to a another web server for processing. Responses
		/// are returned asynchronously as they are received.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Location">New location</param>
		/// <param name="IncludeSubPaths">If subpaths are to be redirected as well.</param>
		/// <param name="Permanent">If redirection is permanent.</param>
		public HttpRedirectionResource(string ResourceName, string Location, bool IncludeSubPaths, bool Permanent)
			: base(ResourceName)
		{
			this.location = Location;
			this.includeSubPaths = IncludeSubPaths;
			this.permanent = Permanent;
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => this.includeSubPaths;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// If the PATCH method is allowed.
		/// </summary>
		public bool AllowsPATCH => true;

		/// <summary>
		/// If the TRACE method is allowed.
		/// </summary>
		public bool AllowsTRACE => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task GET(HttpRequest Request, HttpResponse Response)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the ranged GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="FirstInterval">First byte range interval.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval FirstInterval)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task POST(HttpRequest Request, HttpResponse Response)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the ranged POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task POST(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task PUT(HttpRequest Request, HttpResponse Response)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the ranged PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the OPTIONS method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public override Task OPTIONS(HttpRequest Request, HttpResponse Response)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task PATCH(HttpRequest Request, HttpResponse Response)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the ranged PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task PATCH(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes the TRACE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task TRACE(HttpRequest Request, HttpResponse Response)
		{
			return Task.CompletedTask;  // Never called. Request processed by custom Execute method below.
		}

		/// <summary>
		/// Executes a method on the resource. The default behaviour is to call the corresponding execution methods defined in the specialized
		/// interfaces <see cref="IHttpGetMethod"/>, <see cref="IHttpPostMethod"/>, <see cref="IHttpPutMethod"/> and <see cref="IHttpDeleteMethod"/>
		/// if they are defined for the resource.
		/// </summary>
		/// <param name="Server">HTTP Server</param>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		public override async Task Execute(HttpServer Server, HttpRequest Request, HttpResponse Response)
		{
			if (this.includeSubPaths)
			{
				if (this.permanent)
					await Response.SendResponse(new PermanentRedirectException(this.location + Request.SubPath));
				else
					await Response.SendResponse(new TemporaryRedirectException(this.location + Request.SubPath));
			}
			else
			{
				if (this.permanent)
					await Response.SendResponse(new PermanentRedirectException(this.location));
				else
					await Response.SendResponse(new TemporaryRedirectException(this.location));
			}
		}
	}
}
