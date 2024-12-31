using System;
using System.Threading.Tasks;
using System.Threading;

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
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferWarning(DateTime Timestamp, string Text, ISniffEventProcessor Processor)
			: base(Timestamp, Text, Processor)
		{
		}

		/// <summary>
		/// Executes the operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		/// <param name="RegisterCancelToken">If task can be cancelled.</param>
		protected override sealed Task Execute(CancellationToken Cancel, bool RegisterCancelToken)
		{
			return this.Processor.Process(this);
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.Warning(this.Timestamp, this.Text);
		}

		/// <inheritdoc/>
		public override string ToString()
        {
            return "Warning: " + this.Text;
        }
    }
}
