﻿using System;
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
		public SnifferTxBinary(DateTime Timestamp, byte[] Data)
			: base(Timestamp, Data)
		{
		}

		/// <summary>
		/// Replays the event to a given sniffer.
		/// </summary>
		/// <param name="Sniffer">Sniffer.</param>
		public override Task Replay(ISniffer Sniffer)
		{
			return Sniffer.TransmitBinary(this.Timestamp, this.Data);
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
