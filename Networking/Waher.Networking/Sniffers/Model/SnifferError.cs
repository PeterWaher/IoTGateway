﻿using System;
using System.Threading;
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
		/// <param name="Processor">Sniff event processor</param>
		public SnifferError(DateTime Timestamp, string Text, ISniffEventProcessor Processor)
			: base(Timestamp, Text, Processor)
		{
		}

		/// <summary>
		/// Executes the operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		public override sealed Task Execute(CancellationToken Cancel)
		{
			return this.Processor.Process(this, Cancel);
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Error(this.Timestamp, this.Text);
		}

		/// <inheritdoc/>
		public override string ToString()
        {
            return "ERROR: " + this.Text;
        }
    }
}
