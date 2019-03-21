using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for binary-based sniffer events.
	/// </summary>
	public abstract class SnifferBinaryEvent : SnifferEvent
	{
		private byte[] data;

		/// <summary>
		/// Base class for binary-based sniffer events.
		/// </summary>
		/// <param name="Data">Data.</param>
		public SnifferBinaryEvent(byte[] Data)
			: base()
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
