using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.WebSockets;
using Waher.Networking.Sniffers;
using Waher.Runtime.IO;
using Waher.Runtime.Profiling;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class WebSocketTests : IUserSource
	{
		private const int MaxTextSize = 64 * 1024;
		private const int MaxBinarySize = 1024 * 1024;
		private HttpServer server;
		private ConsoleEventSink sink = null;
		private XmlFileSniffer xmlSniffer = null;

		private WebSocketListener webSocketListener;

		public TestContext TestContext { get; set; }

		[TestInitialize]
		public void TestInitialize()
		{
			string SnifferFileName = this.TestContext.TestName;
			if (string.IsNullOrEmpty(SnifferFileName))
				SnifferFileName = "WebSocket";

			SnifferFileName = "Sniffers" + Path.DirectorySeparatorChar + SnifferFileName + ".xml";

			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			File.Delete(SnifferFileName);
			this.xmlSniffer = new XmlFileSniffer(SnifferFileName,
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				int.MaxValue, BinaryPresentationMethod.ByteCount);

			X509Certificate2 Certificate = Certificates.LoadCertificate("Waher.Networking.HTTP.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			this.server = new HttpServer(8081, 8088, Certificate,
				new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine),
				this.xmlSniffer);

			this.server.SetHttp2ConnectionSettings(true, 65535, 65535, 16384, 100, 8192, false, false, true, false);

			this.server.ConnectionProfiled += async (sender, e) =>
			{
				string Uml = e.Profiler.ExportPlantUml(TimeUnit.MilliSeconds);
				await Files.WriteAllTextAsync(Path.ChangeExtension(SnifferFileName, "uml"), Uml);
			};

			this.webSocketListener = new WebSocketListener("/ws", false, MaxTextSize, MaxBinarySize, "chat");
			this.server.Register(this.webSocketListener);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.webSocketListener is not null)
			{
				this.server.Unregister(this.webSocketListener);
				this.webSocketListener.Dispose();
				this.webSocketListener = null;
			}

			if (this.server is not null)
			{
				await this.server.DisposeAsync();
				this.server = null;
			}

			if (this.xmlSniffer is not null)
			{
				await this.xmlSniffer.DisposeAsync();
				this.xmlSniffer = null;
			}

			if (this.sink is not null)
			{
				Log.Unregister(this.sink);
				await this.sink.DisposeAsync();
				this.sink = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (UserName == "User")
				return Task.FromResult<IUser>(new User());
			else
				return Task.FromResult<IUser>(null);
		}

		private ClientWebSocket CreateClient(string ProtocolVersion)
		{
			ClientWebSocket Client = new();

			Client.Options.HttpVersion = new Version(ProtocolVersion);
			Client.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
			
			return Client;
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_01_Connect_Reject(string ProtocolVersion, bool Encryption)
		{
			this.webSocketListener.Accept += (Sender, e) =>
			{
				if (!e.Socket.HttpRequest.Header.TryGetHeaderField("Origin", out HttpField Origin) ||
					Origin.Value != "UnitTest")
				{
					throw new ForbiddenException();
				}

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);

			await Assert.ThrowsAsync<WebSocketException>(async () =>
				await Client.ConnectAsync(GetUri(Encryption),
					new HttpMessageInvoker(Handler), CancellationToken.None));
		}

		private static SocketsHttpHandler PrepareHandler()
		{
			SocketsHttpHandler Handler = new();

			Handler.SslOptions.RemoteCertificateValidationCallback = delegate (object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};

			return Handler;
		}

		private static Uri GetUri(bool Encryption)
		{
			if (Encryption)
				return new Uri("wss://localhost:8088/ws");
			else
				return new Uri("ws://localhost:8081/ws");
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_02_Connect_Accept(string ProtocolVersion, bool Encryption)
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

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			Client.Options.SetRequestHeader("Origin", "UnitTest");

			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			Assert.AreEqual(WebSocketState.Open, Client.State);
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_03_ReceiveText(string ProtocolVersion, bool Encryption)
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					if (e2.Payload == "Hello World")
						Result.SetResult(true);
					else
						Result.SetResult(false);

					return Task.CompletedTask;
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello World")),
				WebSocketMessageType.Text, true, CancellationToken.None);

			if (!Result.Task.Wait(5000, CancellationToken.None))
				Assert.Fail("No text delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong text delivered.");
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_04_ReceiveBinary(string ProtocolVersion, bool Encryption)
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.BinaryReceived += (sender2, e2) =>
				{
					if (e2.Payload.Length == 4 &&
						e2.Payload.ReadByte() == 1 &&
						e2.Payload.ReadByte() == 2 &&
						e2.Payload.ReadByte() == 3 &&
						e2.Payload.ReadByte() == 4)
					{
						Result.SetResult(true);
					}
					else
						Result.SetResult(false);

					return Task.CompletedTask;
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>([1, 2, 3, 4]),
				WebSocketMessageType.Binary, true, CancellationToken.None);

			if (!Result.Task.Wait(5000, CancellationToken.None))
				Assert.Fail("No binary data delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong binary data delivered.");
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_05_ReceiveTextFragmented(string ProtocolVersion, bool Encryption)
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					if (e2.Payload == "Hello World")
						Result.SetResult(true);
					else
						Result.SetResult(false);

					return Task.CompletedTask;
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello ")),
				WebSocketMessageType.Text, false, CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("World")),
				WebSocketMessageType.Text, true, CancellationToken.None);

			if (!Result.Task.Wait(5000, CancellationToken.None))
				Assert.Fail("No text delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong text delivered.");
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_06_ReceiveFragmented(string ProtocolVersion, bool Encryption)
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.BinaryReceived += (sender2, e2) =>
				{
					if (e2.Payload.Length == 4 &&
						e2.Payload.ReadByte() == 1 &&
						e2.Payload.ReadByte() == 2 &&
						e2.Payload.ReadByte() == 3 &&
						e2.Payload.ReadByte() == 4)
					{
						Result.SetResult(true);
					}
					else
						Result.SetResult(false);

					return Task.CompletedTask;
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>([1, 2]),
				WebSocketMessageType.Binary, false, CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>([3, 4]),
				WebSocketMessageType.Binary, true, CancellationToken.None);

			if (!Result.Task.Wait(5000, CancellationToken.None))
				Assert.Fail("No binary data delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong binary data delivered.");
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_07_ReceiveLargeText(string ProtocolVersion, bool Encryption)
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.BinaryReceived += (sender2, e2) =>
				{
					if (e2.Payload.Length == 100000)
						Result.SetResult(true);
					else
						Result.SetResult(false);

					return Task.CompletedTask;
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(new string('A', 100000))),
				WebSocketMessageType.Text, true, CancellationToken.None);

			if (!Result.Task.Wait(5000, CancellationToken.None))
				Assert.Fail("No text delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong text delivered.");
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_08_ReceiveLargeBinary(string ProtocolVersion, bool Encryption)
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.BinaryReceived += (sender2, e2) =>
				{
					Result.SetResult(false);
					return Task.CompletedTask;
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			Task _ = Task.Run(async () =>
			{
				await Client.SendAsync(new ArraySegment<byte>(new byte[MaxBinarySize * 2]),
					WebSocketMessageType.Binary, true, CancellationToken.None);

			}, CancellationToken.None);

			Task _2 = Task.Run(async () =>
			{
				try
				{
					WebSocketReceiveResult ReadResult = await Client.ReceiveAsync(
						new ArraySegment<byte>(new byte[MaxBinarySize * 2]), CancellationToken.None);

					if (ReadResult.CloseStatus.HasValue &&
						ReadResult.CloseStatus.Value == System.Net.WebSockets.WebSocketCloseStatus.MessageTooBig)
					{
						Result.TrySetException(new WebSocketException());
					}
				}
				catch (Exception ex)
				{
					Result.TrySetException(ex);
				}

			}, CancellationToken.None);

			Task _3 = Task.Run(async () =>
			{
				await Task.Delay(5000, CancellationToken.None);

				if (Client.State == WebSocketState.Aborted)
					Result.TrySetException(new WebSocketException());
				else
					Result.TrySetException(new TimeoutException());

			}, CancellationToken.None);

			await Assert.ThrowsAsync<WebSocketException>(async () =>
				await Result.Task);
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_09_SendText(string ProtocolVersion, bool Encryption)
		{
			this.webSocketListener.Connected += (Sender, e) =>
			{
				return e.Socket.Send("Hello World");
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
			Assert.IsTrue(Result.EndOfMessage);

			string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

			Assert.AreEqual("Hello World", s);
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_10_SendBinary(string ProtocolVersion, bool Encryption)
		{
			this.webSocketListener.Connected += (Sender, e) =>
			{
				return e.Socket.Send([1, 2, 3, 4]);
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Binary, Result.MessageType);
			Assert.IsTrue(Result.EndOfMessage);

			byte[] Bin = Buffer.Array;
			int i;

			Assert.AreEqual(4, Result.Count);

			for (i = 0; i < 4; i++)
				Assert.AreEqual(i + 1, Bin[i]);
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_11_SendTextFragmented(string ProtocolVersion, bool Encryption)
		{
			this.webSocketListener.Connected += async (Sender, e) =>
			{
				await e.Socket.Send("Hello ", true);
				await e.Socket.Send("World", false);
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
			Assert.IsFalse(Result.EndOfMessage);

			string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

			Assert.AreEqual("Hello ", s);

			Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
			Assert.IsTrue(Result.EndOfMessage);

			s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

			Assert.AreEqual("World", s);
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_12_SendBinaryFragmented(string ProtocolVersion, bool Encryption)
		{
			this.webSocketListener.Connected += async (Sender, e) =>
			{
				await e.Socket.Send([1, 2], true);
				await e.Socket.Send([3, 4], false);
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Binary, Result.MessageType);
			Assert.IsFalse(Result.EndOfMessage);

			byte[] Bin = Buffer.Array;

			Assert.AreEqual(2, Result.Count);
			Assert.AreEqual(1, Bin[0]);
			Assert.AreEqual(2, Bin[1]);

			Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Binary, Result.MessageType);
			Assert.IsTrue(Result.EndOfMessage);

			Bin = Buffer.Array;

			Assert.AreEqual(2, Result.Count);
			Assert.AreEqual(3, Bin[0]);
			Assert.AreEqual(4, Bin[1]);
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_13_Work(string ProtocolVersion, bool Encryption)
		{
			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					return e2.Socket.Send(e2.Payload);
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			int i;

			for (i = 0; i < 10000; i++)
			{
				await Client.SendAsync(new(Encoding.UTF8.GetBytes(i.ToString())),
					WebSocketMessageType.Text, true, CancellationToken.None);

				ArraySegment<byte> Buffer = new(new byte[16]);
				WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

				Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
				Assert.IsTrue(Result.EndOfMessage);

				string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

				Assert.AreEqual(i, int.Parse(s));
			}
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_14_Pong(string ProtocolVersion, bool Encryption)
		{
			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					return e2.Socket.Send(e2.Payload);
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);

			int i;

			for (i = 0; i < 3; i++)
			{
				await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(i.ToString())),
					WebSocketMessageType.Text, true, CancellationToken.None);

				ArraySegment<byte> Buffer = new(new byte[16]);
				WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

				Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
				Assert.IsTrue(Result.EndOfMessage);

				string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

				Assert.AreEqual(i, int.Parse(s));

				Thread.Sleep(60000);    // ClientWebSocket sends unsolicited pong messages to keep the connection alive.
			}
		}

		[TestMethod]
		[DataRow("1.1", false)]
		[DataRow("2.0", false)]
		[DataRow("1.1", true)]
		[DataRow("2.0", true)]
		public async Task Test_15_Close(string ProtocolVersion, bool Encryption)
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (Sender, e) =>
			{
				e.Socket.Closed += (sender2, e2) =>
				{
					Result.SetResult(e2.Code == (int)WebSockets.WebSocketCloseStatus.Normal &&
						e2.Reason == "Manual");

					return Task.CompletedTask;
				};

				return Task.CompletedTask;
			};

			using SocketsHttpHandler Handler = PrepareHandler();
			using ClientWebSocket Client = this.CreateClient(ProtocolVersion);
			await Client.ConnectAsync(GetUri(Encryption),
				new HttpMessageInvoker(Handler), CancellationToken.None);
			await Client.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Manual", CancellationToken.None);

			if (!Result.Task.Wait(5000, CancellationToken.None))
				Assert.Fail("Close event not received.");

			if (!Result.Task.Result)
				Assert.Fail("Close data not delivered correctly.");
		}

	}
}
