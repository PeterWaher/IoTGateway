using System;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.Inventory;

namespace Waher.Security.DTLS.Test
{
	[TestClass]
	public class DtlsTests
	{
		private Udp udp;
		private DtlsEndpoint dtls;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(typeof(ICipher).Assembly);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.udp = new Udp();

			this.dtls = new DtlsEndpoint(this.udp, "testid", new byte[] { 1, 2, 3, 4 });
			// Set Pre-shared keys at: http://leshan.eclipse.org/#/security
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
		}

		[TestMethod]
		public void Test_01_Handshake()
		{
			this.dtls.StartHandshake();
			System.Threading.Thread.Sleep(1000);
		}

		private class Udp : ICommunicationLayer, IDisposable
		{
			private UdpClient client;

			public Udp()
			{
				this.client = new UdpClient(5684, AddressFamily.InterNetwork);
				//this.client.Connect("vs0.inf.ethz.ch", 5684);
				//this.client.Connect("californium.eclipse.org", 5684);
				this.client.Connect("leshan.eclipse.org", 5684);

				this.BeginRead();
			}

			private async void BeginRead()
			{
				try
				{
					while (this.client != null)
					{
						UdpReceiveResult Data = await this.client.ReceiveAsync();
						this.PacketReceived?.Invoke(Data.Buffer);
					}
				}
				catch (Exception)
				{
				}
			}

			public event DataReceivedEventHandler PacketReceived;

			public void Dispose()
			{
				if (this.client != null)
				{
					this.client.Dispose();
					this.client = null;
				}
			}

			public void SendPacket(byte[] Packet)
			{
				this.client.SendAsync(Packet, Packet.Length);
			}
		}
	}
}
