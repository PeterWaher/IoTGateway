using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.Cluster.Messages;
using Waher.Networking.Cluster.Test.TestObjects;
using Waher.Networking.Sniffers;

namespace Waher.Networking.Cluster.Test
{
	[TestClass]
	public class EndpointTests
	{
		internal static readonly IPAddress clusterAddress = IPAddress.Parse("239.255.0.0");
		private ClusterEndpoint endpoint1 = null;
		private ClusterEndpoint endpoint2 = null;
		private XmlFileSniffer xmlSniffer1 = null;
		private XmlFileSniffer xmlSniffer2 = null;


		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Runtime.Inventory.Types.Initialize(
				typeof(EndpointTests).Assembly,
				typeof(ClusterEndpoint).Assembly);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			File.Delete("Cluster1.xml");
			this.xmlSniffer1 = new XmlFileSniffer("Cluster1.xml",
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				int.MaxValue, BinaryPresentationMethod.Hexadecimal);

			this.endpoint1 = new ClusterEndpoint(clusterAddress, 12345, "UnitTest", this.xmlSniffer1,
				new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine));
			this.endpoint1.GetStatus += (Sender, e) =>
			{
				e.Status = 1;
				return Task.CompletedTask;
			};

			File.Delete("Cluster2.xml");
			this.xmlSniffer2 = new XmlFileSniffer("Cluster2.xml",
					@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
					int.MaxValue, BinaryPresentationMethod.Hexadecimal);

			this.endpoint2 = new ClusterEndpoint(clusterAddress, 12345, "UnitTest", this.xmlSniffer2);
			this.endpoint2.GetStatus += (Sender, e) =>
			{
				e.Status = 2;
				return Task.CompletedTask;
			};
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.endpoint1 is not null)
			{
				await this.endpoint1.DisposeAsync();
				this.endpoint1 = null;
			}

			if (this.endpoint2 is not null)
			{
				await this.endpoint2.DisposeAsync();
				this.endpoint2 = null;
			}

			if (this.xmlSniffer1 is not null)
			{
				await this.xmlSniffer1.DisposeAsync();
				this.xmlSniffer1 = null;
			}

			if (this.xmlSniffer2 is not null)
			{
				await this.xmlSniffer2.DisposeAsync();
				this.xmlSniffer2 = null;
			}
		}

		[TestMethod]
		public async Task Test_01_Send_Unacknowledged_Message()
		{
			await this.TestUnacknowledgedMessage("Hello World!");
		}

		private async Task TestUnacknowledgedMessage(string Text)
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);
			Message Msg = new()
			{
				Text = Text,
				Timestamp = DateTime.Now
			};

			this.endpoint1.OnMessageReceived += (Sender, e) =>
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

				return Task.CompletedTask;
			};

			await this.endpoint2.SendMessageUnacknowledged(Msg);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 5000));
		}

		[TestMethod]
		public async Task Test_02_Fragmentation()
		{
			await this.TestUnacknowledgedMessage(new string('x', 80000));
		}

		[TestMethod]
		public async Task Test_03_LargeMessage()
		{
			await this.TestUnacknowledgedMessage(new string('x', 1000000));
		}

		[TestMethod]
		public void Test_04_EndpointStatuses()
		{
			ManualResetEvent AliveReceived = new(false);

			this.endpoint1.OnMessageReceived += (Sender, e) =>
			{
				if (e.Message is Alive)
					AliveReceived.Set();

				return Task.CompletedTask;
			};

			Assert.IsTrue(AliveReceived.WaitOne(10000), "Alive message not received.");

			Assert.AreEqual(1, this.endpoint1.LocalStatus);

			EndpointStatus[] RemoteStatus = this.endpoint1.GetRemoteStatuses();
			Assert.AreEqual(1, RemoteStatus.Length);
			Assert.AreEqual(2, RemoteStatus[0].Status);
		}

		[TestMethod]
		public async Task Test_05_Send_Acknowledged_Message()
		{
			await this.TestAcknowledgedMessage("Hello World!");
		}

		private async Task TestAcknowledgedMessage(string Text)
		{
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);

			Message Msg = new()
			{
				Text = Text,
				Timestamp = DateTime.Now
			};

			this.endpoint1.OnMessageReceived += (Sender, e) =>
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

				return Task.CompletedTask;
			};

			await this.endpoint2.SendMessageAcknowledged(Msg, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 1);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 20000));
			Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 20000));
		}

		[TestMethod]
		public async Task Test_06_Fragmentation_Ack()
		{
			await this.TestAcknowledgedMessage(new string('x', 80000));
		}

		[TestMethod]
		public async Task Test_07_LargeMessage_Ack()
		{
			await this.TestAcknowledgedMessage(new string('x', 1000000));
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
		public async Task Test_09_RequestResponse()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);
			Add Add = new()
			{
				A = 3,
				B = 4
			};

			await this.endpoint2.ExecuteCommand<int>(Add, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 9);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 5000));
		}

		[TestMethod]
		public async Task Test_10_RequestResponse_Error()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);
			Error ErrorCommand = new()
			{
				A = 3,
				B = 4
			};

			await this.endpoint2.ExecuteCommand<int>(ErrorCommand, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 10);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 5000));
		}

		[TestMethod]
		public async Task Test_11_Send_Assured_Message()
		{
			await this.TestAssuredMessage("Hello World!");
		}

		private async Task TestAssuredMessage(string Text)
		{
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);

			Message Msg = new()
			{
				Text = Text,
				Timestamp = DateTime.Now
			};

			this.endpoint1.OnMessageReceived += (Sender, e) =>
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

				return Task.CompletedTask;
			};

			await this.endpoint2.SendMessageAssured(Msg, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 1);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 20000));
			Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 20000));
		}

		[TestMethod]
		public async Task Test_12_Fragmentation_Assured()
		{
			await this.TestAssuredMessage(new string('x', 80000));
		}

		[TestMethod]
		public async Task Test_13_LargeMessage_Assured()
		{
			await this.TestAssuredMessage(new string('x', 1000000));
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
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);

			this.endpoint2.Lock("Resource", 2000, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 16);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 5000));
		}

		[TestMethod]
		public void Test_17_Lock_Collision()
		{
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);

			this.endpoint2.Lock("Resource", 2000, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 17.1);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 5000));

			this.endpoint2.Lock("Resource", 2000, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 17.2);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 5000));
		}

		[TestMethod]
		public async Task Test_18_Lock_Collision_Release()
		{
			ManualResetEvent Done1 = new(false);
			ManualResetEvent Error1 = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);

			await this.endpoint2.Lock("Resource", 2000, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 18.1);

			Assert.AreEqual(0, WaitHandle.WaitAny([Done1, Error1], 5000));

			await this.endpoint2.Lock("Resource", 2000, (Sender, e) =>
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

				return Task.CompletedTask;
			}, 18.2);

			await this.endpoint2.Release("Resource");

			Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 5000));
		}

	}
}
