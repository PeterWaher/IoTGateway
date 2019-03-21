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
		/// <param name="Text">Text.</param>
		public SnifferTxText(string Text)
			: base(Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.TransmitText(this.Text);
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
