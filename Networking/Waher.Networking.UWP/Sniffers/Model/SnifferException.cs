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
		/// <param name="Text">Text.</param>
		public SnifferException(string Text)
			: base(Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Exception (this.Text);
		}
	}
}
