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
		private UdpClient udpClient;
		private DtlsOverUdp dtlsOverUdp;
		private IPEndPoint remoteEndpoint;
		private TextWriterSniffer sniffer;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(typeof(ICipher).Assembly);
			Log.Register(new ConsoleEventSink());
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.sniffer = new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal);

			//this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("vs0.inf.ethz.ch")[0], 5684);
			//this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("californium.eclipse.org")[0], 5684);
			this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("leshan.eclipse.org")[0], 5684);
			//this.remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("lsys-home.dyndns.org")[0], 5684);

			this.udpClient = new UdpClient(5684, AddressFamily.InterNetwork);
			this.dtlsOverUdp = new DtlsOverUdp(this.udpClient, DtlsMode.Client, null, null, this.sniffer);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.dtlsOverUdp != null)
			{
				this.dtlsOverUdp.Dispose();
				this.dtlsOverUdp = null;
			}

			if (this.udpClient != null)
			{
				this.udpClient.Dispose();
				this.udpClient = null;
			}

			this.sniffer = null;
		}

		[TestMethod]
		public void Test_01_Handshake()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.dtlsOverUdp.DTLS.OnHandshakeSuccessful += (sender, e) => Done.Set();
			this.dtlsOverUdp.DTLS.OnHandshakeFailed += (sender, e) => Error.Set();

			this.dtlsOverUdp.DTLS.StartHandshake(this.remoteEndpoint,
				new PresharedKey("testid", new byte[] { 1, 2, 3, 4 }));

			// Set Pre-shared keys at: http://leshan.eclipse.org/#/security

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 60000));
		}

		[TestMethod]
		public void Test_02_Retransmissions()
		{
			this.dtlsOverUdp.DTLS.ProbabilityPacketLoss = 0.3;
			this.Test_01_Handshake();
		}

	}
}
