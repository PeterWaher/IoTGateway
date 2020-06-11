using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Represents a sniffer text transmission event.
	/// </summary>
	public class SnifferTxText : SnifferTextEvent
	{
		/// <summary>
		/// Represents a sniffer text transmission event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		public SnifferTxText(DateTime Timestamp, string Text)
			: base(Timestamp, Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.TransmitText(this.Timestamp, this.Text);
		}

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return "TX: " + this.Text;
        }
    }
}
