using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.P2P;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Implements a Proxy resource that allows Web clients to fetch HTTP-based resources over HTTPX.
	/// </summary>
	public class HttpxProxy : HttpAsynchronousResource, IDisposable, IHttpGetMethod, IHttpGetRangesMethod, IHttpOptionsMethod,
		IHttpPostMethod, IHttpPostRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IHttpTraceMethod, IHttpDeleteMethod
	{
		private readonly XmppClient defaultXmppClient;
		private HttpxClient httpxClient;
		private XmppServerlessMessaging serverlessMessaging;
		private IHttpxCache httpxCache;
		private InBandBytestreams.IbbClient ibbClient = null;
		private P2P.SOCKS5.Socks5Proxy socks5Proxy = null;

		/// <summary>
		/// Implements a Proxy resource that allows Web clients to fetch HTTP-based resources over HTTPX.
		/// </summary>
		/// <param name="ResourceName">Resource name of proxy resource.</param>
		/// <param name="DefaultXmppClient">Default XMPP client.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		public HttpxProxy(string ResourceName, XmppClient DefaultXmppClient, int MaxChunkSize)
			: this(ResourceName, DefaultXmppClient, MaxChunkSize, null, null)
		{
		}

		/// <summary>
		/// Implements a Proxy resource that allows Web clients to fetch HTTP-based resources over HTTPX.
		/// </summary>
		/// <param name="ResourceName">Resource name of proxy resource.</param>
		/// <param name="DefaultXmppClient">Default XMPP client.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		/// <param name="ServerlessMessaging">Serverless messaging manager.</param>
		public HttpxProxy(string ResourceName, XmppClient DefaultXmppClient, int MaxChunkSize, XmppServerlessMessaging ServerlessMessaging)
			: this(ResourceName, DefaultXmppClient, MaxChunkSize, ServerlessMessaging, null)
		{
		}

		/// <summary>
		/// Implements a Proxy resource that allows Web clients to fetch HTTP-based resources over HTTPX.
		/// </summary>
		/// <param name="ResourceName">Resource name of proxy resource.</param>
		/// <param name="DefaultXmppClient">Default XMPP client.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		/// <param name="ServerlessMessaging">Serverless messaging manager.</param>
		/// <param name="HttpxCache">HTTPX cache object.</param>
		public HttpxProxy(string ResourceName, XmppClient DefaultXmppClient, int MaxChunkSize, XmppServerlessMessaging ServerlessMessaging,
			IHttpxCache HttpxCache) : base(ResourceName)
		{
			this.defaultXmppClient = DefaultXmppClient;
			this.httpxClient = new HttpxClient(this.defaultXmppClient, MaxChunkSize);
			this.serverlessMessaging = ServerlessMessaging;
			this.httpxCache = HttpxCache;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.httpxClient != null)
			{
				this.httpxClient.Dispose();
				this.httpxClient = null;
			}
		}

		/// <summary>
		/// Serverless messaging manager.
		/// </summary>
		public XmppServerlessMessaging ServerlessMessaging
		{
			get { return this.serverlessMessaging; }
			set
			{
				if (this.serverlessMessaging != null && this.serverlessMessaging != value)
					throw new Exception("Property already set.");

				this.serverlessMessaging = value;
			}
		}

		/// <summary>
		/// Reference to the HTTPX Cache manager.
		/// </summary>
		public IHttpxCache HttpxCache
		{
			get { return this.httpxCache; }
			set
			{
				if (this.httpxCache != null && this.httpxCache != value)
					throw new Exception("Property already set.");

				this.httpxCache = value;
			}
		}

		/// <summary>
		/// Default XMPP client.
		/// </summary>
		public XmppClient DefaultXmppClient
		{
			get { return this.defaultXmppClient; }
		}

		/// <summary>
		/// Default HTTPX client.
		/// </summary>
		public HttpxClient DefaultHttpxClient
		{
			get { return this.httpxClient; }
		}

		/// <summary>
		/// In-band bytestream client, if supported.
		/// </summary>
		public InBandBytestreams.IbbClient IbbClient
		{
			get { return this.ibbClient; }
			set
			{
				this.ibbClient = value;

				if (this.httpxClient != null)
					this.httpxClient.IbbClient = value;
			}
		}

		/// <summary>
		/// SOCKS5 proxy, if supported.
		/// </summary>
		public P2P.SOCKS5.Socks5Proxy Socks5Proxy
		{
			get { return this.socks5Proxy; }
			set
			{
				this.socks5Proxy = value;

				if (this.httpxClient != null)
					this.httpxClient.Socks5Proxy = value;
			}
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions
		{
			get
			{
				return false;
			}
		}

		private async void Request(string Method, HttpRequest Request, HttpResponse Response)
		{
			try
			{
				string Url = Request.SubPath;
				if (Url.StartsWith("/"))
					Url = Url.Substring(1);

				if (!Url.StartsWith("httpx://", StringComparison.OrdinalIgnoreCase))
					throw new BadRequestException();

				int i = Url.IndexOf('/', 8);
				if (i < 0)
					throw new BadRequestException();

				string BareJID = Url.Substring(8, i - 8);
				string LocalUrl = Url.Substring(i);

				IHttpxCachedResource CachedResource;

				if (Method == "GET" && this.httpxCache != null)
				{
					if ((CachedResource = await this.httpxCache.TryGetCachedResource(BareJID, LocalUrl)) != null)
					{
						if (Request.Header.IfNoneMatch != null)
						{
							if (CachedResource.ETag != null && Request.Header.IfNoneMatch.Value == CachedResource.ETag)
								throw new NotModifiedException();
						}
						else if (Request.Header.IfModifiedSince != null)
						{
							DateTimeOffset? Limit;

							if ((Limit = Request.Header.IfModifiedSince.Timestamp).HasValue &&
								HttpFolderResource.LessOrEqual(CachedResource.LastModified.UtcDateTime, Limit.Value.ToUniversalTime()))
							{
								throw new NotModifiedException();
							}
						}

						HttpFolderResource.SendResponse(CachedResource.FileName, CachedResource.ContentType, CachedResource.ETag,
							CachedResource.LastModified.UtcDateTime, Response);

						return;
					}
				}

				RosterItem Item = this.defaultXmppClient.GetRosterItem(BareJID);
				if (Item is null)
				{
					if (!XmppClient.BareJidRegEx.IsMatch(BareJID))
						throw new BadRequestException();

					// TODO: Request presence subscription, if user authenticated and request valid.

					throw new ConflictException();  // TODO: Provide body describing error.
				}
				else
				{
					foreach (PresenceEventArgs e in Item.Resources)
					{
						// TODO: Select one based on features.

						if (this.serverlessMessaging != null)
						{
							this.serverlessMessaging.GetPeerConnection(e.From, this.SendP2P, new SendP2pRec()
							{
								item = Item,
								method = Method,
								fullJID = e.From,
								localUrl = LocalUrl,
								request = Request,
								response = Response
							});
						}
						else
							this.SendRequest(this.httpxClient, e.From, Method, BareJID, LocalUrl, Request, Response);

						return;
					}

					throw new ServiceUnavailableException();
				}
			}
			catch (Exception ex)
			{
				Response.SendResponse(ex);
			}
		}

		private class SendP2pRec
		{
			public RosterItem item;
			public string method;
			public string fullJID;
			public string localUrl;
			public HttpRequest request;
			public HttpResponse response;
		}

		private void SendP2P(object Sender, PeerConnectionEventArgs e)
		{
			SendP2pRec Rec = (SendP2pRec)e.State;

			try
			{
				if (e.Client is null)
					this.SendRequest(this.httpxClient, Rec.fullJID, Rec.method, Rec.fullJID, Rec.localUrl, Rec.request, Rec.response);
				else
				{
					HttpxClient HttpxClient;
					
					if (e.Client.SupportsFeature(HttpxClient.Namespace) &&
						e.Client.TryGetTag("HttpxClient", out object Obj) &&
						(HttpxClient = Obj as HttpxClient) != null)
					{
						this.SendRequest(HttpxClient, Rec.fullJID, Rec.method, Rec.fullJID, Rec.localUrl, Rec.request, Rec.response);
					}
					else
						this.SendRequest(this.httpxClient, Rec.fullJID, Rec.method, Rec.fullJID, Rec.localUrl, Rec.request, Rec.response);
				}
			}
			catch (Exception ex)
			{
				Rec.response.SendResponse(ex);
			}
		}

		private void SendRequest(HttpxClient HttpxClient, string To, string Method, string BareJID, string LocalUrl,
			HttpRequest Request, HttpResponse Response)
		{
			LinkedList<HttpField> Headers = new LinkedList<HttpField>();

			foreach (HttpField Header in Request.Header)
			{
				switch (Header.Key.ToLower())
				{
					case "host":
						Headers.AddLast(new HttpField("Host", BareJID));
						break;

					case "cookie":
					case "set-cookie":
						// Do not forward cookies.
						break;

					default:
						Headers.AddLast(Header);
						break;
				}
			}

			ReadoutState State = new ReadoutState(Response, BareJID, LocalUrl)
			{
				Cacheable = (Method == "GET" && this.httpxCache != null)
			};

			string s = LocalUrl;
			int i = s.IndexOf('.');
			if (i > 0)
			{
				s = s.Substring(i + 1);
				i = s.IndexOfAny(new char[] { '?', '#' });
				if (i > 0)
					s = s.Substring(0, i);

				if (this.httpxCache.CanCache(BareJID, LocalUrl, InternetContent.GetContentType(s)))
				{
					LinkedListNode<HttpField> Loop = Headers.First;
					LinkedListNode<HttpField> Next;

					while (Loop != null)
					{
						Next = Loop.Next;

						switch (Loop.Value.Key.ToLower())
						{
							case "if-match":
							case "if-modified-since":
							case "if-none-match":
							case "if-range":
							case "if-unmodified-since":
								Headers.Remove(Loop);
								break;
						}

						Loop = Next;
					}
				}
			}

			HttpxClient.Request(To, Method, LocalUrl, Request.Header.HttpVersion, Headers, Request.HasData ? Request.DataStream : null,
				this.RequestResponse, this.ResponseData, State);
		}

		private void RequestResponse(object Sender, HttpxResponseEventArgs e)
		{
			ReadoutState State2 = (ReadoutState)e.State;

			State2.Response.StatusCode = e.StatusCode;
			State2.Response.StatusMessage = e.StatusMessage;

			if (e.HttpResponse != null)
			{
				foreach (KeyValuePair<string, string> Field in e.HttpResponse.GetHeaders())
				{
					switch (Field.Key.ToLower())
					{
						case "cookie":
						case "set-cookie":
							// Do not forward cookies.
							break;

						case "content-type":
							State2.ContentType = Field.Value;
							State2.Response.SetHeader(Field.Key, Field.Value);
							break;

						case "etag":
							State2.ETag = Field.Value;
							State2.Response.SetHeader(Field.Key, Field.Value);
							break;

						case "last-modified":
							DateTimeOffset TP;
							if (CommonTypes.TryParseRfc822(Field.Value, out TP))
								State2.LastModified = TP;
							State2.Response.SetHeader(Field.Key, Field.Value);
							break;

						case "expires":
							if (CommonTypes.TryParseRfc822(Field.Value, out TP))
								State2.Expires = TP;
							State2.Response.SetHeader(Field.Key, Field.Value);
							break;

						case "cache-control":
							State2.CacheControl = Field.Value;
							State2.Response.SetHeader(Field.Key, Field.Value);
							break;

						case "pragma":
							State2.Pragma = Field.Value;
							State2.Response.SetHeader(Field.Key, Field.Value);
							break;

						default:
							State2.Response.SetHeader(Field.Key, Field.Value);
							break;
					}
				}
			}

			if (!e.HasData)
				State2.Response.SendResponse();
			else
			{
				if (e.StatusCode == 200 && State2.Cacheable && State2.CanCache &&
					this.httpxCache.CanCache(State2.BareJid, State2.LocalResource, State2.ContentType))
				{
					State2.TempOutput = new TemporaryFile();
				}

				if (e.Data != null)
					this.BinaryDataReceived(State2, true, e.Data);
			}
		}

		private void ResponseData(object Sender, HttpxResponseDataEventArgs e)
		{
			ReadoutState State2 = (ReadoutState)e.State;

			this.BinaryDataReceived(State2, e.Last, e.Data);
		}

		private void BinaryDataReceived(ReadoutState State2, bool Last, byte[] Data)
		{
			try
			{
				State2.Response.Write(Data);
			}
			catch (Exception)
			{
				State2.Dispose();
				return;
			}

			if (State2.TempOutput != null)
				State2.TempOutput.Write(Data, 0, Data.Length);

			if (Last)
			{
				State2.Response.SendResponse();
				this.AddToCacheAsync(State2);
			}
		}

		private async void AddToCacheAsync(ReadoutState State)
		{
			try
			{
				if (State.TempOutput != null)
				{
					State.TempOutput.Position = 0;

					await this.httpxCache.AddToCache(State.BareJid, State.LocalResource, State.ContentType, State.ETag,
						State.LastModified.Value, State.Expires, State.TempOutput);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				try
				{
					State.Dispose();
				}
				catch (Exception ex2)
				{
					Log.Critical(ex2);
				}
			}
		}

		private class ReadoutState : IDisposable
		{
			public bool Cacheable = false;
			public HttpResponse Response;
			public string ETag = null;
			public string BareJid = null;
			public string LocalResource = null;
			public string ContentType = null;
			public string CacheControl = null;
			public string Pragma = null;
			public DateTimeOffset? Expires = null;
			public DateTimeOffset? LastModified = null;
			public TemporaryFile TempOutput = null;

			public ReadoutState(HttpResponse Response, string BareJid, string LocalResource)
			{
				this.Response = Response;
				this.BareJid = BareJid;
				this.LocalResource = LocalResource;
			}

			public bool CanCache
			{
				get
				{
					if (this.ETag is null || !this.LastModified.HasValue)
						return false;

					if (this.CacheControl != null)
					{
						if ((this.CacheControl.Contains("no-cache") || this.CacheControl.Contains("no-store")))
							return false;

						if (!this.Expires.HasValue)
						{
							string s = this.CacheControl;
							int i = s.IndexOf("max-age");
							int c = s.Length;
							char ch;

							while (i < c && ((ch = s[i]) <= ' ' || ch == '=' || ch == 160))
								i++;

							int j = i;

							while (j < c && (ch = s[j]) >= '0' && ch <= '9')
								j++;

							if (j > i && int.TryParse(s.Substring(i, j - i), out j))
								this.Expires = DateTimeOffset.UtcNow.AddSeconds(j);
						}
					}

					if (this.Pragma != null && this.Pragma.Contains("no-cache"))
						return false;

					return true;
				}
			}

			public void Dispose()
			{
				if (this.TempOutput != null)
				{
					this.TempOutput.Dispose();
					this.TempOutput = null;
				}
			}
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void GET(HttpRequest Request, HttpResponse Response)
		{
			this.Request("GET", Request, Response);
		}

		/// <summary>
		/// Executes the ranged GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="FirstInterval">First byte range interval.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval FirstInterval)
		{
			this.Request("GET", Request, Response);
		}

		/// <summary>
		/// If the OPTIONS method is allowed.
		/// </summary>
		public bool AllowsOPTIONS
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the OPTIONS method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void OPTIONS(HttpRequest Request, HttpResponse Response)
		{
			this.Request("OPTIONS", Request, Response);
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void POST(HttpRequest Request, HttpResponse Response)
		{
			this.Request("POST", Request, Response);
		}

		/// <summary>
		/// Executes the ranged POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void POST(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			this.Request("POST", Request, Response);
		}

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void PUT(HttpRequest Request, HttpResponse Response)
		{
			this.Request("PUT", Request, Response);
		}

		/// <summary>
		/// Executes the ranged PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			this.Request("PUT", Request, Response);
		}

		/// <summary>
		/// If the TRACE method is allowed.
		/// </summary>
		public bool AllowsTRACE
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the TRACE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void TRACE(HttpRequest Request, HttpResponse Response)
		{
			this.Request("TRACE", Request, Response);
		}

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void DELETE(HttpRequest Request, HttpResponse Response)
		{
			this.Request("DELETE", Request, Response);
		}
	}
}
