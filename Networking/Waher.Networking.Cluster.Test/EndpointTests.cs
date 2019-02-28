using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.Cluster.Test.TestObjects;
using Waher.Networking.Cluster.Messages;

namespace Waher.Networking.Cluster.Test
{
	[TestClass]
	public class EndpointTests
	{
		internal static readonly IPAddress clusterAddress = IPAddress.Parse("224.0.0.0");
		private ClusterEndpoint endpoint1 = null;
		private ClusterEndpoint endpoint2 = null;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Runtime.Inventory.Types.Initialize(
				typeof(EndpointTests).Assembly,
				typeof(Waher.Networking.Cluster.ClusterEndpoint).Assembly);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.endpoint1 = new ClusterEndpoint(clusterAddress, 12345, "UnitTest",
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine));
			this.endpoint1.GetStatus += (sender, e) => e.Status = 1;

			foreach (IPEndPoint Endpoint in this.endpoint1.Endpoints)
			{
				this.endpoint2 = new ClusterEndpoint(Endpoint.Address, Endpoint.Port, "UnitTest");
				this.endpoint2.GetStatus += (sender, e) => e.Status = 2;
				break;
			}
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.endpoint1?.Dispose();
			this.endpoint1 = null;

			this.endpoint2?.Dispose();
			this.endpoint2 = null;
		}

		[TestMethod]
		public void Test_01_Send_Unacknowledged_Message()
		{
			this.TestUnacknowledgedMessage("Hello World!");
		}

		private void TestUnacknowledgedMessage(string Text)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			Message Msg = new Message()
			{
				Text = Text,
				Timestamp = DateTime.Now
			};

			this.endpoint1.OnMessageReceived += (sender, e) =>
			{
				if (e.Message is Message Msg2)
				{
					if (Msg.Text == Msg2.Text &&
					Msg.Timestamp == Msg2.Timestamp)
					{
						Done.Set();
					}
					else
						Error.Set();
				}
			};

			this.endpoint2.SendMessageUnacknowledged(Msg);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}

		[TestMethod]
		public void Test_02_Fragmentation()
		{
			this.TestUnacknowledgedMessage(new string('x', 80000));
		}

		[TestMethod]
		public void Test_03_LargeMessage()
		{
			this.TestUnacknowledgedMessage(new string('x', 1000000));
		}

		[TestMethod]
		public void Test_04_EndpointStatuses()
		{
			ManualResetEvent AliveReceived = new ManualResetEvent(false);

			this.endpoint1.OnMessageReceived += (sender, e) =>
			{
				if (e.Message is Alive)
					AliveReceived.Set();
			};

			Assert.IsTrue(AliveReceived.WaitOne(10000), "Alive message not received.");

			Assert.AreEqual(1, this.endpoint1.LocalStatus);

			EndpointStatus[] RemoteStatus = this.endpoint1.GetRemoteStatuses();
			Assert.AreEqual(1, RemoteStatus.Length);
			Assert.AreEqual(2, RemoteStatus[0].Status);
		}

	}
}
