using System;

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
		/// <param name="Data">Data.</param>
		public SnifferTxBinary(byte[] Data)
			: base(Data)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override void Replay(ISniffer Sniffer)
		{
			Sniffer.TransmitBinary(this.Data);
		}

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
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
