using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.Synchronization
{
	/// <summary>
	/// Implements the clock synchronization extesion as defined by the IEEE XMPP IoT Interface working group.
	/// </summary>
	public class SynchronizationClient : IDisposable
	{
		/// <summary>
		/// urn:ieee:iot:synchronization:1.0
		/// </summary>
		public const string NamespaceSynchronization = "urn:ieee:iot:synchronization:1.0";

		private static Calibration calibration;
		private readonly static Stopwatch clock = CreateWatch();

		private List<Rec> history = null;
		private XmppClient client;
		private Timer timer;
		private SpikeRemoval latency100NsWindow;
		private SpikeRemoval latencyHfWindow;
		private SpikeRemoval difference100NsWindow;
		private SpikeRemoval differenceHfWindow;
		private string clockSourceJID;
		private long rawLatency100Ns = 0;
		private long? rawLatencyHf = null;
		private long filteredLatency100Ns = 0;
		private long? filteredLatencyHf = null;
		private long avgLatency100Ns = 0;
		private long? avgLatencyHf = null;
		private long rawClockDifference100Ns = 0;
		private long? rawClockDifferenceHf = null;
		private long filteredClockDifference100Ns = 0;
		private long? filteredClockDifferenceHf = null;
		private long avgClockDifference100Ns = 0;
		private long? avgClockDifferenceHf = null;
		private long sumLatency100Ns = 0;
		private long? sumLatencyHf = null;
		private long sumDifference100Ns = 0;
		private long? sumDifferenceHf = null;
		private int nrLatency100Ns = 0;
		private int? nrLatencyHf = null;
		private int nrDifference100Ns = 0;
		private int? nrDifferenceHf = null;
		private int maxInHistory;
		private bool latencyRemoved = false;
		private bool latencyHfRemoved = false;
		private bool differenceRemoved = false;
		private bool differenceHfRemoved = false;

		private static Stopwatch CreateWatch()
		{
			Stopwatch Watch = new Stopwatch();
			Watch.Start();

			calibration = Calibrate(Watch);

			return Watch;
		}

		/// <summary>
		/// Implements the clock synchronization extesion as defined by the IEEE XMPP IoT Interface working group.
		/// THe internal clock is calibrated with the high frequency timer.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public SynchronizationClient(XmppClient Client)
		{
			this.client = Client;
			this.timer = null;
			this.clockSourceJID = null;

			this.client.RegisterIqGetHandler("req", NamespaceSynchronization, this.Clock, true);
			this.client.RegisterIqGetHandler("sourceReq", NamespaceSynchronization, this.SourceReq, true);
		}

		/// <summary>
		/// Calibrates the internal clock with the high frequency timer.
		/// </summary>
		public static void Calibrate()
		{
			calibration = Calibrate(clock);
		}

		private static Calibration Calibrate(Stopwatch Clock)
		{
			DateTime TP = DateTime.Now;
			long Ticks = TP.Ticks;
			long HF = 0;
			int i;

			for (i = 0; i < 3; i++)   // An extra round, to avoid JIT effects.
			{
				while ((TP = DateTime.UtcNow).Ticks == Ticks)
					;

				HF = Clock.ElapsedTicks;
				Ticks = TP.Ticks;
			}

			return new Calibration()
			{
				Reference = TP,
				ReferenceHfTick = HF,
				TicksTo100Ns = 1e7 / Stopwatch.Frequency
			};
		}

		private class Calibration
		{
			public DateTime Reference;
			public long ReferenceHfTick;
			public double TicksTo100Ns;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public void Dispose()
		{
			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			if (this.client != null)
			{
				this.client.UnregisterIqGetHandler("req", NamespaceSynchronization, this.Clock, true);
				this.client.UnregisterIqGetHandler("sourceReq", NamespaceSynchronization, this.SourceReq, true);
				this.client = null;
			}
		}

		/// <summary>
		/// Current high-resolution date and time
		/// </summary>
		public static DateTimeHF Now
		{
			get
			{
				DateTime NowRef = DateTime.UtcNow;
				long Ticks = clock.ElapsedTicks;
				Calibration Calibration = calibration;

				Ticks -= Calibration.ReferenceHfTick;

				long Ns100 = (long)(Ticks * Calibration.TicksTo100Ns + 0.5);
				long Milliseconds = Ns100 / 10000;

				DateTime Now = Calibration.Reference.AddMilliseconds(Milliseconds);

				if ((Now - NowRef).TotalSeconds >= 1)
				{
					Calibrate();
					return SynchronizationClient.Now;
				}
				else
				{
					Ns100 %= 10000;

					return new DateTimeHF(Now, (int)(Ns100 / 10), (int)(Ns100 % 10), Ticks);
				}
			}
		}

		private void Clock(object Sender, IqEventArgs e)
		{
			DateTimeHF Now = SynchronizationClient.Now;
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<resp xmlns='");
			Xml.Append(NamespaceSynchronization);

			if (Now.Ticks.HasValue)
			{
				Xml.Append("' hf='");
				Xml.Append(Now.Ticks.Value.ToString());
				Xml.Append("' freq='");
				Xml.Append(Stopwatch.Frequency.ToString());
			}

			Xml.Append("'>");
			Encode(Now, Xml);
			Xml.Append("</resp>");

			e.IqResult(Xml.ToString());
		}

		/// <summary>
		/// Encodes a high-frequency based Date and Time value to XML.
		/// </summary>
		/// <param name="Timestamp">Timestamp</param>
		/// <param name="Xml">XML output</param>
		public static void Encode(DateTimeHF Timestamp, StringBuilder Xml)
		{
			Xml.Append(Timestamp.Year.ToString("D4"));
			Xml.Append('-');
			Xml.Append(Timestamp.Month.ToString("D2"));
			Xml.Append('-');
			Xml.Append(Timestamp.Day.ToString("D2"));
			Xml.Append('T');
			Xml.Append(Timestamp.Hour.ToString("D2"));
			Xml.Append(':');
			Xml.Append(Timestamp.Minute.ToString("D2"));
			Xml.Append(':');
			Xml.Append(Timestamp.Second.ToString("D2"));
			Xml.Append('.');
			Xml.Append(Timestamp.Millisecond.ToString("D3"));
			Xml.Append(Timestamp.Microsecond.ToString("D3"));
			Xml.Append(Timestamp.Nanosecond100.ToString("D1"));
			Xml.Append('Z');
		}

		/// <summary>
		/// Measures the difference between the client clock and the clock of a clock source.
		/// </summary>
		/// <param name="ClockSourceJID">JID of clock source.</param>
		/// <param name="Callback">Callback method</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void MeasureClockDifference(string ClockSourceJID, SynchronizationEventHandler Callback, object State)
		{
			DateTimeHF ClientTime1 = SynchronizationClient.Now;

			this.client.SendIqGet(ClockSourceJID, "<req xmlns='" + NamespaceSynchronization + "'/>", (sender, e) =>
			{
				DateTimeHF ClientTime2 = SynchronizationClient.Now;
				XmlElement E;
				long Latency100Ns;
				long ClockDifference100Ns;
				long? LatencyHF;
				long? ClockDifferenceHF;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "resp" && E.NamespaceURI == NamespaceSynchronization &&
					DateTimeHF.TryParse(E.InnerText, out DateTimeHF ServerTime))
				{
					long dt1 = ServerTime - ClientTime1;
					long dt2 = ClientTime2 - ServerTime;
					Latency100Ns = dt1 + dt2;
					ClockDifference100Ns = dt1 - dt2;

					if (E.HasAttribute("hf") && long.TryParse(E.GetAttribute("hf"), out long Hf) &&
						E.HasAttribute("freq") && long.TryParse(E.GetAttribute("freq"), out long Freq))
					{
						if (Freq != Stopwatch.Frequency)
						{
							double d = Hf;
							d *= Stopwatch.Frequency;
							d /= Freq;
							Hf = (long)(d + 0.5);
						}

						dt1 = Hf - ClientTime1.Ticks.Value;
						dt2 = ClientTime2.Ticks.Value - Hf;
						LatencyHF = dt1 + dt2;
						ClockDifferenceHF = dt1 - dt2;
					}
					else
					{
						LatencyHF = null;
						ClockDifferenceHF = null;
					}
				}
				else
				{
					e.Ok = false;
					Latency100Ns = 0;
					ClockDifference100Ns = 0;
					LatencyHF = null;
					ClockDifferenceHF = null;
				}

				try
				{
					Callback?.Invoke(this, new SynchronizationEventArgs(Latency100Ns, ClockDifference100Ns, LatencyHF, ClockDifferenceHF, 
						Stopwatch.Frequency, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, State);
		}

		/// <summary>
		/// Measures the difference between the client clock and the clock of a clock source.
		/// </summary>
		/// <param name="ClockSourceJID">JID of clock source.</param>
		/// <returns>Result of the clock synchronization request.</returns>
		public Task<SynchronizationEventArgs> MeasureClockDifferenceAsync(string ClockSourceJID)
		{
			TaskCompletionSource<SynchronizationEventArgs> Result = new TaskCompletionSource<SynchronizationEventArgs>();

			this.MeasureClockDifference(ClockSourceJID, (sender, e) =>
			{
				Result.SetResult(e);
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Monitors the difference between the client clock and the clock of a clock source,
		/// by regularly measuring latency and clock difference.
		/// </summary>
		/// <param name="ClockSourceJID">JID of clock source.</param>
		/// <param name="IntervalMilliseconds">Interval, in milliseconds.</param>
		public void MonitorClockDifference(string ClockSourceJID, int IntervalMilliseconds)
		{
			this.MonitorClockDifference(ClockSourceJID, IntervalMilliseconds, 100, 16, 6, 3);
		}

		/// <summary>
		/// Monitors the difference between the client clock and the clock of a clock source,
		/// by regularly measuring latency and clock difference.
		/// </summary>
		/// <param name="ClockSourceJID">JID of clock source.</param>
		/// <param name="IntervalMilliseconds">Interval, in milliseconds.</param>
		/// <param name="History">Number of samples to keep in history. (Default=100)	</param>
		/// <param name="WindowSize">Number of samples to keep in memory. (Default=16)</param>
		/// <param name="SpikePosition">Spike removal position, inside window. (Default=6)</param>
		/// <param name="SpikeWidth">Number of samples that can constitute a spike. (Default=3)</param>
		public void MonitorClockDifference(string ClockSourceJID, int IntervalMilliseconds,
			int History, int WindowSize, int SpikePosition, int SpikeWidth)
		{
			if (IntervalMilliseconds < 1000)
				throw new ArgumentException("Interval must be at least 1000 milliseconds.", nameof(IntervalMilliseconds));

			if (WindowSize <= 5)
				throw new ArgumentException("Window size too small.", nameof(WindowSize));

			if (SpikePosition < 0 || SpikePosition >= WindowSize)
				throw new ArgumentException("Spike position must lie inside the window.", nameof(SpikePosition));

			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			if (this.history != null)
				this.history.Clear();
			else
				this.history = new List<Rec>();

			this.clockSourceJID = ClockSourceJID;
			this.latency100NsWindow = new SpikeRemoval(WindowSize, SpikePosition, SpikeWidth);
			this.latencyHfWindow = new SpikeRemoval(WindowSize, SpikePosition, SpikeWidth);
			this.difference100NsWindow = new SpikeRemoval(WindowSize, SpikePosition, SpikeWidth);
			this.differenceHfWindow = new SpikeRemoval(WindowSize, SpikePosition, SpikeWidth);
			this.sumLatency100Ns = 0;
			this.sumLatencyHf = null;
			this.sumDifference100Ns = 0;
			this.sumDifferenceHf = null;
			this.nrLatency100Ns = 0;
			this.nrLatencyHf = null;
			this.nrDifference100Ns = 0;
			this.nrDifferenceHf = null;
			this.maxInHistory = History;

			this.timer = new Timer(this.CheckClock, null, IntervalMilliseconds, IntervalMilliseconds);
		}

		/// <summary>
		/// Stops monitoring clock source.
		/// </summary>
		public void Stop()
		{
			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			this.clockSourceJID = null;
		}

		private void CheckClock(object P)
		{
			this.MeasureClockDifference(this.clockSourceJID, (sender, e) =>
			{
				if (e.Ok)
				{
					this.rawLatency100Ns = e.Latency100Ns;
					this.rawLatencyHf = e.LatencyHF;
					this.rawClockDifference100Ns = e.ClockDifference100Ns;
					this.rawClockDifferenceHf = e.ClockDifferenceHF;

					Rec Rec = new Rec()
					{
						Timestamp = DateTime.Now
					};

					if (this.latency100NsWindow.Add(e.Latency100Ns, out this.latencyRemoved, out long SumSamples, out int NrSamples))
					{
						Rec.SumLatency100Ns = SumSamples;
						Rec.NrLatency100Ns = NrSamples;
					}

					if (this.difference100NsWindow.Add(e.ClockDifference100Ns, out this.differenceRemoved, out SumSamples, out NrSamples))
					{
						Rec.SumDifference100Ns = SumSamples;
						Rec.NrDifference100Ns = NrSamples;
					}

					if (e.LatencyHF.HasValue && this.latencyHfWindow.Add(e.LatencyHF.Value, out this.latencyHfRemoved, out SumSamples, out NrSamples))
					{
						Rec.SumLatencyHf = SumSamples;
						Rec.NrLatencyHf = NrSamples;
					}

					if (e.ClockDifferenceHF.HasValue && this.differenceHfWindow.Add(e.ClockDifferenceHF.Value, out this.differenceHfRemoved, out SumSamples, out NrSamples))
					{
						Rec.SumDifferenceHf = SumSamples;
						Rec.NrDifferenceHf = NrSamples;
					}

					if (Rec.SumLatency100Ns.HasValue || Rec.SumDifference100Ns.HasValue || Rec.SumLatencyHf.HasValue || Rec.SumDifferenceHf.HasValue)
					{
						this.history.Add(Rec);

						if (Rec.SumLatency100Ns.HasValue)
						{
							this.filteredLatency100Ns = (Rec.SumLatency100Ns.Value + (Rec.NrLatency100Ns >> 1)) / Rec.NrLatency100Ns;

							this.sumLatency100Ns += Rec.SumLatency100Ns.Value;
							this.nrLatency100Ns += Rec.NrLatency100Ns;
						}

						if (Rec.SumLatencyHf.HasValue)
						{
							this.filteredLatencyHf = (Rec.SumLatencyHf.Value + (Rec.NrLatencyHf >> 1)) / Rec.NrLatencyHf;

							if (!this.sumLatencyHf.HasValue)
							{
								this.sumLatencyHf = 0;
								this.nrLatencyHf = 0;
							}

							this.sumLatencyHf += Rec.SumLatencyHf.Value;
							this.nrLatencyHf += Rec.NrLatencyHf;
						}

						if (Rec.SumDifference100Ns.HasValue)
						{
							this.filteredClockDifference100Ns = (Rec.SumDifference100Ns.Value + (Rec.NrDifference100Ns >> 1)) / Rec.NrDifference100Ns;

							this.sumDifference100Ns += Rec.SumDifference100Ns.Value;
							this.nrDifference100Ns += Rec.NrDifference100Ns;
						}

						if (Rec.SumDifferenceHf.HasValue)
						{
							this.filteredClockDifferenceHf = (Rec.SumDifferenceHf.Value + (Rec.NrDifferenceHf >> 1)) / Rec.NrDifferenceHf;

							if (!this.sumDifferenceHf.HasValue)
							{
								this.sumDifferenceHf = 0;
								this.nrDifferenceHf = 0;
							}

							this.sumDifferenceHf += Rec.SumDifferenceHf.Value;
							this.nrDifferenceHf += Rec.NrDifferenceHf;
						}

						while (this.history.Count > this.maxInHistory)
						{
							Rec = this.history[0];
							this.history.RemoveAt(0);

							if (Rec.SumLatency100Ns.HasValue)
							{
								this.sumLatency100Ns -= Rec.SumLatency100Ns.Value;
								this.nrLatency100Ns -= Rec.NrLatency100Ns;
							}

							if (Rec.SumLatencyHf.HasValue)
							{
								this.sumLatencyHf -= Rec.SumLatencyHf.Value;
								this.nrLatencyHf -= Rec.NrLatencyHf;
							}

							if (Rec.SumDifference100Ns.HasValue)
							{
								this.sumDifference100Ns -= Rec.SumDifference100Ns.Value;
								this.nrDifference100Ns -= Rec.NrDifference100Ns;
							}

							if (Rec.SumDifferenceHf.HasValue)
							{
								this.sumDifferenceHf -= Rec.SumDifferenceHf.Value;
								this.nrDifferenceHf -= Rec.NrDifferenceHf;
							}
						}

						if (this.nrLatency100Ns > 0)
							this.avgLatency100Ns = (this.sumLatency100Ns + (this.nrLatency100Ns >> 1)) / this.nrLatency100Ns;

						if (this.nrLatencyHf > 0)
							this.avgLatencyHf = (this.sumLatencyHf + (this.nrLatencyHf >> 1)) / this.nrLatencyHf;

						if (this.nrDifference100Ns > 0)
							this.avgClockDifference100Ns = (this.sumDifference100Ns + (this.nrDifference100Ns >> 1)) / this.nrDifference100Ns;

						if (this.nrDifferenceHf > 0)
							this.avgClockDifferenceHf = (this.sumDifferenceHf + (this.nrDifferenceHf >> 1)) / this.nrDifferenceHf;
					}

					try
					{
						this.OnUpdated?.Invoke(this, new EventArgs());
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		/// <summary>
		/// Most recent raw sample of the latency of sending a stanza between the machine and the clock source, or vice versa, measured in
		/// units of 100 ns.
		/// </summary>
		public long RawLatency100Ns => this.rawLatency100Ns;

		/// <summary>
		/// Most recent raw sample of the latency of sending a stanza between the machine and the clock source, or vice versa, measured in
		/// units of the local high-frequency timer.
		/// </summary>
		public long? RawLatencyHf => this.rawLatencyHf;

		/// <summary>
		/// Most recent filtered sample of the latency of sending a stanza between the machine and the clock source, or vice versa, measured in
		/// units of 100 ns.
		/// </summary>
		public long FilteredLatency100Ns => this.filteredLatency100Ns;

		/// <summary>
		/// Most recent filtered sample of the latency of sending a stanza between the machine and the clock source, or vice versa, measured in
		/// units of the local high-frequency timer.
		/// </summary>
		public long? FilteredLatencyHf => this.filteredLatencyHf;

		/// <summary>
		/// If a spike was detected and removed when calculating <see cref="FilteredLatency100Ns"/> from a short sequence of <see cref="RawLatency100Ns"/>.
		/// </summary>
		public bool LatencySpikeRemoved => this.latencyRemoved;

		/// <summary>
		/// If a spike was detected and removed when calculating <see cref="FilteredLatencyHf"/> from a short sequence of <see cref="RawLatencyHf"/>.
		/// </summary>
		public bool LatencyHfSpikeRemoved => this.latencyHfRemoved;

		/// <summary>
		/// Most recent average of the latency of sending a stanza between the machine and the clock source, or vice versa, measured in
		/// units of 100 ns. The average is calculated on a sequence of <see cref="FilteredLatency100Ns"/>.
		/// </summary>
		public long AvgLatency100Ns => this.avgLatency100Ns;

		/// <summary>
		/// Most recent average of the latency of sending a stanza between the machine and the clock source, or vice versa, measured in
		/// units of the local high-frequency timer. The average is calculated on a sequence of <see cref="FilteredLatencyHf"/>.
		/// </summary>
		public long? AvgLatencyHf => this.avgLatencyHf;

		/// <summary>
		/// Most recent raw sample of the clock difference beween the machine and the clock source, measured in
		/// units of 100 ns.
		/// </summary>
		public long RawClockDifference100Ns => this.rawClockDifference100Ns;

		/// <summary>
		/// Most recent raw sample of the clock difference beween the machine and the clock source, measured in
		/// units of the local high-frequency timer.
		/// </summary>
		public long? RawClockDifferenceHf => this.rawClockDifferenceHf;

		/// <summary>
		/// Most recent filtered sample of the clock difference beween the machine and the clock source, measured in
		/// units of 100 ns.
		/// </summary>
		public long FilteredClockDifference100Ns => this.filteredClockDifference100Ns;

		/// <summary>
		/// Most recent filtered sample of the clock difference beween the machine and the clock source, measured in
		/// units of the local high-frequency timer.
		/// </summary>
		public long? FilteredClockDifferenceHf => this.filteredClockDifferenceHf;

		/// <summary>
		/// If a spike was detected and removed when calculating <see cref="FilteredClockDifference100Ns"/> from a short sequence of <see cref="RawClockDifference100Ns"/>.
		/// </summary>
		public bool ClockDifferenceSpikeRemoved => this.differenceRemoved;

		/// <summary>
		/// If a spike was detected and removed when calculating <see cref="FilteredClockDifferenceHf"/> from a short sequence of <see cref="RawClockDifferenceHf"/>.
		/// </summary>
		public bool ClockDifferenceHfSpikeRemoved => this.differenceHfRemoved;

		/// <summary>
		/// Most recent average of the clock difference beween the machine and the clock source, measured in
		/// units of 100 ns. The average is calculated on a sequence of <see cref="FilteredClockDifference100Ns"/>.
		/// </summary>
		public long AvgClockDifference100Ns => this.avgClockDifference100Ns;

		/// <summary>
		/// Most recent average of the clock difference beween the machine and the clock source, measured in
		/// units of the local high-frequency timer. The average is calculated on a sequence of <see cref="FilteredClockDifferenceHf"/>.
		/// </summary>
		public long? AvgClockDifferenceHf => this.avgClockDifferenceHf;

		/// <summary>
		/// High Frequency increments per second, on the local machine.
		/// </summary>
		public long HfFrequency => Stopwatch.Frequency;

		/// <summary>
		/// Event raised when the clock difference estimates have been updated.
		/// </summary>
		public event EventHandler OnUpdated = null;

		private class Rec
		{
			public DateTime Timestamp;
			public long? SumLatency100Ns;
			public long? SumLatencyHf;
			public long? SumDifference100Ns;
			public long? SumDifferenceHf;
			public int NrLatency100Ns;
			public int NrLatencyHf;
			public int NrDifference100Ns;
			public int NrDifferenceHf;
		}

		private void SourceReq(object Sender, IqEventArgs e)
		{
			if (!string.IsNullOrEmpty(this.clockSourceJID))
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<sourceResp xmlns='");
				Xml.Append(NamespaceSynchronization);
				Xml.Append("'>");
				Xml.Append(XML.Encode(XmppClient.GetBareJID(this.clockSourceJID)));
				Xml.Append("</sourceResp>");

				e.IqResult(Xml.ToString());
			}
			else
				e.IqError(new StanzaErrors.ItemNotFoundException("Clock source not used.", e.IQ));
		}

		/// <summary>
		/// Queries an entity about what clock source it uses.
		/// </summary>
		/// <param name="To">Destination JID</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void QueryClockSource(string To, ClockSourceEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<sourceReq xmlns='" + NamespaceSynchronization + "'/>", (sender, e) =>
			{
				XmlElement E;
				string ClockSourceJID = null;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "sourceResp" && E.NamespaceURI == NamespaceSynchronization)
					ClockSourceJID = E.InnerText;

				try
				{
					Callback?.Invoke(this, new ClockSourceEventArgs(ClockSourceJID, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, State);
		}

		/// <summary>
		/// Queries an entity about what clock source it uses.
		/// </summary>
		/// <param name="To">Destination JID</param>
		/// <returns>JID of clock source used by <paramref name="To"/>.</returns>
		public Task<string> QueryClockSourceAsync(string To)
		{
			TaskCompletionSource<string> Result = new TaskCompletionSource<string>();

			this.QueryClockSource(To, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.ClockSourceJID);
				else
					Result.SetException(new Exception(e.ErrorText));

			}, null);

			return Result.Task;
		}

	}
}
