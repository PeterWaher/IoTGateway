using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;

namespace Waher.Networking.Cluster.Test
{
	[TestClass]
	public class EndpointTests
	{
		private static readonly IPAddress clusterAddress = IPAddress.Parse("224.0.0.0");
		private ClusterEndpoint endpoint1 = null;
		private ClusterEndpoint endpoint2 = null;

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
		public void Test_01_Send()
		{
			ManualResetEvent Done = new ManualResetEvent(false);

			this.endpoint1.OnDataReceived += (sender, e) =>
			{
				Done.Set();
			};

			this.endpoint2.Transmit(new byte[] { 1, 2, 3, 4, 5 });

			Assert.IsTrue(Done.WaitOne(5000));
		}

	}
}
