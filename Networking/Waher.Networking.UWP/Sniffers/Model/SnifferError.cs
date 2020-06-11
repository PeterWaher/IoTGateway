using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Represents a sniffer error event.
	/// </summary>
	public class SnifferError : SnifferTextEvent
	{
		/// <summary>
		/// Represents a sniffer error event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		public SnifferError(DateTime Timestamp, string Text)
			: base(Timestamp, Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Error(this.Timestamp, this.Text);
		}

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return "ERROR: " + this.Text;
        }
    }
}
