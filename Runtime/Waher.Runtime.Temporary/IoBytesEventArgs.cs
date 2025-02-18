using System;

namespace Waher.Runtime.Temporary
{
    /// <summary>
    /// Event arguments for an I/O bytes event.
    /// </summary>
    public class IoBytesEventArgs : EventArgs
    {
		/// <summary>
		/// Event arguments for an I/O bytes event.
		/// </summary>
		/// <param name="Nr">Number of bytes</param>
		/// <param name="Total">Total number of bytes</param>
		public IoBytesEventArgs(int Nr, long Total)
        {
            this.Nr = Nr;
            this.Total = Total;
        }

		/// <summary>
		/// Number of bytes
		/// </summary>
		public int Nr { get; }

		/// <summary>
		/// Total number of bytes
		/// </summary>
		public long Total { get; }
	}
}
