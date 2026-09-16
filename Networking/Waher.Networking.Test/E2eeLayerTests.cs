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
	public sealed class E2eeLayerTests
	{
		private static readonly Random rnd = new();
		private static BinaryTcpServer? server;
		private static XmlFileSniffer? serverSniffer;
		private XmlFileSniffer? clientSniffer;
		private BinaryTcpClient? client;
		private E2eeLayer? clientProtocol;

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
				E2eeLayer Protocol = new(e.Client, false, 128, 128, 256,
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

				Protocol.OnTextReceived += async (Sender, Text) =>
				{
					char[] Chars = Text.ToCharArray();
					Array.Reverse(Chars);
					Text = new string(Chars);

					await Protocol.SendAsync(Text);

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
				await this.clientProtocol.DisposeAsync();
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

			this.clientProtocol = new E2eeLayer(this.client, true,
				128, 128, 256, AsymmetricCiphers, SymmetricCiphers, SignedTransfers,
				true, this.clientSniffer);

			await this.TestKeyNegotiation();
		}

		private async Task TestKeyNegotiation()
		{
			CancellationTokenSource Cancel = new();

			Task RemoteEndpoints(object Sender, EventArgs e)
			{
				this.clientSniffer?.Information("Remote endpoints received.");
				return Task.CompletedTask;
			}

			Task ProtocolError(object Sender, EventArgs e)
			{
				Cancel.Cancel();
				return Task.CompletedTask;
			}

			this.clientProtocol!.OnRemoteEndpoints += RemoteEndpoints;
			this.clientProtocol.OnProtocolError += ProtocolError;

			try
			{
				this.clientProtocol.Information("Negotiating keys...");

				Assert.IsTrue(await this.clientProtocol.NegotiateKeys(10000, Cancel.Token));

				this.clientProtocol.Information("Keys negotiated...");

				Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteTypeName));
				Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteAssemblyName));
				Assert.IsFalse(string.IsNullOrEmpty(this.clientProtocol.RemoteImageVersion));
			}
			finally
			{
				this.clientProtocol.OnRemoteEndpoints -= RemoteEndpoints;
				this.clientProtocol.OnProtocolError -= ProtocolError;
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
		public async Task Test_02_SendReceive(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);
			await this.TestSendReceiveBlock(256);
		}

		private async Task TestSendReceiveBlock(int Length)
		{
			TaskCompletionSource<byte[]> Packet = new();

			Task<bool> Received(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
			{
				byte[] Data = SnifferBase.CloneSection(Buffer, Offset, Count);
				Packet.TrySetResult(Data);

				return Task.FromResult(true);
			}

			Task ProtocolError(object Sender, EventArgs e)
			{
				Packet.TrySetException(new Exception("Protocol error."));
				return Task.CompletedTask;
			}

			this.clientProtocol!.OnReceived += Received;
			this.clientProtocol.OnProtocolError += ProtocolError;

			try
			{
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
			finally
			{
				this.clientProtocol.OnReceived -= Received;
				this.clientProtocol.OnProtocolError -= ProtocolError;
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
		public async Task Test_03_SendReceiveRandom(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);
			await this.TestSendReceiveBlock(rnd.Next(1, 100000));
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
		public async Task Test_04_SendReceiveText(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);
			await this.TestSendReceiveText(256);
		}

		private async Task TestSendReceiveText(int Length)
		{
			TaskCompletionSource<string> Packet = new();

			Task<bool> TextReceived(object Sender, string Text)
			{
				Packet.TrySetResult(Text);

				return Task.FromResult(true);
			}

			Task ProtocolError(object Sender, EventArgs e)
			{
				Packet.TrySetException(new Exception("Protocol error."));
				return Task.CompletedTask;
			}

			this.clientProtocol!.OnTextReceived += TextReceived;
			this.clientProtocol.OnProtocolError += ProtocolError;

			try
			{
				_ = Task.Delay(10000, CancellationToken.None).ContinueWith(_ =>
					Packet.TrySetException(new TimeoutException()));

				char[] Data = new char[Length];
				int i;

				for (i = 0; i < Length; i++)
					Data[i] = (char)('A' + (i % 25));

				string s = new(Data);

				Assert.IsTrue(await this.clientProtocol.SendAsync(s));

				s = await Packet.Task;
				Assert.HasCount(Length, Data);

				for (i = 0; i < Length; i++)
					Assert.AreEqual((char)('A' + ((Length - i - 1) % 25)), s[i]);
			}
			finally
			{
				this.clientProtocol.OnTextReceived -= TextReceived;
				this.clientProtocol.OnProtocolError -= ProtocolError;
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
		public async Task Test_05_SendReceiveTextRandom(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);
			await this.TestSendReceiveText(rnd.Next(1, 100000));
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
		public async Task Test_06_SendReceiveTextEmpty(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);
			await this.TestSendReceiveText(0);
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
		public async Task Test_07_MultipleMessages(bool SignedTransfers,
			string? AsymmetricCipher, string? SymmetricCipher)
		{
			await this.Test_01_KeyNegotiation(SignedTransfers, AsymmetricCipher, SymmetricCipher);

			for (int i = 0; i < 100; i++)
			{
				switch (rnd.Next(5))
				{
					case 0:
						await this.TestSendReceiveBlock(256);
						break;

					case 1:
						await this.TestSendReceiveBlock(rnd.Next(1, 100000));
						break;

					case 2:
						await this.TestSendReceiveText(256);
						break;

					case 3:
						await this.TestSendReceiveText(rnd.Next(1, 100000));
						break;

					case 4:
						await this.TestSendReceiveText(0);
						break;
				}
			}
		}

	}
}
