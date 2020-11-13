using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Networking.Sniffers.Model;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Sniffer that stores events in memory.
	/// </summary>
	public class InMemorySniffer : SnifferBase, IEnumerable<SnifferEvent>
	{
		private readonly LinkedList<SnifferEvent> events = new LinkedList<SnifferEvent>();
		private readonly int maxCount;
		private int count = 0;

		/// <summary>
		/// Sniffer that stores events in memory.
		/// </summary>
		public InMemorySniffer()
			: this(int.MaxValue)
		{
		}

		/// <summary>
		/// Sniffer that stores events in memory.
		/// </summary>
		/// <param name="MaxCount">Maximum number of records in memory.</param>
		public InMemorySniffer(int MaxCount)
		{
			this.maxCount = MaxCount;
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public override void Error(DateTime Timestamp, string Error)
		{
			this.Add(new SnifferError(Timestamp, Error));
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public override void Exception(DateTime Timestamp, string Exception)
		{
			this.Add(new SnifferException(Timestamp, Exception));
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public override void Information(DateTime Timestamp, string Comment)
		{
			this.Add(new SnifferInformation(Timestamp, Comment));
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override void ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			this.Add(new SnifferRxBinary(Timestamp, Data));
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			this.Add(new SnifferRxText(Timestamp, Text));
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			this.Add(new SnifferTxBinary(Timestamp, Data));
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override void TransmitText(DateTime Timestamp, string Text)
		{
			this.Add(new SnifferTxText(Timestamp, Text));
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public override void Warning(DateTime Timestamp, string Warning)
		{
			this.Add(new SnifferWarning(Timestamp, Warning));
		}

		private void Add(SnifferEvent Event)
		{
			lock (this.events)
			{
				this.events.AddLast(Event);
				if (this.count >= this.maxCount)
					this.events.RemoveFirst();
				else
					this.count++;
			}
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.ToArray().GetEnumerator();
		}

		/// <summary>
		/// <see cref="IEnumerable{SnifferEvent}.GetEnumerator"/>
		/// </summary>
		public IEnumerator<SnifferEvent> GetEnumerator()
		{
			SnifferEvent[] A = this.ToArray();
			return ((IEnumerable<SnifferEvent>)A).GetEnumerator();
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffable">Receiver of sniffer events.</param>
		public void Replay(Sniffable Sniffable)
		{
			if (Sniffable.HasSniffers)
				this.Replay(Sniffable.Sniffers);
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffers">Receiver of sniffer events.</param>
		public void Replay(params ISniffer[] Sniffers)
		{
			foreach (SnifferEvent Event in this.ToArray())
			{
				foreach (ISniffer Sniffer in Sniffers)
					Event.Replay(Sniffer);
			}
		}

		/// <summary>
		/// Returns recorded events as an array.
		/// </summary>
		/// <returns>Recorded events.</returns>
		public SnifferEvent[] ToArray()
		{
			lock (this.events)
			{
				SnifferEvent[] Result = new SnifferEvent[this.count];
				this.events.CopyTo(Result, 0);
				return Result;
			}
		}
	}
}
