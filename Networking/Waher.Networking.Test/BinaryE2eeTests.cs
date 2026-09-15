using System.Security.Cryptography;
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
		private static readonly Random rnd = new();
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

				Protocol.OnReceived += async (Sender, ConstantBuffer, Buffer, Offset, Count) =>
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
						Result = await Protocol.NegotiateKeys(10000, CancellationToken.None);
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
				}, CancellationToken.None);

				return Task.CompletedTask;
			};

			await server.Open();
		}

		[ClassCleanup(InheritanceBehavior.None)]
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
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(false, nameof(EllipticCurveEndpoint), null)]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(EllipticCurveEndpoint), null)]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), null)]
		[DataRow(false, nameof(RsaEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(RsaEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(RsaEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(RsaEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(RsaEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(RsaEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(RsaEndpoint), null)]
		[DataRow(false, null, nameof(Aes256))]
		[DataRow(false, null, nameof(ChaCha20))]
		[DataRow(false, null, nameof(ChaCha20Poly1305))]
		[DataRow(true, null, nameof(Aes256))]
		[DataRow(true, null, nameof(ChaCha20))]
		[DataRow(true, null, nameof(ChaCha20Poly1305))]
		[DataRow(true, null, null)]
		public async Task Test_01_KeyNegotiation(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			Type[]? AsymmetricCiphers = AsymmetricCipher is null ? null 
				: [Types.GetType(AsymmetricCipher)];
			
			Type[]? SymmetricCiphers = SymmetricCipher is null ? null 
				: [Types.GetType(SymmetricCipher)];

			this.clientProtocol = new BinaryE2eeProtocol(this.client, true,
				128, 128, 256, AsymmetricCiphers, SymmetricCiphers, SignedTransfers,
				true, this.clientSniffer);

			await this.TestKeyNegotiation();
		}

		private async Task TestKeyNegotiation()
		{
			CancellationTokenSource Cancel = new();

			this.clientProtocol!.OnRemoteEndpoints += (_, e) =>
			{
				this.clientSniffer?.Information("Remote endpoints received.");
				return Task.CompletedTask;
			};

			this.clientProtocol.OnProtocolError += (_, e) =>
			{
				Cancel.Cancel();
				return Task.CompletedTask;
			};

			this.clientProtocol.Information("Negotiating keys...");

			Assert.IsTrue(await this.clientProtocol.NegotiateKeys(10000, Cancel.Token));

			this.clientProtocol.Information("Keys negotiated...");

			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteTypeName));
			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteAssemblyName));
			Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteImageVersion));
		}

		[TestMethod]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(false, nameof(EllipticCurveEndpoint), null)]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(EllipticCurveEndpoint), null)]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), null)]
		[DataRow(false, nameof(RsaEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(RsaEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(RsaEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(RsaEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(RsaEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(RsaEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(RsaEndpoint), null)]
		[DataRow(false, null, nameof(Aes256))]
		[DataRow(false, null, nameof(ChaCha20))]
		[DataRow(false, null, nameof(ChaCha20Poly1305))]
		[DataRow(true, null, nameof(Aes256))]
		[DataRow(true, null, nameof(ChaCha20))]
		[DataRow(true, null, nameof(ChaCha20Poly1305))]
		[DataRow(true, null, null)]
		public async Task Test_02_SendReceive(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);
			await this.TestSendReceiveBlock(256);
		}

		private async Task TestSendReceiveBlock(int Length)
		{
			TaskCompletionSource<byte[]> Packet = new();

			this.clientProtocol!.OnReceived += (Sender, ConstantBuffer, Buffer, Offset, Count) =>
			{
				byte[] Data = SnifferBase.CloneSection(Buffer, Offset, Count);
				Packet.TrySetResult(Data);

				return Task.FromResult(true);
			};

			this.clientProtocol.OnProtocolError += (_, e) =>
			{
				Packet.TrySetException(new Exception("Protocol error."));
				return Task.CompletedTask;
			};

			_ = Task.Delay(10000, CancellationToken.None).ContinueWith(_ =>
				Packet.TrySetException(new TimeoutException()));

			byte[] Data = new byte[Length];
			int i;

			for (i = 0; i < Length; i++)
				Data[i] = (byte)i;

			Assert.IsTrue(await this.clientProtocol.SendAsync(true, Data));

			Data = await Packet.Task;
			Assert.HasCount(Length, Data);

			for (i = 0; i < Length; i++)
				Assert.AreEqual((byte)(Length - i - 1), Data[i]);
		}

		[TestMethod]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(EllipticCurveEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(false, nameof(EllipticCurveEndpoint), null)]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(EllipticCurveEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(EllipticCurveEndpoint), null)]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(ModuleLatticeEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(ModuleLatticeEndpoint), null)]
		[DataRow(false, nameof(RsaEndpoint), nameof(Aes256))]
		[DataRow(false, nameof(RsaEndpoint), nameof(ChaCha20))]
		[DataRow(false, nameof(RsaEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(RsaEndpoint), nameof(Aes256))]
		[DataRow(true, nameof(RsaEndpoint), nameof(ChaCha20))]
		[DataRow(true, nameof(RsaEndpoint), nameof(ChaCha20Poly1305))]
		[DataRow(true, nameof(RsaEndpoint), null)]
		[DataRow(false, null, nameof(Aes256))]
		[DataRow(false, null, nameof(ChaCha20))]
		[DataRow(false, null, nameof(ChaCha20Poly1305))]
		[DataRow(true, null, nameof(Aes256))]
		[DataRow(true, null, nameof(ChaCha20))]
		[DataRow(true, null, nameof(ChaCha20Poly1305))]
		[DataRow(true, null, null)]
		public async Task Test_03_SendReceiveRandom(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);
			await this.TestSendReceiveBlock(rnd.Next(1, 100000));
		}
	}
}
