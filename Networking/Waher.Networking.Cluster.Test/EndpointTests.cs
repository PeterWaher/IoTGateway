using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.Cluster.Test.TestObjects;
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

			foreach (IPEndPoint Endpoint in this.endpoint1.Endpoints)
			{
				this.endpoint2 = new ClusterEndpoint(Endpoint.Address, Endpoint.Port, "UnitTest");
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
			ManualResetEvent Done = new ManualResetEvent(false);

			this.endpoint1.OnDataReceived += (sender, e) =>
			{
				Done.Set();
			};

			this.endpoint2.SendMessageUnacknowledged(new Strings()
			{
				S1 = "Hello",
				S2 = "World",
				S3 = "!"
			});

			Assert.IsTrue(Done.WaitOne(5000));
		}

	}
}
