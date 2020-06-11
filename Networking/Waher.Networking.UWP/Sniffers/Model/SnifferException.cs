using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Represents a sniffer exception event.
	/// </summary>
	public class SnifferException : SnifferTextEvent
	{
		/// <summary>
		/// Represents a sniffer exception event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		public SnifferException(DateTime Timestamp, string Text)
			: base(Timestamp, Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Exception(this.Timestamp, this.Text);
		}

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return "EXCEPTION: " + this.Text;
        }
    }
}
