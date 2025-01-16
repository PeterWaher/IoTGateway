using System;
using Waher.Runtime.Queue;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for sniffer events.
	/// </summary>
	public abstract class SnifferEvent : WorkItem
	{
		private readonly DateTime timestamp;
		private readonly ISniffEventProcessor processor;

		/// <summary>
		/// Base class for sniffer events.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Processor">Processor</param>
		public SnifferEvent(DateTime Timestamp, ISniffEventProcessor Processor)
		{
			this.timestamp = Timestamp;
			this.processor = Processor;
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp => this.timestamp;

		/// <summary>
		/// Asynchronous processor of event.
		/// </summary>
		protected ISniffEventProcessor Processor => this.processor;

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public abstract void Replay(ISniffer Sniffer);

		/// <summary>
		/// Converts the sniffer event to a string.
		/// </summary>
		/// <returns>String-representation of sniffer event.</returns>
		public abstract new string ToString();
	}
}
