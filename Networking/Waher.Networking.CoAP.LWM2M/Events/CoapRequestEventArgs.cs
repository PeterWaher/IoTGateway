using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.CoAP;

namespace Waher.Networking.LWM2M.Events
{
	/// <summary>
	/// Delegate for CoAP request event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void CoapRequestEventHandler(object Sender, CoapRequestEventArgs e);

	/// <summary>
	/// Event arguments for CoAP request events.
	/// </summary>
    public class CoapRequestEventArgs : EventArgs
    {
		private CoapMessage request;

		/// <summary>
		/// Event arguments for CoAP request events.
		/// </summary>
		/// <param name="Request">Request</param>
		public CoapRequestEventArgs(CoapMessage Request)
		{
			this.request = Request;
		}

		/// <summary>
		/// Request message.
		/// </summary>
		public CoapMessage Request
		{
			get { return this.request; }
		}
    }
}
