using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Text;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.HTTPX;
using Waher.Persistence;
using Waher.Persistence.Files;

namespace Waher.Networking.XMPP.Test.E2eTests
{
	[TestClass]
	public class XmppHttpxTests : E2eTests
	{
		private static FilesProvider provider;
		private static string dataFolder;

		private HttpServer webServer;
		private HttpxClient httpxClient1;
		private HttpxClient httpxClient2;
		private HttpxServer httpxServer;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			dataFolder = Path.Combine("Data", "XmppE2eTests");

			provider = await FilesProvider.CreateAsync(dataFolder, "Default", 8192, 1000, 8192, Encoding.UTF8, 10000, false);
			Database.Register(provider, false);

			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			await DisposeSnifferAndLog();

			if (provider is not null)
			{
				Database.Register(new NullDatabaseProvider(), false);
				await provider.DisposeAsync();
				provider = null;
			}

			if (!string.IsNullOrEmpty(dataFolder) && Directory.Exists(dataFolder))
				Directory.Delete(dataFolder, true);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.webServer = new HttpServer(8083);

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

					if (i != await Request.DataStream.ReadAsync(Buf, 0, BufSize, CancellationToken.None))
						throw new IOException("Unexpected end of file.");

					await Response.Write(false, Buf, 0, i);
					c -= i;
				}

				await Response.SendResponse();
			});
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

		public override void PrepareClient1(XmppClient Client, int SecurityStrength)
		{
			base.PrepareClient1(Client, SecurityStrength);
			this.httpxClient1 = new HttpxClient(Client, this.endpointSecurity1, 8192);

			foreach (ISniffer Sniffer in Client.Sniffers)
				this.webServer.Add(Sniffer);
		}

		public override void PrepareClient2(XmppClient Client, int SecurityStrength)
		{
			base.PrepareClient2(Client, SecurityStrength);
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
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		public async Task HTTPX_Test_01_GET(AsymmetricCipher EccType, int SecurityStrength, SymmetricCipher SymmetricCipherType)
		{
			this.PrepareEndpoints(EccType, SecurityStrength, SymmetricCipherType);
			await this.ConnectClients(SecurityStrength);

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
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		public async Task HTTPX_Test_02_GET_PostBack(AsymmetricCipher EccType, int SecurityStrength, SymmetricCipher SymmetricCipherType)
		{
			this.PrepareEndpoints(EccType, SecurityStrength, SymmetricCipherType);
			await this.ConnectClients(SecurityStrength);

			PostBack PostBack = new();

			this.webServer.Register(PostBack);
			this.httpxClient1.PostResource = PostBack;

			this.DoGet(2);
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		public async Task HTTPX_Test_03_POST(AsymmetricCipher EccType, int SecurityStrength, SymmetricCipher SymmetricCipherType)
		{
			this.PrepareEndpoints(EccType, SecurityStrength, SymmetricCipherType);
			await this.ConnectClients(SecurityStrength);

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

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 5000), "Response not returned.");
			Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 5000), "Data not returned.");
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		public async Task HTTPX_Test_04_POST_PostBack(AsymmetricCipher EccType, int SecurityStrength, SymmetricCipher SymmetricCipherType)
		{
			this.PrepareEndpoints(EccType, SecurityStrength, SymmetricCipherType);
			await this.ConnectClients(SecurityStrength);

			PostBack PostBack = new();

			this.webServer.Register(PostBack);
			this.httpxClient1.PostResource = PostBack;

			await this.DoPost(4);
		}

	}
}
