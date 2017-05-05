using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Represents a sniffer text reception event.
	/// </summary>
	public class SnifferRxText : SnifferTextEvent
	{
		/// <summary>
		/// Represents a sniffer text reception event.
		/// </summary>
		/// <param name="Text">Text.</param>
		public SnifferRxText(string Text)
			: base(Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.ReceiveText(this.Text);
		}
	}
}
