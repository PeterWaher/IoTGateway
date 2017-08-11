using System;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;

namespace Waher.Security.DTLS.Test
{
	[TestClass]
	public class DtlsTests
	{
		private DtlsEndpoint client;
		private DtlsEndpoint server;
		private DtlsBridge toServer;
		private DtlsBridge toClient;
		private ConsoleOutSniffer clientSniffer;
		private Users users;

		[TestInitialize]
		public void TestInitialize()
		{
			this.clientSniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal);
			this.users = new Users(new User("testid", "01020304", "HEX"));

			this.toServer = new DtlsBridge(null);
			this.toClient = new DtlsBridge(null);

			this.toServer.RemoteBridge = toClient;
			this.toClient.RemoteBridge = toServer;

			this.client = new DtlsEndpoint(toServer, this.clientSniffer);
			this.server = new DtlsEndpoint(toClient, this.users);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}

			if (this.server != null)
			{
				this.server.Dispose();
				this.server = null;
			}

			this.toServer = null;
			this.toClient = null;
			this.users = null;
			this.clientSniffer = null;
		}

		[TestMethod]
		public void Test_01_Handshake()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.client.OnHandshakeSuccessful += (sender, e) => Done.Set();
			this.client.OnHandshakeFailed += (sender, e) => Error.Set();

			this.client.StartHandshake(string.Empty, "testid", new byte[] { 1, 2, 3, 4 });

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}

		// TODO: Fragmentation. Max datagram size.
		// TODO: Test retransmissions, including lost Finished messages.
		// TODO: Session resumption
		// TODO: Re-handshake
		// TODO: application data

	}
}
