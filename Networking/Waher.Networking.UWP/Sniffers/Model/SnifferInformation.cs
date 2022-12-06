using System;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers.Model
{
    /// <summary>
    /// Represents a sniffer information event.
    /// </summary>
    public class SnifferInformation : SnifferTextEvent
    {
        /// <summary>
        /// Represents a sniffer information event.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Text">Text.</param>
        public SnifferInformation(DateTime Timestamp, string Text)
            : base(Timestamp, Text)
        {
        }

        /// <summary>
        /// Replays the event to a given sniffer.
        /// </summary>
        /// <param name="Sniffer">Sniffer.</param>
        public override Task Replay(ISniffer Sniffer)
        {
            return Sniffer.Information(this.Timestamp, this.Text);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Information: " + this.Text;
        }
    }
}
