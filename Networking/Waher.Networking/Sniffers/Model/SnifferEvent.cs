using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for sniffer events.
	/// </summary>
	public abstract class SnifferEvent
	{
		private readonly DateTime timestamp;

		/// <summary>
		/// Base class for sniffer events.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		public SnifferEvent(DateTime Timestamp)
		{
			this.timestamp = Timestamp;
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp
		{
			get { return this.timestamp; }
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public abstract void Replay(ISniffer Sniffer);
	}
}
