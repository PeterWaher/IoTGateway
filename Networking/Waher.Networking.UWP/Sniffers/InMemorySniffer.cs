using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.Sniffers.Model;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Sniffer that stores events in memory.
	/// </summary>
	public class InMemorySniffer : SnifferBase, IEnumerable<SnifferEvent>, IDisposable
	{
		private readonly LinkedList<SnifferEvent> events = new LinkedList<SnifferEvent>();
		private readonly int maxCount;
		private int count = 0;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

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
		public override Task Error(DateTime Timestamp, string Error)
		{
			return this.Add(new SnifferError(Timestamp, Error));
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public override Task Exception(DateTime Timestamp, string Exception)
		{
			return this.Add(new SnifferException(Timestamp, Exception));
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public override Task Information(DateTime Timestamp, string Comment)
		{
			return this.Add(new SnifferInformation(Timestamp, Comment));
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override Task ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			return this.Add(new SnifferRxBinary(Timestamp, Data));
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task ReceiveText(DateTime Timestamp, string Text)
		{
			return this.Add(new SnifferRxText(Timestamp, Text));
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			return this.Add(new SnifferTxBinary(Timestamp, Data));
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task TransmitText(DateTime Timestamp, string Text)
		{
			return this.Add(new SnifferTxText(Timestamp, Text));
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public override Task Warning(DateTime Timestamp, string Warning)
		{
			return this.Add(new SnifferWarning(Timestamp, Warning));
		}

		private async Task Add(SnifferEvent Event)
		{
			await this.semaphore.WaitAsync();
			try
			{
				this.events.AddLast(Event);
				if (this.count >= this.maxCount)
					this.events.RemoveFirst();
				else
					this.count++;
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.ToArrayAsync().Result.GetEnumerator();
		}

		/// <summary>
		/// <see cref="IEnumerable{SnifferEvent}.GetEnumerator"/>
		/// </summary>
		public IEnumerator<SnifferEvent> GetEnumerator()
		{
			SnifferEvent[] A = this.ToArrayAsync().Result;
			return ((IEnumerable<SnifferEvent>)A).GetEnumerator();
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffable">Receiver of sniffer events.</param>
		[Obsolete("Use ReplayAsync instead, for better asynchronous performance.")]
		public void Replay(Sniffable Sniffable)
		{
			this.ReplayAsync(Sniffable).Wait();
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffers">Receiver of sniffer events.</param>
		[Obsolete("Use ReplayAsync instead, for better asynchronous performance.")]
		public void Replay(params ISniffer[] Sniffers)
		{
			this.ReplayAsync(Sniffers).Wait();
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffable">Receiver of sniffer events.</param>
		public async Task ReplayAsync(Sniffable Sniffable)
		{
			if (Sniffable.HasSniffers)
				await this.ReplayAsync(Sniffable.Sniffers);
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffers">Receiver of sniffer events.</param>
		public async Task ReplayAsync(params ISniffer[] Sniffers)
		{
			foreach (SnifferEvent Event in await this.ToArrayAsync())
			{
				foreach (ISniffer Sniffer in Sniffers)
					Event.Replay(Sniffer);
			}
		}

		/// <summary>
		/// Returns recorded events as an array.
		/// </summary>
		/// <returns>Recorded events.</returns>
		[Obsolete("Use ToArrayAsync instead, for better asynchronous performance.")]
		public SnifferEvent[] ToArray()
		{
			return this.ToArrayAsync().Result;
		}

		/// <summary>
		/// Returns recorded events as an array.
		/// </summary>
		/// <returns>Recorded events.</returns>
		public async Task<SnifferEvent[]> ToArrayAsync()
		{
			await this.semaphore.WaitAsync();
			try
			{
				SnifferEvent[] Result = new SnifferEvent[this.count];
				this.events.CopyTo(Result, 0);
				return Result;
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.semaphore.Dispose();
		}
	}
}
