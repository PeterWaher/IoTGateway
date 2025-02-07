using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for text-based sniffer events.
	/// </summary>
	public abstract class SnifferTextEvent : SnifferEvent
	{
		private readonly string text;

		/// <summary>
		/// Base class for text-based sniffer events.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferTextEvent(DateTime Timestamp, string Text, ISniffEventProcessor Processor)
			: base(Timestamp, Processor)
		{
			this.text = Text;
		}

		/// <summary>
		/// Text
		/// </summary>
		public string Text => this.text;
	}
}
