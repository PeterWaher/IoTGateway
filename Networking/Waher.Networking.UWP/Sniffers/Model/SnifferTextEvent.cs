using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for text-based sniffer events.
	/// </summary>
	public abstract class SnifferTextEvent : SnifferEvent
	{
		private string text;

		/// <summary>
		/// Base class for text-based sniffer events.
		/// </summary>
		/// <param name="Text">Text.</param>
		public SnifferTextEvent(string Text)
			: base()
		{
			this.text = Text;
		}

		/// <summary>
		/// Text
		/// </summary>
		public string Text
		{
			get { return this.text; }
		}
	}
}
