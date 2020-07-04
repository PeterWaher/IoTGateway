using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Events;
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
			await this.Measure(this.client2.FullJID);
		}

		private async Task Measure(string Jid)
		{
			SynchronizationEventArgs e = await this.synchronizationClient1.MeasureClockDifferenceAsync(Jid);

			double LatencyMs = e.Latency100Ns * 1e-4;
			double DifferenceMs = e.ClockDifference100Ns * 1e-4;
			double LatencyMsHf = e.LatencyHF.Value * 1e3 / e.HfFrequency;
			double DifferenceMsHf = e.ClockDifferenceHF.Value * 1e3 / e.HfFrequency;

			Console.Out.WriteLine("Latency (ms)\tDifference (ms)\tLatency (HF)\tDifference (HF)");
			Console.Out.WriteLine(LatencyMs.ToString() + "\t" + DifferenceMs.ToString() + "\t" +
				LatencyMsHf.ToString() + "\t" + DifferenceMsHf.ToString());
		}

		[TestMethod]
		public void Control_Test_02_Monitor_30()
		{
			this.ConnectClients();
			this.Monitor(this.client2.FullJID, 30, 1000, "Monitor30.tsv");
		}

		[TestMethod]
		public void Control_Test_03_Monitor_200()
		{
			this.ConnectClients();
			this.Monitor(this.client2.FullJID, 200, 1000, "Monitor200.tsv");
		}

		[TestMethod]
		public void Control_Test_04_Monitor_1000()
		{
			this.ConnectClients();
			this.Monitor(this.client2.FullJID, 1000, 1000, "Monitor1000.tsv");
		}

		private void Monitor(string Source, int RecordsLeft, int IntervalMs, string FileName)
		{
			using (StreamWriter w = File.CreateText(FileName))
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);
				double HfToMs = 1e3 / this.synchronizationClient1.HfFrequency;

				w.WriteLine("Date\tTime\tRaw Latency (ms)\tRaw Difference (ms)\tLatency Spike\tDifference Spike\tFiltered Latency (ms)\tFiltered Difference (ms)\tAvg Latency (ms)\tAvg Difference (ms)\tRaw HF Latency (ms)\tRaw HF Difference (ms)\tHF Latency Spike\tHF Difference Spike\tFiltered HF Latency (ms)\tFiltered HF Difference (ms)\tAvg HF Latency (ms)\tAvg HF Difference (ms)");

				this.synchronizationClient1.OnUpdated += (sender, e) =>
				{
					try
					{
						DateTime TP = DateTime.Now;

						w.Write(TP.Date.ToShortDateString());
						w.Write("\t");
						w.Write(TP.ToLongTimeString());
						w.Write("\t");
						if (this.synchronizationClient1.RawLatency100Ns.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.RawLatency100Ns.Value * 1e-4));
						w.Write("\t");
						if (this.synchronizationClient1.RawClockDifference100Ns.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.RawClockDifference100Ns.Value * 1e-4));
						w.Write("\t");
						if (this.synchronizationClient1.LatencySpikeRemoved.HasValue)
							w.Write(CommonTypes.Encode(this.synchronizationClient1.LatencySpikeRemoved.Value));
						w.Write("\t");
						if (this.synchronizationClient1.ClockDifferenceSpikeRemoved.HasValue)
							w.Write(CommonTypes.Encode(this.synchronizationClient1.ClockDifferenceSpikeRemoved.Value));
						w.Write("\t");
						if (this.synchronizationClient1.FilteredLatency100Ns.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.FilteredLatency100Ns.Value * 1e-4));
						w.Write("\t");
						if (this.synchronizationClient1.FilteredClockDifference100Ns.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.FilteredClockDifference100Ns.Value * 1e-4));
						w.Write("\t");
						if (this.synchronizationClient1.AvgLatency100Ns.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.AvgLatency100Ns.Value * 1e-4));
						w.Write("\t");
						if (this.synchronizationClient1.AvgClockDifference100Ns.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.AvgClockDifference100Ns.Value * 1e-4));
						w.Write("\t");
						if (this.synchronizationClient1.RawLatencyHf.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.RawLatencyHf.Value * HfToMs));
						w.Write("\t");
						if (this.synchronizationClient1.RawClockDifferenceHf.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.RawClockDifferenceHf.Value * HfToMs));
						w.Write("\t");
						if (this.synchronizationClient1.LatencyHfSpikeRemoved.HasValue)
							w.Write(CommonTypes.Encode(this.synchronizationClient1.LatencyHfSpikeRemoved.Value));
						w.Write("\t");
						if (this.synchronizationClient1.ClockDifferenceHfSpikeRemoved.HasValue)
							w.Write(CommonTypes.Encode(this.synchronizationClient1.ClockDifferenceHfSpikeRemoved.Value));
						w.Write("\t");
						if (this.synchronizationClient1.FilteredLatencyHf.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.FilteredLatencyHf.Value * HfToMs));
						w.Write("\t");
						if (this.synchronizationClient1.FilteredClockDifferenceHf.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.FilteredClockDifferenceHf.Value * HfToMs));
						w.Write("\t");
						if (this.synchronizationClient1.AvgLatencyHf.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.AvgLatencyHf.Value * HfToMs));
						w.Write("\t");
						if (this.synchronizationClient1.AvgClockDifferenceHf.HasValue)
							w.Write(Expression.ToString(this.synchronizationClient1.AvgClockDifferenceHf.Value * HfToMs));

						w.WriteLine();

						if (--RecordsLeft == 0)
							Done.Set();
					}
					catch (Exception ex)
					{
						Console.Out.WriteLine(ex.Message);
						Console.Out.WriteLine(Log.CleanStackTrace(ex.StackTrace));
						Error.Set();
					}
				};

				this.synchronizationClient1.MonitorClockDifference(Source, 1000);
				try
				{
					this.synchronizationClient2.QueryClockSource(this.client1.FullJID, (sender, e) =>
					{
						if (!e.Ok || e.ClockSourceJID != XmppClient.GetBareJID(Source))
							Error.Set();
					}, null);

					Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }));
				}
				finally
				{
					this.synchronizationClient1.Stop();
					w.Flush();
				}
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

		[TestMethod]
		public async Task Control_Test_06_Measure_Server()
		{
			this.ConnectClients();
			await this.Measure("waher.se");
		}

		[TestMethod]
		public void Control_Test_07_Monitor_30_Server()
		{
			this.ConnectClients();
			this.Monitor("waher.se", 30, 1000, "Monitor30Server.tsv");
		}

		[TestMethod]
		public void Control_Test_08_Monitor_200_Server()
		{
			this.ConnectClients();
			this.Monitor("waher.se", 200, 1000, "Monitor200Server.tsv");
		}

		[TestMethod]
		public void Control_Test_09_Monitor_1000_Server()
		{
			this.ConnectClients();
			this.Monitor("waher.se", 1000, 1000, "Monitor1000Server.tsv");
		}

		[TestMethod]
		public async Task Control_Test_10_Measure_Federated_Server()
		{
			this.ConnectClients();
			await this.Measure("cybercity.online");
		}

		[TestMethod]
		public void Control_Test_11_Monitor_30_Federated_Server()
		{
			this.ConnectClients();
			this.Monitor("cybercity.online", 30, 1000, "Monitor30FederatedServer.tsv");
		}

		[TestMethod]
		public void Control_Test_12_Monitor_200_Federated_Server()
		{
			this.ConnectClients();
			this.Monitor("cybercity.online", 200, 1000, "Monitor200FederatedServer.tsv");
		}

		[TestMethod]
		public void Control_Test_13_Monitor_1000_Federated_Server()
		{
			this.ConnectClients();
			this.Monitor("cybercity.online", 1000, 1000, "Monitor1000FederatedServer.tsv");
		}

	}
}
