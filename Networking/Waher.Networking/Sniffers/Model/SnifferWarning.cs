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
		/// <param name="Text">Text.</param>
		public SnifferWarning(string Text)
			: base(Text)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Warning(this.Text);
		}
	}
}
