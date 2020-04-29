using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Delegate for delivery event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void DeliveryEventHandler(object Sender, DeliveryEventArgs e);

	/// <summary>
	/// Event arguments for delivery events.
	/// </summary>
	public class DeliveryEventArgs : EventArgs
	{
		private readonly object state;
		private readonly bool ok;

		internal DeliveryEventArgs(object State, bool Ok)
		{
			this.state = State;
			this.ok = Ok;
		}

		/// <summary>
		/// Oritinal state object.
		/// </summary>
		public object State { get { return this.state; } }

		/// <summary>
		/// If the delivery was successful (true) or failed (false).
		/// </summary>
		public bool Ok { get { return this.ok; } }
	}
}
