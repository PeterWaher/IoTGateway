using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for sniffer events.
	/// </summary>
	public abstract class SnifferEvent
	{
		private DateTime timestamp;

		/// <summary>
		/// Base class for sniffer events.
		/// </summary>
		public SnifferEvent()
		{
			this.timestamp = DateTime.Now;
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
