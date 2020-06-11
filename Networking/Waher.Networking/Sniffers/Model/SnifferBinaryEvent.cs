using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for binary-based sniffer events.
	/// </summary>
	public abstract class SnifferBinaryEvent : SnifferEvent
	{
		private readonly byte[] data;

		/// <summary>
		/// Base class for binary-based sniffer events.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Data.</param>
		public SnifferBinaryEvent(DateTime Timestamp, byte[] Data)
			: base(Timestamp)
		{
			this.data = Data;
		}

		/// <summary>
		/// Data
		/// </summary>
		public byte[] Data
		{
			get { return this.data; }
		}
    }
}
