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
		internal static readonly IPAddress clusterAddress = IPAddress.Parse("239.255.0.0");
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
				this.endpoint2.AddRemoteStatus(Endpoint, null);
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

		[TestMethod]
		public void Test_05_Send_Acknowledged_Message()
		{
			this.TestAcknowledgedMessage("Hello World!");
		}

		private void TestAcknowledgedMessage(string Text)
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);

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
						Done1.Set();
					}
					else
						Error1.Set();
				}
			};

			this.endpoint2.SendMessageAcknowledged(Msg, (sender, e) =>
			{
				if (e.Message == Msg &&
					e.Responses.Length == 1 &&
					e.Responses[0].ACK.HasValue &&
					e.Responses[0].ACK.Value &&
					e.State.Equals(1))
				{
					Done2.Set();
				}
				else
					Error2.Set();
			}, 1);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 20000));
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 20000));
		}

		[TestMethod]
		public void Test_06_Fragmentation_Ack()
		{
			this.TestAcknowledgedMessage(new string('x', 80000));
		}

		[TestMethod]
		public void Test_07_LargeMessage_Ack()
		{
			this.TestAcknowledgedMessage(new string('x', 1000000));
		}

		[TestMethod]
		public async Task Test_08_Ping()
		{
			EndpointAcknowledgement[] Response = await this.endpoint2.PingAsync();

			Assert.AreEqual(1, Response.Length);
			Assert.AreEqual(true, Response[0].ACK.HasValue);
			Assert.AreEqual(true, Response[0].ACK.Value);
		}

		[TestMethod]
		public void Test_09_RequestResponse()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			Add Add = new Add()
			{
				A = 3,
				B = 4
			};

			this.endpoint2.ExecuteCommand<int>(Add, (sender, e) =>
			{
				if (e.Ok &&
					e.Responses.Length == 1 &&
					e.Responses[0].Ok &&
					e.Responses[0].Response == 7 &&
					e.State.Equals(9))
				{
					Done.Set();
				}
				else
					Error.Set();
			}, 9);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}

		[TestMethod]
		public void Test_10_RequestResponse_Error()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			Error ErrorCommand = new Error()
			{
				A = 3,
				B = 4
			};

			this.endpoint2.ExecuteCommand<int>(ErrorCommand, (sender, e) =>
			{
				if (!e.Ok &&
					e.Responses.Length == 1 &&
					!e.Responses[0].Ok &&
					e.Responses[0].Error is ArgumentException ex &&
					ex.Message == "7" &&
					e.State.Equals(10))
				{
					Done.Set();
				}
				else
					Error.Set();
			}, 10);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}

		[TestMethod]
		public void Test_11_Send_Assured_Message()
		{
			this.TestAssuredMessage("Hello World!");
		}

		private void TestAssuredMessage(string Text)
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);

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
						Done1.Set();
					}
					else
						Error1.Set();
				}
			};

			this.endpoint2.SendMessageAssured(Msg, (sender, e) =>
			{
				if (e.Message == Msg &&
					e.Responses.Length == 1 &&
					e.Responses[0].ACK.HasValue &&
					e.Responses[0].ACK.Value &&
					e.State.Equals(1))
				{
					Done2.Set();
				}
				else
					Error2.Set();
			}, 1);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 20000));
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 20000));
		}

		[TestMethod]
		public void Test_12_Fragmentation_Assured()
		{
			this.TestAssuredMessage(new string('x', 80000));
		}

		[TestMethod]
		public void Test_13_LargeMessage_Assured()
		{
			this.TestAssuredMessage(new string('x', 1000000));
		}

		[TestMethod]
		public async Task Test_14_Echo()
		{
			EndpointResponse<string>[] Response = await this.endpoint2.EchoAsync("Hello World!");

			Assert.AreEqual(1, Response.Length);
			Assert.AreEqual(true, Response[0].Ok);
			Assert.AreEqual("Hello World!", Response[0].Response);
		}

		[TestMethod]
		public async Task Test_15_Assemblies()
		{
			EndpointResponse<string[]>[] Response = await this.endpoint2.GetAssembliesAsync();

			Assert.AreEqual(1, Response.Length);
			Assert.AreEqual(true, Response[0].Ok);
		}

		[TestMethod]
		public void Test_16_Lock()
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);

			this.endpoint2.Lock("Resource", 2000, (sender, e) =>
			{
				if (e.LockSuccessful &&
					e.Resource == "Resource" &&
					e.LockedBy is null &&
					e.State.Equals(16))
				{
					Done1.Set();
				}
				else
					Error1.Set();
			}, 16);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 5000));
		}

		[TestMethod]
		public void Test_17_Lock_Collision()
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);

			this.endpoint2.Lock("Resource", 2000, (sender, e) =>
			{
				if (e.LockSuccessful &&
					e.Resource == "Resource" &&
					e.LockedBy is null &&
					e.State.Equals(17.1))
				{
					Done1.Set();
				}
				else
					Error1.Set();
			}, 17.1);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 5000));

			this.endpoint2.Lock("Resource", 2000, (sender, e) =>
			{
				if (!e.LockSuccessful &&
					e.Resource == "Resource" &&
					e.LockedBy is null &&
					e.State.Equals(17.2))
				{
					Done2.Set();
				}
				else
					Error2.Set();
			}, 17.2);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 5000));
		}

		[TestMethod]
		public void Test_18_Lock_Collision_Release()
		{
			ManualResetEvent Done1 = new ManualResetEvent(false);
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);

			this.endpoint2.Lock("Resource", 2000, (sender, e) =>
			{
				if (e.LockSuccessful &&
					e.Resource == "Resource" &&
					e.LockedBy is null &&
					e.State.Equals(18.1))
				{
					Done1.Set();
				}
				else
					Error1.Set();
			}, 18.1);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 5000));

			this.endpoint2.Lock("Resource", 2000, (sender, e) =>
			{
				if (e.LockSuccessful &&
					e.Resource == "Resource" &&
					e.LockedBy is null &&
					e.State.Equals(18.2))
				{
					Done2.Set();
				}
				else
					Error2.Set();
			}, 18.2);

			this.endpoint2.Release("Resource");

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 5000));
		}

	}
}
