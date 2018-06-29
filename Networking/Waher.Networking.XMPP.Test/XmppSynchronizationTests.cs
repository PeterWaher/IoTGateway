using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.Synchronization;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppSynchronizationTests : CommunicationTests
	{
		private SynchronizationClient synchronizationClient1;
		private SynchronizationClient synchronizationClient2;

		public override void ConnectClients()
		{
			base.ConnectClients();

			Assert.AreEqual(XmppState.Connected, this.client1.State);
			Assert.AreEqual(XmppState.Connected, this.client2.State);

			this.synchronizationClient1 = new SynchronizationClient(this.client1);
			this.synchronizationClient2 = new SynchronizationClient(this.client2);
		}

		public override void DisposeClients()
		{
			if (this.synchronizationClient2 != null)
			{
				this.synchronizationClient2.Dispose();
				this.synchronizationClient2 = null;
			}

			if (this.synchronizationClient1 != null)
			{
				this.synchronizationClient1.Dispose();
				this.synchronizationClient1 = null;
			}

			base.DisposeClients();
		}

		[TestMethod]
		public async Task Control_Test_01_Measure()
		{
			this.ConnectClients();

			SynchronizationEventArgs e = await this.synchronizationClient1.MeasureClockDifferenceAsync(this.client2.FullJID);

			Console.Out.WriteLine("Latency (s)\tDifference (s)");
			Console.Out.WriteLine(e.LatencySeconds.ToString() + "\t" + e.ClockDifferenceSeconds.ToString());
		}

		[TestMethod]
		public async Task Control_Test_02_Measure_10()
		{
			this.ConnectClients();

			int i;

			Console.Out.WriteLine("Latency (s)\tDifference (s)");

			for (i = 0; i < 10; i++)
			{
				SynchronizationEventArgs e = await this.synchronizationClient1.MeasureClockDifferenceAsync(this.client2.FullJID);

				Console.Out.WriteLine(e.LatencySeconds.ToString() + "\t" + e.ClockDifferenceSeconds.ToString());

				Thread.Sleep(5000);
			}
		}

	}
}
