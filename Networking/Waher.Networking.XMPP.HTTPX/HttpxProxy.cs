using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.HTTPX;

namespace Waher.Networking.XMPP.HTTPX
{
	public class HttpxProxy : HttpAsynchronousResource, IDisposable, IHttpGetMethod, IHttpGetRangesMethod, IHttpOptionsMethod,
		IHttpPostMethod, IHttpPostRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IHttpTraceMethod
	{
		private XmppClient xmppClient;
		private HttpxClient httpxClient;

		public HttpxProxy(string ResourceName, XmppClient XmppClient)
			: base(ResourceName)
		{
			this.xmppClient = XmppClient;
			this.httpxClient = new HttpxClient(this.xmppClient);
		}

		public void Dispose()
		{
			if (this.httpxClient != null)
			{
				this.httpxClient.Dispose();
				this.httpxClient = null;
			}
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

			RosterItem Item = this.xmppClient.GetRosterItem(BareJID);
			if (Item == null)
			{
				if (!XmppClient.BareJidRegEx.IsMatch(BareJID))
					throw new BadRequestException();

				// TODO: Request presence subscription, if user authenticated and request valid.

				throw new ConflictException();  // TODO: Provide body describing error.
			}
			else if (Item.HasLastPresence)
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
							// Do not forward cookies.
							break;

						default:
							Headers.AddLast(Header);
							break;
					}
				}

				this.httpxClient.Request(Item.LastPresenceFullJid, Method, LocalUrl, Request.Header.HttpVersion, Headers, 
					Request.HasData ? Request.DataStream : null, (sender, e) =>
					{
						Response.StatusCode = e.StatusCode;
						Response.StatusMessage = e.StatusMessage;

						if (e.HttpResponse != null)
						{
							foreach (KeyValuePair<string, string> Field in e.HttpResponse.GetHeaders())
								Response.SetHeader(Field.Key, Field.Value);
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
			else
				new ServiceUnavailableException();
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
