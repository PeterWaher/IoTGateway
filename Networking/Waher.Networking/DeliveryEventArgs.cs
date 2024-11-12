using System;

namespace Waher.Networking
{
	/// <summary>
	/// Event arguments for delivery events.
	/// </summary>
	public class DeliveryEventArgs : EventArgs
	{
		private readonly object state;
		private readonly bool ok;

		/// <summary>
		/// Event arguments for delivery events.
		/// </summary>
		/// <param name="State">State object</param>
		///	<param name="Ok">If delivery was successful.</param>
		public DeliveryEventArgs(object State, bool Ok)
		{
			this.state = State;
			this.ok = Ok;
		}

		/// <summary>
		/// Oritinal state object.
		/// </summary>
		public object State => this.state;

		/// <summary>
		/// If the delivery was successful (true) or failed (false).
		/// </summary>
		public bool Ok => this.ok;
	}
}
