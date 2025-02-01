using System;

namespace Waher.Runtime.Queue
{
	/// <summary>
	/// Event arguments for processor events.
	/// </summary>
	public class ProcessorEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for processor events.
		/// </summary>
		/// <param name="ProcessorIndex">Zero-based processor index.</param>
		public ProcessorEventArgs(int ProcessorIndex)
			: base()
		{
			this.ProcessorIndex = ProcessorIndex;
		}

		/// <summary>
		/// Zero-based processor index.
		/// </summary>
		public int ProcessorIndex { get; }
	}
}
