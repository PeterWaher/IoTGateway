using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Text;
using Waher.Content.Xml;
using Waher.Content.Xml.Text;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Events;
using Waher.Runtime.Temporary;
using Waher.Security;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// HTTPX server.
	/// </summary>
	public class HttpxServer : XmppExtension, IDisposable
	{
		/// <summary>
		/// String identifying the extension on the client.
		/// </summary>
		public const string ExtensionId = "XEP-0332-s";

		private readonly HttpServer server;
		private InBandBytestreams.IbbClient ibbClient = null;
		private P2P.SOCKS5.Socks5Proxy socks5Proxy = null;
		private readonly int maxChunkSize;
		private bool requiresE2e = false;

		/// <summary>
		/// HTTPX server.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="Server">HTTP Server.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		public HttpxServer(XmppClient Client, HttpServer Server, int MaxChunkSize)
			: base(Client)
		{
			this.server = Server;
			this.maxChunkSize = MaxChunkSize;

			HttpxChunks.RegisterChunkReceiver(this.client);

			this.client.RegisterIqSetHandler("req", HttpxClient.Namespace, this.ReqReceived, true);
			this.client.RegisterIqGetHandler("req", HttpxClient.Namespace, this.ReqReceived, false);
			this.client.RegisterMessageHandler("cancel", HttpxClient.Namespace, this.CancelReceived, false);
		}

		/// <summary>
		/// Associated HTTP server.
		/// </summary>
		public HttpServer HttpServer => this.server;

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { ExtensionId };

		/// <summary>
		/// If end-to-end encryption is required to be able to access web content via HTTPX.
		/// Default is false.
		/// </summary>
		public bool RequiresE2e
		{
			get => this.requiresE2e;
			set => this.requiresE2e = value;
		}

		/// <summary>
		/// In-band bytestream client, if supported.
		/// </summary>
		public InBandBytestreams.IbbClient IbbClient
		{
			get => this.ibbClient;
			set => this.ibbClient = value;
		}

		/// <summary>
		/// SOCKS5 Proxy, if supported.
		/// </summary>
		public P2P.SOCKS5.Socks5Proxy Socks5Proxy
		{
			get => this.socks5Proxy;
			set => this.socks5Proxy = value;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			HttpxChunks.UnregisterChunkReceiver(this.client);

			this.client.UnregisterIqSetHandler("req", HttpxClient.Namespace, this.ReqReceived, true);
			this.client.UnregisterIqGetHandler("req", HttpxClient.Namespace, this.ReqReceived, false);
			this.client.UnregisterMessageHandler("cancel", HttpxClient.Namespace, this.CancelReceived, false);

			base.Dispose();
		}

		private async Task ReqReceived(object Sender, IqEventArgs e)
		{
			if (this.requiresE2e && !e.UsesE2eEncryption)
			{
				await e.IqError(new StanzaErrors.ForbiddenException("End-to-end encryption required.", e.IQ));
				return;
			}

			string Method = XML.Attribute(e.Query, "method");
			string Resource = XML.Attribute(e.Query, "resource");
			string Version = XML.Attribute(e.Query, "version");
			int MaxChunkSize = XML.Attribute(e.Query, "maxChunkSize", 0);
			string PostResource = XML.Attribute(e.Query, "post");
			bool Sipub = XML.Attribute(e.Query, "sipub", true);
			bool Ibb = XML.Attribute(e.Query, "ibb", true);
			bool Socks5 = XML.Attribute(e.Query, "s5", true);
			bool Jingle = XML.Attribute(e.Query, "jingle", true);
			Stream DataStream = null;
			List<KeyValuePair<string, string>> HeaderFields = new List<KeyValuePair<string, string>>();
			HttpRequestHeader Header = null;

			if (MaxChunkSize <= 0 || MaxChunkSize > this.maxChunkSize)
				MaxChunkSize = this.maxChunkSize;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "headers":
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "header")
							{
								string Key = XML.Attribute((XmlElement)N2, "name");
								string Value = N2.InnerText;

								HeaderFields.Add(new KeyValuePair<string, string>(Key, Value));
							}
						}
						break;

					case "data":
						Header = new HttpRequestHeader(Method, Resource, Version, HttpxGetter.HttpxUriScheme,
							this.server.VanityResources, HeaderFields.ToArray());

						foreach (XmlNode N2 in N.ChildNodes)
						{
							switch (N2.LocalName)
							{
								case "text":
									MemoryStream ms = new MemoryStream();

									if (Header.ContentType is null)
										Header.Add(new HttpField("Content-Type", PlainTextCodec.DefaultContentType));

									byte[] Data = Header.ContentType.Encoding.GetBytes(N2.InnerText);
									ms.Write(Data, 0, Data.Length);
									DataStream = ms;
									break;

								case "xml":
									ms = new MemoryStream();

									if (Header.ContentType is null)
										Header.Add(new HttpField("Content-Type", XmlCodec.DefaultContentType));

									Data = Header.ContentType.Encoding.GetBytes(N2.InnerText);
									ms.Write(Data, 0, Data.Length);
									ms.Position = 0;
									DataStream = ms;
									break;

								case "base64":
									ms = new MemoryStream();
									Data = Convert.FromBase64String(N2.InnerText);
									ms.Write(Data, 0, Data.Length);
									ms.Position = 0;
									DataStream = ms;
									break;

								case "chunkedBase64":
									TemporaryStream File = new TemporaryStream();
									string StreamId = XML.Attribute((XmlElement)N2, "streamId");

									Header ??= new HttpRequestHeader(Method, Resource, Version, HttpxGetter.HttpxUriScheme,
										this.server.VanityResources, HeaderFields.ToArray());

									HttpxChunks.chunkedStreams.Add(e.From + " " + StreamId, new ServerChunkRecord(this, e.Id, e.From, e.To,
										new HttpRequest(this.server, Header, File, e.From, e.To, e.UsesE2eEncryption),
										e.E2eEncryption, e.E2eReference, File, MaxChunkSize, Sipub, Ibb, Socks5, Jingle, PostResource));
									return;

								case "sipub":
									// TODO: Implement File Transfer support.
									break;

								case "ibb":
									// TODO: Implement In-band byte streams support.
									break;

								case "s5":
									// TODO: Implement SOCKS5 streams support.
									break;

								case "jingle":
									// TODO: Implement Jingle support.
									break;
							}
						}
						break;
				}
			}

			Header ??= new HttpRequestHeader(Method, Resource, Version, HttpxGetter.HttpxUriScheme,
				this.server.VanityResources, HeaderFields.ToArray());

			await this.Process(e.Id, e.From, e.To, new HttpRequest(this.server, Header, DataStream, e.From, e.To, e.UsesE2eEncryption),
				e.E2eEncryption, e.E2eReference, MaxChunkSize, PostResource, Ibb, Socks5);
		}

		internal async Task Process(string Id, string From, string To, HttpRequest Request, IEndToEndEncryption E2e,
			string EndpointReference, int MaxChunkSize, string PostResource, bool Ibb, bool Socks5)
		{
			HttpAuthenticationScheme[] AuthenticationSchemes;
			bool Result;

			try
			{
				if (NetworkingModule.Stopping)
				{
					this.server.RequestReceived(Request, From, null, null);
					await this.SendQuickResponse(Request, E2e, EndpointReference, Id, From, To,
						ServiceUnavailableException.Code, ServiceUnavailableException.StatusMessage,
						false, MaxChunkSize, new KeyValuePair<string, string>("Retry-After", "300"));
					Result = false;
				}
				else if (this.server.TryGetResource(Request.Header.Resource, true, out HttpResource Resource, out string SubPath))
				{
					Request.Resource = Resource;

					this.server.RequestReceived(Request, From, Resource, SubPath);

					AuthenticationSchemes = Resource.GetAuthenticationSchemes(Request);
					if (!(AuthenticationSchemes is null) && AuthenticationSchemes.Length > 0 && Request.Header.Method != "OPTIONS")
					{
						foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
						{
							if (Scheme.UserSessions && Request.Session is null)
							{
								string HttpSessionID = HttpResource.GetSessionId(Request, Request.Response);

								if (!string.IsNullOrEmpty(HttpSessionID))
									Request.Session = this.server.GetSession(HttpSessionID);
							}

							IUser User = await Scheme.IsAuthenticated(Request);
							if (!(User is null))
							{
								Request.User = User;
								break;
							}
						}

						if (Request.User is null)
						{
							List<KeyValuePair<string, string>> Challenges = new List<KeyValuePair<string, string>>();

							foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
							{
								string Challenge = Scheme.GetChallenge();
								if (!string.IsNullOrEmpty(Challenge))
									Challenges.Add(new KeyValuePair<string, string>("WWW-Authenticate", Challenge));
							}

							await this.SendQuickResponse(Request, E2e, EndpointReference, Id, From, To, 401, "Unauthorized", false, MaxChunkSize, Challenges.ToArray());
							Request.Dispose();
							return;
						}
					}

					Request.SubPath = SubPath;
					Resource.Validate(Request);

					if (!(Request.Header.Expect is null))
					{
						if (Request.Header.Expect.Continue100)
						{
							if (!Request.HasData)
							{
								await this.SendQuickResponse(Request, E2e, EndpointReference, Id, From, To, 100, "Continue", false, MaxChunkSize);
								return;
							}
						}
						else
						{
							await this.SendQuickResponse(Request, E2e, EndpointReference, Id, From, To, 417, "Expectation Failed", true, MaxChunkSize);
							Request.Dispose();
							return;
						}
					}

					await this.ExecuteRequest(E2e, EndpointReference, Id, From, To, MaxChunkSize, PostResource, Ibb, Socks5, Request, Resource);
					return;
				}
				else
				{
					this.server.RequestReceived(Request, From, null, null);
					await this.SendQuickResponse(Request, E2e, EndpointReference, Id, From, To, 404, "Not Found", false, MaxChunkSize);
					Result = true;
				}
			}
			catch (Exception ex)
			{
				Result = (Request.Header.Expect is null || !Request.Header.Expect.Continue100 || Request.HasData);
				await this.SendQuickResponse(Request, E2e, EndpointReference, Id, From, To, ex, !Result, MaxChunkSize, PostResource);
			}

			Request.Dispose();
		}

		private async Task ExecuteRequest(IEndToEndEncryption E2e, string EndpointReference, string Id, string From, string To,
			int MaxChunkSize, string PostResource, bool Ibb, bool Socks5, HttpRequest Request, HttpResource Resource)
		{
			HttpResponse Response = null;

			try
			{
				Response = new HttpResponse(new HttpxResponse(this.client, E2e, Id, From, To, MaxChunkSize, PostResource,
					Ibb ? this.ibbClient : null, Socks5 ? this.socks5Proxy : null), this.server, Request);

				await Resource.Execute(this.server, Request, Response);
			}
			catch (Exception ex)
			{
				if (Response is null || !Response.HeaderSent)
					await this.SendQuickResponse(Request, E2e, EndpointReference, Id, From, To, ex, true, MaxChunkSize, PostResource);
			}
			finally
			{
				Request.Dispose();
			}
		}

		private Task SendQuickResponse(HttpRequest Request, IEndToEndEncryption E2e, string EndpointReference, string Id, string To,
			string From, int Code, string Message, bool CloseAfterTransmission, int MaxChunkSize, params KeyValuePair<string, string>[] HeaderFields)
		{
			HttpResponse Response = new HttpResponse(new HttpxResponse(this.client, E2e, Id, To, From, MaxChunkSize, null, null, null), this.server, Request)
			{
				StatusCode = Code,
				StatusMessage = Message,
				ContentLength = null,
				ContentType = null,
				ContentLanguage = null
			};

			foreach (KeyValuePair<string, string> P in HeaderFields)
				Response.SetHeader(P.Key, P.Value);

			if (CloseAfterTransmission)
				Response.SetHeader("Connection", "close");

			return Response.SendResponse();
		}

		private Task SendQuickResponse(HttpRequest Request, IEndToEndEncryption E2e, string EndpointReference, string Id, string To,
			string From, Exception Exception, bool CloseAfterTransmission, int MaxChunkSize, string PostResource)
		{
			HttpResponse Response = new HttpResponse(new HttpxResponse(this.client, E2e, Id, To, From, MaxChunkSize, PostResource, null, null), this.server, Request);

			if (CloseAfterTransmission)
				Response.SetHeader("Connection", "close");

			return Response.SendResponse(Exception);
		}

		private Task CancelReceived(object Sender, MessageEventArgs e)
		{
			if (this.requiresE2e && !e.UsesE2eEncryption)
				return Task.CompletedTask;

			string StreamId = XML.Attribute(e.Content, "streamId");

			HttpxResponse.CancelChunkedTransfer(e.From, e.To, StreamId);

			return Task.CompletedTask;
		}

		internal async Task<ContentStreamResponse> GetLocalTempStreamAsync(string LocalUrl, TemporaryStream Destination)
		{
			Tuple<int, string, byte[]> T = await this.server.GET(LocalUrl, new Script.Variables());
			int Code = T.Item1;
			string ContentType = T.Item2;
			byte[] Bin = T.Item3;

			if (Code < 200 || Code >= 300)
				return new ContentStreamResponse(new HttpException(Code, HttpException.GetStatusMessage(Code), Bin, ContentType));

			bool DestinationCreated = false;

			if (Destination is null)
			{
				Destination = new TemporaryStream();
				DestinationCreated = true;
			}

			try
			{
				await Destination.WriteAsync(Bin, 0, Bin.Length);
			}
			catch (Exception ex)
			{
				if (DestinationCreated)
					Destination.Dispose();

				return new ContentStreamResponse(ex);
			}

			return new ContentStreamResponse(ContentType, Destination);
		}

	}
}
