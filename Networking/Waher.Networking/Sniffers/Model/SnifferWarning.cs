using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Represents a sniffer warning event.
	/// </summary>
	public class SnifferWarning : SnifferTextEvent
	{
		/// <summary>
		/// Represents a sniffer warning event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		public SnifferWarning(DateTime Timestamp, string Text)
			: base(Timestamp, Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Warning(this.Timestamp, this.Text);
		}

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return "Warning: " + this.Text;
        }
    }
}
