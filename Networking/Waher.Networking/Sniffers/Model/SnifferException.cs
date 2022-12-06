using System;
using System.Threading.Tasks;

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
		public override Task Replay(ISniffer Sniffer)
		{
			return Sniffer.Exception(this.Timestamp, this.Text);
		}

		/// <inheritdoc/>
		public override string ToString()
        {
            return "EXCEPTION: " + this.Text;
        }
    }
}
