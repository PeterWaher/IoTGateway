using System;

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
		/// <param name="Text">Text.</param>
		public SnifferInformation(string Text)
			: base(Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Information(this.Text);
		}
	}
}
