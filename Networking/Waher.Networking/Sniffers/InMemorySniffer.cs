using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers.Model;
using Waher.Runtime.Collections;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Sniffer that stores events in memory.
	/// </summary>
	public class InMemorySniffer : SnifferBase, IEnumerable<SnifferEvent>, IDisposable
	{
		private readonly ChunkedList<SnifferEvent> events = new ChunkedList<SnifferEvent>();
		private readonly int maxCount;
		private int count = 0;
		private bool disposed = false;

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
		/// How the sniffer handles binary data.
		/// </summary>
		public override BinaryPresentationMethod BinaryPresentationMethod => BinaryPresentationMethod.Hexadecimal;

		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxBinary Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxBinary Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxText Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxText Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferInformation Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferWarning Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferError Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferException Event)
		{
			this.Add(Event);
			return Task.CompletedTask;
		}

		private void Add(SnifferEvent Event)
		{
			if (!this.disposed)
			{
				lock (this.events)
				{
					this.events.Add(Event);
					if (this.count >= this.maxCount)
						this.events.RemoveFirst();
					else
						this.count++;
				}
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
		/// <param name="ComLayer">Receiver of sniffer events.</param>
		public void Replay(CommunicationLayer ComLayer)
		{
			if (ComLayer.HasSniffers)
				this.Replay(ComLayer.Sniffers);
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
			if (this.disposed)
				return Array.Empty<SnifferEvent>();
			else
			{
				lock (this.events)
				{
					SnifferEvent[] Result = new SnifferEvent[this.count];
					this.events.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		/// <inheritdoc/>
		public override Task DisposeAsync()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				lock (this.events)
				{
					this.events.Clear();
				}
			}

			return base.DisposeAsync();
		}
	}
}
