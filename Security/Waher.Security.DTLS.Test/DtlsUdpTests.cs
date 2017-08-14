using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;

namespace Waher.Security.DTLS.Test
{
	[TestClass]
	public class DtlsUdpTests
	{
		private Udp udp;
		private DtlsEndpoint dtls;
		private IPEndPoint remoteEndpoint;
		private ConsoleOutSniffer sniffer;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(typeof(ICipher).Assembly);
			Log.Register(new ConsoleEventSink());
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal);

			//this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("vs0.inf.ethz.ch")[0], 5684);
			//this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("californium.eclipse.org")[0], 5684);
			this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("leshan.eclipse.org")[0], 5684);
			//this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("lsys-home.dyndns.org")[0], 5684);

			this.udp = new Udp(this.remoteEndpoint.Address.ToString(), this.remoteEndpoint.Port);
			this.dtls = new DtlsEndpoint(DtlsMode.Client, this.udp, this.sniffer);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.dtls != null)
			{
				this.dtls.Dispose();
				this.dtls = null;
			}

			if (this.udp != null)
			{
				this.udp.Dispose();
				this.udp = null;
			}

			this.sniffer = null;
		}

		[TestMethod]
		public void Test_01_Handshake()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.dtls.OnHandshakeSuccessful += (sender, e) => Done.Set();
			this.dtls.OnHandshakeFailed += (sender, e) => Error.Set();

			this.dtls.StartHandshake(this.remoteEndpoint, "testid", new byte[] { 1, 2, 3, 4 });
			// this.dtls.StartHandshake(this.remoteEndpoint, "testidigen", new byte[] { 0x12, 0x34, 0x56, 0x78 });
			// this.dtls.StartHandshake(this.remoteEndpoint, "Test client", new byte[] { 1, 2, 3, 4 });

			// Set Pre-shared keys at: http://leshan.eclipse.org/#/security

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 60000));
		}

		[TestMethod]
		public void Test_02_Retransmissions()
		{
			this.dtls.ProbabilityPacketLoss = 0.3;
			this.Test_01_Handshake();
		}

	}
}
