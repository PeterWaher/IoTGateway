using System;
using System.Threading.Tasks;

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
		public override Task Replay(ISniffer Sniffer)
		{
			return Sniffer.Error(this.Timestamp, this.Text);
		}

		/// <inheritdoc/>
		public override string ToString()
        {
            return "ERROR: " + this.Text;
        }
    }
}
