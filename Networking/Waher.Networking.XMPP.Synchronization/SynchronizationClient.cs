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

		private List<Rec> history = null;
		private XmppClient client;
		private Timer timer;
		private SpikeRemoval latency100NsWindow;
		private SpikeRemoval difference100NsWindow;
		private Stopwatch clock;
		private Calibration calibration;
		private string clockSourceJID;
		private long rawLatency100Ns = 0;
		private long filteredLatency100Ns = 0;
		private long avgLatency100Ns = 0;
		private long rawClockDifference100Ns = 0;
		private long filteredClockDifference100Ns = 0;
		private long avgClockDifference100Ns = 0;
		private long sumLatency100Ns;
		private long sumDifference100Ns;
		private int nrLatency100Ns;
		private int nrDifference100Ns;
		private int maxInHistory;
		private bool latencyRemoved = false;
		private bool differenceRemoved = false;

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
			this.clock = new Stopwatch();
			this.clock.Start();

			this.Calibrate();

			this.client.RegisterIqGetHandler("clock", NamespaceSynchronization, this.Clock, true);
		}

		/// <summary>
		/// Calibrates the internal clock with the high frequency timer.
		/// </summary>
		public void Calibrate()
		{
			DateTime TP = DateTime.Now;
			long Ticks = TP.Ticks;
			long HF = 0;
			int i;

			for (i = 0; i < 3; i++)   // An extra round, to avoid JIT effects.
			{
				while ((TP = DateTime.UtcNow).Ticks == Ticks)
					;

				HF = this.clock.ElapsedTicks;
				Ticks = TP.Ticks;
			}

			this.calibration = new Calibration()
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
			if (this.clock != null)
			{
				this.clock.Stop();
				this.clock = null;
			}

			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			if (this.client != null)
			{
				this.client.UnregisterIqGetHandler("clock", NamespaceSynchronization, this.Clock, true);
				this.client = null;
			}
		}

		/// <summary>
		/// Current high-resolution date and time
		/// </summary>
		public DateTimeHF Now
		{
			get
			{
				DateTime NowRef = DateTime.UtcNow;
				long Ticks = this.clock.ElapsedTicks;
				Calibration Calibration = this.calibration;

				Ticks -= Calibration.ReferenceHfTick;

				long Ns100 = (long)(Ticks * Calibration.TicksTo100Ns + 0.5);
				long Milliseconds = Ns100 / 10000;

				DateTime Now = Calibration.Reference.AddMilliseconds(Milliseconds);

				if ((Now - NowRef).TotalSeconds >= 1)
				{
					this.Calibrate();
					return this.Now;
				}
				else
				{
					Ns100 %= 10000;

					return new DateTimeHF(Now, (int)(Ns100 / 10), (int)(Ns100 % 10));
				}
			}
		}

		private void Clock(object Sender, IqEventArgs e)
		{
			DateTimeHF Now = this.Now;
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<clock xmlns='");
			Xml.Append(NamespaceSynchronization);
			Xml.Append("'>");
			Encode(Now, Xml);
			Xml.Append("</clock>");

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
			DateTimeHF ClientTime1 = this.Now;
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<clock xmlns='");
			Xml.Append(NamespaceSynchronization);
			Xml.Append("'>");
			Encode(ClientTime1, Xml);
			Xml.Append("</clock>");

			this.client.SendIqGet(ClockSourceJID, Xml.ToString(), (sender, e) =>
			{
				DateTimeHF ClientTime2 = this.Now;
				XmlElement E;
				long Latency100Ns;
				long ClockDifference100Ns;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "clock" && E.NamespaceURI == NamespaceSynchronization &&
					DateTimeHF.TryParse(E.InnerText, out DateTimeHF ServerTime))
				{
					long dt1 = ServerTime - ClientTime1;
					long dt2 = ClientTime2 - ServerTime;
					Latency100Ns = dt1 + dt2;
					ClockDifference100Ns = dt1 - dt2;
				}
				else
				{
					e.Ok = false;
					Latency100Ns = 0;
					ClockDifference100Ns = 0;
				}

				try
				{
					Callback?.Invoke(this, new SynchronizationEventArgs(Latency100Ns, ClockDifference100Ns, e));
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
			this.difference100NsWindow = new SpikeRemoval(WindowSize, SpikePosition, SpikeWidth);
			this.sumLatency100Ns = 0;
			this.sumDifference100Ns = 0;
			this.nrLatency100Ns = 0;
			this.nrDifference100Ns = 0;
			this.maxInHistory = History;

			this.timer = new Timer(this.CheckClock, null, IntervalMilliseconds, IntervalMilliseconds);
		}

		private void CheckClock(object P)
		{
			this.MeasureClockDifference(this.clockSourceJID, (sender, e) =>
			{
				if (e.Ok)
				{
					this.rawLatency100Ns = e.Latency100Ns;
					this.rawClockDifference100Ns = e.ClockDifference100Ns;

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

					if (Rec.SumLatency100Ns.HasValue || Rec.SumDifference100Ns.HasValue)
					{
						this.history.Add(Rec);

						if (Rec.SumLatency100Ns.HasValue)
						{
							this.filteredLatency100Ns = (Rec.SumLatency100Ns.Value + (Rec.NrLatency100Ns >> 1)) / Rec.NrLatency100Ns;

							this.sumLatency100Ns += Rec.SumLatency100Ns.Value;
							this.nrLatency100Ns += Rec.NrLatency100Ns;
						}

						if (Rec.SumDifference100Ns.HasValue)
						{
							this.filteredClockDifference100Ns = (Rec.SumDifference100Ns.Value + (Rec.NrDifference100Ns >> 1)) / Rec.NrDifference100Ns;

							this.sumDifference100Ns += Rec.SumDifference100Ns.Value;
							this.nrDifference100Ns += Rec.NrDifference100Ns;
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

							if (Rec.SumDifference100Ns.HasValue)
							{
								this.sumDifference100Ns -= Rec.SumDifference100Ns.Value;
								this.nrDifference100Ns -= Rec.NrDifference100Ns;
							}
						}

						if (this.nrLatency100Ns > 0)
							this.avgLatency100Ns = (this.sumLatency100Ns + (this.nrLatency100Ns >> 1)) / this.nrLatency100Ns;

						if (this.nrDifference100Ns > 0)
							this.avgClockDifference100Ns = (this.sumDifference100Ns + (this.nrDifference100Ns >> 1)) / this.nrDifference100Ns;
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
		/// Most recent raw sample of the latency of sending a stanza between the machine and the clock source, or vice versa.
		/// </summary>
		public long RawLatency100Ns => this.rawLatency100Ns;

		/// <summary>
		/// Most recent filtered sample of the latency of sending a stanza between the machine and the clock source, or vice versa.
		/// </summary>
		public long FilteredLatency100Ns => this.filteredLatency100Ns;

		/// <summary>
		/// If a spike was detected and removed when calculating <see cref="FilteredLatency100Ns"/> from a short sequence of <see cref="RawLatency100Ns"/>.
		/// </summary>
		public bool LatencySpikeRemoved => this.latencyRemoved;

		/// <summary>
		/// Most recent average of the latency of sending a stanza between the machine and the clock source, or vice versa.
		/// The average is calculated on a sequence of <see cref="FilteredLatency100Ns"/>.
		/// </summary>
		public long AvgLatency100Ns => this.avgLatency100Ns;

		/// <summary>
		/// Most recent raw sample of the clock difference beween the machine and the clock source.
		/// </summary>
		public long RawClockDifference100Ns => this.rawClockDifference100Ns;

		/// <summary>
		/// Most recent filtered sample of the clock difference beween the machine and the clock source.
		/// </summary>
		public long FilteredClockDifference100Ns => this.filteredClockDifference100Ns;

		/// <summary>
		/// If a spike was detected and removed when calculating <see cref="FilteredClockDifference100Ns"/> from a short sequence of <see cref="RawClockDifference100Ns"/>.
		/// </summary>
		public bool ClockDifferenceSpikeRemoved => this.latencyRemoved;

		/// <summary>
		/// Most recent average of the clock difference beween the machine and the clock source.
		/// The average is calculated on a sequence of <see cref="FilteredClockDifference100Ns"/>.
		/// </summary>
		public long AvgClockDifference100Ns => this.avgClockDifference100Ns;

		/// <summary>
		/// Event raised when the clock difference estimates have been updated.
		/// </summary>
		public event EventHandler OnUpdated = null;

		private class Rec
		{
			public DateTime Timestamp;
			public long? SumLatency100Ns;
			public long? SumDifference100Ns;
			public int NrLatency100Ns;
			public int NrDifference100Ns;
		}

	}
}
