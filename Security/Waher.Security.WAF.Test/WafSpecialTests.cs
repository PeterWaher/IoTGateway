using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.WebSockets;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.Security.WAF.Test
{
	[TestClass]
	public sealed class WafSpecialTests
	{
		private const int MaxTextSize = 64 * 1024;
		private const int MaxBinarySize = 1024 * 1024;
		private HttpServer server = null;
		private X509Certificate2 certificate;
		private ConsoleOutSniffer sniffer;
		private WebSocketListener webSocketListener;

		public TestContext TestContext { get; set; }

		private Task SetupServer(string FirewallFileName)
		{
			WebApplicationFirewall Firewall = WebApplicationFirewall.LoadFromFile(
				Path.Combine("Data", "Special", FirewallFileName), WafTests.auditor, "Data");

			return this.SetupServer(Firewall);
		}

		private async Task SetupServer(WebApplicationFirewall Firewall)
		{
			this.certificate = Certificates.LoadCertificate("Waher.Security.WAF.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			this.server = new([8081], [8088], this.certificate, false,
				ClientCertificates.Optional, true, [], false)
			{
				WebApplicationFirewall = Firewall
			};

			this.server.SetHttp2ConnectionSettings(true, 65535, 65535, 16384, 100, 8192, false, false, true, false);

			Types.SetModuleParameter("HTTP", this.server);

			this.webSocketListener = new WebSocketListener("/ws", false, MaxTextSize, MaxBinarySize, "chat");
			this.server.Register(this.webSocketListener);

			this.sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Base64, LineEnding.NewLine);
			this.server.Add(this.sniffer);

			await Database.Clear("OpenIntelligence");
			await Database.Clear("WafLists");
			await Database.Clear("WafListsIgnoreCase");
		}

		private async Task CloseServer()
		{
			if (this.webSocketListener is not null)
			{
				this.server.Unregister(this.webSocketListener);
				this.webSocketListener.Dispose();
				this.webSocketListener = null;
			}

			if (this.server is not null)
			{
				await this.server?.DisposeAsync();
				this.server = null;
			}

			if (this.sniffer is not null)
			{
				await this.sniffer.FlushAsync();
				await this.sniffer.DisposeAsync();
			}
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			string TestName = this.TestContext!.TestName;
			await this.SetupServer(TestName + ".xml");
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			await this.CloseServer();
		}

		[TestMethod]
		[DataRow("/ws", false, "1.1")]
		[DataRow("/ws", true, "1.1")]
		[DataRow("/ws", false, "2.0")]
		[DataRow("/ws", true, "2.0")]
		public async Task Test_001_WebSockets(string Resource, bool Encrypted, string ProtocolVersion)
		{
			this.webSocketListener.Accept += (Sender, e) =>
			{
				if (e.Socket is null)
					Assert.Fail("Socket not set.");

				if (!e.Socket.HttpRequest.Header.TryGetHeaderField("Origin", out HttpField Origin) ||
					Origin.Value != "UnitTest")
				{
					throw new ForbiddenException();
				}

				return Task.CompletedTask;
			};

			this.webSocketListener.Connected += (Sender, e) =>
			{
				if (e.Socket is null)
					Assert.Fail("Socket not set.");

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = new();
			using ClientWebSocket Client = CreateClient(new Version(ProtocolVersion));
			Client.Options.SetRequestHeader("Origin", "UnitTest");

			Uri Uri = Encrypted
				? new Uri("wss://localhost:8088" + Resource)
				: new Uri("ws://localhost:8081" + Resource);

			Handler.SslOptions.RemoteCertificateValidationCallback = delegate (object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};

			await Client.ConnectAsync(Uri, new HttpMessageInvoker(Handler), CancellationToken.None);

			Assert.AreEqual(WebSocketState.Open, Client.State);
		}

		private static ClientWebSocket CreateClient(Version ProtocolVersion)
		{
			ClientWebSocket Client = new();

			Client.Options.HttpVersion = ProtocolVersion;
			Client.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
			
			return Client;
		}

	}
}
