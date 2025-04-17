using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers.Model
{
    /// <summary>
    /// Represents a sniffer binary reception event.
    /// </summary>
    public class SnifferRxBinary : SnifferBinaryEvent
    {
		/// <summary>
		/// Represents a sniffer binary reception event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferRxBinary(DateTime Timestamp, int Count, ISniffEventProcessor Processor)
			: base(Timestamp, Count, Processor)
		{
		}

		/// <summary>
		/// Represents a sniffer binary reception event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Data.</param>
		/// <param name="Offset">Offset into array.</param>
		/// <param name="Count">Number of bytes</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferRxBinary(DateTime Timestamp, byte[] Data, int Offset, int Count, ISniffEventProcessor Processor)
            : base(Timestamp, Data, Offset, Count, Processor)
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
			if (this.HasData)
				Sniffer.ReceiveBinary(this.Timestamp, true, this.Data, this.Offset, this.Count);
			else
				Sniffer.ReceiveBinary(this.Timestamp, this.Count);
		}

		/// <inheritdoc/>
		public override string ToString()
        {
            if (this.Count == 1)
                return "RX: (1 byte)";
            else
                return "RX: (" + this.Count.ToString() + " bytes)";
        }
    }
}
