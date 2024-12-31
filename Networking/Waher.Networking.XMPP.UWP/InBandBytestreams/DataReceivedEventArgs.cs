using System;

namespace Waher.Networking.XMPP.InBandBytestreams
{
	/// <summary>
	/// Event arguments containing received binary data.
	/// </summary>
	public class DataReceivedEventArgs : EventArgs
	{
		private readonly bool constantBuffer;
		private readonly byte[] data;
		private readonly object state;

		/// <summary>
		/// Event arguments containing received binary data.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Data received.</param>
		/// <param name="State">State object.</param>
		public DataReceivedEventArgs(bool ConstantBuffer, byte[] Data, object State)
		{
			this.constantBuffer = ConstantBuffer;
			this.data = Data;
			this.state = State;
		}

		/// <summary>
		/// If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).
		/// </summary>
		public bool ConstantBuffer => this.constantBuffer;

		/// <summary>
		/// Binary data received.
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;
	}
}
