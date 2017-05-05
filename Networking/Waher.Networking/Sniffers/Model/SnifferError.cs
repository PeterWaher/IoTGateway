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
		/// <param name="Text">Text.</param>
		public SnifferError(string Text)
			: base(Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Error(this.Text);
		}
	}
}
