using Waher.Networking.E2ee;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.ChaChaPoly.E2EE;
using Waher.Security.E2EE;
using Waher.Security.EllipticCurves.E2EE;
using Waher.Security.PQC.E2EE;

namespace Waher.Networking.Test
{
	[TestClass]
	[DoNotParallelize]
	public sealed class BinaryE2eeTests
	{
		private ConsoleOutSniffer? sniffer;
		private BinaryTcpServer? server;
		private BinaryTcpClient? client;
		private BinaryE2eeProtocol? clientProtocol;
		private BinaryE2eeProtocol? serverProtocol;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			Types.Initialize(
				typeof(IE2eEndpoint).Assembly,
				typeof(E2eEndpoint).Assembly,
				typeof(EllipticCurveEndpoint).Assembly,
				typeof(ModuleLatticeEndpoint).Assembly,
				typeof(ChaCha20).Assembly);
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			this.sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine);
			this.server = new BinaryTcpServer(true, 8081, TimeSpan.FromSeconds(10), false);
			this.client = new BinaryTcpClient(true);
			this.clientProtocol = null;
			this.serverProtocol = null;

			this.server.OnClientConnected += (_, e) =>
			{
				this.serverProtocol = new BinaryE2eeProtocol(e.Client, false,
					128, 128, 256, false, true);

				return this.serverProtocol.NegotiateKeys(10000);
			};

			await this.server.Open();
			Assert.IsTrue(await this.client.ConnectAsync("localhost", 8081));
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.clientProtocol is not null)
			{
				await this.clientProtocol.FlushAsync();
				this.clientProtocol.Dispose();
				this.clientProtocol = null;
			}

			if (this.client is not null)
			{
				await this.client.FlushAsync();
				await this.client.DisposeAsync();
				this.client = null;
			}

			if (this.sniffer is not null)
			{
				await this.sniffer.FlushAsync();
				await this.sniffer.DisposeAsync();
				this.sniffer = null;
			}

			if (this.serverProtocol is not null)
			{
				await this.serverProtocol.FlushAsync();
				this.serverProtocol.Dispose();
				this.serverProtocol = null;
			}

			if (this.server is not null)
			{
				this.server.Dispose();
				this.server = null;
			}
		}

		[TestMethod]
		public async Task Test_01_EllipticCurves()
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, [typeof(EllipticCurveEndpoint)], false, true, this.sniffer);

			await this.TestKeyNegotiation();
		}

		private async Task TestKeyNegotiation()
		{
			this.clientProtocol!.OnRemoteEndpoints += (_, e) =>
			{
				this.sniffer?.Information("Remote endpoints received.");
				return Task.CompletedTask;
			};

			Assert.IsTrue(await this.clientProtocol.NegotiateKeys(10000));

			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteTypeName));
			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteAssemblyName));
			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteImageVersion));
		}

		[TestMethod]
		public async Task Test_02_ModuleLattice()
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, [typeof(ModuleLatticeEndpoint)], false, true, this.sniffer);

			await this.TestKeyNegotiation();
		}

		[TestMethod]
		public async Task Test_03_RSA()
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, [typeof(RsaEndpoint)], false, true, this.sniffer);

			await this.TestKeyNegotiation();
		}

		[TestMethod]
		public async Task Test_04_Any()
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, false, true, this.sniffer);

			await this.TestKeyNegotiation();
		}

		// TODO: Signed transfers
	}
}
