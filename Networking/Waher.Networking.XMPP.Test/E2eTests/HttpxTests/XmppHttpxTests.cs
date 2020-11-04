using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.HTTPX;

namespace Waher.Networking.XMPP.Test.E2eTests.HttpxTests
{
	[TestClass]
	public abstract class XmppHttpxTests : E2eTests
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

			this.webServer.Register("/Echo", null, async (Request, Response) =>
			{
				if (!Request.HasData)
					throw new BadRequestException("No data.");

				Response.StatusCode = 200;
				Response.ContentType = Request.Header.ContentType.Value;

				long c = Request.DataStream.Length;
				int BufSize = (int)Math.Min(65536, c);
				byte[] Buf = new byte[BufSize];
				int i;

				while (c > 0)
				{
					i = (int)Math.Min(c, BufSize);

					if (i != await (Request.DataStream.ReadAsync(Buf, 0, BufSize)))
						throw new IOException("Unexpected end of file.");

					await Response.Write(Buf, 0, i);
					c -= i;
				}

				await Response.SendResponse();
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
			this.httpxClient1 = new HttpxClient(Client, this.endpointSecurity1, 8192);

			foreach (ISniffer Sniffer in Client.Sniffers)
				this.webServer.Add(Sniffer);
		}

		public override void PrepareClient2(XmppClient Client)
		{
			base.PrepareClient2(Client);
			this.httpxClient2 = new HttpxClient(Client, this.endpointSecurity2, 8192);
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
			this.DoGet(1);
		}

		private void DoGet(int Nr)
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
					if (e.Ok && e.HasData && e.State.Equals(Nr))
					{
						ms = new MemoryStream();

						if (!(e.Data is null))
							ms.Write(e.Data, 0, e.Data.Length);

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

						if (Decoded is string s && s == "World" && e.State.Equals(Nr))
							Done2.Set();
						else
							Error2.Set();
					}

					return Task.CompletedTask;
				}, Nr);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 5000), "Response not returned.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 5000), "Data not returned.");
		}

		[TestMethod]
		public void HTTPX_Test_02_GET_PostBack()
		{
			PostBack PostBack = new PostBack();

			this.webServer.Register(PostBack);
			this.httpxClient1.PostResource = PostBack;

			this.DoGet(2);
		}

		[TestMethod]
		public void HTTPX_Test_03_POST()
		{
			this.DoPost(3);
		}

		private void DoPost(int Nr)
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);
			MemoryStream ms = null;
			string ContentType = null;
			byte[] Bin = new byte[1024 * 1024];
			string Message;

			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				Rnd.GetBytes(Bin);
			}

			Message = Convert.ToBase64String(Bin);

			this.httpxClient1.POST(this.client2.FullJID, "/Echo", Message,
				(sender, e) =>
				{
					if (e.Ok && e.HasData && e.State.Equals(Nr))
					{
						ms = new MemoryStream();

						if (!(e.Data is null))
							ms.Write(e.Data, 0, e.Data.Length);

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

						if (Decoded is string s && s == Message && e.State.Equals(Nr))
							Done2.Set();
						else
							Error2.Set();
					}

					return Task.CompletedTask;
				}, Nr);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 120000), "Response not returned.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 120000), "Data not returned.");
		}

		[TestMethod]
		public void HTTPX_Test_04_POST_PostBack()
		{
			PostBack PostBack = new PostBack();

			this.webServer.Register(PostBack);
			this.httpxClient1.PostResource = PostBack;

			this.DoPost(4);
		}

	}
}
