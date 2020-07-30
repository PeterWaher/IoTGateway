using System;
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
	public delegate void HttpMethodHandler(HttpRequest Request, HttpResponse Response);

	/// <summary>
	/// Delegate for HTTP method handlers.
	/// </summary>
	/// <param name="Request">HTTP Request</param>
	/// <param name="Response">HTTP Response</param>
	/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
	public delegate Task HttpMethodHandlerAsync(HttpRequest Request, HttpResponse Response);

	/// <summary>
	/// Base class for all HTTP resources.
	/// </summary>
	public abstract class HttpResource
	{
		private readonly List<HttpServer> servers = new List<HttpServer>();
		private readonly string[] allowedMethods;
		private readonly IHttpGetMethod get;
		private readonly IHttpGetRangesMethod getRanges;
		private readonly IHttpPostMethod post;
		private readonly IHttpPostRangesMethod postRanges;
		private readonly IHttpPutMethod put;
		private readonly IHttpPutRangesMethod putRanges;
		private readonly IHttpDeleteMethod delete;
		private readonly IHttpOptionsMethod options;
		private readonly IHttpTraceMethod trace;
		private readonly string resourceName;
		private HttpServer[] serversStatic = new HttpServer[0];

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
			this.delete = this as IHttpDeleteMethod;
			this.options = this as IHttpOptionsMethod;
			this.trace = this as IHttpTraceMethod;

			List<string> Methods = new List<string>();

			if ((this.get != null && this.get.AllowsGET) || (this.getRanges != null && this.getRanges.AllowsGET))
			{
				Methods.Add("GET");
				Methods.Add("HEAD");
			}

			if ((this.post != null && this.post.AllowsPOST) || (this.postRanges != null && this.postRanges.AllowsPOST))
				Methods.Add("POST");

			if ((this.put != null && this.put.AllowsPUT) || (this.putRanges != null && this.putRanges.AllowsPUT))
				Methods.Add("PUT");

			if (this.delete != null && this.delete.AllowsDELETE)
				Methods.Add("DELETE");

			if (this.options != null && this.options.AllowsOPTIONS)
				Methods.Add("OPTIONS");

			if (this.trace != null && this.trace.AllowsTRACE)
				Methods.Add("TRACE");

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
		/// If the resource uses user sessions.
		/// </summary>
		public abstract bool UserSessions
		{
			get;
		}

		/// <summary>
		/// Array of allowed methods.
		/// </summary>
		public string[] AllowedMethods
		{
			get { return this.allowedMethods; }
		}

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
		/// <param name="Server">HTTP Server</param>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public virtual async Task Execute(HttpServer Server, HttpRequest Request, HttpResponse Response)
		{
			HttpRequestHeader Header = Request.Header;
			string Method = Request.Header.Method;

			if (this.UserSessions && Request.Session is null)
			{
				HttpFieldCookie Cookie;
				string HttpSessionID;

				if ((Cookie = Request.Header.Cookie) is null || string.IsNullOrEmpty(HttpSessionID = Cookie["HttpSessionID"]))
				{
					HttpSessionID = Convert.ToBase64String(Hashes.ComputeSHA512Hash(Guid.NewGuid().ToByteArray()));
					Response.SetCookie(new Cookie("HttpSessionID", HttpSessionID, null, "/", null, false, true));
				}

				Request.Session = Server.GetSession(HttpSessionID);
			}

			switch (Method)
			{
				case "GET":
				case "HEAD":
					if (this.getRanges != null)
					{
						Response.SetHeader("Accept-Ranges", "bytes");

						if (Header.Range != null)
						{
							ByteRangeInterval FirstInterval = Header.Range.FirstInterval;
							if (FirstInterval is null)
								throw new RangeNotSatisfiableException();
							else
							{
								Response.OnlyHeader = Method == "HEAD";
								Response.StatusCode = 206;
								Response.StatusMessage = "Partial Content";

								await this.getRanges.GET(Request, Response, FirstInterval);
							}
						}
						else
						{
							Response.OnlyHeader = Method == "HEAD";

							if (this.get != null)
								await this.get.GET(Request, Response);
							else
								await this.getRanges.GET(Request, Response, new ByteRangeInterval(0, null));
						}
					}
					else if (this.get != null)
					{
						Response.OnlyHeader = Method == "HEAD";
						await this.get.GET(Request, Response);
					}
					else
						throw new MethodNotAllowedException(this.allowedMethods);
					break;

				case "POST":
					if (this.postRanges != null)
					{
						if (Header.ContentRange != null)
						{
							ContentByteRangeInterval Interval = Header.ContentRange.Interval;
							if (Interval is null)
								throw new RangeNotSatisfiableException();
							else
								await this.postRanges.POST(Request, Response, Interval);
						}
						else
						{
							if (this.post != null)
								await this.post.POST(Request, Response);
							else
							{
								long Total;

								if (Header.ContentLength != null)
									Total = Header.ContentLength.ContentLength;
								else if (Request.DataStream != null)
									Total = Request.DataStream.Position;
								else
									Total = 0;

								await this.postRanges.POST(Request, Response, new ContentByteRangeInterval(0, Total - 1, Total));
							}
						}
					}
					else if (this.post != null)
						await this.post.POST(Request, Response);
					else
						throw new MethodNotAllowedException(this.allowedMethods);
					break;

				case "PUT":
					if (this.putRanges != null)
					{
						if (Header.ContentRange != null)
						{
							ContentByteRangeInterval Interval = Header.ContentRange.Interval;
							if (Interval is null)
								throw new RangeNotSatisfiableException();
							else
								await this.putRanges.PUT(Request, Response, Interval);
						}
						else
						{
							if (this.put != null)
								await this.put.PUT(Request, Response);
							else
							{
								long Total;

								if (Header.ContentLength != null)
									Total = Header.ContentLength.ContentLength;
								else if (Request.DataStream != null)
									Total = Request.DataStream.Position;
								else
									Total = 0;

								await this.putRanges.PUT(Request, Response, new ContentByteRangeInterval(0, Total - 1, Total));
							}
						}
					}
					else if (this.put != null)
						await this.put.PUT(Request, Response);
					else
						throw new MethodNotAllowedException(this.allowedMethods);
					break;

				case "DELETE":
					if (this.delete is null)
						throw new MethodNotAllowedException(this.allowedMethods);
					else
						await this.delete.DELETE(Request, Response);
					break;

				case "OPTIONS":
					if (this.options is null)
						throw new MethodNotAllowedException(this.allowedMethods);
					else
						await this.options.OPTIONS(Request, Response);
					break;

				case "TRACE":
					if (this.trace is null)
						throw new MethodNotAllowedException(this.allowedMethods);
					else
						await this.trace.TRACE(Request, Response);
					break;

				default:
					throw new MethodNotAllowedException(this.allowedMethods);
			}

			if (this.Synchronous)
			{
				await Response.SendResponse();
				Response.Dispose();
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

	}
}
