using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;

namespace Waher.Security.DTLS.Test
{
	[TestClass]
	public class DtlsTests
	{
		private DtlsEndpoint client;
		private DtlsEndpoint server;
		private DtlsBridge toServer;
		private DtlsBridge toClient;
		private ConsoleOutSniffer sniffer;
		private Users users;

		[TestInitialize]
		public void TestInitialize()
		{
			this.sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine);
			this.users = new Users(new User("testid", "01020304", "HEX"));

			this.toServer = new DtlsBridge(null);
			this.toClient = new DtlsBridge(null);

			this.toServer.RemoteBridge = this.toClient;
			this.toClient.RemoteBridge = this.toServer;

			this.client = new DtlsEndpoint(DtlsMode.Client, this.toServer, this.sniffer);
			this.server = new DtlsEndpoint(DtlsMode.Server, this.toClient, this.users);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.client is not null)
			{
				this.client.Dispose();
				this.client = null;
			}

			if (this.server is not null)
			{
				this.server.Dispose();
				this.server = null;
			}

			this.toServer = null;
			this.toClient = null;
			this.users = null;
			this.sniffer = null;
		}

		[TestMethod]
		public void Test_01_Handshake()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);

			this.client.OnHandshakeSuccessful += (sender, e) =>
			{
				Done.Set();
				return Task.CompletedTask;
			};
			this.client.OnHandshakeFailed += (sender, e) => 
			{ 
				Error.Set(); 
				return Task.CompletedTask;
			};

			this.client.StartHandshake(string.Empty, 
				new PresharedKey("testid", new byte[] { 1, 2, 3, 4 }));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 60000));
		}

		[TestMethod]
		public void Test_02_ApplicationData()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);

			this.server.OnApplicationDataReceived += (sender, e) =>
			{
				try
				{
					AesCcmTests.AssertEqual(new byte[] { 1, 2, 3, 4, 5 }, e.ApplicationData);
					Done.Set();
				}
				catch (Exception)
				{
					Error.Set();
				}
			
				return Task.CompletedTask;
			};

			this.Test_01_Handshake();

			this.client.SendApplicationData(new byte[] { 1, 2, 3, 4, 5 }, string.Empty);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 60000));
		}

		[TestMethod]
		public void Test_03_GracefulShutdown()
		{
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);

			this.Test_02_ApplicationData();

			this.client.OnStateChanged += (sender, e) =>
			{
				if (e.State == DtlsState.Closed)
					Done1.Set();
				else
					Error1.Set();
			
				return Task.CompletedTask;
			};

			this.server.OnStateChanged += (sender, e) =>
			{
				if (e.State == DtlsState.Closed)
					Done2.Set();
				else
					Error2.Set();
			
				return Task.CompletedTask;
			};

			this.client.Dispose();
			this.client = null;

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 60000));
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 60000));
		}

		[TestMethod]
		public void Test_04_Retransmissions()
		{
			this.client.ProbabilityPacketLoss = 0.3;
			this.Test_01_Handshake();
		}

		// TODO: Fragmentation. Max datagram size.
		// TODO: Session resumption
		// TODO: Re-handshake

	}
}
