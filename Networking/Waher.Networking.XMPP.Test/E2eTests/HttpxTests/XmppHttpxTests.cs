using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Text;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;

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
		public async Task TestInitialize()
		{
			this.webServer = new HttpServer(8081);

			this.webServer.Register("/Hello", (Request, Response) =>
			{
				if (!Request.Encrypted)
					throw new BadRequestException("Request must be encrypted.");

				Response.ContentType = PlainTextCodec.DefaultContentType;
				Response.Write("World");
				return Response.SendResponse();
			});

			this.webServer.Register("/Echo", null, async (Request, Response) =>
			{
				if (!Request.Encrypted)
					throw new BadRequestException("Request must be encrypted.");

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

					if (i != await Request.DataStream.ReadAsync(Buf, 0, BufSize))
						throw new IOException("Unexpected end of file.");

					await Response.Write(false, Buf, 0, i);
					c -= i;
				}

				await Response.SendResponse();
			});

			this.endpoints1 = this.GenerateEndpoints(new Aes256());
			this.endpoints2 = this.GenerateEndpoints(new Aes256());

			await this.ConnectClients();
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.webServer is not null)
			{
				await this.webServer.DisposeAsync();
				this.webServer = null;
			}

			await this.DisposeClients();
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
			this.httpxServer = new HttpxServer(Client, this.webServer, 8192)
			{
				RequiresE2e = true
			};
		}

		public override Task DisposeClients()
		{
			this.httpxServer?.Dispose();
			this.httpxServer = null;

			this.httpxClient1?.Dispose();
			this.httpxClient1 = null;

			this.httpxClient2?.Dispose();
			this.httpxClient2 = null;

			return base.DisposeClients();
		}

		[TestMethod]
		public void HTTPX_Test_01_GET()
		{
			this.DoGet(1);
		}

		private void DoGet(int Nr)
		{
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);
			MemoryStream ms = null;
			string ContentType = null;

			this.httpxClient1.GET(this.client2.FullJID, "/Hello",
				(Sender, e) =>
				{
					if (e.Ok && e.HasData && e.State.Equals(Nr))
					{
						ms = new MemoryStream();

						if (e.Data is not null)
							ms.Write(e.Data, 0, e.Data.Length);

						ContentType = e.HttpResponse.ContentType;
						Done1.Set();
					}
					else
						Error1.Set();

					return Task.CompletedTask;
				},
				async (Sender, e) =>
				{
					ms?.Write(e.Data, 0, e.Data.Length);

					if (e.Last)
					{
						ContentResponse Decoded = await InternetContent.DecodeAsync(ContentType, ms.ToArray(), null);

						if (!Decoded.HasError && Decoded.Decoded is string s && s == "World" && e.State.Equals(Nr))
							Done2.Set();
						else
							Error2.Set();
					}
				}, Nr);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 5000), "Response not returned.");
			Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 5000), "Data not returned.");
		}

		[TestMethod]
		public void HTTPX_Test_02_GET_PostBack()
		{
			PostBack PostBack = new();

			this.webServer.Register(PostBack);
			this.httpxClient1.PostResource = PostBack;

			this.DoGet(2);
		}

		[TestMethod]
		public async Task HTTPX_Test_03_POST()
		{
			await this.DoPost(3);
		}

		private async Task DoPost(int Nr)
		{
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);
			MemoryStream ms = null;
			string ContentType = null;
			byte[] Bin = new byte[1024 * 1024];
			string Message;

			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				Rnd.GetBytes(Bin);
			}

			Message = Convert.ToBase64String(Bin);

			await this.httpxClient1.POST(this.client2.FullJID, "/Echo", Message,
				(Sender, e) =>
				{
					if (e.Ok && e.HasData && e.State.Equals(Nr))
					{
						ms = new MemoryStream();

						if (e.Data is not null)
							ms.Write(e.Data, 0, e.Data.Length);

						ContentType = e.HttpResponse.ContentType;
						Done1.Set();
					}
					else
						Error1.Set();

					return Task.CompletedTask;
				},
				async (Sender, e) =>
				{
					ms?.Write(e.Data, 0, e.Data.Length);

					if (e.Last)
					{
						ContentResponse Decoded = await InternetContent.DecodeAsync(ContentType, ms.ToArray(), null);

						if (!Decoded.HasError && Decoded.Decoded is string s && s == Message && e.State.Equals(Nr))
							Done2.Set();
						else
							Error2.Set();
					}
				}, Nr);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 120000), "Response not returned.");
			Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 120000), "Data not returned.");
		}

		[TestMethod]
		public async Task HTTPX_Test_04_POST_PostBack()
		{
			PostBack PostBack = new();

			this.webServer.Register(PostBack);
			this.httpxClient1.PostResource = PostBack;

			await this.DoPost(4);
		}

	}
}
