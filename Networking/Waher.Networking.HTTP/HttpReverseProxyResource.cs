using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Runtime.IO;
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
		IHttpDeleteMethod, IHttpPatchMethod, IHttpPatchRangesMethod, IHttpTraceMethod,
		ICommunicationLayer
	{
		private static readonly SortedDictionary<string, string> httpHeaderFields = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "A-IM", "A-IM" },
			{ "Accept", "Accept" },
			{ "Accept-CH", "Accept-CH" },
			{ "Accept-Charset", "Accept-Charset" },
			{ "Accept-Datetime", "Accept-Datetime" },
			{ "Accept-Encoding", "Accept-Encoding" },
			{ "Accept-Language", "Accept-Language" },
			{ "Accept-Patch", "Accept-Patch" },
			{ "Accept-Ranges", "Accept-Ranges" },
			{ "Access-Control-Allow-Credentials", "Access-Control-Allow-Credentials" },
			{ "Access-Control-Allow-Headers", "Access-Control-Allow-Headers" },
			{ "Access-Control-Allow-Methods", "Access-Control-Allow-Methods" },
			{ "Access-Control-Allow-Origin", "Access-Control-Allow-Origin" },
			{ "Access-Control-Expose-Headers", "Access-Control-Expose-Headers" },
			{ "Access-Control-Max-Age", "Access-Control-Max-Age" },
			{ "Access-Control-Request-Headers", "Access-Control-Request-Headers" },
			{ "Access-Control-Request-Method", "Access-Control-Request-Method" },
			{ "Age", "Age" },
			{ "Allow", "Allow" },
			{ "Alt-Svc", "Alt-Svc" },
			{ "Authorization", "Authorization" },
			{ "Cache-Control", "Cache-Control" },
			{ "Connection", "Connection" },
			{ "Content-Disposition", "Content-Disposition" },
			{ "Content-Encoding", "Content-Encoding" },
			{ "Content-Language", "Content-Language" },
			{ "Content-Length", "Content-Length" },
			{ "Content-Location", "Content-Location" },
			{ "Content-MD5", "Content-MD5" },
			{ "Content-Range", "Content-Range" },
			{ "Content-Security-Policy", "Content-Security-Policy" },
			{ "Content-Type", "Content-Type" },
			{ "Cookie", "Cookie" },
			{ "Correlation-ID", "Correlation-ID" },
			{ "Date", "Date" },
			{ "Delta-Base", "Delta-Base" },
			{ "DNT", "DNT" },
			{ "ETag", "ETag" },
			{ "Expect", "Expect" },
			{ "Expect-CT", "Expect-CT" },
			{ "Expires", "Expires" },
			{ "Forwarded", "Forwarded" },
			{ "From", "From" },
			{ "Front-End-Https", "Front-End-Https" },
			{ "Host", "Host" },
			{ "HTTP2-Settings", "HTTP2-Settings" },
			{ "If-Match", "If-Match" },
			{ "If-Modified-Since", "If-Modified-Since" },
			{ "If-None-Match", "If-None-Match" },
			{ "If-Range", "If-Range" },
			{ "If-Unmodified-Since", "If-Unmodified-Since" },
			{ "IM", "IM" },
			{ "Last-Modified", "Last-Modified" },
			{ "Link", "Link" },
			{ "Location", "Location" },
			{ "Max-Forwards", "Max-Forwards" },
			{ "NEL", "NEL" },
			{ "Origin", "Origin" },
			{ "P3P", "P3P" },
			{ "Permissions-Policy", "Permissions-Policy" },
			{ "Pragma", "Pragma" },
			{ "Prefer", "Prefer" },
			{ "Preference-Applied", "Preference-Applied" },
			{ "Proxy-Authenticate", "Proxy-Authenticate" },
			{ "Proxy-Authorization", "Proxy-Authorization" },
			{ "Proxy-Connection", "Proxy-Connection" },
			{ "Public-Key-Pins", "Public-Key-Pins" },
			{ "Range", "Range" },
			{ "Referer", "Referer" },
			{ "Refresh", "Refresh" },
			{ "Report-To", "Report-To" },
			{ "Retry-After", "Retry-After" },
			{ "Save-Data", "Save-Data" },
			{ "Sec-GPC", "Sec-GPC" },
			{ "Server", "Server" },
			{ "Set-Cookie", "Set-Cookie" },
			{ "Status", "Status" },
			{ "Strict-Transport-Security", "Strict-Transport-Security" },
			{ "TE", "TE" },
			{ "Timing-Allow-Origin", "Timing-Allow-Origin" },
			{ "Tk", "Tk" },
			{ "Trailer", "Trailer" },
			{ "Transfer-Encoding", "Transfer-Encoding" },
			{ "Upgrade", "Upgrade" },
			{ "Upgrade-Insecure-Requests", "Upgrade-Insecure-Requests" },
			{ "User-Agent", "User-Agent" },
			{ "Vary", "Vary" },
			{ "Via", "Via" },
			{ "Warning", "Warning" },
			{ "WWW-Authenticate", "WWW-Authenticate" },
			{ "X-ATT-DeviceId", "X-ATT-DeviceId" },
			{ "X-Content-Duration", "X-Content-Duration" },
			{ "X-Content-Security-Policy", "X-Content-Security-Policy" },
			{ "X-Content-Type-Options", "X-Content-Type-Options" },
			{ "X-Correlation-ID", "X-Correlation-ID" },
			{ "X-Csrf-Token", "X-Csrf-Token" },
			{ "X-Forwarded-For", "X-Forwarded-For" },
			{ "X-Forwarded-Host", "X-Forwarded-Host" },
			{ "X-Forwarded-Proto", "X-Forwarded-Proto" },
			{ "X-Frame-Options", "X-Frame-Options" },
			{ "X-Http-Method-Override", "X-Http-Method-Override" },
			{ "X-Powered-By", "X-Powered-By" },
			{ "X-Redirect-By", "X-Redirect-By" },
			{ "X-Redirect-By: Polylang", "X-Redirect-By: Polylang" },
			{ "X-Request-ID", "X-Request-ID" },
			{ "X-Requested-With", "X-Requested-With" },
			{ "X-UA-Compatible", "X-UA-Compatible" },
			{ "X-UIDH", "X-UIDH" },
			{ "X-Wap-Profile", "X-Wap-Profile" },
			{ "X-WebKit-CSP", "X-WebKit-CSP" },
			{ "X-XSS-Protection", "X-XSS-Protection" }
		};

		private readonly HttpAuthenticationScheme[] authenticationSchemes;
		private readonly TimeSpan timeout;
		private readonly string baseUri;
		private readonly string privilege;
		private readonly bool baseUriEndsWithSlash;
		private readonly bool useSession;
		private readonly bool encryption;
		private readonly CommunicationLayer comLayer;
		private readonly bool authorization;

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
		/// <param name="Sniffers">Sniffers</param>
		public HttpReverseProxyResource(string ResourceName, string RemoteHost, int Port,
			string RemoteFolder, bool Encryption, TimeSpan Timeout, params ISniffer[] Sniffers)
			: this(ResourceName, RemoteHost, Port, RemoteFolder, Encryption, Timeout,
				  false, null, null, Sniffers)
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
		/// <param name="Sniffers">Sniffers</param>
		public HttpReverseProxyResource(string ResourceName, string RemoteHost, int Port,
			string RemoteFolder, bool Encryption, TimeSpan Timeout, bool UseProxySession,
			params ISniffer[] Sniffers)
			: this(ResourceName, RemoteHost, Port, RemoteFolder, Encryption, Timeout,
				  UseProxySession, null, null, Sniffers)
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
		/// <param name="AuthenticationSchemes">Optional set of authentication schemes.</param>
		/// <param name="RequiredPrivilege">Optional required privilege.</param>
		/// <param name="Sniffers">Sniffers</param>
		public HttpReverseProxyResource(string ResourceName, string RemoteHost, int Port,
			string RemoteFolder, bool Encryption, TimeSpan Timeout, bool UseProxySession,
			HttpAuthenticationScheme[] AuthenticationSchemes, string RequiredPrivilege,
			params ISniffer[] Sniffers)
			: base(ResourceName)
		{
			this.timeout = Timeout;
			this.useSession = UseProxySession;
			this.authenticationSchemes = AuthenticationSchemes;
			this.privilege = RequiredPrivilege;
			this.authorization = !string.IsNullOrEmpty(RequiredPrivilege);

			this.RemoteHost = RemoteHost;
			this.RemotePort = Port;
			this.RemoteFolder = RemoteFolder;

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
			this.baseUriEndsWithSlash = this.baseUri.EndsWith('/');
			this.encryption = Encryption;
			this.comLayer = new CommunicationLayer(true, Sniffers);
		}

		/// <summary>
		/// Host of remote web server.
		/// </summary>
		public string RemoteHost { get; }

		/// <summary>
		/// Port number of remote web server.
		/// </summary>
		public int RemotePort { get; }

		/// <summary>
		/// Optional remote folder where remote content is hosted.
		/// </summary>
		public string RemoteFolder { get; }

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
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			return this.authenticationSchemes;
		}

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
				if (this.authorization)
				{
					IUser User = Request.User;
					if (!(User?.HasPrivilege(this.privilege) ?? false))
					{
						await Response.SendResponse(new ForbiddenException("Access denied."));
						return;
					}
				}

				SessionVariables Session = Request.Session;
				bool HasSniffers = this.comLayer.HasSniffers;
				StringBuilder Message = HasSniffers ? new StringBuilder() : null;

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

				if (this.baseUriEndsWithSlash && (Request.SubPath?.StartsWith('/') ?? false))
					sb.Append(Request.SubPath.Substring(1));
				else
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

				using (HttpClient HttpClient = new HttpClient(Handler, true)
				{
					Timeout = this.timeout
				})
				{
					string Method = Request.Header.Method.ToUpper();

					using (HttpRequestMessage ProxyRequest = new HttpRequestMessage()
					{
						RequestUri = RemoteUri,
						Method = new HttpMethod(Method)
					})
					{
						bool CheckCase = !(Request.Http2Stream is null);
						string FieldName, FieldName2;

						if (HasSniffers)
						{
							Message.Append(Method);
							Message.Append(' ');
							Message.Append(RemoteUri.PathAndQuery);
							Message.Append(" HTTP");

							if (CheckCase)
								Message.Append("/2");
							else
								Message.Append(" 1.1");

							Message.AppendLine();
						}

						if (!(Data is null))
							ProxyRequest.Content = new ByteArrayContent(Data);

						foreach (HttpField Field in Request.Header)
						{
							FieldName = Field.Key;

							if (CheckCase && httpHeaderFields.TryGetValue(FieldName, out FieldName2))
								FieldName = FieldName2;

							switch (FieldName)
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
										ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(Field.Value.Substring(0, i), Field.Value.Substring(i + 1).TrimStart());
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
									ProxyRequest.Content?.Headers.Add(FieldName, Field.Value);
									break;

								default:
									if (!FieldName.StartsWith(':'))     // Avoid HTTP/2 pseudo-headers
										ProxyRequest.Headers.Add(FieldName, Field.Value);
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

						if (HasSniffers)
						{
							foreach (KeyValuePair<string, IEnumerable<string>> Header in ProxyRequest.Headers)
							{
								Message.Append(Header.Key);
								Message.Append(": ");
								Message.AppendLine(string.Join(", ", Header.Value));
							}

							if (!(ProxyRequest.Content is null))
							{
								foreach (KeyValuePair<string, IEnumerable<string>> Header in ProxyRequest.Content.Headers)
								{
									Message.Append(Header.Key);
									Message.Append(": ");
									Message.AppendLine(string.Join(", ", Header.Value));
								}
							}

							this.comLayer.TransmitText(Message.ToString());
							Message.Clear();

							if (!(Data is null))
								this.comLayer.TransmitBinary(false, Data);
						}

						HttpResponseMessage ProxyResponse = await HttpClient.SendAsync(ProxyRequest);

						Response.StatusCode = (int)ProxyResponse.StatusCode;
						Response.StatusMessage = ProxyResponse.ReasonPhrase;

						if (HasSniffers)
						{
							Message.Append(Response.StatusCode.ToString());
							Message.Append(' ');
							Message.AppendLine(Response.StatusMessage);
						}

						foreach (KeyValuePair<string, IEnumerable<string>> Header in ProxyResponse.Headers)
						{
							FieldName = Header.Key;

							if (CheckCase && httpHeaderFields.TryGetValue(FieldName, out FieldName2))
								FieldName = FieldName2;

							if (HasSniffers)
							{
								Message.Append(FieldName);
								Message.Append(": ");
								Message.AppendLine(string.Join(", ", Header.Value));
							}

							switch (FieldName)
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
										Response.SetHeader(FieldName, Value);
									break;
							}
						}

						if (!(ProxyResponse.Content is null))
						{
							foreach (KeyValuePair<string, IEnumerable<string>> Header in ProxyResponse.Content.Headers)
							{
								FieldName = Header.Key;

								if (CheckCase && httpHeaderFields.TryGetValue(FieldName, out FieldName2))
									FieldName = FieldName2;

								if (HasSniffers)
								{
									Message.Append(FieldName);
									Message.Append(": ");
									Message.AppendLine(string.Join(", ", Header.Value));
								}

								switch (FieldName)
								{
									case "Content-Length":
									case "Content-Encoding":
										break;

									default:
										foreach (string Value in Header.Value)
											Response.SetHeader(FieldName, Value);
										break;
								}
							}

							if (HasSniffers)
								this.ReceiveText(Message.ToString());

							byte[] Bin = await ProxyResponse.Content.ReadAsByteArrayAsync();

							if (HasSniffers)
								this.ReceiveBinary(true, Bin);

							Response.ContentLength = Bin.Length;

							await this.BeforeForwardResponse.Raise(this, new ProxyResponseEventArgs(ProxyResponse, Request, Response), false);

							await Response.Write(true, Bin);
						}
						else
						{
							if (HasSniffers)
								this.ReceiveText(Message.ToString());

							await this.BeforeForwardResponse.Raise(this, new ProxyResponseEventArgs(ProxyResponse, Request, Response), false);
						}

						await Response.SendResponse();
					}
				}
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

		#region ICommunicationLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => this.comLayer.DecoupledEvents;

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		public void Add(ISniffer Sniffer)
		{
			this.comLayer.Add(Sniffer);
		}

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			this.comLayer.AddRange(Sniffers);
		}

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer) => this.comLayer.Remove(Sniffer);

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers => this.comLayer.Sniffers;

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers => this.comLayer.HasSniffers;

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<ISniffer> GetEnumerator() => this.comLayer.GetEnumerator();

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.comLayer.GetEnumerator();

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count) => this.comLayer.ReceiveBinary(Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data) => this.comLayer.ReceiveBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.comLayer.ReceiveBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count) => this.comLayer.TransmitBinary(Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data) => this.comLayer.TransmitBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.comLayer.TransmitBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text) => this.comLayer.ReceiveText(Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text) => this.comLayer.TransmitText(Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment) => this.comLayer.Information(Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning) => this.comLayer.Warning(Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error) => this.comLayer.Error(Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception) => this.comLayer.Exception(Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception) => this.comLayer.Exception(Exception);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count) => this.comLayer.ReceiveBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.comLayer.ReceiveBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.comLayer.ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count) => this.comLayer.TransmitBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.comLayer.TransmitBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.comLayer.TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(DateTime Timestamp, string Text) => this.comLayer.ReceiveText(Timestamp, Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(DateTime Timestamp, string Text) => this.comLayer.TransmitText(Timestamp, Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(DateTime Timestamp, string Comment) => this.comLayer.Information(Timestamp, Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(DateTime Timestamp, string Warning) => this.comLayer.Warning(Timestamp, Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(DateTime Timestamp, string Error) => this.comLayer.Error(Timestamp, Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, string Exception) => this.comLayer.Exception(Timestamp, Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, Exception Exception) => this.comLayer.Exception(Timestamp, Exception);

		#endregion

	}
}
