using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Represents a sniffer binary transmission event.
	/// </summary>
	public class SnifferTxBinary : SnifferBinaryEvent
	{
		/// <summary>
		/// Represents a sniffer binary transmission event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Data.</param>
		/// <param name="Offset">Offset into array.</param>
		/// <param name="Count">Number of bytes</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferTxBinary(DateTime Timestamp, byte[] Data, int Offset, int Count, ISniffEventProcessor Processor)
			: base(Timestamp, Data, Offset, Count, Processor)
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
			Sniffer.TransmitBinary(this.Timestamp, true, this.Data, this.Offset, this.Count);
		}

		/// <inheritdoc/>
		public override string ToString()
        {
            int Len = this.Data?.Length ?? 0;

            if (Len == 1)
                return "TX: (1 byte)";
            else
                return "TX: (" + Len.ToString() + " bytes)";
        }
    }
}
