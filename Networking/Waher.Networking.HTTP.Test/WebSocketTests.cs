﻿using System;
using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.WebSockets;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class WebSocketTests : IUserSource
	{
		private const int MaxTextSize = 64 * 1024;
		private const int MaxBinarySize = 1024 * 1024;
		private static HttpServer server;
		private static ConsoleEventSink sink = null;

		private WebSocketListener webSocketListener;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			sink = new ConsoleEventSink();
			Log.Register(sink);

			X509Certificate2 Certificate = Resources.LoadCertificate("Waher.Networking.HTTP.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			server = new HttpServer(8081, 8088, Certificate, new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine))
			{
				new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine)
			};

			ServicePointManager.ServerCertificateValidationCallback = delegate (Object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			server?.Dispose();
			server = null;

			if (sink is not null)
			{
				Log.Unregister(sink);
				sink.Dispose();
				sink = null;
			}
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.webSocketListener = new WebSocketListener("/ws", false, MaxTextSize, MaxBinarySize, "chat");
			server.Register(this.webSocketListener);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.webSocketListener is not null)
			{
				server.Unregister(this.webSocketListener);
				this.webSocketListener.Dispose();
				this.webSocketListener = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (UserName == "User")
				return Task.FromResult<IUser>(new User());
			else
				return Task.FromResult<IUser>(null);
		}

		[TestMethod]
		[ExpectedException(typeof(WebSocketException))]
		public async Task Test_01_Connect_Reject()
		{
			this.webSocketListener.Accept += (sender, e) =>
			{
				if (!e.Socket.HttpRequest.Header.TryGetHeaderField("Origin", out HttpField Origin) ||
					Origin.Value != "UnitTest")
				{
					throw new ForbiddenException();
				}
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);
		}

		[TestMethod]
		public async Task Test_02_Connect_Accept()
		{
			this.webSocketListener.Accept += (sender, e) =>
			{
				if (e.Socket is null)
					Assert.Fail("Socket not set.");

				if (!e.Socket.HttpRequest.Header.TryGetHeaderField("Origin", out HttpField Origin) ||
					Origin.Value != "UnitTest")
				{
					throw new ForbiddenException();
				}
			};

			this.webSocketListener.Connected += (sender, e) =>
			{
				if (e.Socket is null)
					Assert.Fail("Socket not set.");
			};

			using ClientWebSocket Client = new();
			Client.Options.SetRequestHeader("Origin", "UnitTest");
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			Assert.AreEqual(WebSocketState.Open, Client.State);
		}

		[TestMethod]
		public async Task Test_03_ReceiveText()
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					if (e2.Payload == "Hello World")
						Result.SetResult(true);
					else
						Result.SetResult(false);
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello World")),
				WebSocketMessageType.Text, true, CancellationToken.None);

			if (!Result.Task.Wait(5000))
				Assert.Fail("No text delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong text delivered.");
		}

		[TestMethod]
		public async Task Test_04_ReceiveBinary()
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (sender, e) =>
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
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(new byte[] { 1, 2, 3, 4 }),
				WebSocketMessageType.Binary, true, CancellationToken.None);

			if (!Result.Task.Wait(5000))
				Assert.Fail("No binary data delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong binary data delivered.");
		}

		[TestMethod]
		public async Task Test_05_ReceiveTextFragmented()
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					if (e2.Payload == "Hello World")
						Result.SetResult(true);
					else
						Result.SetResult(false);
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello ")),
				WebSocketMessageType.Text, false, CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("World")),
				WebSocketMessageType.Text, true, CancellationToken.None);

			if (!Result.Task.Wait(5000))
				Assert.Fail("No text delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong text delivered.");
		}

		[TestMethod]
		public async Task Test_06_ReceiveFragmented()
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (sender, e) =>
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
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(new byte[] { 1, 2 }),
				WebSocketMessageType.Binary, false, CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(new byte[] { 3, 4 }),
				WebSocketMessageType.Binary, true, CancellationToken.None);

			if (!Result.Task.Wait(5000))
				Assert.Fail("No binary data delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong binary data delivered.");
		}

		[TestMethod]
		public async Task Test_07_ReceiveLargeText()
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.BinaryReceived += (sender2, e2) =>
				{
					if (e2.Payload.Length == 100000)
						Result.SetResult(true);
					else
						Result.SetResult(false);
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(new string('A', 100000))),
				WebSocketMessageType.Text, true, CancellationToken.None);

			if (!Result.Task.Wait(5000))
				Assert.Fail("No text delivered.");

			if (!Result.Task.Result)
				Assert.Fail("Wrong text delivered.");
		}

		[TestMethod]
		[ExpectedException(typeof(WebSocketException))]
		public async Task Test_08_ReceiveLargeBinary()
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.BinaryReceived += (sender2, e2) =>
				{
					Result.SetResult(false);
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			await Client.SendAsync(new ArraySegment<byte>(new byte[MaxBinarySize * 2]),
				WebSocketMessageType.Binary, true, CancellationToken.None);

			if (Client.State == WebSocketState.Aborted)
				throw new WebSocketException();	// Not necessary in previous versions of .NET. This Exception has been swallowed by internal processing in more recent versions.

			Task _ = Task.Delay(5000).ContinueWith((_) => Result.TrySetException(new TimeoutException()));

			await Result.Task;
			Assert.Fail("Binary data received, contrary to expectation.");
		}

		[TestMethod]
		public async Task Test_09_SendText()
		{
			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.Send("Hello World");
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
			Assert.AreEqual(true, Result.EndOfMessage);

			string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

			Assert.AreEqual("Hello World", s);
		}

		[TestMethod]
		public async Task Test_10_SendBinary()
		{
			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.Send(new byte[] { 1, 2, 3, 4 });
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Binary, Result.MessageType);
			Assert.AreEqual(true, Result.EndOfMessage);

			byte[] Bin = Buffer.Array;
			int i;

			Assert.AreEqual(4, Result.Count);

			for (i = 0; i < 4; i++)
				Assert.AreEqual(i + 1, Bin[i]);
		}

		[TestMethod]
		public async Task Test_11_SendTextFragmented()
		{
			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.Send("Hello ", true);
				e.Socket.Send("World", false);
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
			Assert.AreEqual(false, Result.EndOfMessage);

			string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

			Assert.AreEqual("Hello ", s);

			Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
			Assert.AreEqual(true, Result.EndOfMessage);

			s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

			Assert.AreEqual("World", s);
		}

		[TestMethod]
		public async Task Test_12_SendBinaryFragmented()
		{
			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.Send(new byte[] { 1, 2 }, true);
				e.Socket.Send(new byte[] { 3, 4 }, false);
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			ArraySegment<byte> Buffer = new(new byte[1024]);
			WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Binary, Result.MessageType);
			Assert.AreEqual(false, Result.EndOfMessage);

			byte[] Bin = Buffer.Array;

			Assert.AreEqual(2, Result.Count);
			Assert.AreEqual(1, Bin[0]);
			Assert.AreEqual(2, Bin[1]);

			Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

			Assert.AreEqual(WebSocketMessageType.Binary, Result.MessageType);
			Assert.AreEqual(true, Result.EndOfMessage);

			Bin = Buffer.Array;

			Assert.AreEqual(2, Result.Count);
			Assert.AreEqual(3, Bin[0]);
			Assert.AreEqual(4, Bin[1]);
		}

		[TestMethod]
		public async Task Test_13_Work()
		{
			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					e2.Socket.Send(e2.Payload);
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			int i;

			for (i = 0; i < 10000; i++)
			{
				await Client.SendAsync(new(Encoding.UTF8.GetBytes(i.ToString())),
					WebSocketMessageType.Text, true, CancellationToken.None);

				ArraySegment<byte> Buffer = new(new byte[16]);
				WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

				Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
				Assert.AreEqual(true, Result.EndOfMessage);

				string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

				Assert.AreEqual(i, int.Parse(s));
			}
		}

		[TestMethod]
		public async Task Test_14_Pong()
		{
			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.TextReceived += (sender2, e2) =>
				{
					e2.Socket.Send(e2.Payload);
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);

			int i;

			for (i = 0; i < 3; i++)
			{
				await Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(i.ToString())),
					WebSocketMessageType.Text, true, CancellationToken.None);

				ArraySegment<byte> Buffer = new(new byte[16]);
				WebSocketReceiveResult Result = await Client.ReceiveAsync(Buffer, CancellationToken.None);

				Assert.AreEqual(WebSocketMessageType.Text, Result.MessageType);
				Assert.AreEqual(true, Result.EndOfMessage);

				string s = Encoding.UTF8.GetString(Buffer.Array, 0, Result.Count);

				Assert.AreEqual(i, int.Parse(s));

				Thread.Sleep(60000);    // ClientWebSocket sends unsolicited pong messages to keep the connection alive.
			}
		}

		[TestMethod]
		public async Task Test_15_Close()
		{
			TaskCompletionSource<bool> Result = new();

			this.webSocketListener.Connected += (sender, e) =>
			{
				e.Socket.Closed += (sender2, e2) =>
				{
					Result.SetResult(e2.Code == (int)WebSockets.WebSocketCloseStatus.Normal &&
						e2.Reason == "Manual");
				};
			};

			using ClientWebSocket Client = new();
			await Client.ConnectAsync(new Uri("ws://localhost:8081/ws"), CancellationToken.None);
			await Client.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Manual", CancellationToken.None);

			if (!Result.Task.Wait(5000))
				Assert.Fail("Close event not received.");

			if (!Result.Task.Result)
				Assert.Fail("Close data not delivered correctly.");
		}

	}
}
