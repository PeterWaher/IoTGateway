using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.P2P;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Implements a Proxy resource that allows Web clients to fetch HTTP-based resources over HTTPX.
	/// </summary>
	public class HttpxProxy : HttpAsynchronousResource, IDisposable, IHttpGetMethod, IHttpGetRangesMethod, IHttpOptionsMethod,
		IHttpPostMethod, IHttpPostRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IHttpTraceMethod
	{
		private XmppClient defaultXmppClient;
		private HttpxClient httpxClient;
		private XmppServerlessMessaging serverlessMessaging;

		/// <summary>
		/// Implements a Proxy resource that allows Web clients to fetch HTTP-based resources over HTTPX.
		/// </summary>
		/// <param name="ResourceName">Resource name of proxy resource.</param>
		/// <param name="DefaultXmppClient">Default XMPP client.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		public HttpxProxy(string ResourceName, XmppClient DefaultXmppClient, int MaxChunkSize)
			: this(ResourceName, DefaultXmppClient, MaxChunkSize, null)
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
			: base(ResourceName)
		{
			this.defaultXmppClient = DefaultXmppClient;
			this.httpxClient = new HttpxClient(this.defaultXmppClient, MaxChunkSize);
			this.serverlessMessaging = ServerlessMessaging;
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

		public override bool HandlesSubPaths
		{
			get
			{
				return true;
			}
		}

		public override bool UserSessions
		{
			get
			{
				return false;
			}
		}

		private void Request(string Method, HttpRequest Request, HttpResponse Response)
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

			RosterItem Item = this.defaultXmppClient.GetRosterItem(BareJID);
			if (Item == null)
			{
				if (!XmppClient.BareJidRegEx.IsMatch(BareJID))
					throw new BadRequestException();

				// TODO: Request presence subscription, if user authenticated and request valid.

				throw new ConflictException();  // TODO: Provide body describing error.
			}
			else if (Item.HasLastPresence)
			{
				if (this.serverlessMessaging != null)
				{
					this.serverlessMessaging.GetPeerConnection(BareJID, (sender, e) =>
					{
						if (e.Client == null)
							this.SendRequest(this.httpxClient, Item.LastPresenceFullJid, Method, BareJID, LocalUrl, Request, Response);
						else
						{
							HttpxClient HttpxClient;
							object Obj;

							if (e.Client.SupportsFeature(HttpxClient.Namespace) &&
								e.Client.TryGetTag("HttpxClient", out Obj) &&
								(HttpxClient = Obj as HttpxClient) != null)
							{
								this.SendRequest(HttpxClient, BareJID, Method, BareJID, LocalUrl, Request, Response);
							}
							else
								this.SendRequest(this.httpxClient, Item.LastPresenceFullJid, Method, BareJID, LocalUrl, Request, Response);
						}
					}, null);
				}
				else
					this.SendRequest(this.httpxClient, Item.LastPresenceFullJid, Method, BareJID, LocalUrl, Request, Response);
			}
			else
				throw new ServiceUnavailableException();
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

			HttpxClient.Request(To, Method, LocalUrl, Request.Header.HttpVersion, Headers, Request.HasData ? Request.DataStream : null, 
				(sender, e) =>
				{
					Response.StatusCode = e.StatusCode;
					Response.StatusMessage = e.StatusMessage;

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

								default:
									Response.SetHeader(Field.Key, Field.Value);
									break;
							}
						}
					}

					if (!e.HasData)
						Response.SendResponse();

				}, (sender, e) =>
				{
					Response.Write(e.Data);

					if (e.Last)
						Response.SendResponse();
				}, null);
		}

		public void GET(HttpRequest Request, HttpResponse Response)
		{
			this.Request("GET", Request, Response);
		}

		public void GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval FirstInterval)
		{
			this.Request("GET", Request, Response);
		}

		public void OPTIONS(HttpRequest Request, HttpResponse Response)
		{
			this.Request("OPTIONS", Request, Response);
		}

		public void POST(HttpRequest Request, HttpResponse Response)
		{
			this.Request("POST", Request, Response);
		}

		public void POST(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			this.Request("POST", Request, Response);
		}

		public void PUT(HttpRequest Request, HttpResponse Response)
		{
			this.Request("PUT", Request, Response);
		}

		public void PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			this.Request("PUT", Request, Response);
		}

		public void TRACE(HttpRequest Request, HttpResponse Response)
		{
			this.Request("TRACE", Request, Response);
		}
	}
}
