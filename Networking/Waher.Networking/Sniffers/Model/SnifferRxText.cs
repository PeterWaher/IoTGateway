using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Represents a sniffer text reception event.
	/// </summary>
	public class SnifferRxText : SnifferTextEvent
	{
		/// <summary>
		/// Represents a sniffer text reception event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		public SnifferRxText(DateTime Timestamp, string Text)
			: base(Timestamp, Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.ReceiveText(this.Timestamp, this.Text);
		}

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return "RX: " + this.Text;
        }
    }
}
