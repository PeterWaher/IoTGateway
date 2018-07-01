using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.XMPP.Synchronization;
using Waher.Script;

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

			Console.Out.WriteLine("Latency (ms)\tDifference (ms)");
			Console.Out.WriteLine((e.Latency100Ns * 1e-4).ToString() + "\t" + (e.ClockDifference100Ns * 1e-4).ToString());
		}

		[TestMethod]
		public void Control_Test_02_Monitor_30()
		{
			this.Monitor(30, 1000, "Monitor30.tsv");
		}

		[TestMethod]
		public void Control_Test_03_Monitor_200()
		{
			this.Monitor(200, 1000, "Monitor200.tsv");
		}

		[TestMethod]
		public void Control_Test_04_Monitor_1000()
		{
			this.Monitor(1000, 1000, "Monitor1000.tsv");
		}

		private void Monitor(int RecordsLeft, int IntervalMs, string FileName)
		{
			this.ConnectClients();

			using (StreamWriter w = File.CreateText(FileName))
			{
				ManualResetEvent Done = new ManualResetEvent(false);

				w.WriteLine("Date\tTime\tRaw Latency (ms)\tRaw Difference (ms)\tLatency Spike\tDifference Spike\tFiltered Latency (ms)\tFiltered Difference (ms)\tAvg Latency (ms)\tAvg Difference (ms)");

				this.synchronizationClient1.OnUpdated += (sender, e) =>
				{
					DateTime TP = DateTime.Now;

					w.WriteLine(
						TP.Date.ToShortDateString() + "\t" +
						TP.ToLongTimeString() + "\t" +
						Expression.ToString(this.synchronizationClient1.RawLatency100Ns * 1e-4) + "\t" +
						Expression.ToString(this.synchronizationClient1.RawClockDifference100Ns * 1e-4) + "\t" +
						CommonTypes.Encode(this.synchronizationClient1.LatencySpikeRemoved) + "\t" +
						CommonTypes.Encode(this.synchronizationClient1.ClockDifferenceSpikeRemoved) + "\t" +
						Expression.ToString(this.synchronizationClient1.FilteredLatency100Ns * 1e-4) + "\t" +
						Expression.ToString(this.synchronizationClient1.FilteredClockDifference100Ns * 1e-4) + "\t" +
						Expression.ToString(this.synchronizationClient1.AvgLatency100Ns * 1e-4) + "\t" +
						Expression.ToString(this.synchronizationClient1.AvgClockDifference100Ns * 1e-4));

					if (--RecordsLeft == 0)
						Done.Set();
				};

				this.synchronizationClient1.MonitorClockDifference(this.client2.FullJID, 1000);

				Done.WaitOne();

				w.Flush();
			}
		}

		[TestMethod]
		public void Control_Test_05_DateTime_Resolution()
		{
			using (StreamWriter w = File.CreateText("DateTimeResolution.tsv"))
			{
				Stopwatch Watch = new Stopwatch();
				int RecordsLeft = 1000;
				DateTime TP0 = DateTime.Now;
				DateTime TP = TP0;
				DateTime TP2;
				long Ticks;
				long TP0Ticks = TP0.Ticks;

				Watch.Start();

				w.WriteLine("Time\tTicks\tHF\tFrequency");

				while (RecordsLeft-- > 0)
				{
					while ((TP2 = DateTime.Now).Ticks == TP.Ticks)
						;

					Ticks = Watch.ElapsedTicks;
					w.WriteLine(
						TP2.ToLongTimeString() + "." + TP2.Millisecond.ToString("D3") + "\t" +
						(TP2.Ticks - TP0Ticks).ToString() + "\t" +
						Ticks.ToString() + "\t" +
						Stopwatch.Frequency.ToString());

					TP = TP2;
				}

				Watch.Stop();

				w.Flush();
			}
		}

	}
}
