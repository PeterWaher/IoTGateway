﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Delegate for HTTP method handlers.
	/// </summary>
	/// <param name="Request">HTTP Request</param>
	/// <param name="Response">HTTP Response</param>
	/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
	public delegate Task HttpMethodHandler(HttpRequest Request, HttpResponse Response);

	/// <summary>
	/// Base class for all HTTP resources.
	/// </summary>
	public abstract class HttpResource : IHttpOptionsMethod
	{
		/// <summary>
		/// The Cookie Key for HTTP Session Identifiers: "HttpSessionID"
		/// </summary>
		public const string HttpSessionID = "HttpSessionID";

		/// <summary>
		/// Session variable name for the HttpSessionID cookie variable.
		/// </summary>
		internal const string SpacePrefixedHttpSessionID = " " + HttpSessionID;

		private readonly List<HttpServer> servers = new List<HttpServer>();
		private readonly string[] allowedMethods;
		private readonly IHttpGetMethod get;
		private readonly IHttpGetRangesMethod getRanges;
		private readonly IHttpPostMethod post;
		private readonly IHttpPostRangesMethod postRanges;
		private readonly IHttpPutMethod put;
		private readonly IHttpPutRangesMethod putRanges;
		private readonly IHttpPatchMethod patch;
		private readonly IHttpPatchRangesMethod patchRanges;
		private readonly IHttpDeleteMethod delete;
		private readonly IHttpOptionsMethod options;
		private readonly IHttpTraceMethod trace;
		private readonly IHttpConnectMethod connect;
		private readonly string resourceName;
		private HttpServer[] serversStatic = Array.Empty<HttpServer>();

		/// <summary>
		/// Base class for all HTTP resources.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public HttpResource(string ResourceName)
		{
			this.resourceName = ResourceName;
			this.get = this as IHttpGetMethod;
			this.getRanges = this as IHttpGetRangesMethod;
			this.post = this as IHttpPostMethod;
			this.postRanges = this as IHttpPostRangesMethod;
			this.put = this as IHttpPutMethod;
			this.putRanges = this as IHttpPutRangesMethod;
			this.patch = this as IHttpPatchMethod;
			this.patchRanges = this as IHttpPatchRangesMethod;
			this.delete = this as IHttpDeleteMethod;
			this.options = this;
			this.trace = this as IHttpTraceMethod;
			this.connect = this as IHttpConnectMethod;

			List<string> Methods = new List<string>();

			if ((!(this.get is null) && this.get.AllowsGET) || (!(this.getRanges is null) && this.getRanges.AllowsGET))
			{
				Methods.Add("GET");
				Methods.Add("HEAD");
			}

			if ((!(this.post is null) && this.post.AllowsPOST) || (!(this.postRanges is null) && this.postRanges.AllowsPOST))
				Methods.Add("POST");

			if ((!(this.put is null) && this.put.AllowsPUT) || (!(this.putRanges is null) && this.putRanges.AllowsPUT))
				Methods.Add("PUT");

			if ((!(this.patch is null) && this.patch.AllowsPATCH) || (!(this.patchRanges is null) && this.patchRanges.AllowsPATCH))
				Methods.Add("PATCH");

			if (!(this.delete is null) && this.delete.AllowsDELETE)
				Methods.Add("DELETE");

			if (!(this.options is null) && this.options.AllowsOPTIONS)
				Methods.Add("OPTIONS");

			if (!(this.trace is null) && this.trace.AllowsTRACE)
				Methods.Add("TRACE");

			if (!(this.connect is null) && this.connect.AllowsCONNECT)
				Methods.Add("CONNECT");

			this.allowedMethods = Methods.ToArray();
		}

		/// <summary>
		/// Method called when a resource has been registered on a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public virtual void AddReference(HttpServer Server)
		{
			lock (this.servers)
			{
				this.servers.Add(Server);
				this.serversStatic = this.servers.ToArray();
			}
		}

		/// <summary>
		/// Method called when a resource has been unregistered from a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public virtual bool RemoveReference(HttpServer Server)
		{
			lock (this.servers)
			{
				if (this.servers.Remove(Server))
				{
					this.serversStatic = this.servers.ToArray();
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Name of resource.
		/// </summary>
		public string ResourceName => this.resourceName;

		/// <summary>
		/// If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).
		/// </summary>
		public abstract bool Synchronous { get; }

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public abstract bool HandlesSubPaths { get; }

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public abstract bool UserSessions { get; }

		/// <summary>
		/// Array of allowed methods.
		/// </summary>
		public string[] AllowedMethods => this.allowedMethods;

		/// <summary>
		/// Salt value used when calculating ETag values.
		/// </summary>
		public string ETagSalt
		{
			get
			{
				HttpServer[] Servers = this.serversStatic;

				switch (Servers.Length)
				{
					case 0:
						return null;

					case 1:
						return Servers[0].ETagSalt;

					default:
						StringBuilder sb = new StringBuilder();
						string s;

						foreach (HttpServer Server in Servers)
						{
							s = Server.ETagSalt;
							if (!string.IsNullOrEmpty(s))
								sb.Append(s);
						}

						return sb.ToString();
				}
			}
		}

		/// <summary>
		/// Computes an ETag value for a resource.
		/// </summary>
		/// <param name="fs">Stream containing data of resource.</param>
		/// <returns>ETag value.</returns>
		public string ComputeETag(Stream fs)
		{
			string ETag = Hashes.ComputeSHA1HashString(fs);
			fs.Position = 0;

			string Salt = this.ETagSalt;
			if (!string.IsNullOrEmpty(Salt))
				ETag = Hashes.ComputeSHA1HashString(Encoding.UTF8.GetBytes(ETag + Salt));

			return ETag;
		}

		/// <summary>
		/// Computes an ETag value for a resource.
		/// </summary>
		/// <param name="Binary">Binary representation of resource.</param>
		/// <returns>ETag value.</returns>
		public string ComputeETag(byte[] Binary)
		{
			return ComputeETag(Binary, this.ETagSalt);
		}

		/// <summary>
		/// Computes an ETag value for a resource.
		/// </summary>
		/// <param name="Binary">Binary representation of resource.</param>
		/// <param name="Salt">Salt for ETag hash computation.</param>
		/// <returns>ETag value.</returns>
		public static string ComputeETag(byte[] Binary, string Salt)
		{
			string ETag = Hashes.ComputeSHA1HashString(Binary);

			if (!string.IsNullOrEmpty(Salt))
				ETag = Hashes.ComputeSHA1HashString(Encoding.UTF8.GetBytes(ETag + Salt));

			return ETag;
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
		/// Gets the session ID used for a request.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Optional Response object.</param>
		/// <returns>Session ID</returns>
		public static string GetSessionId(HttpRequest Request, HttpResponse Response)
		{
			HttpFieldCookie Cookie = Request.Header.Cookie;
			string HttpSessionID;

			if (!(Cookie is null) && !string.IsNullOrEmpty(HttpSessionID = Cookie[HttpResource.HttpSessionID]))
				return HttpSessionID;

			if (!(Response?.Cookies is null))
			{
				foreach (Cookie SetCookie in Response.Cookies)
				{
					if (SetCookie.Name == HttpResource.HttpSessionID)
						return SetCookie.Value;
				}
			}

			HttpSessionID = Convert.ToBase64String(Hashes.ComputeSHA512Hash(Guid.NewGuid().ToByteArray()));
			Response?.SetCookie(new Cookie(HttpResource.HttpSessionID, HttpSessionID, null, "/", null, false, true));

			return HttpSessionID;
		}

		/// <summary>
		/// Executes a method on the resource. The default behaviour is to call the corresponding execution methods defined in the specialized
		/// interfaces <see cref="IHttpGetMethod"/>, <see cref="IHttpPostMethod"/>, <see cref="IHttpPutMethod"/> and <see cref="IHttpDeleteMethod"/>
		/// if they are defined for the resource.
		/// </summary>
		/// <param name="Server">HTTP Server</param>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public virtual async Task Execute(HttpServer Server, HttpRequest Request, HttpResponse Response)
		{
			HttpRequestHeader Header = Request.Header;
			string Method = Request.Header.Method;
			SessionVariables SessionVariables;

			if (this.UserSessions)
			{
				string HttpSessionID;

				if ((SessionVariables = Request.Session) is null)
				{
					HttpSessionID = GetSessionId(Request, Response);
					Request.Session = SessionVariables = Server.GetSession(HttpSessionID);
				}
				else if (Request.tempSession)
				{
					HttpSessionID = Convert.ToBase64String(Hashes.ComputeSHA512Hash(Guid.NewGuid().ToByteArray()));
					Response.SetCookie(new Cookie(HttpResource.HttpSessionID, HttpSessionID, null, "/", null, false, true));

					Server.SetSession(HttpSessionID, Request.Session);
					Request.tempSession = false;
				}

				await SessionVariables.LockAsync();

				SessionVariables.CurrentRequest = Request;
				SessionVariables.CurrentResponse = Response;
			}
			else
				SessionVariables = null;

			try
			{
				switch (Method)
				{
					case "GET":
					case "HEAD":
						if (!(this.getRanges is null))
						{
							Response.SetHeader("Accept-Ranges", "bytes");

							if (!(Header.Range is null))
							{
								ByteRangeInterval FirstInterval = Header.Range.FirstInterval;
								if (FirstInterval is null)
									throw new RangeNotSatisfiableException();
								else
								{
									Response.StatusCode = 206;
									Response.StatusMessage = "Partial Content";

									await this.getRanges.GET(Request, Response, FirstInterval);
								}
							}
							else
							{
								if (!(this.get is null))
									await this.get.GET(Request, Response);
								else
									await this.getRanges.GET(Request, Response, new ByteRangeInterval(0, null));
							}
						}
						else if (!(this.get is null))
							await this.get.GET(Request, Response);
						else
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						break;

					case "POST":
						if (!(this.postRanges is null))
						{
							if (!(Header.ContentRange is null))
							{
								ContentByteRangeInterval Interval = Header.ContentRange.Interval;
								if (Interval is null)
									throw new RangeNotSatisfiableException();
								else
									await this.postRanges.POST(Request, Response, Interval);
							}
							else
							{
								if (!(this.post is null))
									await this.post.POST(Request, Response);
								else
								{
									long Total;

									if (!(Header.ContentLength is null))
										Total = Header.ContentLength.ContentLength;
									else if (!(Request.DataStream is null))
										Total = Request.DataStream.Position;
									else
										Total = 0;

									await this.postRanges.POST(Request, Response, new ContentByteRangeInterval(0, Total - 1, Total));
								}
							}
						}
						else if (!(this.post is null))
							await this.post.POST(Request, Response);
						else
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						break;

					case "PUT":
						if (!(this.putRanges is null))
						{
							if (!(Header.ContentRange is null))
							{
								ContentByteRangeInterval Interval = Header.ContentRange.Interval;
								if (Interval is null)
									throw new RangeNotSatisfiableException();
								else
									await this.putRanges.PUT(Request, Response, Interval);
							}
							else
							{
								if (!(this.put is null))
									await this.put.PUT(Request, Response);
								else
								{
									long Total;

									if (!(Header.ContentLength is null))
										Total = Header.ContentLength.ContentLength;
									else if (!(Request.DataStream is null))
										Total = Request.DataStream.Position;
									else
										Total = 0;

									await this.putRanges.PUT(Request, Response, new ContentByteRangeInterval(0, Total - 1, Total));
								}
							}
						}
						else if (!(this.put is null))
							await this.put.PUT(Request, Response);
						else
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						break;

					case "PATCH":
						if (!(this.patchRanges is null))
						{
							if (!(Header.ContentRange is null))
							{
								ContentByteRangeInterval Interval = Header.ContentRange.Interval;
								if (Interval is null)
									throw new RangeNotSatisfiableException();
								else
									await this.patchRanges.PATCH(Request, Response, Interval);
							}
							else
							{
								if (!(this.patch is null))
									await this.patch.PATCH(Request, Response);
								else
								{
									long Total;

									if (!(Header.ContentLength is null))
										Total = Header.ContentLength.ContentLength;
									else if (!(Request.DataStream is null))
										Total = Request.DataStream.Position;
									else
										Total = 0;

									await this.patchRanges.PATCH(Request, Response, new ContentByteRangeInterval(0, Total - 1, Total));
								}
							}
						}
						else if (!(this.patch is null))
							await this.patch.PATCH(Request, Response);
						else
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						break;

					case "DELETE":
						if (this.delete is null)
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						else
							await this.delete.DELETE(Request, Response);
						break;

					case "OPTIONS":
						if (this.options is null)
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						else
							await this.options.OPTIONS(Request, Response);
						break;

					case "TRACE":
						if (this.trace is null)
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						else
							await this.trace.TRACE(Request, Response);
						break;

					case "CONNECT":
						if (this.connect is null)
							throw new MethodNotAllowedException(this.allowedMethods, Request);
						else
							await this.connect.CONNECT(Request, Response);
						break;

					default:
						throw new MethodNotAllowedException(this.allowedMethods, Request);
				}

				if (this.Synchronous)
				{
					await Response.SendResponse();
					await Response.DisposeAsync();
				}
			}
			finally
			{
				if (!(SessionVariables is null))
				{
					SessionVariables.CurrentRequest = null;
					SessionVariables.CurrentResponse = null;

					SessionVariables.Release();
				}
			}
		}

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public virtual HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			return null;
		}

		/// <summary>
		/// If the OPTIONS method is allowed.
		/// </summary>
		public bool AllowsOPTIONS => true;

		/// <summary>
		/// Executes the OPTIONS method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async virtual Task OPTIONS(HttpRequest Request, HttpResponse Response)
		{
			SetTransparentCorsHeaders(this, Request, Response);

			bool First = true;
			StringBuilder Options = new StringBuilder();

			foreach (string Option in this.allowedMethods)
			{
				if (First)
					First = false;
				else
					Options.Append(", ");

				Options.Append(Option);
			}

			Response.SetHeader("Allow", Options.ToString());
			Response.StatusCode = 204;
			Response.StatusMessage = "No Content";

			await Response.SendResponse();
		}

		/// <summary>
		/// Sets CORS headers for a resource, allowing it to be embedded in other sites.
		/// </summary>
		/// <param name="Resource">Resource being processed.</param>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="Response">HTTP Response object.</param>
		public static void SetTransparentCorsHeaders(HttpResource Resource, HttpRequest Request, HttpResponse Response)
		{
			if (Request.Header.TryGetHeaderField("Origin", out HttpField Origin))
				Response.SetHeader("Access-Control-Allow-Origin", Origin.Value);
			else
				Response.SetHeader("Access-Control-Allow-Origin", "*");

			if (Request.Header.TryGetHeaderField("Access-Control-Request-Headers", out HttpField AccessControlRequestHeaders))
				Response.SetHeader("Access-Control-Allow-Headers", AccessControlRequestHeaders.Value);

			if (Request.Header.TryGetHeaderField("Access-Control-Request-Method", out HttpField _))
			{
				StringBuilder Methods = new StringBuilder();
				bool First = true;

				foreach (string Method in Resource.AllowedMethods)
				{
					if (First)
						First = false;
					else
						Methods.Append(", ");

					Methods.Append(Method);
				}

				Response.SetHeader("Access-Control-Allow-Methods", Methods.ToString());
			}
		}

		/// <summary>
		/// Returns default content for an error, for the resource. If returning null, server will choose default content.
		/// </summary>
		/// <param name="StatusCode">Status code.</param>
		/// <returns>Default content, or null if resource lets server choose.</returns>
		public virtual Task<object> DefaultErrorContent(int StatusCode)
		{
			return Task.FromResult<object>(null);
		}

	}
}
