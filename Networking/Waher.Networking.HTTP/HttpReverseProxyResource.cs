using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// An HTTP Reverse proxy resource. Incoming requests are reverted to a another web server for processing. Responses
	/// are returned asynchronously as they are received.
	/// </summary>
	public class HttpReverseProxyResource : HttpAsynchronousResource, IHttpGetMethod, IHttpGetRangesMethod,
		IHttpPostMethod, IHttpPostRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IHttpOptionsMethod,
		IHttpDeleteMethod, IHttpPatchMethod, IHttpPatchRangesMethod, IHttpTraceMethod
	{
		private readonly TimeSpan timeout;
		private readonly string baseUri;
		private readonly bool useSession;
		private readonly bool encryption;

		/// <summary>
		/// An HTTP Reverse proxy resource. Incoming requests are reverted to a another web server for processing. Responses
		/// are returned asynchronously as they are received.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RemoteHost">Host of remote web server.</param>
		/// <param name="Port">Port number of remote web server.</param>
		/// <param name="RemoteFolder">Optional remote folder where remote content is hosted.</param>
		/// <param name="Encryption">If encryption (https) should be used.</param>
		/// <param name="Timeout">Timeout threshold.</param>
		public HttpReverseProxyResource(string ResourceName, string RemoteHost, int Port,
			string RemoteFolder, bool Encryption, TimeSpan Timeout)
			: this(ResourceName, RemoteHost, Port, RemoteFolder, Encryption, Timeout, false)
		{
		}

		/// <summary>
		/// An HTTP Reverse proxy resource. Incoming requests are reverted to a another web server for processing. Responses
		/// are returned asynchronously as they are received.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RemoteHost">Host of remote web server.</param>
		/// <param name="Port">Port number of remote web server.</param>
		/// <param name="RemoteFolder">Optional remote folder where remote content is hosted.</param>
		/// <param name="Encryption">If encryption (https) should be used.</param>
		/// <param name="Timeout">Timeout threshold.</param>
		/// <param name="UseProxySession">If the proxy resource should add a session requirement
		/// as well. This allows the proxy resource to forward user infomration to underlying
		/// services, etc. (Default=false)</param>
		public HttpReverseProxyResource(string ResourceName, string RemoteHost, int Port,
			string RemoteFolder, bool Encryption, TimeSpan Timeout, bool UseProxySession)
			: base(ResourceName)
		{
			this.timeout = Timeout;
			this.useSession = UseProxySession;

			StringBuilder sb = new StringBuilder();
			int DefaultPort;

			sb.Append("http");
			if (Encryption)
			{
				sb.Append('s');
				DefaultPort = HttpServer.DefaultHttpsPort;
			}
			else
				DefaultPort = HttpServer.DefaultHttpPort;

			sb.Append("://");
			sb.Append(RemoteHost);

			if (Port != DefaultPort)
			{
				sb.Append(':');
				sb.Append(Port.ToString());
			}

			if (!string.IsNullOrEmpty(RemoteFolder))
				sb.Append(RemoteFolder);

			this.baseUri = sb.ToString();
			this.encryption = Encryption;
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => this.useSession;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If forwarding is encrypted.
		/// </summary>
		public bool Encrypted => this.encryption;

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
		public override Task Execute(HttpServer Server, HttpRequest Request, HttpResponse Response)
		{
			this.ProcessProxyRequest(Server, Request, Response);
			return Task.CompletedTask;
		}

		private async void ProcessProxyRequest(HttpServer Server, HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Variables Session = Request.Session;

				if (this.useSession)
				{
					string HttpSessionID;

					if (Session is null)
					{
						HttpSessionID = GetSessionId(Request, Response);
						Request.Session = Session = Server.GetSession(HttpSessionID);
					}
					else if (Request.tempSession)
					{
						HttpSessionID = Convert.ToBase64String(Hashes.ComputeSHA512Hash(Guid.NewGuid().ToByteArray()));
						Response.SetCookie(new Cookie(HttpResource.HttpSessionID, HttpSessionID, null, "/", null, false, true));

						Session = Server.SetSession(HttpSessionID, Request.Session);
						Request.tempSession = false;
					}
				}
				else
					Session = null;

				StringBuilder sb = new StringBuilder();

				sb.Append(this.baseUri);
				sb.Append(Request.SubPath);

				if (!string.IsNullOrEmpty(Request.Header.QueryString))
				{
					sb.Append('?');
					sb.Append(Request.Header.QueryString);
				}

				if (!Uri.TryCreate(sb.ToString(), UriKind.Absolute, out Uri RemoteUri))
				{
					await Response.SendResponse(new BadRequestException());
					return;
				}

				byte[] Data;

				if (Request.HasData)
				{
					Request.DataStream.Position = 0;
					Data = await Request.DataStream.ReadAllAsync();
				}
				else
					Data = null;


				HttpClientHandler Handler = WebGetter.GetClientHandler();
				string s;

				using HttpClient HttpClient = new HttpClient(Handler, true)
				{
					Timeout = this.timeout
				};
				using HttpRequestMessage ProxyRequest = new HttpRequestMessage()
				{
					RequestUri = RemoteUri,
					Method = new HttpMethod(Request.Header.Method)
				};
				if (!(Data is null))
					ProxyRequest.Content = new ByteArrayContent(Data);

				foreach (HttpField Field in Request.Header)
				{
					switch (Field.Key)
					{
						case "Accept":
							if (!ProxyRequest.Headers.Accept.TryParseAdd(Field.Value))
								throw new InvalidOperationException("Invalid Accept header value: " + Field.Value);
							break;

						case "Authorization":
							int i = Field.Value.IndexOf(' ');
							if (i < 0)
								ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(Field.Value);
							else
								ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(Field.Value[..i], Field.Value[(i + 1)..].TrimStart());
							break;

						case "Cookie":
							foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(Field.Value))
							{
								if (this.useSession && P.Key == HttpSessionID && !(Session is null))
								{
									if (Session.TryGetVariable(SpacePrefixedHttpSessionID, out Variable v) &&
										v.ValueObject is Cookie ProxyCookie)
									{
										Handler.CookieContainer.Add(ProxyRequest.RequestUri, new System.Net.Cookie(ProxyCookie.Name, ProxyCookie.Value));
									}
								}
								else
									Handler.CookieContainer.Add(ProxyRequest.RequestUri, new System.Net.Cookie(P.Key, P.Value));
							}
							break;

						case "Content-Encoding":
						case "Content-Length":
						case "Transfer-Encoding":
						case "Host":
						case "Accept-Encoding":
							// Igore; will be re-coded.
							break;

						case "Allow":
						case "Content-Disposition":
						case "Content-Language":
						case "Content-Location":
						case "Content-MD5":
						case "Content-Range":
						case "Content-Type":
						case "Last-Modified":
							ProxyRequest.Content?.Headers.Add(Field.Key, Field.Value);
							break;

						default:
							if (!Field.Key.StartsWith(':'))     // Avoid HTTP/2 pseudo-headers
								ProxyRequest.Headers.Add(Field.Key, Field.Value);
							break;
					}
				}

				// Forwarded:			https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Forwarded
				// X-Forwarded-Host:	https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-Host
				// X-Forwarded-For		https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-For
				// X-Forwarded-Proto	https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-Proto
				// Via					https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Via

				sb.Clear();

				sb.Append("by=");
				sb.Append(Request.LocalEndPoint);

				sb.Append(";for=");
				sb.Append(s = Request.RemoteEndPoint);

				if (Request.Header.TryGetHeaderField("X-Forwarded-For", out HttpField ForwardedFor))
					ProxyRequest.Headers.Add("X-Forwarded-For", s + ", " + ForwardedFor.Value);
				else
					ProxyRequest.Headers.Add("X-Forwarded-For", s);

				sb.Append(";proto=http");
				if (Request.Encrypted)
				{
					sb.Append('s');
					ProxyRequest.Headers.Add("X-Forwarded-Proto", "https");
				}
				else
					ProxyRequest.Headers.Add("X-Forwarded-Proto", "http");

				if (!(Request.Header.Host is null))
				{
					sb.Append(";host=");
					sb.Append(s = Request.Header.Host.Value);

					ProxyRequest.Headers.Add("X-Forwarded-Host", s);
					ProxyRequest.Headers.Add("Forwarded", sb.ToString());

					sb.Clear();

					sb.Append("HTTP/");
					sb.Append(CommonTypes.Encode(Request.Header.HttpVersion, 1));
					sb.Append(' ');
					sb.Append(s);

					if (Request.Header.TryGetHeaderField("Via", out HttpField Via))
					{
						sb.Append(", ");
						sb.Append(Via.Value);
					}

					ProxyRequest.Headers.Add("Via", sb.ToString());
				}
				else
					ProxyRequest.Headers.Add("Forwarded", sb.ToString());

				await this.BeforeForwardRequest.Raise(this, new ProxyRequestEventArgs(ProxyRequest, Request, Response), false);

				HttpResponseMessage ProxyResponse = await HttpClient.SendAsync(ProxyRequest);

				Response.StatusCode = (int)ProxyResponse.StatusCode;
				Response.StatusMessage = ProxyResponse.ReasonPhrase;

				foreach (KeyValuePair<string, IEnumerable<string>> Header in ProxyResponse.Headers)
				{
					switch (Header.Key)
					{
						case "Transfer-Encoding":
						case "X-Content-Type-Options":
							break;

						case "Set-Cookie":
							if (!(Session is null))
							{
								foreach (string Value in Header.Value)
								{
									Cookie Cookie = Cookie.FromSetCookie(Value);
									if (Cookie is null)
										continue;

									if (Cookie.Name == HttpSessionID)
										Session[SpacePrefixedHttpSessionID] = Cookie;
									else
										Response.SetCookie(Cookie);
								}
							}
							else
							{
								foreach (string Value in Header.Value)
								{
									Cookie Cookie = Cookie.FromSetCookie(Value);
									if (Cookie is null)
										continue;

									Response.SetCookie(Cookie);
								}
							}
							break;

						default:
							foreach (string Value in Header.Value)
								Response.SetHeader(Header.Key, Value);
							break;
					}
				}

				if (!(ProxyResponse.Content is null))
				{
					foreach (KeyValuePair<string, IEnumerable<string>> Header in ProxyResponse.Content.Headers)
					{
						switch (Header.Key)
						{
							case "Content-Length":
							case "Content-Encoding":
								break;

							default:
								foreach (string Value in Header.Value)
									Response.SetHeader(Header.Key, Value);
								break;
						}
					}

					byte[] Bin = await ProxyResponse.Content.ReadAsByteArrayAsync();

					Response.ContentLength = Bin.Length;

					await this.BeforeForwardResponse.Raise(this, new ProxyResponseEventArgs(ProxyResponse, Request, Response), false);

					await Response.Write(Bin);
				}
				else
					await this.BeforeForwardResponse.Raise(this, new ProxyResponseEventArgs(ProxyResponse, Request, Response), false);

				await Response.SendResponse();
			}
			catch (HttpException ex)
			{
				try
				{
					await Response.SendResponse(ex);
				}
				catch (Exception)
				{
					// Ignore
				}
			}
			catch (Exception ex)
			{
				try
				{
					await Response.SendResponse(ex);
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Event raised before a request is forwarded
		/// </summary>
		public event EventHandlerAsync<ProxyRequestEventArgs> BeforeForwardRequest;

		/// <summary>
		/// Event raised before a response is forwarded
		/// </summary>
		public event EventHandlerAsync<ProxyResponseEventArgs> BeforeForwardResponse;
	}
}
