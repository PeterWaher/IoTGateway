using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;

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
		private readonly Uri baseUri;

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
			: base(ResourceName)
		{
			this.timeout = Timeout;

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

			this.baseUri = new Uri(sb.ToString());
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

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
				Uri RemoteUri;

				if (string.IsNullOrEmpty(Request.Header.QueryString))
				{
					if (!Uri.TryCreate(this.baseUri, Request.SubPath, out RemoteUri))
					{
						await Response.SendResponse(new BadRequestException());
						return;
					}
				}
				else
				{
					if (!Uri.TryCreate(this.baseUri, Request.SubPath + "?" + Request.Header.QueryString, out RemoteUri))
					{
						await Response.SendResponse(new BadRequestException());
						return;
					}
				}

				HttpClientHandler Handler = WebGetter.GetClientHandler();

				using (HttpClient HttpClient = new HttpClient(Handler, true)
				{
					Timeout = this.timeout
				})
				{
					using (HttpRequestMessage ProxyRequest = new HttpRequestMessage()
					{
						RequestUri = RemoteUri,
						Method = new HttpMethod(Request.Header.Method)
					})
					{
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
										ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(Field.Value.Substring(0, i), Field.Value.Substring(i + 1).TrimStart());
									break;

								case "Cookie":
									string Name = null;
									string Value = null;
									string Path = null;
									string Domain = null;
									bool First = true;

									foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(Field.Value))
									{
										if (First)
										{
											Name = P.Key;
											Value = P.Value;
											First = false;
										}
										else
										{
											switch (P.Key.ToLower())
											{
												case "path":
													Path = P.Value;
													break;

												case "domain":
													Domain = P.Value;
													break;
											}
										}
									}

									if (First)
										break;

									System.Net.Cookie Cookie;

									if (!string.IsNullOrEmpty(Domain))
										Cookie = new System.Net.Cookie(Name, Value, Path ?? string.Empty, Domain);
									else if (!string.IsNullOrEmpty(Path))
										Cookie = new System.Net.Cookie(Name, Value, Path);
									else
										Cookie = new System.Net.Cookie(Name, Value);

									Handler.CookieContainer.Add(ProxyRequest.RequestUri, Cookie);
									break;

								default:
									ProxyRequest.Headers.Add(Field.Key, Field.Value);
									break;
							}
						}

						if (Request.HasData)
						{
							Request.DataStream.Position = 0;
							ProxyRequest.Content = new StreamContent(Request.DataStream);
						}

						HttpResponseMessage ProxyResponse = await HttpClient.SendAsync(ProxyRequest);

						Response.StatusCode = (int)ProxyResponse.StatusCode;
						Response.StatusMessage = ProxyResponse.ReasonPhrase;

						foreach (KeyValuePair<string, IEnumerable<string>> Header in ProxyResponse.Headers)
						{
							foreach (string Value in Header.Value)
								Response.SetHeader(Header.Key, Value);
						}

						if (!(ProxyResponse.Content is null))
						{
							byte[] Bin = await ProxyResponse.Content.ReadAsByteArrayAsync();
							string ContentType = ProxyResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

							Response.ContentType = ContentType;
							Response.ContentLength = Bin.Length;

							await Response.WriteRawAsync(Bin);
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
	}
}
