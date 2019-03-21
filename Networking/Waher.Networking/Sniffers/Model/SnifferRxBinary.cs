using System;

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
        /// <param name="Data">Data.</param>
        public SnifferRxBinary(byte[] Data)
            : base(Data)
        {
        }

        /// <summary>
        /// Replays the event to a given sniffer.
        /// </summary>
        /// <param name="Sniffer">Sniffer.</param>
        public override void Replay(ISniffer Sniffer)
        {
            Sniffer.ReceiveBinary(this.Data);
        }

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            int Len = this.Data?.Length ?? 0;

            if (Len == 1)
                return "RX: (1 byte)";
            else
                return "RX: (" + Len.ToString() + " bytes)";
        }
    }
}
