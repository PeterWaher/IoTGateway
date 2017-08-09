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

			//this.dtls = new DtlsEndpoint(this.udp, "testid", new byte[] { 1, 2, 3, 4 });
			this.dtls = new DtlsEndpoint(this.udp, "testidigen", new byte[] { 0x12, 0x34, 0x56, 0x78});
			//this.dtls = new DtlsEndpoint(this.udp, "Test client", new byte[] { 1, 2, 3, 4 });
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
		public void Test_01_PseudoRandomFunction()
		{
			ICipher Cipher = new Waher.Security.DTLS.Ciphers.TLS_PSK_WITH_AES_128_CCM_8();

			byte[] Secret = new byte[]
			{
				0x9b, 0xbe, 0x43, 0x6b, 0xa9, 0x40, 0xf0, 0x17,
				0xb1, 0x76, 0x52, 0x84, 0x9a, 0x71, 0xdb, 0x35
			};

			byte[] Seed = new byte[]
			{
				0xa0, 0xba, 0x9f, 0x93, 0x6c, 0xda, 0x31, 0x18,
				0x27, 0xa6, 0xf7, 0x96, 0xff, 0xd5, 0x19, 0x8c
			};

			byte[] ExpectedOutput = new byte[]
			{
				0xe3, 0xf2, 0x29, 0xba, 0x72, 0x7b, 0xe1, 0x7b,
				0x8d, 0x12, 0x26, 0x20, 0x55, 0x7c, 0xd4, 0x53,
				0xc2, 0xaa, 0xb2, 0x1d, 0x07, 0xc3, 0xd4, 0x95,
				0x32, 0x9b, 0x52, 0xd4, 0xe6, 0x1e, 0xdb, 0x5a,
				0x6b, 0x30, 0x17, 0x91, 0xe9, 0x0d, 0x35, 0xc9,
				0xc9, 0xa4, 0x6b, 0x4e, 0x14, 0xba, 0xf9, 0xaf,
				0x0f, 0xa0, 0x22, 0xf7, 0x07, 0x7d, 0xef, 0x17,
				0xab, 0xfd, 0x37, 0x97, 0xc0, 0x56, 0x4b, 0xab,
				0x4f, 0xbc, 0x91, 0x66, 0x6e, 0x9d, 0xef, 0x9b,
				0x97, 0xfc, 0xe3, 0x4f, 0x79, 0x67, 0x89, 0xba,
				0xa4, 0x80, 0x82, 0xd1, 0x22, 0xee, 0x42, 0xc5,
				0xa7, 0x2e, 0x5a, 0x51, 0x10, 0xff, 0xf7, 0x01,
				0x87, 0x34, 0x7b, 0x66
			};


			byte[] Random = Cipher.PRF(Secret, "test label", Seed, 100);

			AesCcmTests.AssertEqual(ExpectedOutput, Random);
		}

		[TestMethod]
		public void Test_02_Handshake()
		{
			this.dtls.StartHandshake();
			System.Threading.Thread.Sleep(2000);
		}

		// TODO: Test retransmissions, including lost Finished messages.

		private class Udp : ICommunicationLayer, IDisposable
		{
			private UdpClient client;

			public Udp()
			{
				this.client = new UdpClient(5684, AddressFamily.InterNetwork);
				//this.client.Connect("vs0.inf.ethz.ch", 5684);
				//this.client.Connect("californium.eclipse.org", 5684);
				//this.client.Connect("leshan.eclipse.org", 5684);
				this.client.Connect("lsys-home.dyndns.org", 5684);

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
