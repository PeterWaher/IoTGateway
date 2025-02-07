using System;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for binary-based sniffer events.
	/// </summary>
	public abstract class SnifferBinaryEvent : SnifferEvent
	{
		private readonly byte[] data;
		private readonly int offset;
		private readonly int count;
		private readonly bool hasData;

		/// <summary>
		/// Base class for binary-based sniffer events.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferBinaryEvent(DateTime Timestamp, int Count, ISniffEventProcessor Processor)
			: base(Timestamp, Processor)
		{
			this.data = null;
			this.offset = 0;
			this.count = Count;
			this.hasData = false;
		}

		/// <summary>
		/// Base class for binary-based sniffer events.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Data.</param>
		/// <param name="Offset">Offset into array.</param>
		/// <param name="Count">Number of bytes</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferBinaryEvent(DateTime Timestamp, byte[] Data, int Offset, int Count, ISniffEventProcessor Processor)
			: base(Timestamp, Processor)
		{
			this.data = Data;
			this.offset = Offset;
			this.count = Count;
			this.hasData = true;
		}

		/// <summary>
		/// If binary data is available in the event (true), or if only the
		/// <see cref="Count"/> property is available, for information.
		/// </summary>
		public bool HasData => this.hasData;

		/// <summary>
		/// Data
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// Offset into <see cref="Data"/>.
		/// </summary>
		public int Offset => this.offset;

		/// <summary>
		/// Number of bytes
		/// </summary>
		public int Count => this.count;
    }
}
