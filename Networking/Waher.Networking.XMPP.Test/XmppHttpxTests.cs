using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.StreamErrors;
using Waher.Runtime.Cache;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppHttpxTests : CommunicationTests
	{
		private HttpServer webServer;
		private HttpxClient httpxClient1;
		private HttpxClient httpxClient2;
		private HttpxServer httpxServer;

		[TestInitialize]
		public void TestInitialize()
		{
			this.webServer = new HttpServer(8080);

			this.webServer.Register("/Hello", (Request, Response) =>
			{
				Response.ContentType = "text/plain";
				Response.Write("World");
				return Response.SendResponse();
			});

			this.ConnectClients();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.webServer?.Dispose();
			this.webServer = null;
		}

		public override void PrepareClient1(XmppClient Client)
		{
			base.PrepareClient1(Client);
			this.httpxClient1 = new HttpxClient(Client, 8192);
		}

		public override void PrepareClient2(XmppClient Client)
		{
			base.PrepareClient2(Client);
			this.httpxClient2 = new HttpxClient(Client, 8192);
			this.httpxServer = new HttpxServer(Client, this.webServer, 8192);
		}

		public override void DisposeClients()
		{
			this.httpxServer?.Dispose();
			this.httpxServer = null;

			this.httpxClient1?.Dispose();
			this.httpxClient1 = null;

			this.httpxClient2?.Dispose();
			this.httpxClient2 = null;

			base.DisposeClients();
		}

		[TestMethod]
		public void HTTPX_Test_01_GET()
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);
			MemoryStream ms = null;
			string ContentType = null;

			this.httpxClient1.GET(this.client2.FullJID, "/Hello",
				(sender, e) =>
				{
					if (e.Ok && e.HasData && e.State.Equals(1))
					{
						ms = new MemoryStream();
						ContentType = e.HttpResponse.ContentType;
						Done1.Set();
					}
					else
						Error1.Set();

					return Task.CompletedTask;
				},
				(sender, e) =>
				{
					ms?.Write(e.Data, 0, e.Data.Length);

					if (e.Last)
					{
						object Decoded = InternetContent.Decode(ContentType, ms.ToArray(), null);

						if (Decoded is string s && s == "World" && e.State.Equals(1))
							Done2.Set();
						else
							Error2.Set();
					}

					return Task.CompletedTask;
				}, 1);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 5000), "Response not returned.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 5000), "Data not returned.");
		}

		[TestMethod]
		public void HTTPX_Test_02_GET_PostBack()
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);
			PostBack PostBack = new PostBack();
			MemoryStream ms = null;
			string ContentType = null;

			this.webServer.Register(PostBack);
			this.httpxClient1.PostResource = PostBack;

			this.httpxClient1.GET(this.client2.FullJID, "/Hello",
				(sender, e) =>
				{
					if (e.Ok)
						Done1.Set();
					else
						Error1.Set();

					return Task.CompletedTask;
				},
				(sender, e) =>
				{
					if (e.Last)
						Done2.Set();

					return Task.CompletedTask;
				}, 1);

			this.httpxClient1.GET(this.client2.FullJID, "/Hello",
				(sender, e) =>
				{
					if (e.Ok && e.HasData && e.State.Equals(2))
					{
						ms = new MemoryStream();
						ContentType = e.HttpResponse.ContentType;
						Done1.Set();
					}
					else
						Error1.Set();

					return Task.CompletedTask;
				},
				(sender, e) =>
				{
					ms?.Write(e.Data, 0, e.Data.Length);

					if (e.Last)
					{
						object Decoded = InternetContent.Decode(ContentType, ms.ToArray(), null);

						if (Decoded is string s && s == "World" && e.State.Equals(2))
							Done2.Set();
						else
							Error2.Set();
					}

					return Task.CompletedTask;
				}, 2);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 5000), "Response not returned.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 5000), "Data not returned.");
		}

		private class PostBack : HttpSynchronousResource, IPostResource, IHttpPostMethod
		{
			private Cache<string, KeyValuePair<PostBackEventHandler, object>> queries = null;
			private readonly object synchObj = new object();
			private readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

			public PostBack()
				: base("/PostBack")
			{
			}

			public override bool HandlesSubPaths => true;
			public override bool UserSessions => false;
			public bool AllowsPOST => true;

			public string GetUrl(PostBackEventHandler Callback, object State)
			{
				byte[] Bin = new byte[32];
				string Key;

				lock (this.synchObj)
				{
					rnd.GetBytes(Bin);
					Key = Base64Url.Encode(Bin);

					if (this.queries is null)
					{
						this.queries = new Cache<string, KeyValuePair<PostBackEventHandler, object>>(int.MaxValue, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
						this.queries.Removed += this.Queries_Removed;
					}

					this.queries[Key] = new KeyValuePair<PostBackEventHandler, object>(Callback, State);
					this.queries[string.Empty] = new KeyValuePair<PostBackEventHandler, object>(null, null);    // Keep cache active to avoid multiple recreation when a series of requests occur in sequence.
				}

				return "http://localhost:8080" + this.ResourceName + "/" + Key;
			}

			private void Queries_Removed(object Sender, CacheItemEventArgs<string, KeyValuePair<PostBackEventHandler, object>> e)
			{
				lock (this.synchObj)
				{
					if (!(this.queries is null) && this.queries.Count == 0)
					{
						this.queries.Dispose();
						this.queries = null;
					}
				}
			}

			public Task POST(HttpRequest Request, HttpResponse Response)
			{
				if (!Request.HasData)
					throw new BadRequestException("Missing data.");

				string ContentType = Request.Header.ContentType?.Value;
				if (string.IsNullOrEmpty(ContentType) || Array.IndexOf(Content.Binary.BinaryDecoder.BinaryContentTypes, ContentType) < 0)
					throw new BadRequestException("Expected Binary data.");

				string From = Request.Header.From?.Value;
				if (string.IsNullOrEmpty(From))
					throw new BadRequestException("No From header.");

				string To = Request.Header["Origin"];
				if (string.IsNullOrEmpty(To))
					throw new BadRequestException("No Origin header.");

				string Referer = Request.Header.Referer?.Value;
				string EndpointReference;
				string SymmetricCipherReference;
				int i;

				if (!string.IsNullOrEmpty(Referer) && (i = Referer.IndexOf(':')) >= 0)
				{
					EndpointReference = Referer.Substring(0, i);
					SymmetricCipherReference = Referer.Substring(i + 1);
				}
				else
				{
					EndpointReference = string.Empty;
					SymmetricCipherReference = string.Empty;
				}

				string Key = Request.SubPath;
				if (string.IsNullOrEmpty(Key))
					throw new BadRequestException("No sub-path provided.");

				Key = Key.Substring(1);

				KeyValuePair<PostBackEventHandler, object> Rec;

				lock (this.synchObj)
				{
					if (this.queries is null || !this.queries.TryGetValue(Key, out Rec))
						throw new NotFoundException("Resource sub-key not found.");
				}

				Request.DataStream.Position = 0;

				return Rec.Key.Invoke(this, new PostBackEventArgs(Request.DataStream, Rec.Value, From, To, EndpointReference, SymmetricCipherReference));
			}
		}

	}
}
