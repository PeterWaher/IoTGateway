using System;
using System.Collections.Generic;
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
		private string clockSourceJID;
		private int count;
		private int countAvg;
		private int maxItems;
		private int spikePos;
		private long sumLatencyMyS;
		private long sumDifferenceMyS;

		/// <summary>
		/// Implements the clock synchronization extesion as defined by the IEEE XMPP IoT Interface working group.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public SynchronizationClient(XmppClient Client)
		{
			this.client = Client;
			this.timer = null;
			this.clockSourceJID = null;

			this.client.RegisterIqGetHandler("clock", NamespaceSynchronization, this.Clock, true);
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
				this.client.UnregisterIqGetHandler("clock", NamespaceSynchronization, this.Clock, true);
				this.client = null;
			}

		}

		private void Clock(object Sender, IqEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<clock xmlns='");
			Xml.Append(NamespaceSynchronization);
			Xml.Append("'>");
			Xml.Append(XML.Encode(DateTime.UtcNow));
			Xml.Append("</clock>");

			e.IqResult(Xml.ToString());
		}

		/// <summary>
		/// Measures the difference between the client clock and the clock of a clock source.
		/// </summary>
		/// <param name="ClockSourceJID">JID of clock source.</param>
		/// <param name="Callback">Callback method</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void MeasureClockDifference(string ClockSourceJID, SynchronizationEventHandler Callback, object State)
		{
			DateTime ClientTime1 = DateTime.UtcNow;

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<clock xmlns='");
			Xml.Append(NamespaceSynchronization);
			Xml.Append("'>");
			Xml.Append(XML.Encode(ClientTime1));
			Xml.Append("</clock>");

			this.client.SendIqGet(ClockSourceJID, Xml.ToString(), (sender, e) =>
			{
				DateTime ClientTime2 = DateTime.UtcNow;
				XmlElement E;
				double LatencySeconds;
				double ClockDifferenceSeconds;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "clock" && E.NamespaceURI == NamespaceSynchronization &&
					XML.TryParse(E.InnerText, out DateTime ServerTime))
				{
					TimeSpan dt1 = ServerTime - ClientTime1;
					TimeSpan dt2 = ClientTime2 - ServerTime;
					LatencySeconds = (dt1 + dt2).TotalSeconds * 0.5;
					ClockDifferenceSeconds = (dt1 - dt2).TotalSeconds * 0.5;
				}
				else
				{
					e.Ok = false;
					LatencySeconds = double.NaN;
					ClockDifferenceSeconds = double.NaN;
				}

				try
				{
					Callback?.Invoke(this, new SynchronizationEventArgs(LatencySeconds, ClockDifferenceSeconds, e));
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
			this.MonitorClockDifference(ClockSourceJID, IntervalMilliseconds, 100, 10);
		}

		/// <summary>
		/// Monitors the difference between the client clock and the clock of a clock source,
		/// by regularly measuring latency and clock difference.
		/// </summary>
		/// <param name="ClockSourceJID">JID of clock source.</param>
		/// <param name="IntervalMilliseconds">Interval, in milliseconds.</param>
		/// <param name="WindowSize">Number of samples to keep in memory. (Default=100)</param>
		/// <param name="SpikePosition">Spike removal position, inside window. (Default=10)</param>
		public void MonitorClockDifference(string ClockSourceJID, int IntervalMilliseconds,
			int WindowSize, int SpikePosition)
		{
			if (IntervalMilliseconds < 5000)
				throw new ArgumentException("Interval must be at least 5000 milliseconds.", nameof(IntervalMilliseconds));

			if (WindowSize <= 2)
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
			this.maxItems = WindowSize;
			this.spikePos = SpikePosition;
			this.count = 0;
			this.sumDifferenceMyS = 0;
			this.sumLatencyMyS = 0;
			this.timer = new Timer(this.CheckClock, null, IntervalMilliseconds, IntervalMilliseconds);
		}

		private void CheckClock(object P)
		{
			this.MeasureClockDifference(this.clockSourceJID, (sender, e) =>
			{
				if (e.Ok)
				{
					Rec Rec = new Rec()
					{
						LatencyMyS = (long)(e.LatencySeconds * 1e6 + 0.5),
						DifferenceMyS = (long)(e.ClockDifferenceSeconds * 1e6 + 0.5)
					};

					this.history.Add(Rec);

					this.sumLatencyMyS += Rec.LatencyMyS;
					this.sumDifferenceMyS += Rec.DifferenceMyS;

					if (this.count >= this.maxItems)
					{
						Rec = this.history[this.count - 1];
						this.history.RemoveAt(this.count - 1);

						if (Rec != null)
						{
							this.sumLatencyMyS -= Rec.LatencyMyS;
							this.sumDifferenceMyS -= Rec.DifferenceMyS;
						}
					}
					else
						this.count++;


				}
			}, null);
		}

		private class Rec
		{
			public long LatencyMyS;
			public long DifferenceMyS;
		}


	}
}
