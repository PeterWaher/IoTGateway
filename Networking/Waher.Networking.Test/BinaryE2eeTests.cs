using Waher.Events;
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
		private static BinaryTcpServer? server;
		private static XmlFileSniffer? serverSniffer;
		private XmlFileSniffer? clientSniffer;
		private BinaryTcpClient? client;
		private BinaryE2eeProtocol? clientProtocol;

		public TestContext TestContext { get; set; }

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(IE2eEndpoint).Assembly,
				typeof(E2eEndpoint).Assembly,
				typeof(EllipticCurveEndpoint).Assembly,
				typeof(ModuleLatticeEndpoint).Assembly,
				typeof(ChaCha20).Assembly);
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			if (Directory.Exists("Sniffers\\E2EE"))
				Directory.Delete("Sniffers\\E2EE", true);

			server = new BinaryTcpServer(true, 8081, TimeSpan.FromSeconds(10), false);

			server.OnClientConnected += (_, e) =>
			{
				BinaryE2eeProtocol Protocol = new(e.Client, false, 128, 128, 256,
					[
						typeof(EllipticCurveEndpoint), 
						typeof(ModuleLatticeEndpoint),
						typeof(RsaEndpoint)
					], false, true);

				if (serverSniffer is not null)
					Protocol.Add(serverSniffer);

				Protocol.OnReceived += async (object Sender, bool ConstantBuffer, 
					byte[] Buffer, int Offset, int Count) =>
				{
					byte[] Data = SnifferBase.CloneSection(Buffer, Offset, Count);
					Array.Reverse(Data);

					await Protocol.SendAsync(true, Data);

					return true;
				};

				Task.Run(async () =>
				{
					bool Result;

					try
					{
						Result = await Protocol.NegotiateKeys(10000);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						Result = false;
					}

					if (!Result)
					{
						try
						{
							await e.Client.DisposeAsync();
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				});

				return Task.CompletedTask;
			};

			await server.Open();
		}

		[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
		public static void ClassCleanup()
		{
			if (server is not null)
			{
				server.Dispose();
				server = null;
			}
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			this.clientSniffer = new XmlFileSniffer(
				"Sniffers\\E2EE\\" + this.TestContext.TestName + "\\Client.xml",
				"..\\..\\..\\..\\..\\Waher.IoTGateway.Resources\\Transforms\\SnifferXmlToHtml.xslt",
				7, BinaryPresentationMethod.Hexadecimal);

			serverSniffer = new XmlFileSniffer(
				"Sniffers\\E2EE\\" + this.TestContext.TestName + "\\Server.xml",
				"..\\..\\..\\..\\..\\Waher.IoTGateway.Resources\\Transforms\\SnifferXmlToHtml.xslt",
				7, BinaryPresentationMethod.Hexadecimal);

			this.client = new BinaryTcpClient(true);
			this.clientProtocol = null;

			this.clientSniffer.DisableMask();
			serverSniffer.DisableMask();

			this.clientSniffer.Information("Connecting to server...");
			Assert.IsTrue(await this.client.ConnectAsync("localhost", 8081, true));
			this.clientSniffer.Information("Connection to server established...");
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

			if (this.clientSniffer is not null)
			{
				await this.clientSniffer.FlushAsync();
				await this.clientSniffer.DisposeAsync();
				this.clientSniffer = null;
			}

			if (serverSniffer is not null)
			{
				await serverSniffer.FlushAsync();
				await serverSniffer.DisposeAsync();
				serverSniffer = null;
			}
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_01_KeyNegotiation_EllipticCurves(bool SignedTransfers)
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, [typeof(EllipticCurveEndpoint)], SignedTransfers,
				true, this.clientSniffer);

			await this.TestKeyNegotiation();
		}

		private async Task TestKeyNegotiation()
		{
			this.clientProtocol!.OnRemoteEndpoints += (_, e) =>
			{
				this.clientSniffer?.Information("Remote endpoints received.");
				return Task.CompletedTask;
			};

			this.clientProtocol.Information("Negotiating keys...");

			Assert.IsTrue(await this.clientProtocol.NegotiateKeys(10000));

			this.clientProtocol.Information("Keys negotiated...");

			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteTypeName));
			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteAssemblyName));
			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteImageVersion));
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_02_KeyNegotiation_ModuleLattice(bool SignedTransfers)
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, [typeof(ModuleLatticeEndpoint)], SignedTransfers,
				true, this.clientSniffer);

			await this.TestKeyNegotiation();
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_03_KeyNegotiation_RSA(bool SignedTransfers)
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, [typeof(RsaEndpoint)], SignedTransfers, true, this.clientSniffer);

			await this.TestKeyNegotiation();
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_04_KeyNegotiation_Any(bool SignedTransfers)
		{
			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, SignedTransfers, true, this.clientSniffer);

			await this.TestKeyNegotiation();
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_05_SendReceive_EllipticCurves(bool SignedTransfers)
		{
			await this.Test_01_KeyNegotiation_EllipticCurves(SignedTransfers);
			await this.TestSendReceive();
		}

		private async Task TestSendReceive()
		{
			TaskCompletionSource<byte[]> Packet = new TaskCompletionSource<byte[]>();

			this.clientProtocol!.OnReceived += (object Sender, bool ConstantBuffer,
				byte[] Buffer, int Offset, int Count) =>
			{
				byte[] Data = SnifferBase.CloneSection(Buffer, Offset, Count);
				Packet.TrySetResult(Data);

				return Task.FromResult(true);
			};

			_ = Task.Delay(10000).ContinueWith(_ => 
				Packet.TrySetException(new TimeoutException()));

			byte[] Data = new byte[256];
			int i;

			for (i = 0; i < 256; i++)
				Data[i] = (byte)i;

			Assert.IsTrue(await this.clientProtocol.SendAsync(true, Data));

			Data = await Packet.Task;
			Assert.AreEqual(256, Data.Length);

			for (i = 0; i < 256; i++)
				Assert.AreEqual((byte)(255 - i), Data[i]);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_06_SendReceive_ModuleLattice(bool SignedTransfers)
		{
			await this.Test_02_KeyNegotiation_ModuleLattice(SignedTransfers);
			await this.TestSendReceive();
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_07_SendReceive_RSA(bool SignedTransfers)
		{
			await this.Test_03_KeyNegotiation_RSA(SignedTransfers);
			await this.TestSendReceive();
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task Test_08_SendReceive_Any(bool SignedTransfers)
		{
			await this.Test_04_KeyNegotiation_Any(SignedTransfers);
			await this.TestSendReceive();
		}
	}
}
